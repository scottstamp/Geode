using System;
using System.IO;
using System.Net;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.Serialization.Json;

using Geode.Habbo;
using Geode.Network;
using Geode.Habbo.Messages;
using Geode.Network.Protocol;

namespace Geode.Extension
{
    public class TService : IExtension
    {
        private readonly HNode _installer;
        private readonly IExtension _container;
        private readonly List<DataCaptureAttribute> _unknownDataAttributes;
        private readonly Dictionary<ushort, Action<HPacket>> _extensionEvents;
        private readonly Dictionary<ushort, List<DataCaptureAttribute>> _outDataAttributes, _inDataAttributes;

        private static readonly DataContractJsonSerializer _worldSerializer;

        public const int EXTENSION_INFO = 1;
        public const int MANIPULATED_PACKET = 2;
        public const int REQUEST_FLAGS = 3;
        public const int SEND_MESSAGE = 4;
        public const int EXTENSION_CONSOLE_LOG = 98;

        public Incoming In { get; private set; }
        public Outgoing Out { get; private set; }
        public string Revision { get; private set; }
        public HotelEndPoint HotelServer { get; private set; }

        private readonly IDictionary<int, HEntity> _entities;
        public IReadOnlyDictionary<int, HEntity> Entities { get; }

        private readonly IDictionary<int, HWallItem> _wallItems;
        public IReadOnlyDictionary<int, HWallItem> WallItems { get; }

        private readonly IDictionary<int, HFloorItem> _floorItems;
        public IReadOnlyDictionary<int, HFloorItem> FloorItems { get; }

        public static IPEndPoint DefaultModuleServer { get; }

        static TService()
        {
            _worldSerializer = new DataContractJsonSerializer(typeof(HWorld));

            DefaultModuleServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9092);
        }

        public TService(IExtension container)
            : this(container, null, null)
        { }
        public TService(IExtension container, IPEndPoint moduleServer)
            : this(container, null, moduleServer)
        { }

        protected TService()
            : this(null, null, null)
        { }
        protected TService(IPEndPoint moduleServer)
            : this(null, null, moduleServer)
        { }

        protected TService(TService parent)
            : this(null, parent, null)
        { }
        protected TService(TService parent, IPEndPoint moduleServer)
            : this(null, parent, moduleServer)
        { }

        private TService(IExtension container, TService parent, IPEndPoint moduleServer)
        {
            _container = container ?? this;
            _unknownDataAttributes = parent?._unknownDataAttributes ?? new List<DataCaptureAttribute>();
            _inDataAttributes = parent?._inDataAttributes ?? new Dictionary<ushort, List<DataCaptureAttribute>>();
            _outDataAttributes = parent?._outDataAttributes ?? new Dictionary<ushort, List<DataCaptureAttribute>>();

            _extensionEvents = new Dictionary<ushort, Action<HPacket>>
            {
                [1] = _container.OnDoubleClick,
                [2] = _container.OnInfoRequest,
                [3] = _container.OnPacketIntercept,
                [4] = _container.OnFlagsCheck,
                [5] = _container.OnConnected,
                [6] = _container.OnDisconnected,
                [7] = _container.OnInitialized
            };

            _entities = new ConcurrentDictionary<int, HEntity>();
            Entities = new ReadOnlyDictionary<int, HEntity>(_entities);

            _wallItems = new ConcurrentDictionary<int, HWallItem>();
            WallItems = new ReadOnlyDictionary<int, HWallItem>(_wallItems);

            _floorItems = new ConcurrentDictionary<int, HFloorItem>();
            FloorItems = new ReadOnlyDictionary<int, HFloorItem>(_floorItems);

            if (LicenseManager.UsageMode != LicenseUsageMode.Runtime) return;
            foreach (MethodInfo method in _container.GetType().GetAllMethods())
            {
                foreach (var dataCaptureAtt in method.GetCustomAttributes<DataCaptureAttribute>())
                {
                    dataCaptureAtt.Method = method;
                    if (_unknownDataAttributes.Any(dca => dca.Equals(dataCaptureAtt))) continue;

                    dataCaptureAtt.Target = _container;
                    if (dataCaptureAtt.Id != null)
                    {
                        AddCallback(dataCaptureAtt, (ushort)dataCaptureAtt.Id);
                    }
                    else _unknownDataAttributes.Add(dataCaptureAtt);
                }
            }

            _installer = HNode.ConnectNewAsync(moduleServer ?? DefaultModuleServer).GetAwaiter().GetResult();
            if (_installer == null) throw new Exception("Failed to establish a connection with the extension server: " + moduleServer);
            Task handleInstallerDataTask = HandleInstallerDataAsync();
        }

        public virtual void OnFlagsCheck(HPacket packet)
        { }
        public virtual void OnDoubleClick(HPacket packet)
        { }
        public virtual void OnInfoRequest(HPacket packet)
        {
            var infoResponsePacket = new EvaWirePacket(EXTENSION_INFO);
            AssemblyName moduleAssemblyName = Assembly.GetAssembly(_container.GetType()).GetName();
            var moduleAtt = _container.GetType().GetCustomAttribute<ModuleAttribute>();

            infoResponsePacket.Write(moduleAtt?.Title ?? moduleAssemblyName.Name); // Title
            infoResponsePacket.Write(moduleAtt?.Author ?? string.Empty); // Author
            infoResponsePacket.Write(moduleAssemblyName.Version.ToString()); // Version
            infoResponsePacket.Write(moduleAtt?.Description ?? string.Empty);
            infoResponsePacket.Write(true); // UtilizingOnDoubleClick

            infoResponsePacket.Write(false); // IsInstalledExtension
            infoResponsePacket.Write(string.Empty); // FileName
            infoResponsePacket.Write(string.Empty); // Cookie

            infoResponsePacket.Write(false); // LeaveButtonVisible
            infoResponsePacket.Write(false); // DeleteButtonVisible

            _installer.SendPacketAsync(infoResponsePacket);
        }
        public virtual void OnPacketIntercept(HPacket packet)
        {
            int stringifiedInteceptionDataLength = packet.ReadInt32();
            string stringifiedInterceptionData = Encoding.GetEncoding("latin1").GetString(packet.ReadBytes(stringifiedInteceptionDataLength));

            var dataInterceptedArgs = new DataInterceptedEventArgs(stringifiedInterceptionData);
            HandleGameObjects(dataInterceptedArgs.Packet, dataInterceptedArgs.IsOutgoing);

            Dictionary<ushort, List<DataCaptureAttribute>> callbacks = dataInterceptedArgs.IsOutgoing ? _outDataAttributes : _inDataAttributes;
            if (callbacks.TryGetValue(dataInterceptedArgs.Packet.Id, out List<DataCaptureAttribute> attributes))
            {
                foreach (DataCaptureAttribute attribute in attributes)
                {
                    dataInterceptedArgs.Packet.Position = 0;
                    attribute.Invoke(dataInterceptedArgs);
                }
            }

            string stringified = dataInterceptedArgs.ToString(true);
            _installer.SendPacketAsync(2, stringified.Length, Encoding.GetEncoding("latin1").GetBytes(stringified));
        }

        public virtual void OnInitialized(HPacket packet)
        { }
        public virtual void OnConnected(HPacket packet)
        {
            HotelServer = HotelEndPoint.Parse(packet.ReadUTF8(), packet.ReadInt32());
            Revision = packet.ReadUTF8();

            HWorld world = null;
            string messagesPath = packet.ReadUTF8();
            using (FileStream messagesStream = File.OpenRead(messagesPath))
            {
                world = (HWorld)_worldSerializer.ReadObject(messagesStream);

                Revision = world.Revision;
                In = new Incoming(world.In);
                Out = new Outgoing(world.Out);
            }
            ResolveCallbacks();
        }
        public virtual void OnDisconnected(HPacket packet)
        { }

        public Task<int> SendToClientAsync(byte[] data)
        {
            return _installer.SendPacketAsync(2, false, data.Length, data);
        }
        public Task<int> SendToClientAsync(HPacket packet)
        {
            return SendToClientAsync(packet.ToBytes());
        }
        public Task<int> SendToClientAsync(ushort id, params object[] values)
        {
            return SendToClientAsync(EvaWirePacket.Construct(id, values));
        }

        public Task<int> SendToServerAsync(byte[] data)
        {
            return _installer.SendPacketAsync(2, true, data.Length, data);
        }
        public Task<int> SendToServerAsync(HPacket packet)
        {
            return SendToServerAsync(packet.ToBytes());
        }
        public Task<int> SendToServerAsync(ushort id, params object[] values)
        {
            return SendToServerAsync(EvaWirePacket.Construct(id, values));
        }

        public HMessages GetMessages(bool isOutgoing) => isOutgoing ? (HMessages)Out : In;
        public HMessage GetMessage(ushort id, bool isOutgoing) => GetMessages(isOutgoing).GetMessage(id);
        public HMessage GetMessage(string identifier, bool isOutgoing) => GetMessages(isOutgoing).GetMessage(identifier);

        private void ResolveCallbacks()
        {
            var unresolved = new Dictionary<string, IList<string>>();
            foreach (PropertyInfo property in _container.GetType().GetAllProperties())
            {
                var messageAtt = property.GetCustomAttribute<MessageAttribute>();
                if (string.IsNullOrWhiteSpace(messageAtt?.Identifier)) continue;

                HMessage message = GetMessage(messageAtt.Identifier, messageAtt.IsOutgoing);
                if (message == null)
                {
                    if (!unresolved.TryGetValue(messageAtt.Identifier, out IList<string> users))
                    {
                        users = new List<string>();
                        unresolved.Add(messageAtt.Identifier, users);
                    }
                    users.Add($"Property({property.Name})");
                }
                else property.SetValue(_container, message);
            }
            foreach (DataCaptureAttribute dataCaptureAtt in _unknownDataAttributes)
            {
                if (string.IsNullOrWhiteSpace(dataCaptureAtt.Identifier)) continue;
                HMessage message = GetMessage(dataCaptureAtt.Identifier, dataCaptureAtt.IsOutgoing);
                if (message == null)
                {
                    if (!unresolved.TryGetValue(dataCaptureAtt.Identifier, out IList<string> users))
                    {
                        users = new List<string>();
                        unresolved.Add(dataCaptureAtt.Identifier, users);
                    }
                    users.Add($"Method({dataCaptureAtt.Method})");
                }
                else AddCallback(dataCaptureAtt, message.Id);
            }
            if (unresolved.Count > 0)
            {
                throw new MessagesResolveException(Revision, unresolved);
            }
        }
        private async Task HandleInstallerDataAsync()
        {
            await Task.Yield();
            try
            {
                HPacket packet = await _installer.ReceivePacketAsync().ConfigureAwait(false);
                if (packet == null) Environment.Exit(0);

                Task handleInstallerDataTask = HandleInstallerDataAsync();
                if (_extensionEvents.TryGetValue(packet.Id, out Action<HPacket> handler))
                {
                    handler(packet);
                }
            }
            catch { Environment.Exit(0); }
        }
        private void HandleGameObjects(HPacket packet, bool isOutgoing)
        {
            packet.Position = 0;
            if (!isOutgoing)
            {
                if (packet.Id == In.RoomUsers)
                {
                    foreach (HEntity entity in HEntity.Parse(packet))
                    {
                        _entities[entity.Index] = entity;
                    }
                }
                else if (packet.Id == In.RoomWallItems)
                {

                    foreach (HWallItem wallItem in HWallItem.Parse(packet))
                    {
                        _wallItems[wallItem.Id] = wallItem;
                    }
                }
                else if (packet.Id == In.RoomFloorItems)
                {
                    foreach (HFloorItem floorItem in HFloorItem.Parse(packet))
                    {
                        _floorItems[floorItem.Id] = floorItem;
                    }
                }
                else if (packet.Id == In.RoomHeightMap)
                {
                    _entities.Clear();
                    _wallItems.Clear();
                    _floorItems.Clear();
                }
            }
            packet.Position = 0;
        }
        private void AddCallback(DataCaptureAttribute attribute, ushort id)
        {
            Dictionary<ushort, List<DataCaptureAttribute>> callbacks =
                attribute.IsOutgoing ? _outDataAttributes : _inDataAttributes;

            if (!callbacks.TryGetValue(id, out List<DataCaptureAttribute> attributes))
            {
                attributes = new List<DataCaptureAttribute>();
                callbacks.Add(id, attributes);
            }
            attributes.Add(attribute);
        }

        public virtual void Dispose()
        {
            _inDataAttributes.Clear();
            _outDataAttributes.Clear();
            _unknownDataAttributes.Clear();
        }

        [DataContract]
        private class HWorld
        {
            [DataMember]
            public string Revision { get; set; }

            [DataMember]
            public int FileLength { get; set; }

            [DataMember(Name = "Incoming")]
            public List<HMessage> In { get; set; }

            [DataMember(Name = "Outgoing")]
            public List<HMessage> Out { get; set; }
        }
    }
}