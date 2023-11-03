using System.Collections.Generic;
using PocketGems.Parameters.Interface;

namespace PocketGems
{
    public interface ITestInfo : IBaseInfo
    {
        [AttachFieldAttribute("test")]
        string DisplayName { get; }
        int Cost { get; }
        IReadOnlyList<int> Costs { get; }
    }
}
