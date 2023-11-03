using System;
using System.Reflection;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Container for validation to cache data about the current ParameterStructReference property.
    /// </summary>
    internal struct StructReferenceInfo
    {
        /// <summary>
        /// The property info of the property
        /// </summary>
        public PropertyInfo ReferencePropertyInfo { get; }
        /// <summary>
        /// The interface type of the struct.  (e.g. IRewardStruct)
        /// </summary>
        public Type ReferenceGenericType { get; }
        /// <summary>
        /// True if the PropertyInfo is a ReadOnlyList of ParameterStructReferences
        /// </summary>
        public bool IsList { get; }

        /// <summary>
        /// A cached getter to get the Struct from the object.
        /// </summary>
        public MethodInfo ReferenceStructGetter { get; }

        public StructReferenceInfo(PropertyInfo referencePropertyInfo, Type referenceGenericType, bool isList,
            MethodInfo referenceStructGetter)
        {
            ReferencePropertyInfo = referencePropertyInfo;
            ReferenceGenericType = referenceGenericType;
            IsList = isList;
            ReferenceStructGetter = referenceStructGetter;
        }
    }
}
