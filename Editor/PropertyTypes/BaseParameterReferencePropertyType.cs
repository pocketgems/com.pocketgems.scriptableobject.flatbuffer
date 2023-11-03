using System;
using System.Reflection;

namespace PocketGems.Parameters.PropertyTypes
{
    internal abstract class BaseParameterReferencePropertyType : BasePropertyType, IPropertyType
    {
        protected readonly Type _genericType;

        protected BaseParameterReferencePropertyType(PropertyInfo propertyInfo, Type genericType) : base(propertyInfo)
        {
            _genericType = genericType;
        }

        /// <summary>
        /// Types that have generic arguments have their type name with a suffix.  E.g ParameterReference`1
        /// This function removes the suffix so the original type name can be utilized.
        /// </summary>
        /// <param name="genericTypeName">name property of the type</param>
        /// <returns></returns>
        protected string SanitizedPropertyTypeName()
        {
            var genericTypeName = PropertyTypeName;
            var index = genericTypeName.IndexOf('`');
            return index == -1 ? genericTypeName : genericTypeName.Substring(0, index);
        }
    }
}
