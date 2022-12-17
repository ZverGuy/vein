namespace vein.runtime;

using System;

public class FieldIsNotDeclaredException : Exception
{
    public FieldIsNotDeclaredException(FieldName field) : base($"Field '{field.Name}' is not declared.")
    {

    }
}