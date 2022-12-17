namespace vein.runtime;

using System;
using System.Collections.Generic;

public class ConstStorage
{
    internal readonly Dictionary<FieldName, object> storage = new();


    public void Stage(FieldName name, object o)
    {
        var type = o.GetType();

        if (!type.IsPrimitive && type != typeof(string) && type != typeof(Half) /* why half is not primitive?... why...*/)
            throw new ConstCannotUseNonPrimitiveTypeException(name, type);

        //logger.Information("Staged [{@name}, {@o}] into constant table.", name, o);
        storage.Add(name, o);
    }

    public object Get(FieldName name) => storage[name];


}