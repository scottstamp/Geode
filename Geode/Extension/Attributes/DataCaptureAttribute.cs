using System;
using System.Reflection;
using System.Collections.Generic;

using Geode.Habbo;
using Geode.Network;
using Geode.Network.Protocol;

namespace Geode.Extension
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public abstract class DataCaptureAttribute : Attribute, IEquatable<DataCaptureAttribute>
    {
        public ushort? Id { get; }
        public bool IsOutgoing { get; }
        public string Identifier { get; }

        internal object Target { get; set; }
        internal MethodInfo Method { get; set; }

        private DataCaptureAttribute(bool isOutgoing)
        {
            IsOutgoing = isOutgoing;
        }
        public DataCaptureAttribute(ushort id, bool isOutgoing)
            : this(isOutgoing)
        {
            Id = id;
        }
        public DataCaptureAttribute(string identifier, bool isOutgoing)
            : this(isOutgoing)
        {
            Identifier = identifier;
        }

        internal void Invoke(DataInterceptedEventArgs args)
        {
            object[] parameters = CreateValues(args);
            object result = Method?.Invoke(Target, parameters);

            switch (result)
            {
                case bool isBlocked:
                {
                    args.IsBlocked = isBlocked;
                    break;
                }
                case HPacket packet:
                {
                    args.Packet = packet;
                    break;
                }
                case object[] chunks:
                {
                    args.Packet = args.Packet.Format.CreatePacket(args.Packet.Id, chunks);
                    break;
                }
            }
        }
        private object[] CreateValues(DataInterceptedEventArgs args)
        {
            ParameterInfo[] parameters = Method.GetParameters();
            var values = new object[parameters.Length];

            int position = 0;
            for (int i = 0; i < values.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                switch (Type.GetTypeCode(parameter.ParameterType))
                {
                    case TypeCode.UInt16:
                    {
                        if (parameter.Name.Equals("id", StringComparison.OrdinalIgnoreCase))
                        {
                            values[i] = args.Packet.Id;
                        }
                        else
                        {
                            values[i] = args.Packet.ReadUInt16(ref position);
                        }
                        break;
                    }

                    case TypeCode.Int32:
                    values[i] = args.Packet.ReadInt32(ref position);
                    break;

                    case TypeCode.Boolean:
                    values[i] = args.Packet.ReadBoolean(ref position);
                    break;

                    case TypeCode.Byte:
                    values[i] = args.Packet.ReadByte(ref position);
                    break;

                    case TypeCode.String:
                    values[i] = args.Packet.ReadUTF8(ref position);
                    break;

                    case TypeCode.Double:
                    values[i] = args.Packet.ReadDouble(ref position);
                    break;

                    case TypeCode.Object:
                    {
                        if (parameter.ParameterType == typeof(DataInterceptedEventArgs))
                        {
                            values[i] = args;
                        }
                        else if (parameter.ParameterType == typeof(HPacket))
                        {
                            values[i] = args.Packet;
                        }
                        else if (parameter.ParameterType == typeof(HPoint))
                        {
                            values[i] = new HPoint(args.Packet.ReadInt32(ref position), args.Packet.ReadInt32(ref position));
                        }
                        else if (parameter.ParameterType == typeof(byte[]))
                        {
                            int length = args.Packet.ReadInt32(ref position);
                            values[i] = args.Packet.ReadBytes(length, ref position);
                        }
                        else if (typeof(IList<HEntity>).IsAssignableFrom(parameter.ParameterType))
                        {
                            args.Packet.Position = 0;
                            values[i] = HEntity.Parse(args.Packet);
                        }
                        else if (typeof(IList<HFloorItem>).IsAssignableFrom(parameter.ParameterType))
                        {
                            args.Packet.Position = 0;
                            values[i] = HFloorItem.Parse(args.Packet);
                        }
                        else if (typeof(IList<HWallItem>).IsAssignableFrom(parameter.ParameterType))
                        {
                            args.Packet.Position = 0;
                            values[i] = HWallItem.Parse(args.Packet);
                        }
                        else if (typeof(IList<HEntityUpdate>).IsAssignableFrom(parameter.ParameterType))
                        {
                            args.Packet.Position = 0;
                            values[i] = HEntityUpdate.Parse(args.Packet);
                        }
                        break;
                    }
                }
            }
            return values;
        }

        public bool Equals(DataCaptureAttribute other)
        {
            if (Id != other.Id) return false;
            if (Identifier != other.Identifier) return false;
            if (!Method.Equals(other.Method)) return false;
            return true;
        }
    }
}