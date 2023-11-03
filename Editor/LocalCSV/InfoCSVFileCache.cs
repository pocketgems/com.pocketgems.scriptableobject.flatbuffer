using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Models;
using PocketGems.Parameters.Util;

namespace PocketGems.Parameters.Editor.LocalCSV
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
