using System;
using System.Collections.Generic;
using PocketGems.Parameters.Common.PropertyTypes.Editor;

namespace PocketGems.Parameters.Common.Models.Editor
{
    /// <summary>
    /// Model representing a developer defined interface under the ParameterInterfaces assembly in the Unity project.
    /// </summary>
    public interface IParameterInterface
    {
        /// <summary>
        /// Type for the interface
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Interface's name
        /// </summary>
        string InterfaceName { get; }

        /// <summary>
        /// Base name of the interface without the I prefix.
        /// </summary>
        string BaseName { get; }

        /// <summary>
        /// Namespace to generate the output code under.
        /// </summary>
        string GeneratedNameSpace { get; }

        /// <summary>
        /// Generated class that implements this interface and returns data from the flat buffer.
        /// </summary>
        /// <param name="includeExtension">if true, includes the file extension</param>
        /// <returns>the FlatBuffer object class name</returns>
        string FlatBufferClassName(bool includeExtension);

        /// <summary>
        /// Generated struct that represents data stored in the flat buffer.
        /// </summary>
        /// <param name="includeExtension">if true, includes the file extension</param>
        /// <returns>the FlatBuffer struct name</returns>
        string FlatBufferStructName(bool includeExtension);

        /// <summary>
        /// Generated validator that implements the IDataValidator interface.
        /// </summary>
        /// <param name="includeExtension">if true, includes the file extension</param>
        /// <returns>the scriptable object class name</returns>
        string ValidatorClassName(bool includeExtension);

        // properties
        IReadOnlyList<Type> OrderedBaseInterfaceTypes { get; }

        /// <summary>
        ///  Property type models that represent each property getter.
        /// </summary>
        IReadOnlyList<IPropertyType> PropertyTypes { get; }

        /// <summary>
        /// Validate this interface & it's properties.
        /// </summary>
        /// <param name="outErrors">non empty list of errors if any (else empty list)</param>
        /// <returns>true if the interface doesn't have errors, false otherwise</returns>
        public bool Validate(out IReadOnlyList<string> outErrors);
    }
}
