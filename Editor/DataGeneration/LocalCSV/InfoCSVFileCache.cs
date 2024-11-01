using PocketGems.Parameters.Common.Models.Editor;
using PocketGems.Parameters.Common.Util.Editor;
using PocketGems.Parameters.Interface;

namespace PocketGems.Parameters.DataGeneration.LocalCSV.Editor
{
    internal class InfoCSVFileCache : CSVFileCache<IBaseInfo, IParameterInfo>, IInfoCSVFileCache
    {
        public InfoCSVFileCache(string csvDir, bool attemptLoadExistingOnLoad) : base(csvDir, attemptLoadExistingOnLoad)
        {
        }

        protected override string BaseName<T>() => NamingUtil.BaseNameFromInfoInterfaceName(typeof(T).Name);
        protected override bool RequiresIdentifier => true;
    }
}
