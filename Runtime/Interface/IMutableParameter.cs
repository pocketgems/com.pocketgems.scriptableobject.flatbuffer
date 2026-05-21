namespace PocketGems.Parameters.Interface
{
    public interface IMutableParameter
    {
        /// <summary>
        /// Edits a particular property.
        /// </summary>
        /// <param name="parameterManager">the parameter manager to use</param>
        /// <param name="propertyName">the property name</param>
        /// <param name="value">the new value</param>
        /// <param name="error">any errors if the return value is false</param>
        /// <returns>true if successful</returns>
        bool EditProperty(IParameterManager parameterManager, string propertyName, string value, out string error);

        /// <summary>
        /// Reverts a single property edit that was applied with the EditProperty call.
        /// </summary>
        /// <param name="propertyName">the property name to revert</param>
        /// <param name="error">any errors if the return value is false</param>
        /// <returns>true if successful</returns>
        bool RevertEditedProperty(string propertyName, out string error);

        /// <summary>
        /// Remove all edits that were applied with the EditProperty call.
        /// </summary>
        void RemoveAllEdits();

        /// <summary>
        /// Make a copy that points to the data of the current mutable parameter.
        ///
        /// This EXCLUDES any changes that were made from EditProperty().  Any calls EditProperty() to the original
        /// or newly returned parameter will not affect each other.
        /// </summary>
        /// <param name="parameterManager">the parameter manager for it's internal parameter lookup</param>
        /// <returns>a new mutable parameter</returns>
        IMutableParameter CreateLinkedMutableParameter(IParameterManager parameterManager);
    }
}
