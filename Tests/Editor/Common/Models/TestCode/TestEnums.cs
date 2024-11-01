using System;

public enum ValidEnum
{
    Value1,
    Value2,
    Value3
}

[Flags]
public enum ValidFlagEnum
{
    None = 0,
    Value1 = 1,
    Value2 = 2,
    Value4 = 4,
    Value8 = 8
}

internal enum InvalidInternalEnum
{
    Value1,
    Value2,
    Value3
}

namespace MyNameSpace
{
    public enum InvalidNamespaceEnum
    {
        Value1,
        Value2,
        Value3
    }
}
