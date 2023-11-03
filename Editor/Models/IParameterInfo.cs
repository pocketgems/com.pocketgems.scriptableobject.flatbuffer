using System;

namespace PocketGems.Parameters.Models
{
    public interface IParameterInfo : IParameterInterface
    {
        /// <summary>
        /// Finds and returns the actual Scriptable Object type if the class has been generated already.
        /// </summary>
        /// <returns>Scriptable object type if it exists, null otherwise.</returns>
        Type ScriptableObjectType();

        /// <summary>
        /// Generated Scriptable Object that implements this interface and returns data from a Scriptable Object.
        /// </summary>
        /// <param name="includeExtension">if true, includes the file extension</param>
        /// <returns>the scriptable object class name</returns>
        string ScriptableObjectClassName(bool includeExtension);
    }
}
