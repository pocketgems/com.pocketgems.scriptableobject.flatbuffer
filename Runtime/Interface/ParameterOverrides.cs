using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PocketGems.Parameters.Interface
{
    /// <summary>
    /// Internal container ab tests that IMutableParameter might have.
    ///
    /// Developer Memory Notes:
    /// This container could be implemented with a string[] instead of the current two data structures.
    /// This would come at different trade offs depending on usage.
    ///
    ///  Bit Array + Dict: bit array uses 1 bit per (index), dict uses 24 Bytes per entry + internal data structures overhead.
    ///  string[]: array uses a block of memory 8 Bytes per pointer (index) size + object header.
    ///
    /// Some high level analysis:
    /// - If overriding is sparse (e.g. just 1 field), string[] is more efficient if the # of fields is less than 30.
    /// - If overriding is dense (e.g. all fields), string[] is always more efficient because you use all the references.
    /// - Overall if 25% of the fields are overridden, string[] is more efficient, otherwise dict[] is more efficient.
    ///
    /// Our general use case assume that overriding should be sparse.  Most designers would override a whole column
    /// of data which would mean sparse for each row of data. The only exception is if we add a whole rows of data,
    /// the override would be dense.  This should only be a one off and wouldn't be done often.  If many rows are added
    /// it should be done via a whole parameter byte file rather than through overrides.
    /// </summary>
    public class ParameterOverrides
    {
        /// <summary>
        /// We use the overrides flag for performance and only access the dict if needed since dict
        /// is slower due to hashing and looking up.
        /// </summary>
        private BitArray _overridesFlag;

        /// <summary>
        /// Internal dictionary of overridden fields.
        /// </summary>
        private Dictionary<int, object> _overridesDict;

        public bool TryGetOverride(int index, out object value)
        {
            if (index < 0)
            {
                Debug.LogError("Attempting to fetch index outside of bounds.");
                value = null;
                return false;
            }

            if (_overridesFlag == null)
            {
                value = null;
                return false;
            }

            if (index < 0 || index >= _overridesFlag.Count)
            {
                Debug.LogError("Attempting to fetch index outside of bounds.");
                value = null;
                return false;
            }

            if (!_overridesFlag[index])
            {
                value = null;
                return false;
            }

            value = _overridesDict[index];
            return true;
        }

        public int OverrideCount => _overridesDict?.Count ?? 0;

        public bool RemoveOverride(int index, out string error)
        {
            if (_overridesFlag == null)
            {
                error = "No overrides to remove.";
                return false;
            }

            if (index < 0 || index >= _overridesFlag.Count)
            {
                error = "Attempting to fetch index outside of bounds.";
                return false;
            }

            if (!_overridesFlag[index])
            {
                error = "No overrides to remove.";
                return false;
            }

            error = null;
            _overridesFlag[index] = false;
            _overridesDict.Remove(index);
            return true;
        }

        public bool SetEditedProperty(int index, object obj, int maxSize, out string error)
        {
            if (maxSize <= 0)
            {
                error = "Max size must be a positive number - package bug.";
                return false;
            }

            if (maxSize <= index)
            {
                error = "Max size must be greater than index - package bug.";
                return false;
            }

            if (_overridesFlag == null)
            {
                _overridesFlag = new(maxSize);
                _overridesDict = new();
            }

            if (maxSize != _overridesFlag.Count)
            {
                error = "Max size changed - package bug.";
                return false;
            }

            if (_overridesFlag[index])
            {
                error = "Overrides is already set.";
                return false;
            }

            error = null;
            _overridesFlag[index] = true;
            _overridesDict[index] = obj;
            return true;
        }
    }
}
