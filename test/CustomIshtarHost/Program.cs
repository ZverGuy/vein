using System.Diagnostics;
using System.Runtime.InteropServices;

var ishtarModule = NativeLibrary.Load("./ishtar.dll");
var name = "test_app";

unsafe
{
    var vm_load = (delegate*<VMCtx*, void>)NativeLibrary.GetExport(ishtarModule, "VM_CREATE") ;
    var vm_warmup =  (delegate*<char**, int, void>)NativeLibrary.GetExport(ishtarModule, "VM_WARMUP");
    var vm_create_vault =  (delegate*<char*, char*, void>)NativeLibrary.GetExport(ishtarModule, "VM_CREATE_VAULT");
    var vm_get_vault_Name =  (delegate*<char*>)NativeLibrary.GetExport(ishtarModule, "VM_VAULT_NAME");

    var ctx = new VMCtx();

    ctx.allocator = &NativeMemory.Alloc;

    vm_load(&ctx);
    vm_warmup(null, 0);
    fixed (char* vname = name)
        vm_create_vault(vname, null);
    var ename = new string(vm_get_vault_Name());
    Debug.Assert(name == ename);
}

public unsafe struct VMCtx
{
    public delegate*<nuint, void*> allocator;
    public bool IsTraceEnabled;
    public bool IsConsoleEnabled;
}
