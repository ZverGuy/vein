namespace ishtar.emit.extensions;

using global::ishtar;
using vein.extensions;

public static class OpCodeExtensions
{
    public static bool InRange(this OpCode opcode, OpCodeValue start, OpCodeValue end)
        => ((ushort)start..(ushort)end).InRange(opcode.Value);
}