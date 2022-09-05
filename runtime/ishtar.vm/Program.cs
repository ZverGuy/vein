namespace vein.runtime;

using ishtar.vm;

[ExcludeFromCodeCoverage]
internal class Program
{
#if EXPERIMENTAL_JIT
        public unsafe static void TestJitFunctional()
        {
            var r = IshtarJIT.CPUFeatureList();

            var unused = default(NativeApi.Protection);
            var ptr = NativeLibrary.Load("sample_native_library.dll");
            var fnPtr1 = NativeLibrary.GetExport(ptr, "_sample_1");
            var fnPtr5 = NativeLibrary.GetExport(ptr, "_sample_3");
            var result1 = 0;
            var result2 = 15;
            int* mem1 = &result1;
            void* mem2 = &result2;

            var bw = mem1[0];


            ((delegate*<void>)IshtarJIT.WrapNativeCall(fnPtr5.ToPointer(), mem1, mem2))();

            //((delegate*<int, void>)IshtarJIT.WrapNativeCall(fnPtr5.ToPointer(), &mem))(55);
            var b = *((int*)mem1);
            var c = *((int*)mem2);
        }
#endif

    public static unsafe int Main(string[] args)
        => Bootstrapper.Main(args, true, true);
}
