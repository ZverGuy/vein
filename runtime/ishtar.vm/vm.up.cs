namespace ishtar.vm;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using vein.fs;
using vein.runtime;

[ExcludeFromCodeCoverage]
public static class Bootstrapper
{
    public static int Main(string[] args, bool traceEnable = false, bool consoleEnable = false)
    {
        if (traceEnable)
            ishtar.Trace.init();
        if (consoleEnable)
            SetUpConsole();

        #if MANAGED
        if (args.Contains("--managed-debug"))
            while (!System.Diagnostics.Debugger.IsAttached)
                Thread.Sleep(200);
        #endif

        IshtarCore.INIT();
        SetUpVTables();
        IshtarGC.INIT();
        InternalFFI.INIT();

        SetUpVault();

        var exit_code = LoadEntryModule(args, out var entry_point);

        if (0 != exit_code)
            return exit_code;

        if (entry_point is null)
        {
            VM.FastFail(WNE.MISSING_METHOD, $"Entry point is not defined.", IshtarFrames.EntryPoint);
            return -280;
        }


        return 0;
    }


    public static unsafe int LoadEntryModule(string[] args, out RuntimeIshtarMethod entry_point, bool consoleEnable = false)
    {
        var masterModule = default(IshtarAssembly);
        var resolver = default(AssemblyResolver);
        entry_point = default;

        if (AssemblyBundle.IsBundle(out var bundle))
        {
            resolver = AppVault.CurrentVault.GetResolver();
            masterModule = bundle.Assemblies.First();
            resolver.AddInMemory(bundle);
        }
        else
        {
            if (args.Length < 1)
                return -1;
            var entry = new FileInfo(args.First());
            if (!entry.Exists)
            {
                VM.println("no file");
                return -2;
            }
            AppVault.CurrentVault.WorkDirectory = entry.Directory;
            resolver = AppVault.CurrentVault.GetResolver();
            masterModule = IshtarAssembly.LoadFromFile(entry);
            resolver.AddSearchPath(entry.Directory);
        }


        var module = resolver.Resolve(masterModule);

        foreach (var @class in module.class_table.OfType<RuntimeIshtarClass>())
            @class.init_vtable();

        entry_point = module.GetEntryPoint();

        var frame = ExecuteEntryPoint(entry_point);
       
        if (frame.exception is not null)
        {
            if (consoleEnable)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"unhandled exception '{frame.exception.value->decodeClass().Name}' was thrown. \n" +
                                  $"{frame.exception.stack_trace}");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
                return -849;
        }
        
        return 0;
    }

    public static void SetUpConsole()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            Console.OutputEncoding = Encoding.Unicode;
    }


    public unsafe static CallFrame ExecuteEntryPoint(RuntimeIshtarMethod entry_point, bool displayElapsed = true)
    {
        var args_ = stackalloc stackval[1];

        var frame = new CallFrame
        {
            args = args_,
            method = entry_point,
            level = 0
        };

        {// i don't know why
            IshtarCore.INIT_ADDITIONAL_MAPPING();
            SetUpVTables();
        }
        
        var watcher = Stopwatch.StartNew();
        VM.exec_method(frame);
        watcher.Stop();

        if (displayElapsed)
            Console.WriteLine($"Elapsed: {watcher.Elapsed}");

        return frame;
    }

    public static void SetUpVault()
        => AppVault.CurrentVault = new AppVault("app");


    public static void SetUpVTables()
    {
        foreach (var @class in VeinCore.All.OfType<RuntimeIshtarClass>())
            @class.init_vtable();
    }
}
