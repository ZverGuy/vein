#!/bin/dotnet run
#r "nuget: Newtonsoft.Json, 12.0.3"
#r "nuget: YamlDotNet, 9.1.4"
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
const string header = "/// <auto-generated> don't touch this file, for modification use gen.csx </auto-generated>";

record OpCode(
    string name, 
    byte? override_size, 
    string description, string note = null, 
    string flow = null, string chain = null);

var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)  // see height_in_inches in sample yml 
                .Build();
var content = File.ReadAllText("opcodes.yaml");
var obj = deserializer.Deserialize<object>(content);
var ops = new List<OpCode>();



#region parsing
T getKeyAndRemove<T>(object obj, string name)
{
    var dict = (IDictionary) obj;
    if (!dict.Contains(name))
        return default;
    var result = dict[name];
    dict.Remove(name);
    return (T)result;
}
IEnumerable<OpCode> fillData(string name, IDictionary<object, object> kv)
{
    var desc = getKeyAndRemove<string>(kv, "description");
    var note = getKeyAndRemove<string>(kv, "note");
    var flow = getKeyAndRemove<string>(kv, "flow");
    var chain = getKeyAndRemove<string>(kv, "chain");
    var size_str = getKeyAndRemove<string>(kv, "override-size");
    var range_str = getKeyAndRemove<string>(kv, "range");
    var size = size_str is not null ? byte.Parse(size_str) : (byte)0;
    if (range_str is null)
        yield return new OpCode(name, size, desc, note);
    else if(bool.Parse(range_str))
    {
        foreach (var i in Enumerable.Range(0, 6))
            yield return new OpCode($"{name}.{i}", 0, desc);
        yield return new OpCode($"{name}.S", size == 0 ? 1 : size, desc);
    }
}
foreach (var v in (IList)obj)
{
    var (key, raw_value) = ((IDictionary<object, object>)v).Single();
    var value = (IDictionary<object, object>) raw_value;
    var name = (string)key;
    var variations = getKeyAndRemove<List<object>>(value, "variations")?.Select(x => x.ToString())?.ToArray();

    
    var result = fillData(name, value).ToArray();
    var hasSaved = getKeyAndRemove<string>(value, "use-root") == "true";
    
    ops.AddRange(result);
    
    
    if (variations is { } && variations.Count() != 0)
    {
        var res = result.First();
        ops.AddRange(variations.Select(s => 
            new OpCode($"{name}.{s}", res.override_size, res.description)));
    }

    if (value.Any())
    {
        foreach (var o in value)
        {
            var (key1, value1) = o;
            result = fillData($"{name}.{(string) key1}", (IDictionary<object, object>) value1).ToArray();
            ops.AddRange(result);
        }
    }
    if (!hasSaved)
    if ((variations is { } && variations.Count() != 0) || value.Any())
        ops.Remove(ops.First(x => x.name.Equals(name)));
}
#endregion
var cpp_def = new StringBuilder();
var cs_def = new StringBuilder();
var cs_def_props = new StringBuilder();

void gen_cpp_def(StringBuilder builder)
{
    foreach(var i in ops.Select((x, y) => (x.name, y, x.override_size)))
        builder.AppendLine($"OP_DEF({i.name.Replace(".", "_")}, 0x{i.y:X2}, {i.override_size})");
}

void gen_cs_def(StringBuilder builder)
{
    builder.AppendLine(header);
    builder.AppendLine("namespace ishtar \n{");
    builder.AppendLine("\tpublic enum OpCodeValue : ushort \n\t{");

    foreach(var i in ops.Select((x, y) => (x.name, y)))
    {
        var desc = ops[i.y].description;
        var note = ops[i.y].note;
        builder.AppendLine($"\t\t/// <summary>");
        builder.AppendLine($"\t\t/// {desc}");
        builder.AppendLine($"\t\t/// </summary>");
        if (note is not null)
        {
            builder.AppendLine($"\t\t/// <remarks>");
            builder.AppendLine($"\t\t/// {note}");
            builder.AppendLine($"\t\t/// </remarks>");
        }
        builder.AppendLine($"\t\t{i.name.Replace(".", "_")} = 0x{i.y:X2},");
    }
        
        
    builder.AppendLine("\t}\n}");
}

int get(string r)
{
    if (r is null)
        return 0;
    return int.Parse(r);
}

void gen_cs_props(StringBuilder builder)
{
    int CreateFlag(byte size, int flow, int chain) 
            => ((chain << 0xC) | 0x1F) | ((flow << 0x11) | 0x1F) | ((size << 0x16) | 0x1F);
    builder.AppendLine(header);
    builder.AppendLine("namespace ishtar \n{");
    builder.AppendLine("\tusing global::wave.runtime;");
    builder.AppendLine("\tusing global::ishtar.emit;");
    builder.AppendLine("\tusing global::System.Collections.Generic;");
    builder.AppendLine("\tpublic static class OpCodes \n\t{");
    foreach(var i in ops.Select((x, y) => (x.name, y, x.override_size, get(x.flow), get(x.chain))))
    {
        var desc = ops[i.y].description;
        var note = ops[i.y].note;
        builder.AppendLine($"\t\t/// <summary>");
        builder.AppendLine($"\t\t/// {desc}");
        builder.AppendLine($"\t\t/// size: {i.override_size}");
        builder.AppendLine($"\t\t/// flow: {i.Item4}");
        builder.AppendLine($"\t\t/// chain: {i.Item5}");
        builder.AppendLine($"\t\t/// </summary>");
        if (note is not null)
        {
            builder.AppendLine($"\t\t/// <remarks>");
            builder.AppendLine($"\t\t/// {note}");
            builder.AppendLine($"\t\t/// </remarks>");
        }
        builder.AppendLine($"\t\tpublic static OpCode {i.name.Replace(".", "_")} "+
        $"= new (0x{i.y:X2}, 0x{CreateFlag(i.override_size ?? 0, i.Item4, i.Item5):X8});");
    }
    builder.AppendLine("\n\t\tpublic static Dictionary<OpCodeValue, OpCode> all = new ()");
    builder.AppendLine("\t\t{");
    foreach(var i in ops.Select((x, y) => x.name))
        builder.AppendLine($"\t\t\t{{OpCodeValue.{i.Replace(".", "_")}, {i.Replace(".", "_")}}},");
    builder.AppendLine("\t\t};");
    builder.AppendLine("\t}\n}");
}
/*
---
description: Nope operation.
---

# .NOP

| Name | Size | ControlChain | FlowControl |
| :--- | :--- | :--- | :--- |
| NOP | 0 | None | None |



    */

if (Args.Contains("--generate-docs"))
{
    Directory.CreateDirectory("./docs");
    var summary = new StringBuilder();
    foreach (var o in ops)
    {
        var name = $".{o.name.ToLowerInvariant()}";
        
        summary.AppendLine($"## .{o.name.ToUpperInvariant()}");
        summary.AppendLine("");
        summary.AppendLine($"{o.description}");
        summary.AppendLine("");
        summary.AppendLine("| Name | Size | ControlChain | FlowControl |");
        summary.AppendLine("| :--- | :--- | :--- | :--- |");
        summary.AppendLine($"| {o.name.ToUpperInvariant()} | {o.override_size} | {get(o.chain)} | {get(o.flow)} |");
        summary.AppendLine("");
    }
    if (File.Exists($"./docs/!SUMMARY.md"))
        File.Delete($"./docs/!SUMMARY.md");
    File.WriteAllText($"./docs/!SUMMARY.md", summary.ToString());
}

gen_cpp_def(cpp_def);
gen_cs_def(cs_def);
gen_cs_props(cs_def_props);

if (File.Exists("opcodes.def"))
    File.Delete("opcodes.def");
if (File.Exists("opcodes.def.cs"))
    File.Delete("opcodes.def.cs");
if (File.Exists("opcodes.list.def.cs"))
    File.Delete("opcodes.list.def.cs");



File.WriteAllText("opcodes.def", cpp_def.ToString());
File.WriteAllText("opcodes.def.cs", cs_def.ToString());
File.WriteAllText("opcodes.list.def.cs", cs_def_props.ToString());