using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Version = Oldsu.Enums.Version;

namespace Oldsu.Bancho
{
    public enum BanchoPacketType
    {
        In,
        Out
    }

    public class BanchoPacketAttribute : System.Attribute
    {
        public ushort Id { get; }

        public Version Version { get; }
        
        public BanchoPacketType Type { get; }

        public BanchoPacketAttribute(ushort id, Version version, BanchoPacketType type)
        {
            Id = id;
            Version = version;
            Type = type;
        }
    }

    public interface IBanchoCustomSerializer
    {
        void Serialize(object self, object instance, BinaryWriter writer);
        object Deserialize(object instance, BinaryReader reader);
    }
    
    public class BanchoCustomSerializerAttribute : System.Attribute
    {
        public IBanchoCustomSerializer Serializer { get; }

        public BanchoCustomSerializerAttribute(Type serializer)
        {
            Serializer = (IBanchoCustomSerializer)Activator.CreateInstance(serializer)!;
        }
    }
    
    public class BanchoSerializableAttribute : System.Attribute
    {
        public bool Optional { get; }
        
        public int ArrayElementCount { get; }

        public int Index { get; }
        
        public BanchoSerializableAttribute(
            [CallerLineNumber] int index = 0,
            bool optional = false, int arrayElementCount = 0)
        {
            Index = index;
            Optional = optional;
            ArrayElementCount = arrayElementCount;
        }
    }
    
    public class BanchoBuffer
    {
        public byte[] Data { get; set; }
    }

    public static class BanchoSerializer
    {
        #region TypeMember's

        public abstract class TypeMember
        {
            public bool IsOptional { get; }
            public MemberInfo Info { get; }

            public BanchoSerializableAttribute Attribute { get; }

            public object? GetValueFromObject(object instance)
            {
                return Info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)Info).GetValue(instance),

                    _ => throw new Exception()
                };
            }

            protected void SetValueToObject(object instance, object value)
            {
                switch (Info.MemberType)
                {
                    case MemberTypes.Field:
                        ((FieldInfo)Info).SetValue(instance, value);
                        break;

                    default:
                        throw new Exception(Info.MemberType.ToString());
                }

                ;
            }

            protected TypeMember(MemberInfo info, BanchoSerializableAttribute attribute)
            {
                Info = info;
                Attribute = attribute;
                IsOptional = attribute.Optional;
            }

            public virtual void ReadFromStream(object? instance, BinaryReader br) =>
                SetValueToObject(instance!, ReadValueFromStream(br)!);

            public virtual void WriteToStream(object? instance, BinaryWriter bw)
                => WriteValueToStream(GetValueFromObject(instance!)!, bw);
            
            public abstract object? ReadValueFromStream(BinaryReader br);
            public abstract void WriteValueToStream(object value, BinaryWriter bw);

            
        }

        private class IntMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadInt32();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((int)value);
            }

            public IntMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class UIntMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadUInt32();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((uint)value);
            }

            public UIntMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class LongMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadInt64();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((long)value);
            }

            public LongMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }

        private class ULongMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadUInt64();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((ulong)value);
            }

            public ULongMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }

        private class ShortMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadInt16();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((short)value);
            }

            public ShortMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class UShortMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadUInt16();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((ushort)value);
            }

            public UShortMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class ByteMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadByte();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((byte)value);
            }

            public ByteMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class SByteMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadSByte();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((sbyte)value);
            }

            public SByteMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        private class FloatMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadSingle();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((float)value);
            }

            public FloatMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }

        private class BoolMember : TypeMember
        {
            public override object ReadValueFromStream(BinaryReader br)
            {
                return br.ReadBoolean();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                bw.Write((bool)value);
            }

            public BoolMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }
        
        
        private class ListMember : TypeMember
        {
            private readonly Type _typeArgument;
            

            public override object ReadValueFromStream(BinaryReader br)
            {
                var length = br.ReadInt32();
                var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(_typeArgument))!;

                for (; length > 0; length--)
                {
                    var element = _child.ReadValueFromStream(br);
                    list.Add(element);
                }

                return list;
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                var list = (IList)value;
                bw.Write(list.Count);

                foreach (var element in list)
                    _child.WriteValueToStream(element, bw);
            }

            private readonly TypeMember _child;
            
            public ListMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
                _typeArgument = info.MemberType switch
                {
                   MemberTypes.Field => ((FieldInfo)info).FieldType.GenericTypeArguments[0],
                   MemberTypes.Property => ((PropertyInfo)info).PropertyType.GenericTypeArguments[0],
                   
                   _ => throw new Exception()
                };

                _child = GetTypeMember(_typeArgument, info, attrib);
            }
        }
        
        private class StringMember : TypeMember
        {
            public override object? ReadValueFromStream(BinaryReader br)
            {
                return br.ReadByte() != 0xb ? null : br.ReadString();
            }

            public override void WriteValueToStream(object? value, BinaryWriter bw)
            {
                if (value == null)
                    bw.Write((byte)0x0);
                else
                {
                    bw.Write((byte)0xb);
                    bw.Write((string)value!);
                }
            }

            public StringMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
            }
        }

        private class ArrayMember : TypeMember
        {
            private readonly TypeMember _child;
            private readonly Type _type;

            public ArrayMember(MemberInfo info, BanchoSerializableAttribute attribute) 
                : base(info, attribute)
            {
                _type = GetMemberType(info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)info).FieldType.GetElementType()!,
                   
                    _ => throw new Exception()
                });

                _child = GetTypeMember(_type, info, attribute);
            }

            public override void ReadFromStream(object? instance, BinaryReader br)
            {
                var array = Array.CreateInstance(_type, Attribute.ArrayElementCount)!;
                
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(_child.ReadValueFromStream(br), i);
                    
                SetValueToObject(instance!, array);
            }

            public override void WriteToStream(object? instance, BinaryWriter bw)
            {
                var array = (Array)GetValueFromObject(instance)!;

                if (Attribute.ArrayElementCount != array.Length)
                    throw new InvalidDataException(
                        $"Configured array size is {Attribute.ArrayElementCount}, got {array.Length}");

                for (int i = 0; i < array.Length; i++)
                    _child.WriteValueToStream(array.GetValue(i)!, bw);
            }

            public override object? ReadValueFromStream(BinaryReader br)
            {
                var array = Array.CreateInstance(_type, Attribute.ArrayElementCount)!;
                
                for (int i = 0; i < array.Length; i++)
                    array.SetValue(_child.ReadValueFromStream(br), i);

                return array;
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                var array = (Array)value;

                if (Attribute.ArrayElementCount != array.Length)
                    throw new InvalidDataException(
                        $"Configured array size is {Attribute.ArrayElementCount}, got {array.Length}");

                for (int i = 0; i < array.Length; i++)
                    _child.WriteValueToStream(array.GetValue(i)!, bw);
            }
        }
        
        private class ObjectMember : TypeMember
        {
            private Type _type;

            public override object? ReadValueFromStream(BinaryReader br)
            {
                return Read(br, _type);
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                Write(value, bw);
            }

            public ObjectMember(MemberInfo info, BanchoSerializableAttribute attrib) : base(info, attrib)
            {
                _type = GetMemberType(info);
            }
        }

        private class CustomSerializeMember : TypeMember
        {
            private readonly IBanchoCustomSerializer _customSerializer; 
            
            public CustomSerializeMember(
                BanchoCustomSerializerAttribute customSerializer,
                MemberInfo info, 
                BanchoSerializableAttribute attribute) : base(info, attribute)
            {
                _customSerializer = customSerializer.Serializer;
            }

            public override void ReadFromStream(object? instance, BinaryReader br)
            {
                SetValueToObject(instance!, _customSerializer.Deserialize(instance!, br));
            }

            public override void WriteToStream(object? instance, BinaryWriter bw)
            {
                _customSerializer.Serialize(GetValueFromObject(instance!)!, instance!, bw);
            }

            public override object? ReadValueFromStream(BinaryReader br)
            {
                throw new NotImplementedException();
            }

            public override void WriteValueToStream(object value, BinaryWriter bw)
            {
                throw new NotImplementedException();
            }
        }
        
        #endregion

        private static Type GetMemberType(MemberInfo info)
        {
            var type = info.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)info).FieldType,
                MemberTypes.TypeInfo => ((TypeInfo)info).AsType(),
                
                _ => throw new Exception(info.MemberType.ToString())
            };

            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            if (type.IsValueType)
            {
                Type? nullableType = Nullable.GetUnderlyingType(type);
                
                if (nullableType != null)
                    type = nullableType!;
            }
            
            return type;
        }


        private static readonly IReadOnlyDictionary<(ushort, Version), Type> _inPackets =
            Assembly.GetAssembly(typeof(BanchoSerializer))!.GetTypes()
                .Where(t =>
                {
                    var attribute = t.GetCustomAttribute<BanchoPacketAttribute>();
                    return attribute is { Type: BanchoPacketType.In };
                })
                .ToDictionary(t =>
                {
                    var attribute = t.GetCustomAttribute<BanchoPacketAttribute>()!;
                    return (attribute.Id, attribute.Version);
                });

        private static readonly ConcurrentDictionary<Type, BanchoPacketAttribute> _packetAttributeCache = new();
        private static readonly ConcurrentDictionary<Type, ImmutableArray<TypeMember>> _memberCache = new();

        private static IEnumerable<MemberInfo> GetAllMemberInfo(Type type) => type.GetMembers();

        private static TypeMember GetTypeMember(Type memberType, 
            MemberInfo memberInfo, BanchoSerializableAttribute attrib)
        {
            return memberType.ToString() switch
            {
                "System.Byte" => new ByteMember(memberInfo, attrib),
                "System.SByte" => new SByteMember(memberInfo, attrib),
                "System.Int16" => new ShortMember(memberInfo, attrib),
                "System.UInt16" => new UShortMember(memberInfo, attrib),
                "System.Int32" => new IntMember(memberInfo, attrib),
                "System.Int64" => new LongMember(memberInfo, attrib),
                "System.UInt32" => new UIntMember(memberInfo, attrib),
                "System.UInt64" => new ULongMember(memberInfo, attrib),
                "System.Single" => new FloatMember(memberInfo, attrib),
                "System.String" => new StringMember(memberInfo, attrib),
                "System.Boolean" => new BoolMember(memberInfo, attrib),
                "System.Collections.Generic.List" => new ListMember(memberInfo, attrib),

                _ => new ObjectMember(memberInfo, attrib)
            };
        }
        
        private static ImmutableArray<TypeMember> GetTypeMembers(Type type)
        {
            var members = GetAllMemberInfo(type)
                .Where(memberInfo => memberInfo.GetCustomAttribute<BanchoSerializableAttribute>() != null &&
                                     memberInfo.MemberType != MemberTypes.TypeInfo)
                .OrderBy(memberInfo => memberInfo.GetCustomAttribute<BanchoSerializableAttribute>()!.Index)
                .Select(memberInfo =>
                {
                    var attrib = memberInfo.GetCustomAttribute<BanchoSerializableAttribute>()!;
                    
                    var customSerializer = memberInfo.GetCustomAttribute<BanchoCustomSerializerAttribute>();
                    if (customSerializer != null)
                        return new CustomSerializeMember(customSerializer, memberInfo, attrib);

                    var memberType = GetMemberType(memberInfo);

                    if (memberType.IsArray)
                        return new ArrayMember(memberInfo, attrib);

                    return GetTypeMember(memberType, memberInfo, attrib);
                }).ToImmutableArray();

            return members;
        }

        private static ImmutableArray<TypeMember> GetOrAddCachedMembers(Type type) => 
            _memberCache.GetOrAdd(type, GetTypeMembers);

        private static BanchoPacketAttribute GetOrAddCachedPacketAttributes(Type type)
        {
            return _packetAttributeCache.GetOrAdd(type, (t) =>
            {
                var attrib = t.GetCustomAttribute<BanchoPacketAttribute>();

                if (attrib == null)
                    throw new ArgumentException("Missing BanchoPacketAttribute.");
                
                return attrib;
            });
        }
        
        private static (ushort id, int length) ReadPacketHeader(BinaryReader br)
        {
            var id = br.ReadUInt16();
            var _ = br.ReadBoolean();
            var length = br.ReadInt32();

            return (id, length);
        }

        private static void Write(object instance, BinaryWriter bw)
        {
            var type = instance.GetType();
            var members = GetOrAddCachedMembers(type);

            foreach (var member in members)
            {
                if (member.IsOptional)
                {
                    if (member.GetValueFromObject(instance) == null)
                    {
                        bw.Write(false);
                        continue;
                    }
                    
                    bw.Write(true);
                }
                
                member.WriteToStream(instance, bw);
            }
        }

        private static object Read(BinaryReader br, Type type)
        {
            var members = GetOrAddCachedMembers(type);
            var instance = Activator.CreateInstance(type)!;

            foreach (var member in members)
            {
                if (member.IsOptional && !br.ReadBoolean())
                    continue;
                
                member.ReadFromStream(instance, br);
            }

            return instance;
        }

        public static object? Deserialize(byte[]? data, Version version)
        {
            if (data == null)
                return null;
            
            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);

            var (id, length) = ReadPacketHeader(br);

            Type type;

            if (!_inPackets.TryGetValue((id, Version.NotApplicable), out type!) &&
                !_inPackets.TryGetValue((id, version), out type!))
            {
                return null;
            }

            object instance;
            
            if ((type.BaseType ?? type) == typeof(BanchoBuffer))
            {
                instance = Activator.CreateInstance(type)!; 
                ((BanchoBuffer)instance).Data = br.ReadBytes(length);
            }
            else
                instance = Read(br, type);

            return instance;
        }

        public static byte[] Serialize(object instance)
        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true);
            Type type = instance.GetType();

            var attrib = GetOrAddCachedPacketAttributes(type);

            bw.Write(attrib.Id);
            bw.Write((byte)0x0);

            if (instance is BanchoBuffer buffer)
            {
                bw.Write(buffer.Data.Length);
                bw.Write(buffer.Data);
            }
            else
            {
                bw.Write((int)0x0);

                Write(instance, bw);

                bw.Seek(3, SeekOrigin.Begin);
                bw.Write((int)(bw.BaseStream.Length - 7));
            }

            return ms.ToArray();
        }
    }
}