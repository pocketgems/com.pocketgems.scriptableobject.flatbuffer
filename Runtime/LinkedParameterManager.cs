using System;
using System.Collections.Generic;
using System.Diagnostics;
using PocketGems.Parameters.Interface;
using Debug = UnityEngine.Debug;

namespace PocketGems.Parameters
{
    public class LinkedParameterManager : ParameterManager
    {
        public const int DefaultCacheDiscardFrequencySeconds = 60;

        /// <summary>
        /// Constructor for creating a parameter manager that reflects all of the parameters in the
        /// source parameterManager.  Any modifications made with Load() or RemoveAll() to either
        /// parameter manager will affect this shared pool of parameters
        ///
        /// The only modification that isn't linked between the two are the overrides applied with ApplyOverrides() and
        /// ClearAllOverrides().  The parameter factory is copied from the source when this is built, so set it on the
        /// source first (loading the data does).
        ///
        /// This manager is used to save memory when multiple parameter managers are needed with the same
        /// base parameters but different ApplyOverrides() applied to each one.  Using a LinkedParameterManager
        /// comes at a performance cost since each Get query can create a new IMutableParameter.
        ///
        /// An edit's value is only checked when its row is first read, so a bad value on a row nobody has read
        /// doesn't fail ApplyOverrides(): the error is logged on that first read and the row keeps its loaded value.
        /// That is deliberate, so applying overrides doesn't create a linked instance for every edited row.
        /// </summary>
        /// <param name="parameterManager">the source parameter manager</param>
        /// <param name="cacheDiscardFrequencySeconds">the min frequency to check for garbage collected
        /// parameters and discard dictionary entries</param>
        public LinkedParameterManager(ParameterManager parameterManager,
            int cacheDiscardFrequencySeconds = DefaultCacheDiscardFrequencySeconds) :
            base(parameterManager._identifierMappings,
            parameterManager._guidMappings)
        {
            ParameterFactory = parameterManager.ParameterFactory;
            _cacheDiscardStopwatch = Stopwatch.StartNew();
            _cacheDiscardFrequencySeconds = cacheDiscardFrequencySeconds;
            _linkedMutableParameterCache = new();
            _overridesToApplyByParameter = new();
        }

        #region overrides

        /// <inheritdoc cref="IParameterManager.Get{T}()"/>
        public override IEnumerable<T> Get<T>()
        {
            foreach (var parameter in base.Get<T>())
            {
                yield return (T)GetOrCreateLinkedMutableParameter((IMutableParameter)parameter);
            }
        }

        /// <inheritdoc cref="IParameterManager.GetSorted{T}()"/>
        public override IEnumerable<T> GetSorted<T>()
        {
            foreach (var parameter in base.GetSorted<T>())
            {
                yield return (T)GetOrCreateLinkedMutableParameter((IMutableParameter)parameter);
            }
        }

        /// <inheritdoc cref="IMutableParameterManager.ApplyOverrides"/>
        public override bool ApplyOverrides(string json, out IReadOnlyList<string> errors)
        {
            CheckToClearGarbageCollectedParameters();

            return base.ApplyOverrides(json, out errors);
        }

        /// <inheritdoc cref="IMutableParameterManager.ClearAllOverrides"/>
        public override void ClearAllOverrides()
        {
            CheckToClearGarbageCollectedParameters();

            _overridesToApplyByParameter.Clear();

            lock (_linkedMutableParameterCache)
            {
                foreach (var kvp in _linkedMutableParameterCache)
                {
                    if (kvp.Value.Target is IMutableParameter linkedParameter)
                        linkedParameter.RemoveAllEdits();
                }
            }

            // removes added and deleted rows
            base.ClearAllOverrides();
        }

        protected override IMutableParameter Get(string type, string identifier)
        {
            var parameter = base.Get(type, identifier);

            if (parameter == null)
                return null;

            return GetOrCreateLinkedMutableParameter(parameter);
        }

        protected override IMutableParameter GetWithGUID(string typeName, string guid)
        {
            var parameter = base.GetWithGUID(typeName, guid);

            if (parameter == null)
                return null;

            return GetOrCreateLinkedMutableParameter(parameter);
        }

        protected override IMutableParameter GetStructWithGUID(string typeName, string guid, bool errorForMissing)
        {
            var parameter = base.GetStructWithGUID(typeName, guid, errorForMissing);

            if (parameter == null)
                return null;

            return GetOrCreateLinkedMutableParameter(parameter);
        }

        protected override bool ApplyEditOverride(string csvName, string interfaceType, string identifierOrKeyPathOrGuid, string propertyName, string value, out string error)
        {
            // the source row, which the overrides are mapped by
            var mutableParameter = GetCurrentForOverride(interfaceType, identifierOrKeyPathOrGuid);
            if (mutableParameter == null)
            {
                error = EditTargetMissingError(csvName, interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (IsAddedByOverride(mutableParameter))
            {
                error = EditAddedRowError(interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (_overridesToApplyByParameter.TryGetValue(mutableParameter, out var overridesToApply))
            {
                foreach (var (existingProperty, _) in overridesToApply)
                {
                    if (existingProperty == propertyName)
                    {
                        error = $"({interfaceType})[{identifierOrKeyPathOrGuid}] has more than one value for [{propertyName}] assigned ";
                        return false;
                    }
                }
            }

            lock (_linkedMutableParameterCache)
            {
                /*
                 * Apply to the cached linked instance if one exists.  If none does, the value is only checked when the
                 * row is first read, in GetOrCreateLinkedMutableParameter, which logs the error and leaves the loaded
                 * value.  That is a deliberate trade-off: creating a linked instance per edited row here would cost the
                 * memory and time this manager exists to save, so a bad value on an unread row doesn't fail the call.
                 */
                if (_linkedMutableParameterCache.TryGetValue(mutableParameter, out WeakReference weakReference))
                {
                    if (weakReference.Target is IMutableParameter linkedParameter)
                    {
                        if (!linkedParameter.EditProperty(this, propertyName, value, out error))
                        {
                            error =
                                $"Error editing ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}] with value [{value}]: {error}";
                            return false;
                        }
                    }
                }

                // save the override to lazily apply later, once it has applied to any cached instance.  Inside the
                // lock, so a linked instance created meanwhile can't miss it.
                if (overridesToApply == null)
                {
                    overridesToApply = new();
                    _overridesToApplyByParameter[mutableParameter] = overridesToApply;
                }
                overridesToApply.Add((propertyName, value));
            }

            error = null;
            return true;
        }

        protected override bool RemoveEditOverride(string csvName, string interfaceType, string identifierOrKeyPathOrGuid, string propertyName, out string error)
        {
            // the source row, which the overrides are mapped by
            var mutableParameter = GetCurrentForOverride(interfaceType, identifierOrKeyPathOrGuid);
            if (mutableParameter == null)
            {
                error = EditTargetMissingError(csvName, interfaceType, identifierOrKeyPathOrGuid, propertyName);
                return false;
            }

            if (_overridesToApplyByParameter.TryGetValue(mutableParameter, out var overridesToApply))
            {
                overridesToApply.RemoveAll(o => o.Item1 == propertyName);
                if (overridesToApply.Count == 0)
                    _overridesToApplyByParameter.Remove(mutableParameter);
            }

            lock (_linkedMutableParameterCache)
            {
                if (_linkedMutableParameterCache.TryGetValue(mutableParameter, out WeakReference weakReference))
                {
                    if (weakReference.Target is IMutableParameter linkedParameter)
                    {
                        if (!linkedParameter.RevertEditedProperty(propertyName, out error))
                        {
                            error = $"Error reverting edit ({interfaceType})[{identifierOrKeyPathOrGuid}] property [{propertyName}]: {error}";
                            return false;
                        }
                    }
                }
            }

            error = null;
            return true;
        }

        protected override bool HasEditOverride(IMutableParameter parameter) =>
            _overridesToApplyByParameter.ContainsKey(parameter);

        #endregion

        #region private


        /*
         * A mapping of the original mutable parameter to the params to be applied to it.
         */
        private readonly Dictionary<IMutableParameter, List<(string, string)>> _overridesToApplyByParameter;

        /*
         * A weak reference is held onto the returned Mutable Parameters so that external uses that use
         * equality == to compare two Infos or Structs will correctly return true when comparing the same
         * infos that came from this manager multiple times.
         *
         * If the Info/Struct is no longer reference & garbage collected, the cache can safely release it's reference.
         */
        private readonly Dictionary<IMutableParameter, WeakReference> _linkedMutableParameterCache;
        private readonly Stopwatch _cacheDiscardStopwatch;
        private readonly int _cacheDiscardFrequencySeconds;

        private IMutableParameter GetOrCreateLinkedMutableParameter(IMutableParameter sourceParameter)
        {
            // a row this manager added is its own: nothing else shares it and it can't be edited, so it needs no copy
            if (IsAddedByOverride(sourceParameter))
                return sourceParameter;

            CheckToClearGarbageCollectedParameters();

            IMutableParameter linkedParameter;

            lock (_linkedMutableParameterCache)
            {
                // try to get and return previous non garbage collected info/struct
                if (_linkedMutableParameterCache.TryGetValue(sourceParameter, out var weakReference))
                {
                    linkedParameter = weakReference.Target as IMutableParameter;
                    if (linkedParameter != null)
                        return linkedParameter;
                }

                // create a new linked parameter
                linkedParameter = sourceParameter.CreateLinkedMutableParameter(this);

                // fetch overrides by mapping
                if (_overridesToApplyByParameter.TryGetValue(sourceParameter, out var overridesToApply))
                {
                    foreach (var overrideToApply in overridesToApply)
                    {
                        var propertyName = overrideToApply.Item1;
                        var value = overrideToApply.Item2;
                        // apply override to newly created linked parameter
                        if (!linkedParameter.EditProperty(this, propertyName, value, out var error))
                        {
                            Debug.LogError(
                                $"Error editing ({linkedParameter}) property [{propertyName}] with value [{value}]: {error}");
                        }
                    }
                }

                // save created linked parameter to cache
                _linkedMutableParameterCache[sourceParameter] = new WeakReference(linkedParameter);
            }

            return linkedParameter;
        }

        private void CheckToClearGarbageCollectedParameters()
        {
            lock (_linkedMutableParameterCache)
            {
                if (_linkedMutableParameterCache.Count == 0)
                {
                    _cacheDiscardStopwatch.Restart();
                    return;
                }
            }

            if (_cacheDiscardStopwatch.ElapsedMilliseconds < _cacheDiscardFrequencySeconds * 1000)
            {
                return;
            }

            ClearGarbageCollectedParameters();
            _cacheDiscardStopwatch.Restart();
        }

        /// <summary>
        /// Used for unit testing
        /// </summary>
        internal int LinkedMutableParameterCacheCount
        {
            get
            {
                lock (_linkedMutableParameterCache)
                {
                    return _linkedMutableParameterCache.Count;
                }
            }
        }

        private void ClearGarbageCollectedParameters()
        {
            lock (_linkedMutableParameterCache)
            {
                List<IMutableParameter> parametersToRemove = null;
                foreach (var kvp in _linkedMutableParameterCache)
                {
                    if (!kvp.Value.IsAlive)
                    {
                        if (parametersToRemove == null)
                            parametersToRemove = new();
                        parametersToRemove.Add(kvp.Key);
                    }
                }

                if (parametersToRemove != null)
                    foreach (var parameter in parametersToRemove)
                        _linkedMutableParameterCache.Remove(parameter);
            }
        }

        #endregion
    }
}
