using System.Reflection;

namespace PocketGems.Parameters.Validation.Attributes
{
    public interface IValidationAttribute
    {
        /// <summary>
        /// Validates if the property info can be validated by this attribute.
        ///
        /// If this returns true, WillValidateProperty and Validate will be subsequently called.
        /// </summary>
        /// <param name="propertyInfo">property info that this attribute is assigned to</param>
        /// <returns>true if it is valid to assign this attribute to this property</returns>
        bool CanValidate(PropertyInfo propertyInfo);

        /// <summary>
        /// Called after CanValidate() returns true but prior to Validate().
        ///
        /// Allows the Attribute to analyze the propertyInfo and cache/prepare anything needed
        /// to run Validate() optimally.
        /// </summary>
        /// <param name="parameterManager">parameter manager used for validation</param>
        /// <param name="propertyInfo">property info that this attribute is assigned to</param>
        void WillValidateProperty(IParameterManager parameterManager, PropertyInfo propertyInfo);

        /// <summary>
        /// Called on every instance of propertyInfo & value.
        /// </summary>
        /// <param name="parameterManager">parameter manager used for validation</param>
        /// <param name="propertyInfo">The property that's being evaluated.</param>
        /// <param name="value">The value of the property.</param>
        /// <returns>null if the value is valid, return an error string otherwise</returns>
        string Validate(IParameterManager parameterManager, PropertyInfo propertyInfo, object value);
    }
}
