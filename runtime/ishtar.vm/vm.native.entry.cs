namespace ishtar.vm;

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using vein.runtime;

[ExcludeFromCodeCoverage]
public unsafe static class vm_native_entry
{
    private static VMCtx* _VMCtx;

    [UnmanagedCallersOnly(EntryPoint = "VM_CREATE", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static void VM_CREATE(VMCtx* context)
        => _VMCtx = context;
    [UnmanagedCallersOnly(EntryPoint = "VM_DESTROY", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static void VM_DESTROY()
        => _VMCtx = null;

    [UnmanagedCallersOnly(EntryPoint = "VM_WARMUP", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static void VM_WARMUP(char** argc, int args)
    {
        var args_arr = new string[args];

        for (int i = 0; i < args_arr.Length; i++)
            args_arr[i] = new string(argc[i]);

        
        if (_VMCtx->IsConsoleEnabled)
            Bootstrapper.SetUpConsole();
        if (_VMCtx->IsTraceEnabled)
            ishtar.Trace.init();


        IshtarCore.INIT();
        Bootstrapper.SetUpVTables();
        IshtarGC.INIT();
        InternalFFI.INIT();
    }

    [UnmanagedCallersOnly(EntryPoint = "VM_CREATE_VAULT", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static void VM_CREATE_VAULT(char* vault_name, char* working_directory)
    {
        AppVault.CurrentVault = new AppVault(new string(vault_name));
        if (working_directory != null)
            AppVault.CurrentVault.WorkDirectory = new(new(working_directory));
    }

    [UnmanagedCallersOnly(EntryPoint = "VM_VAULT_NAME", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static char* VM_VAULT_NAME()
    {
        fixed (char* p = AppVault.CurrentVault.Name)
            return p;
    }

    [UnmanagedCallersOnly(EntryPoint = "VM_EXEC_FRAME", CallConvs = new Type[] { typeof(CallConvCdecl) })]
    public static stackval* VM_EXECUTE_FRAME(FrameRef* frame)
    {
        var type = AppVault.CurrentVault.GlobalFindType(*frame->runtime_token);

        if (type is null)
            return null;

        if (type.Methods.Count >= frame->index)
            return null;

        var method = type.Methods[frame->index] as RuntimeIshtarMethod;

        if (method is not { } or { IsExtern: true } or { IsStatic: false })
            return null;


        var callframe = new CallFrame()
        {
            args = frame->args,
            level = 0,
            method = method
        };

        VM.exec_method(callframe);

        return callframe.returnValue;
    }


    public struct FrameRef
    {
        public stackval* args;
        public RuntimeTokenRef* runtime_token;
        public int index;
    }

    public struct RuntimeTokenRef
    {
        public ushort ModuleID;
        public ushort ClassID;


        public static implicit operator RuntimeTokenRef(RuntimeToken tok)
            => new() { ClassID = tok.ClassID, ModuleID = tok.ModuleID };
        public static implicit operator RuntimeToken(RuntimeTokenRef tok)
            => new(tok.ModuleID, tok.ClassID);
    }


    public struct VMCtx
    {
        public delegate*<nuint, void*> allocator;
        public bool IsTraceEnabled;
        public bool IsConsoleEnabled;
    }
}
