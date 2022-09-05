namespace ishtar
{
    using System.Collections.Generic;
    using vein.runtime;
    using static System.Console;
    using static vein.runtime.MethodFlags;
    using static vein.runtime.VeinTypeCode;

    public static unsafe class B_Out
    {
        [IshtarExport(1, "@_println")]
        [IshtarExportFlags(Public | Static)]
        public static IshtarObject* FPrintLn(CallFrame current, IshtarObject** args)
        {
            var arg1 = args[0];

            if (arg1 == null)
            {
                current.ThrowException(KnowTypes.NullPointerException(current));
                return null;
            }

            InternalFFI.StaticValidate(current, &arg1);
            InternalFFI.StaticTypeOf(current, &arg1, TYPE_STRING);
            var @class = arg1->decodeClass();

            var str = IshtarMarshal.ToDotnetString(arg1, current);

            Out.WriteLine();
            Out.WriteLine($"\t{str}");
            Out.WriteLine();

            return null;
        }

        [IshtarExport(0, "@_readline")]
        [IshtarExportFlags(Public | Static)]
        public static IshtarObject* FReadLine(CallFrame current, IshtarObject** args)
            => IshtarMarshal.ToIshtarObject(In.ReadLine());


        public static void InitTable(Dictionary<string, RuntimeIshtarMethod> table)
        {
            new RuntimeIshtarMethod("@_println", Public | Static | Extern, ("val", TYPE_STRING))
                .AsNative((delegate*<CallFrame, IshtarObject**, IshtarObject*>)&FPrintLn)
                .AddInto(table, x => x.Name);

            new RuntimeIshtarMethod("@_readline", Public | Static | Extern, TYPE_STRING.AsClass())
                .AsNative((delegate*<CallFrame, IshtarObject**, IshtarObject*>)&FReadLine)
                .AddInto(table, x => x.Name);
        }
    }
}
