namespace ishtar
{
    using System;
    using static vein.runtime.VeinTypeCode;
    public static unsafe class IshtarMarshal
    {
        public static IshtarObject* ToIshtarObject(string str, CallFrame frame = null, IshtarObject** node = null)
        {
            var arg = IshtarGC.AllocObject(TYPE_STRING.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(arg->clazz);
            arg->vtable[clazz.Field["!!value"].vtable_offset] = StringStorage.Intern(str);
            return arg;
        }
        public static IshtarObject* ToIshtarObject(int dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_I4.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (int*)dotnet_value;

            return obj;
        }

        public static IshtarObject* ToIshtarObject(bool dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_BOOLEAN.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (int*)(dotnet_value ? 1 : 0);

            return obj;
        }
        public static IshtarObject* ToIshtarObject(short dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_I2.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (short*)dotnet_value;

            return obj;
        }
        public static IshtarObject* ToIshtarObject(byte dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_I1.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (int*)dotnet_value;

            return obj;
        }
        public static IshtarObject* ToIshtarObject(long dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_I8.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (long*)dotnet_value;

            return obj;
        }

        public static IshtarObject* ToIshtarObject(float dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_I8.AsRuntimeClass(), node);
            var clazz = IshtarUnsafe.AsRef<RuntimeIshtarClass>(obj->clazz);
            obj->vtable[clazz.Field["!!value"].vtable_offset] = (int*)BitConverter.SingleToInt32Bits(dotnet_value);

            return obj;
        }

        public static IshtarObject* ToIshtarObject(nint dotnet_value, CallFrame frame = null, IshtarObject** node = null)
        {
            var obj = IshtarGC.AllocObject(TYPE_RAW.AsRuntimeClass(), node);
            obj->vtable = (void**)dotnet_value;
            return obj;
        }

        public static IshtarObject* ToIshtarObject<X>(X value, CallFrame frame, IshtarObject** node = null)
        {
            switch (typeof(X))
            {
                case { } when typeof(X) == typeof(nint):
                    return ToIshtarObject(cast<nint>(value), frame, node);
                case { } when typeof(X) == typeof(sbyte):
                    return ToIshtarObject(cast<sbyte>(value), frame, node);
                case { } when typeof(X) == typeof(byte):
                    return ToIshtarObject(cast<byte>(value), frame, node);
                case { } when typeof(X) == typeof(short):
                    return ToIshtarObject(cast<short>(value), frame, node);
                case { } when typeof(X) == typeof(ushort):
                    return ToIshtarObject(cast<ushort>(value), frame, node);
                case { } when typeof(X) == typeof(int):
                    return ToIshtarObject(cast<int>(value), frame, node);
                case { } when typeof(X) == typeof(uint):
                    return ToIshtarObject(cast<uint>(value), frame, node);
                case { } when typeof(X) == typeof(long):
                    return ToIshtarObject(cast<long>(value), frame, node);
                case { } when typeof(X) == typeof(ulong):
                    return ToIshtarObject(cast<ulong>(value), frame, node);
                case { } when typeof(X) == typeof(char):
                    return ToIshtarObject(cast<char>(value), frame, node);
                case { } when typeof(X) == typeof(string):
                    return ToIshtarObject(cast<string>(value), frame, node);
                default:
                    VM.FastFail(WNE.TYPE_MISMATCH,
                        $"[marshal::ToIshtarObject] converter for '{typeof(X).Name}' not support.", frame);
                    return default;
            }
        }

        private static X cast<X>(object o) => (X)o;

        public static X ToDotnet<X>(IshtarObject* obj, CallFrame frame)
        {
            switch (typeof(X))
            {
                case { } when typeof(X) == typeof(nint):
                    return (X)(object)ToDotnetPointer(obj, frame);
                case { } when typeof(X) == typeof(sbyte):
                    return (X)(object)ToDotnetInt8(obj, frame);
                case { } when typeof(X) == typeof(byte):
                    return (X)(object)ToDotnetUInt8(obj, frame);
                case { } when typeof(X) == typeof(short):
                    return (X)(object)ToDotnetInt16(obj, frame);
                case { } when typeof(X) == typeof(ushort):
                    return (X)(object)ToDotnetUInt16(obj, frame);
                case { } when typeof(X) == typeof(int):
                    return (X)(object)ToDotnetInt32(obj, frame);
                case { } when typeof(X) == typeof(uint):
                    return (X)(object)ToDotnetUInt32(obj, frame);
                case { } when typeof(X) == typeof(long):
                    return (X)(object)ToDotnetInt64(obj, frame);
                case { } when typeof(X) == typeof(ulong):
                    return (X)(object)ToDotnetUInt64(obj, frame);
                case { } when typeof(X) == typeof(bool):
                    return (X)(object)ToDotnetBoolean(obj, frame);
                case { } when typeof(X) == typeof(char):
                    return (X)(object)ToDotnetChar(obj, frame);
                case { } when typeof(X) == typeof(string):
                    return (X)(object)ToDotnetString(obj, frame);
                default:
                    VM.FastFail(WNE.TYPE_MISMATCH,
                        $"[marshal::ToDotnet] converter for '{typeof(X).Name}' not support.", frame);
                    return default;
            }
        }

        public static sbyte ToDotnetInt8(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_I1);
            var clazz = obj->decodeClass();
            return (sbyte)(sbyte*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static short ToDotnetInt16(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_I2);
            var clazz = obj->decodeClass();
            return (short)(short*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static int ToDotnetInt32(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_I4);
            var clazz = obj->decodeClass();
            return (int)(int*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static long ToDotnetInt64(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_I8);
            var clazz = obj->decodeClass();
            return (long)(long*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }

        public static byte ToDotnetUInt8(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_U1);
            var clazz = obj->decodeClass();
            return (byte)(byte*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static ushort ToDotnetUInt16(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_U2);
            var clazz = obj->decodeClass();
            return (ushort)(ushort*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static uint ToDotnetUInt32(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_U4);
            var clazz = obj->decodeClass();
            return (uint)(uint*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static ulong ToDotnetUInt64(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_U8);
            var clazz = obj->decodeClass();
            return (ulong)(ulong*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }
        public static bool ToDotnetBoolean(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_BOOLEAN);
            var clazz = obj->decodeClass();
            return (int)(int*)obj->vtable[clazz.Field["!!value"].vtable_offset] == 1;
        }
        public static char ToDotnetChar(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_CHAR);
            var clazz = obj->decodeClass();
            return (char)(int)(int*)obj->vtable[clazz.Field["!!value"].vtable_offset];
        }

        public static float ToDotnetFloat(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_R4);
            var clazz = obj->decodeClass();
            return BitConverter.Int32BitsToSingle((int)(int*)obj->vtable[clazz.Field["!!value"].vtable_offset]);
        }

        public static string ToDotnetString(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_STRING);
            var clazz = obj->decodeClass();
            var p = (StrRef*)obj->vtable[clazz.Field["!!value"].vtable_offset];
            return StringStorage.GetString(p, frame);
        }

        public static nint ToDotnetPointer(IshtarObject* obj, CallFrame frame)
        {
            InternalFFI.StaticTypeOf(frame, &obj, TYPE_RAW);
            return (nint)obj->vtable;
        }

        public static IshtarObject* ToIshtarString(IshtarObject* obj, CallFrame frame) => obj->decodeClass().TypeCode switch
        {
            TYPE_U1 => ToIshtarObject($"{ToDotnetUInt8(obj, frame)}"),
            TYPE_I1 => ToIshtarObject($"{ToDotnetInt8(obj, frame)}"),
            TYPE_U2 => ToIshtarObject($"{ToDotnetUInt16(obj, frame)}"),
            TYPE_I2 => ToIshtarObject($"{ToDotnetInt16(obj, frame)}"),
            TYPE_U4 => ToIshtarObject($"{ToDotnetUInt32(obj, frame)}"),
            TYPE_I4 => ToIshtarObject($"{ToDotnetInt32(obj, frame)}"),
            TYPE_U8 => ToIshtarObject($"{ToDotnetUInt64(obj, frame)}"),
            TYPE_I8 => ToIshtarObject($"{ToDotnetInt64(obj, frame)}"),
            TYPE_R4 => ToIshtarObject($"{ToDotnetFloat(obj, frame)}"),
            TYPE_BOOLEAN => ToIshtarObject($"{ToDotnetBoolean(obj, frame)}"),
            TYPE_CHAR => ToIshtarObject($"{ToDotnetChar(obj, frame)}"),
            TYPE_RAW => ToIshtarObject($"0x{ToDotnetPointer(obj, frame):X8}"),
            TYPE_STRING => obj,
            TYPE_FUNCTION => ToIshtarObject(new IshtarLayerFunction(obj, frame).Name),
            _ => ReturnDefault(nameof(ToIshtarString), $"Convert to '{obj->decodeClass().TypeCode}' not supported.", frame),
        };

        private static IshtarObject* ReturnDefault(string name, string msg, CallFrame frame)
        {
            VM.FastFail(WNE.TYPE_MISMATCH,
                $"[marshal::{name}] {msg}", frame);
            return default;
        }

        public static stackval UnBoxing(CallFrame frame, IshtarObject* obj)
        {
            if (obj == null)
                return new stackval();
            var @class = obj->decodeClass();

            var val = new stackval { type = @class.TypeCode };
            if (@class.TypeCode is TYPE_OBJECT or TYPE_CLASS or TYPE_STRING or TYPE_ARRAY or TYPE_RAW or TYPE_FUNCTION)
            {
                val.data.p = (nint)obj;
                return val;
            }

            if (@class.TypeCode is TYPE_NONE or > TYPE_ARRAY or < TYPE_NONE)
            {
                VM.FastFail(WNE.ACCESS_VIOLATION,
                    $"Scalar value type cannot be extracted. [{@class.FullName}]\n" +
                    "Invalid memory address is possible.\n" +
                    "Please report the problem into https://github.com/vein-lang/vein/issues",
                    frame);
                return default;
            }

            switch (val.type)
            {
                case TYPE_I1:
                    val.data.b = ToDotnetInt8(obj, frame);
                    break;
                case TYPE_I2:
                    val.data.s = ToDotnetInt16(obj, frame);
                    break;
                case TYPE_I4:
                    val.data.i = ToDotnetInt32(obj, frame);
                    break;
                case TYPE_I8:
                    val.data.l = ToDotnetInt64(obj, frame);
                    break;
                case TYPE_U1:
                    val.data.ub = ToDotnetUInt8(obj, frame);
                    break;
                case TYPE_U2:
                    val.data.us = ToDotnetUInt16(obj, frame);
                    break;
                case TYPE_U4:
                    val.data.ui = ToDotnetUInt32(obj, frame);
                    break;
                case TYPE_U8:
                    val.data.ul = ToDotnetUInt64(obj, frame);
                    break;
                case TYPE_BOOLEAN:
                    val.data.i = ToDotnetBoolean(obj, frame) ? 1 : 0;
                    break;
                case TYPE_CHAR:
                    val.data.i = ToDotnetChar(obj, frame);
                    break;
                case TYPE_R4:
                    val.data.f_r4 = ToDotnetFloat(obj, frame);
                    break;
                case TYPE_R8 or TYPE_R2 or TYPE_R16:
                    VM.FastFail(WNE.ACCESS_VIOLATION,
                        "Unboxing operation error.\n" +
                        $"Scalar value type '{val.type}' cannot be extracted.\n" +
                        "Currently is not support.\n" +
                        "Please report the problem into https://github.com/vein-lang/vein/issues",
                        frame);
                    return default;
            }

            return val;
        }

        public static IshtarObject* Boxing(CallFrame frame, stackval* p, IshtarObject** node = null)
        {
            if (p->type == TYPE_NONE)
                return null;

            if (p->type is TYPE_OBJECT or TYPE_CLASS or TYPE_STRING or TYPE_ARRAY or TYPE_RAW or TYPE_FUNCTION)
                return (IshtarObject*)p->data.p;
            if (p->type is TYPE_NONE or > TYPE_ARRAY or < TYPE_NONE)
            {
                VM.FastFail(WNE.ACCESS_VIOLATION,
                    "Boxing operation error.\n" +
                    $"Scalar value type cannot be extracted. [{p->type}]\n" +
                    "Invalid memory address is possible.\n" +
                    "Please report the problem into https://github.com/vein-lang/vein/issues",
                    frame);
                return null;
            }

            var clazz = p->type.AsRuntimeClass();
            var obj = IshtarGC.AllocObject(clazz, node);

            InternalFFI.StaticValidateField(frame, &obj, "!!value");

            obj->vtable[clazz.Field["!!value"].vtable_offset] = p->type switch
            {
                TYPE_I1 => (sbyte*)p->data.b,
                TYPE_U1 => (byte*)p->data.ub,
                TYPE_I2 => (short*)p->data.s,
                TYPE_U2 => (ushort*)p->data.us,
                TYPE_I4 => (int*)p->data.i,
                TYPE_U4 => (uint*)p->data.ui,
                TYPE_I8 => (long*)p->data.l,
                TYPE_U8 => (ulong*)p->data.ul,

                TYPE_BOOLEAN => (int*)p->data.i,
                TYPE_CHAR => (int*)p->data.i,

                TYPE_R4 => (int*)BitConverter.SingleToInt32Bits(p->data.f_r4),

                _ => &*p
            };

            return obj;
        }
    }
}
