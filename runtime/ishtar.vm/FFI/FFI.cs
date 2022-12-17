namespace ishtar;

using ishtar.runtime.vin;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using vein.runtime;

public static unsafe class InternalFFI
{
    public static Dictionary<string, RuntimeIshtarMethod> method_table { get; } = new();

    public static void INIT()
    {
        B_Out.InitTable(method_table);
        B_App.InitTable(method_table);
        B_IEEEConsts.InitTable(method_table);
        B_Sys.InitTable(method_table);
        B_String.InitTable(method_table);
        B_StringBuilder.InitTable(method_table);
        B_GC.InitTable(method_table);
        X_Utils.InitTable(method_table);
        B_Type.InitTable(method_table);
        B_Field.InitTable(method_table);
        B_Function.InitTable(method_table);
        B_NAPI.InitTable(method_table);
    }


    [Conditional("STATIC_VALIDATE_IL")]
    public static void StaticValidate(void* p, CallFrame frame)
    {
        if (p != null) return;
        VM.FastFail(WNE.STATE_CORRUPT, "Null pointer state.", frame);
    }
    [Conditional("STATIC_VALIDATE_IL")]
    public static void StaticValidateField(CallFrame current, IshtarObject** arg1, string name)
    {
        StaticValidate(*arg1, current);
        var @class = (*arg1)->decodeClass();
        VM.Assert(@class.FindField(name) != null, WNE.TYPE_LOAD,
            $"Field '{name}' not found in '{@class.Name}'.", current);
    }

    [Conditional("STATIC_VALIDATE_IL")]
    public static void StaticValidate(CallFrame frame, stackval* value, VeinClass clazz)
    {
        frame.assert(clazz is RuntimeIshtarClass);
        frame.assert(value->type != VeinTypeCode.TYPE_NONE);
        var obj = IshtarMarshal.Boxing(frame, value);
        frame.assert(obj->__gc_id != -1);
        var currentClass = obj->decodeClass();
        var targetClass = clazz as RuntimeIshtarClass;
        frame.assert(currentClass.ID == targetClass.ID, $"{currentClass.Name}.ID == {targetClass.Name}.ID");
    }

    [Conditional("STATIC_VALIDATE_IL")]
    public static void StaticValidate(CallFrame current, IshtarObject** arg1)
    {
        StaticValidate(*arg1, current);
        var @class = (*arg1)->decodeClass();
        VM.Assert(@class.is_inited, WNE.TYPE_LOAD, $"Class '{@class.FullName}' corrupted.", current);
        VM.Assert(!@class.IsAbstract, WNE.TYPE_LOAD, $"Class '{@class.FullName}' abstract.", current);
    }
    [Conditional("STATIC_VALIDATE_IL")]
    public static void StaticTypeOf(CallFrame current, IshtarObject** arg1, VeinTypeCode code)
    {
        StaticValidate(*arg1, current);
        var @class = (*arg1)->decodeClass();
        VM.Assert(@class.TypeCode == code, WNE.TYPE_MISMATCH, $"@class.{@class.TypeCode} == {code}", current);
    }

    public static RuntimeIshtarMethod GetMethod(string FullName)
        => method_table.GetValueOrDefault(FullName);


    public static void LinkExternalNativeLibrary(string inportTarget, string fnName,
        RuntimeIshtarMethod importCaller)
    {
        importCaller.PIInfo = PInvokeInfo.New(null) with { iflags = (ushort)PInvokeInfo.Flags.EXTERNAL };

        var entity = new NativeImportEntity(inportTarget, fnName, importCaller);

        if (importCaller.Owner is not RuntimeIshtarClass clazz)
            throw null;
        clazz.nativeImports.Add(entity);
    }



        
    // ==================
    // TODO, move to outside
    // ==================

    private static readonly Dictionary<string, NativeImportCache> _cache = new ();

    public static void LoadNativeLibrary(NativeImportEntity entity, CallFrame frame)
    {
        if (_cache.ContainsKey(entity.entry))
            return;

        var result = NativeStorage.TryLoad(new FileInfo(entity.entry), out var handle);

        if (!result)
        {
            VM.FastFail(WNE.NATIVE_LIBRARY_COULD_NOT_LOAD, $"{entity.entry}", frame);
            return;
        }

        _cache[entity.entry] = new NativeImportCache(entity.entry, handle);
    }

    public static void LoadNativeSymbol(NativeImportEntity entity, CallFrame frame)
    {
        var cached = _cache[entity.entry];

        if (cached.ImportedSymbols.ContainsKey(entity.fn))
            return;

        try
        {
            var symbol = NativeStorage.GetSymbol(cached.handle, entity.fn);

            cached.ImportedSymbols.Add(entity.fn, symbol);
            entity.Handle = symbol;
        }
        catch
        {
            VM.FastFail(WNE.NATIVE_LIBRARY_SYMBOL_COULD_NOT_FOUND, $"{entity.entry}::{entity.fn}", frame);
        }
    }
}

public record NativeImportCache(string entry, nint handle)
{
    public Dictionary<string, nint> ImportedSymbols = new Dictionary<string, nint>();
}

public record NativeImportEntity(string entry, string fn, RuntimeIshtarMethod importer)
{
    public VeinModule Module => importer.Owner.Owner;

    public nint Handle;

    public bool IsBinded() => Handle != IntPtr.Zero;
}