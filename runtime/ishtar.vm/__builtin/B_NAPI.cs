namespace ishtar;

using ishtar.runtime.vin;
using static vein.runtime.MethodFlags;
using static vein.runtime.VeinTypeCode;

public static unsafe class B_NAPI
{
    [IshtarExport(1, "i_call_NAPI_LoadNative")]
    [IshtarExportFlags(Public | Static)]
    public static IshtarObject* LoadNativeLibrary(CallFrame current, IshtarObject** args)
    {
        var arg1 = args[0];

        if (arg1 == null)
        {
            current.ThrowException(KnowTypes.NullPointerException(current));
            return IshtarObject.NullPointer;
        }

        InternalFFI.StaticValidate(current, &arg1);
        InternalFFI.StaticTypeOf(current, &arg1, TYPE_STRING);
        var @class = arg1->decodeClass();

        var libPath = IshtarMarshal.ToDotnetString(arg1, current);
        var libFile = new FileInfo(libPath);

        if (!libFile.Exists)
        {
            current.ThrowException(KnowTypes.VeinLang.FileNotFoundFault(current));
            return IshtarObject.NullPointer;
        }

        var result = NativeStorage.TryLoad(libFile, out var h);

        if (!result)
        {
            current.ThrowException(KnowTypes.NativeFault(current));
            return IshtarObject.NullPointer;
        }


        var handleClass = KnowTypes.VeinLang.Native.NativeHandle(current);
        var handleObj = IshtarGC.AllocObject(handleClass);

        var wrapper = new KnowTypes.WrappedTypes.S_NativeHandle(handleObj, current);

        wrapper.Handle = h;

        return handleObj;
    }


    public static void InitTable(Dictionary<string, RuntimeIshtarMethod> table)
    {
        new RuntimeIshtarMethod("i_call_NAPI_LoadNative", Public | Static | Extern)
            .AsNative((delegate*<CallFrame, IshtarObject**, IshtarObject*>)&LoadNativeLibrary)
            .AddInto(table, x => x.Name);
    }
}
