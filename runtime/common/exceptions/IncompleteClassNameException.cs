namespace vein.exceptions;

using System;

public class IncompleteClassNameException : Exception
{
    public IncompleteClassNameException(string msg) : base(msg) { }
}