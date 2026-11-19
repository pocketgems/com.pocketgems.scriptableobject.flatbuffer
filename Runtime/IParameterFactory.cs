using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters
{
    /// <summary>
    /// A parameter that an IParameterFactory creates, with the interface names to register it under and the columns to
    /// set.  The generated ParameterDataLoader file implements this.
    /// </summary>
    public interface ICreatedParameter
    {
        /// <summary>
        /// The new, empty parameter.  Its values are set with EditProperty.
        /// </summary>
        IMutableParameter Parameter { get; }

        /// <summary>
        /// Whether the parameter is an info rather than a struct.
        /// </summary>
        bool IsInfo { get; }

        /// <summary>
        /// The identifier of an info, or the key path of a struct.
        /// </summary>
        string IdentifierOrKeyPath { get; }

        /// <summary>
        /// Every interface name the parameter is registered under: base interfaces first, then its own.
        /// </summary>
        IReadOnlyList<string> InterfaceNames { get; }

        /// <summary>
        /// Every column an add must set: every property except Identifier.
        /// </summary>
        IReadOnlyList<string> Columns { get; }
    }

    /// <summary>
    /// Creates empty parameters for rows that ApplyOverrides adds.
    ///
    /// The generated ParameterDataLoader implements this and sets itself on the ParameterManager it loads.
    /// </summary>
    public interface IParameterFactory
    {
        /// <summary>
        /// Creates an empty info or struct.
        /// </summary>
        /// <param name="parameterManager">the parameter manager the parameter belongs to</param>
        /// <param name="interfaceName">the interface name, e.g. ICurrencyInfo</param>
        /// <param name="identifierOrKeyPath">an info's identifier, or a struct's key path</param>
        /// <returns>the new parameter, or null if the interface isn't a parameter interface</returns>
        ICreatedParameter Create(IParameterManager parameterManager, string interfaceName, string identifierOrKeyPath);
    }
}
