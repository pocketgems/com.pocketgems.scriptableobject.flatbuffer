using System;
using System.Collections.Generic;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public interface IParameterEnum
    {
        Type Type { get; }

        bool Validate(out List<string> errors);
    }
}
