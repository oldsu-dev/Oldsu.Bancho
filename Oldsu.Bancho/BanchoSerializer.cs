using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Oldsu.Bancho
{
    public class BanchoSerializableAttribute : System.Attribute { } 
    
    public static class BanchoSerializer
    {
        #region TypeMember's

        private abstract class TypeMember
        {
            protected MemberInfo Info { get; }

            protected object GetValueFromObject(object instance)
            {
                return Info.MemberType switch
                {
                    MemberTypes.Field => ((FieldInfo)Info).GetValue(instance),
                    MemberTypes.Property => ((PropertyInfo)Info).GetValue(instance),

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
                
                    case MemberTypes.Property:
                        ((PropertyInfo)Info).SetValue(instance, value);
                        break;

                    default:
                        throw new Exception();
                };
            }

            protected TypeMember(MemberInfo info)
            {
                Info = info;
            }

            public abstract void ReadFromStream(object instance, BinaryReader br);

            public abstract void WriteToStream(object instance, BinaryWriter bw);
        }

        private class IntMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadInt32());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                int value = (int)GetValueFromObject(instance);
                bw.Write(value);
            }

            public IntMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class UIntMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadUInt32());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                uint value = (uint)GetValueFromObject(instance);
                bw.Write(value);
            }

            public UIntMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class LongMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadInt64());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                long value = (long)GetValueFromObject(instance);
                bw.Write(value);
            }

            public LongMember(MemberInfo info) : base(info)
            {
            }
        }

        private class ULongMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadUInt64());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                ulong value = (ulong)GetValueFromObject(instance);
                bw.Write(value);
            }

            public ULongMember(MemberInfo info) : base(info)
            {
            }
        }

        private class ShortMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadInt16());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                short value = (short)GetValueFromObject(instance);
                bw.Write(value);
            }

            public ShortMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class UShortMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadUInt16());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                ushort value = (ushort)GetValueFromObject(instance);
                bw.Write(value);
            }

            public UShortMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class ByteMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadByte());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                byte value = (byte)GetValueFromObject(instance);
                bw.Write(value);
            }

            public ByteMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class SByteMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadSByte());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                sbyte value = (sbyte)GetValueFromObject(instance);
                bw.Write(value);
            }

            public SByteMember(MemberInfo info) : base(info)
            {
            }
        }
        
        private class FloatMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadSingle());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                float value = (float)GetValueFromObject(instance);
                bw.Write(value);
            }

            public FloatMember(MemberInfo info) : base(info)
            {
            }
        }

        private class BoolMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, br.ReadBoolean());
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                bool value = (bool)GetValueFromObject(instance);
                bw.Write(value);
            }

            public BoolMember(MemberInfo info) : base(info)
            {
            }
        }
        
        
        private class ListMember : TypeMember
        {
            private readonly Type _typeArgument;
            
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                var length = br.ReadInt32();
                var list = (IList)instance;

                for (; length > 0; length--)
                {
                    var element = Activator.CreateInstance(_typeArgument);
                    list.Add(element);   
                }
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                var list = (IList)instance;
                bw.Write(list.Count);

                foreach (var element in list)
                    bw.Write(BanchoSerializer.Serialize(element));
            }

            public ListMember(MemberInfo info) : base(info)
            {
                _typeArgument = info.MemberType switch
                {
                   MemberTypes.Field => ((FieldInfo)info).FieldType.GenericTypeArguments[0],
                   MemberTypes.Property => ((PropertyInfo)info).PropertyType.GenericTypeArguments[0],
                   
                   _ => throw new Exception()
                };
            }
        }
        
        private class StringMember : TypeMember
        {
            public override void ReadFromStream(object instance, BinaryReader br)
            {
                if (br.ReadByte() != 0xb)
                    return;

                SetValueToObject(instance, br.ReadString());
            }
            
            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                string value = (string)GetValueFromObject(instance);
                bw.Write(value);
            }

            public StringMember(MemberInfo info) : base(info)
            {
            }
        }

        private class ObjectMember : TypeMember
        {
            private Type _type;

            public override void ReadFromStream(object instance, BinaryReader br)
            {
                SetValueToObject(instance, Read(br, _type));
            }

            public override void WriteToStream(object instance, BinaryWriter bw)
            {
                BanchoSerializer.Write(GetValueFromObject(instance), bw);
            }

            public ObjectMember(MemberInfo info) : base(info)
            {
                _type = GetMemberType(info);
            }
        }
        
        #endregion

        private static Type GetMemberType(MemberInfo info)
        {
            var type = info.MemberType switch
            {
                MemberTypes.Field => ((FieldInfo)info).FieldType,
                MemberTypes.Property => ((PropertyInfo)info).PropertyType,

                _ => throw new Exception()
            };

            if (type.IsEnum)
                type = type.GetEnumUnderlyingType();

            return type;
        }

        private static readonly ConcurrentDictionary<Type, ImmutableArray<TypeMember>> _typeCache = new();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static IEnumerable<MemberInfo> GetAllMemberInfo(Type type) =>
            type.GetMembers().Concat(type.GetProperties());

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ImmutableArray<TypeMember> GetTypeMembers(Type type)
        {
            var members = (from memberInfo in GetAllMemberInfo(type)
                where memberInfo.GetCustomAttribute<BanchoSerializableAttribute>() != null
                select (TypeMember)(GetMemberType(memberInfo).ToString() switch
                {
                    "System.Byte" => new ByteMember(memberInfo),
                    "System.SByte" => new SByteMember(memberInfo),
                    "System.Int16" => new ShortMember(memberInfo),
                    "System.UInt16" => new UShortMember(memberInfo),
                    "System.Int32" => new IntMember(memberInfo),
                    "System.Int64" => new LongMember(memberInfo),
                    "System.UInt32" => new UIntMember(memberInfo),
                    "System.UInt64" => new ULongMember(memberInfo),
                    "System.Single" => new FloatMember(memberInfo),
                    "System.String" => new StringMember(memberInfo),
                    "System.Boolean" => new BoolMember(memberInfo),
                    "System.Collections.Generic.List" => new ListMember(memberInfo),
                    
                    _ => new ObjectMember(memberInfo)
                })).ToImmutableArray();
            
            return members;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ImmutableArray<TypeMember> GetOrAddCachedMembers(Type type)
        {
            return _typeCache.GetOrAdd(type, GetTypeMembers);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Write(object instance, BinaryWriter bw)
        {
            if (instance == null)
                return;

            var type = instance.GetType();
            var members = GetOrAddCachedMembers(type);

            foreach (var member in members)
                member.WriteToStream(instance, bw);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static object Read(BinaryReader br, Type type)
        {
            var members = GetOrAddCachedMembers(type);
            var instance = Activator.CreateInstance(type);

            foreach (var member in members)
                member.ReadFromStream(instance, br);

            return instance;
        }
       

        public static T Deserialize<T>(byte[] data) where T: new()
        {
            if (data == null)
                return default;

            using var ms = new MemoryStream(data);
            using var br = new BinaryReader(ms, Encoding.UTF8, leaveOpen: true);
            
            return (T)Read(br, typeof(T));
        }

        public static byte[] Serialize(object instance)
        {
            if (instance == null)
                return null;
            
            using var ms = new MemoryStream();
            using (var bw = new BinaryWriter(ms, Encoding.UTF8, leaveOpen: true))
                Write(instance, bw);

            return ms.ToArray();
        }
    }
}