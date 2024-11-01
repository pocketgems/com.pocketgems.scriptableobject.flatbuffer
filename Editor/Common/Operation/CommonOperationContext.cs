using System.Collections.Generic;
using PocketGems.Parameters.Common.Editor;
using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;

namespace PocketGems.Parameters.Common.Operation.Editor
{
    public class CommonOperationContext : ICommonOperationContext
    {
        public CommonOperationContext()
        {
            InterfaceHash = Parameters.Common.Util.Editor.InterfaceHash.Create();
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
