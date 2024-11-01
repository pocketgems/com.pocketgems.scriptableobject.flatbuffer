using System.Collections.Generic;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.Operation.Editor
{
    public interface ICommonOperationContext
    {
        // Populated by ParseInterfaceAssemblyOperation
        List<IParameterEnum> ParameterEnums { get; }
        List<IParameterInfo> ParameterInfos { get; }
        List<IParameterStruct> ParameterStructs { get; }

        /// <summary>
        /// Interface Root Path
        /// </summary>
        string InterfaceDirectoryRootPath { get; }

        /// <summary>
        /// Interface Assembly Name
        /// </summary>
        string InterfaceAssemblyName { get; }

        /// <summary>
        /// Wrapper class used for fetching & setting hashes.
        /// </summary>
        IInterfaceHash InterfaceHash { get; }

        /// <summary>
        /// The hash representing the current interface assembly
        /// </summary>
        string InterfaceAssemblyHash { get; set; }
    }
}
