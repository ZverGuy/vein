namespace ishtar
{
    using System;

    [Flags]
    public enum GCFlags
    {
        NONE = 0,
        IMMORTAL = 1 << 1,
        PAIR = 1 << 2
    }
}
