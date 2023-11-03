using System;

namespace PocketGems
{
    [Flags]
    public enum TestFlagEnum
    {
        Value1 = 0,
        Value2 = 1 << 0,
        Value3 = 1 << 1
    }
}
