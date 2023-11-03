using System;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.Validation
{
    /// <summary>
    /// Container to encapsulate what is being validated: Info or Struct within an info.
    /// </summary>
    public struct ValidationObjectData
    {
        // info where data is coming from
        public Type InfoType { get; }
        public IBaseInfo Info { get; }

        // struct to validate (if non null)
        public string StructParentInfoReferenceProperty { get; }
        public string StructKeyPath { get; }
        public IBaseStruct Struct { get; }

        public ValidationObjectData(Type infoType, IBaseInfo info)
        {
            InfoType = infoType;
            Info = info;
            StructParentInfoReferenceProperty = null;
            StructKeyPath = null;
            Struct = null;
        }

        public ValidationObjectData(Type infoType, IBaseInfo info,
            string structParentInfoReferenceProperty, string structKeyPath, IBaseStruct @struct)
        {
            InfoType = infoType;
            Info = info;
            StructParentInfoReferenceProperty = structParentInfoReferenceProperty;
            StructKeyPath = structKeyPath;
            Struct = @struct;
        }
    }
}
