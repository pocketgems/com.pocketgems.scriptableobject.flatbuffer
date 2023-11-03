using System.Collections.Generic;
using PocketGems.Parameters.Models;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Editor.Operation
{
    public class CommonOperationContext : ICommonOperationContext
    {
        public CommonOperationContext()
        {
            InterfaceHash = PocketGems.Parameters.Util.InterfaceHash.Create();
            ParameterEnums = new List<IParameterEnum>();
            ParameterInfos = new List<IParameterInfo>();
            ParameterStructs = new List<IParameterStruct>();
        }

        public List<IParameterEnum> ParameterEnums { get; }
        public List<IParameterInfo> ParameterInfos { get; }
        public List<IParameterStruct> ParameterStructs { get; }

        public string InterfaceDirectoryRootPath => EditorParameterConstants.Interface.DirRoot;
        public string InterfaceAssemblyName => EditorParameterConstants.Interface.AssemblyName;
        public IInterfaceHash InterfaceHash { get; }
        public string InterfaceAssemblyHash { get; set; }
    }
}
