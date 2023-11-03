using System;
using System.Text;
using UnityEngine;

namespace PocketGems.Parameters.Validation
{
    [Serializable]
    public class ValidationError
    {
        public Type InfoType
        {
            get
            {
                if (!_loadedType)
                {
                    if (!string.IsNullOrEmpty(_typeString))
                    {
                        _infoType = Type.GetType(_typeString);
                        if (_infoType == null)
                            Debug.LogError($"Unable to find type {_typeString}");
                    }
                    _loadedType = true;
                }
                return _infoType;
            }
        }
        // default to null to keep serialized and non serialized versions the same
        public string InfoIdentifier => string.IsNullOrEmpty(_infoIdentifier) ? null : _infoIdentifier;
        public string InfoProperty => string.IsNullOrEmpty(_infoProperty) ? null : _infoProperty;
        public string Message => string.IsNullOrEmpty(_message) ? null : _message;
        public string StructKeyPath => string.IsNullOrEmpty(_structKeyPath) ? null : _structKeyPath;
        public string StructProperty => string.IsNullOrEmpty(_structProperty) ? null : _structProperty;

        [NonSerialized] private Type _infoType;
        [NonSerialized] private bool _loadedType;

        [SerializeField] private string _typeString;
        [SerializeField] private string _infoIdentifier;
        [SerializeField] private string _infoProperty;
        [SerializeField] private string _message;
        [SerializeField] private string _structKeyPath;
        [SerializeField] private string _structProperty;

        public ValidationError(Type infoType, string infoIdentifier, string infoProperty, string message,
            string structKeyPath = null, string structProperty = null)
        {
            _infoType = infoType;
            _loadedType = true;
            if (_infoType != null)
                _typeString = infoType.AssemblyQualifiedName;
            _infoIdentifier = infoIdentifier;
            _infoProperty = infoProperty;
            _structKeyPath = structKeyPath;
            _structProperty = structProperty;
            _message = message;
        }

        public override string ToString()
        {
            var message = _message ?? "UNKNOWN ERROR";
            if (InfoType == null)
                return message;
            StringBuilder toString = new StringBuilder();
            toString.Append($"[{InfoType}]");
            if (!string.IsNullOrEmpty(_infoIdentifier))
                toString.Append($"({_infoIdentifier})");
            if (!string.IsNullOrEmpty(_infoProperty))
            {
                toString.Append($" {{Property: {_infoProperty}");
                if (!string.IsNullOrEmpty(StructKeyPath))
                    toString.Append($".{StructKeyPath}");
                if (!string.IsNullOrEmpty(StructProperty))
                    toString.Append($".{StructProperty}");
                toString.Append("}");
            }
            toString.Append($": {message}");
            return toString.ToString();
        }
    }
}
