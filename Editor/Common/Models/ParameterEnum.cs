using System;
using System.Collections.Generic;

namespace PocketGems.Parameters.Common.Models.Editor
{
    public class ParameterEnum : IParameterEnum
    {
        public ParameterEnum(Type type)
        {
            Type = type;
        }

        public Type Type { get; }

        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();

            if (!Type.IsPublic)
                errors.Add($"Enum [{Type.Name}] must be Public.");
            if (Type.Namespace != null)
                errors.Add($"Enum [{Type.Name}] must not have a namespace.");

            return errors.Count == 0;
        }
    }
}
