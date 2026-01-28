using System.Collections.Generic;
using PocketGems.Parameters.Interface;
using PocketGems.Parameters.Interface.Attributes;

namespace PocketGems
{
    public interface ITestInfo : IBaseInfo
    {
        [ParameterAttachFieldAttribute("test")]
        string DisplayName { get; }
        int Cost { get; }
        IReadOnlyList<int> Costs { get; }
    }
}
