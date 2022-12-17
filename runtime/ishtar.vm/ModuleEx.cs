namespace vein.runtime;

using System.Linq;
using global::ishtar;
using vein.runtime;

public static class ModuleEx
{
    public static RuntimeIshtarMethod GetEntryPoint(this VeinModule module)
    {
        foreach (var method in module.class_table.SelectMany(x => x.Methods))
        {
            if (!method.IsStatic)
                continue;
            if (method.Name == "master()")
                return (RuntimeIshtarMethod)method;
        }

        return null;
    }
}