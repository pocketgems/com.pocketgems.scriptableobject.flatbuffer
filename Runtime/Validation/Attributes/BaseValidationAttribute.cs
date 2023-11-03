using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace PocketGems.Parameters.Validation.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class BaseValidationAttribute : Attribute, IValidationAttribute
    {
        protected abstract bool CompatibleWithReadOnlyLists { get; }
        protected abstract bool CheckType(Type type);
        protected IParameterManager _parameterManager;

        private void GetTypeInfo(PropertyInfo propertyInfo, out Type type, out bool isReadOnlyList)
        {
            type = null;
            isReadOnlyList = false;
            var propertyType = propertyInfo.PropertyType;
            if (CheckType(propertyType))
            {
                type = propertyType;
                return;
            }

            if (!CompatibleWithReadOnlyLists)
                return;

            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(IReadOnlyList<>))
            {
                var innerPropertyType = propertyType.GetGenericArguments()[0];
                if (CheckType(innerPropertyType))
                {
                    type = innerPropertyType;
                    isReadOnlyList = true;
                }
            }
        }

        public virtual bool CanValidate(PropertyInfo propertyInfo)
        {
            GetTypeInfo(propertyInfo, out Type type, out _);
            return type != null;
        }

        public virtual void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo)
        {
            _parameterManager = parameterManager;
            GetTypeInfo(propertyInfo, out _validationType, out _isReadOnlyList);
        }

        public virtual string Validate(IParameterManager parameterManager, PropertyInfo propertyInfo, object value)
        {
            if (!_isReadOnlyList)
                return ValidateElement(value);

            if (value == null)
                return ValidateEmptyList();

            // iterate and check
            var enumerable = (IEnumerable)value;
            int index = 0;
            foreach (var element in enumerable)
            {
                var error = ValidateElement(element);
                if (error != null)
                    return $"element[{index}] {error}";
                index++;
            }

            if (index == 0)
                return ValidateEmptyList();
            return null;
        }

        protected Type _validationType;
        protected bool _isReadOnlyList;

        protected abstract string ValidateEmptyList();

        protected abstract string ValidateElement(object element);
    }
}
