namespace vein.runtime;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using exceptions;
using extensions;
using reflection;
using static VeinTypeCode;

public record FieldName(string fullName)
{
    public string Name => fullName.Split('.').Last();
    public string Class => fullName.Split('.').SkipLast(1).Join();


    public static implicit operator string(FieldName t) => t.fullName;
    public static implicit operator FieldName(string t) => new(t);


    public FieldName(string name, string className) : this($"{className}.{name}") { }

    public static FieldName Construct(VeinClass owner, string name)
        => new(name, owner.FullName.Name);

    public static FieldName Resolve(int index, VeinModule module)
    {
        var value = module.fields_table.GetValueOrDefault(index) ??
                    throw new Exception($"FieldName by index '{index}' not found in '{module.Name}' module.");
        return value;
    }

    public override string ToString() => $"{Class}.{Name}";
}

public class VeinProperty : VeinMember, IAspectable
{
    public VeinProperty(VeinClass owner, FieldName fullName, FieldFlags flags, VeinClass propType)
    {
        this.Owner = owner;
        this.FullName = fullName;
        this.Flags = flags;
        this.PropType = propType;
    }

    public FieldName FullName { get; protected internal set; }
    public VeinClass PropType { get; set; }
    public FieldFlags Flags { get; set; }
    public VeinClass Owner { get; set; }
    public List<Aspect> Aspects { get; } = new();

    public override string Name
    {
        get => FullName.Name;
        protected set => throw new NotImplementedException();
    }
    public override VeinMemberKind Kind => VeinMemberKind.Field;

    public bool IsLiteral => this.Flags.HasFlag(FieldFlags.Literal);
    public bool IsStatic => this.Flags.HasFlag(FieldFlags.Static);
    public bool IsPublic => this.Flags.HasFlag(FieldFlags.Public);
    public bool IsReadonly => this.Flags.HasFlag(FieldFlags.Readonly) && Setter is null;
    public bool IsPrivate => !IsPublic;
    [MaybeNull] public VeinMethod Getter { get; set; }
    [MaybeNull] public VeinMethod Setter { get; set; }
    [MaybeNull] public VeinField ShadowField { get; set; }


    public static FieldName GetShadowFieldName(FieldName propName)
        => new FieldName($"$_prop_shadow_{propName.Name}_", propName.Class);
    public static FieldName GetShadowFieldName(string propName)
        => $"$_prop_shadow_{propName}_";

    public static string GetterFnName(string propName)
        => $"get_{propName}";
    public static string SetterFnName(string propName)
        => $"set_{propName}";

    public static MethodFlags ConvertShadowFlags(FieldFlags flags)
    {
        var method = default(MethodFlags);

        if (flags.HasFlag(FieldFlags.Internal))
            method |= MethodFlags.Internal;
        if (flags.HasFlag(FieldFlags.Public))
            method |= MethodFlags.Public;
        if (flags.HasFlag(FieldFlags.Protected))
            method |= MethodFlags.Protected;
        if (flags.HasFlag(FieldFlags.Special))
            method |= MethodFlags.Special;
        if (flags.HasFlag(FieldFlags.Static))
            method |= MethodFlags.Static;
        if (flags.HasFlag(FieldFlags.Virtual))
            method |= MethodFlags.Virtual;
        if (flags.HasFlag(FieldFlags.Abstract))
            method |= MethodFlags.Abstract;
        return method;
    }

    public static VeinProperty RestoreFrom(string name, VeinClass clazz)
    {
        var n = GetShadowFieldName(name);

        var shadowField = clazz.FindField(n);

        if (shadowField is null)
            return null;
        var prop = new VeinProperty(clazz, new FieldName(name, clazz.Name), shadowField.Flags,
            shadowField.FieldType);
        prop.ShadowField = shadowField;

        var setterArgs = shadowField.IsStatic ?
            new VeinClass[0] : new VeinClass[1] { clazz };
        var getterArgs = shadowField.IsStatic ?
            new VeinClass[1] { shadowField.FieldType } : new VeinClass[2] { clazz, shadowField.FieldType };

        var getMethod = clazz.FindMethod(GetterFnName(name), setterArgs, true);

        if (getMethod is not null)
            prop.Getter = getMethod;

        var setMethod = clazz.FindMethod(SetterFnName(name), getterArgs, true);

        if (getMethod is not null)
            prop.Setter = setMethod;

        return prop;
    }
}

public class VeinField : VeinMember, IAspectable
{
    public VeinField(VeinClass owner, FieldName fullName, FieldFlags flags, VeinClass fieldType)
    {
        this.Owner = owner;
        this.FullName = fullName;
        this.Flags = flags;
        this.FieldType = fieldType;
    }
    public FieldName FullName { get; protected internal set; }
    public VeinClass FieldType { get; set; }
    public FieldFlags Flags { get; set; }
    public VeinClass Owner { get; set; }
    public List<Aspect> Aspects { get; } = new();

    public override string Name
    {
        get => FullName.Name;
        protected set => throw new NotImplementedException();
    }
    public override VeinMemberKind Kind => VeinMemberKind.Field;


    public override bool IsSpecial => this.Flags.HasFlag(FieldFlags.Special);
    public bool IsLiteral => this.Flags.HasFlag(FieldFlags.Literal);
    public bool IsStatic => this.Flags.HasFlag(FieldFlags.Static);
    public bool IsPublic => this.Flags.HasFlag(FieldFlags.Public);
    public bool IsPrivate => !IsPublic;
}

public static class VeinFieldExtension
{
    public static Func<string, object> GetConverter(this VeinTypeCode code)
    {
        Func<string, object> result = (code) switch
        {
            (TYPE_BOOLEAN)  => (x) => bool.Parse(x),
            (TYPE_CHAR)     => (x) => char.Parse(x),
            (TYPE_I1)       => (x) => sbyte.Parse(x),
            (TYPE_I2)       => (x) => short.Parse(x),
            (TYPE_I4)       => (x) => int.Parse(x),
            (TYPE_I8)       => (x) => long.Parse(x),
            (TYPE_U1)       => (x) => byte.Parse(x),
            (TYPE_U2)       => (x) => ushort.Parse(x),
            (TYPE_U4)       => (x) => uint.Parse(x),
            (TYPE_U8)       => (x) => ulong.Parse(x),
            (TYPE_R2)       => (x) => Half.Parse(x),
            (TYPE_R4)       => (x) => float.Parse(x),
            (TYPE_R8)       => (x) => double.Parse(x),
            (TYPE_R16)      => (x) => decimal.Parse(x),
            (TYPE_STRING)   => (x) => x,
            _ => throw new NotSupportedException($"Cannot fetch converter for {code}.")
        };

        return WrapConverter(result, code);
    }

    private static Func<string, object> WrapConverter(Func<string, object> actor, VeinTypeCode typeCode) => x =>
    {
        try
        {
            return actor(x);
        }
        catch (OverflowException e)
        {
            throw new ValueWasIncorrectException(x, typeCode, e);
        }
    };

    public static Func<string, object> GetConverter(this VeinField field)
    {
        try
        {
            return GetConverter(field.FieldType.TypeCode);
        }
        catch (NotSupportedException)
        {
            throw new ConvertNotSupportedException(field);
        }
    }
}