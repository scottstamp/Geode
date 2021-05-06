using System;
using System.Net;
using System.Linq;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using Geode.Habbo;
using Geode.Network;
using Geode.Habbo.Messages;
using Geode.Network.Protocol;

namespace Geode.Extension
{
    public class GService : IExtension, IHConnection, IDisposable
    {
        public bool MessagesInfo_Failed = false;
        private readonly HNode _installer;
        private readonly IExtension _container;
        private readonly List<DataCaptureAttribute> _unknownDataAttributes;
        private readonly Dictionary<ushort, Action<HPacket>> _extensionEvents;
        private readonly Dictionary<ushort, List<DataCaptureAttribute>> _outDataAttributes, _inDataAttributes;

        public const ushort EXTENSION_INFO = 1;
        public const ushort MANIPULATED_PACKET = 2;
        public const ushort REQUEST_FLAGS = 3;
        public const ushort SEND_MESSAGE = 4;
        public const ushort EXTENSION_CONSOLE_LOG = 98;

        public Incoming In { get; private set; }
        public Outgoing Out { get; private set; }
        public string ClientVersion { get; private set; }
        public string ClientIdentifier { get; private set; }
        public string ClientType { get; private set; }
        public HotelEndPoint HotelServer { get; private set; }

        private readonly IDictionary<int, HEntity> _entities;
        public IReadOnlyDictionary<int, HEntity> Entities { get; }

        private readonly IDictionary<int, HWallItem> _wallItems;
        public IReadOnlyDictionary<int, HWallItem> WallItems { get; }

        private readonly IDictionary<int, HFloorItem> _floorItems;
        public IReadOnlyDictionary<int, HFloorItem> FloorItems { get; }

        public static IPEndPoint DefaultModuleServer { get; }
        public List<HMessage> MessagesInfoIncoming { get; private set; }
        public List<HMessage> MessagesInfoOutgoing { get; private set; }

        static GService()
        {
            DefaultModuleServer = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9092);
        }

        public GService(IExtension container)
            : this(container, null, null)
        { }
        public GService(IExtension container, IPEndPoint moduleServer)
            : this(container, null, moduleServer)
        { }

        protected GService()
            : this(null, null, null)
        { }
        protected GService(IPEndPoint moduleServer)
            : this(null, null, moduleServer)
        { }

        protected GService(GService parent)
            : this(null, parent, null)
        { }
        protected GService(GService parent, IPEndPoint moduleServer)
            : this(null, parent, moduleServer)
        { }

        private GService(IExtension container, GService parent, IPEndPoint moduleServer)
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
            if (_installer == null) { OnCriticalError("Connection failed"); return; }
            Task handleInstallerDataTask = HandleInstallerDataAsync();
        }

        public void OnEntitiesLoaded(int count)
        { }
        public void OnWallItemsLoaded(int count)
        { }
        public void OnFloorItemsLoaded(int count)
        { }

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
            infoResponsePacket.Write(moduleAtt.UtilizingOnDoubleClick); // UtilizingOnDoubleClick

            infoResponsePacket.Write(false); // IsInstalledExtension
            infoResponsePacket.Write(string.Empty); // FileName
            infoResponsePacket.Write(string.Empty); // Cookie

            infoResponsePacket.Write(moduleAtt.LeaveButtonVisible); // LeaveButtonVisible
            infoResponsePacket.Write(false); // DeleteButtonVisible

            _installer.SendPacketAsync(infoResponsePacket);
        }
        public virtual void OnPacketIntercept(HPacket packet)
        {
            int stringifiedInteceptionDataLength = packet.ReadInt32();
            string stringifiedInterceptionData = Encoding.GetEncoding("latin1").GetString(packet.ReadBytes(stringifiedInteceptionDataLength));

            var dataInterceptedArgs = new DataInterceptedEventArgs(stringifiedInterceptionData);
            OnDataIntercept(dataInterceptedArgs);
        }
        public virtual void OnDataIntercept(DataInterceptedEventArgs data)
        {
            if (MessagesInfo_Failed == false)
            {
                HandleGameObjects(data.Packet, data.IsOutgoing);
            }

            Dictionary<ushort, List<DataCaptureAttribute>> callbacks = data.IsOutgoing ? _outDataAttributes : _inDataAttributes;
            if (callbacks.TryGetValue(data.Packet.Id, out List<DataCaptureAttribute> attributes))
            {
                foreach (DataCaptureAttribute attribute in attributes)
                {
                    data.Packet.Position = 0;
                    attribute.Invoke(data);
                }
            }

            string stringified = data.ToString(true);
            _installer.SendPacketAsync(MANIPULATED_PACKET, stringified.Length, Encoding.GetEncoding("latin1").GetBytes(stringified));
        }

        public virtual void OnInitialized(HPacket packet)
        {
            _installer.SendPacketAsync(REQUEST_FLAGS);
        }
        public virtual void OnConnected(HPacket packet)
        {
            var hHost = packet.ReadUTF8();
            var hPort = packet.ReadInt32();

            ClientVersion = packet.ReadUTF8();
            ClientIdentifier = packet.ReadUTF8();
            ClientType = packet.ReadUTF8();

            if (ClientType == "UNITY")
            {
                MessagesInfo_Failed = true;
                return;
            }

            HotelServer = HotelEndPoint.Parse(hHost, hPort);

            try
            {
                MessagesInfoIncoming = new List<HMessage>();
                MessagesInfoOutgoing = new List<HMessage>();
                Out = new Outgoing(new List<HMessage>());
                In = new Incoming(new List<HMessage>());
                int MessagesInfoLenght = packet.ReadInt32();
                foreach (var i in Enumerable.Range(0, MessagesInfoLenght))
                {
                    int CurrentMessageID = packet.ReadInt32();
                    string CurrentMessageHash = packet.ReadUTF8();
                    string CurrentMessageName = packet.ReadUTF8();
                    string CurrentMessageStructure = packet.ReadUTF8();
                    bool CurrentMessageIsOutgoing = packet.ReadBoolean();
                    string CurrentMessageSource = packet.ReadUTF8();
                    if (string.IsNullOrWhiteSpace(CurrentMessageHash) || CurrentMessageHash == "NULL")
                    {
                        CurrentMessageHash = CurrentMessageName;
                    }
                    CurrentMessageHash = CurrentMessageSource + "_" + CurrentMessageHash;
                    HMessage CurrentHMessage = new HMessage((ushort)CurrentMessageID, CurrentMessageHash, CurrentMessageName, CurrentMessageStructure);
                    if (CurrentMessageIsOutgoing)
                    {
                        MessagesInfoOutgoing.Add(CurrentHMessage);
                    }
                    else
                    {
                        MessagesInfoIncoming.Add(CurrentHMessage);
                    }
                }
                List<HMessage> GeodeOut = new List<HMessage>();
                List<HMessage> GeodeIn = new List<HMessage>();
                foreach (PropertyInfo GeodeOutProperty in Out.GetType().GetProperties())
                {
                    try
                    {
                        if (GeodeOutProperty.PropertyType == typeof(HMessage))
                        {
                            GeodeOut.Add(MessagesInfoOutgoing.First(x => x.Name == GeodeOutProperty.Name));
                        }
                    }
                    catch { Console.WriteLine("MessageInfo not found for: " + GeodeOutProperty.Name); }
                }
                foreach (PropertyInfo GeodeInProperty in In.GetType().GetProperties())
                {
                    try
                    {
                        if (GeodeInProperty.PropertyType == typeof(HMessage))
                        {
                            GeodeIn.Add(MessagesInfoIncoming.First(x => x.Name == GeodeInProperty.Name));
                        }
                    }
                    catch { Console.WriteLine("MessageInfo not found for: " + GeodeInProperty.Name); }
                }
                Out = new Outgoing(GeodeOut);
                In = new Incoming(GeodeIn);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Critical MessagesInfo exception: " + ex.Message);
                MessagesInfo_Failed = true;
            }

            ResolveCallbacks();
        }
        public virtual void OnDisconnected(HPacket packet)
        {
            _entities.Clear();
            _wallItems.Clear();
            _floorItems.Clear();
            _inDataAttributes.Clear();
            _outDataAttributes.Clear();
        }
        public virtual void OnCriticalError(string error_desc)
        {
            Dispose();
        }

        public Task<int> SendToClientAsync(byte[] data)
        {
            return _installer.SendPacketAsync(SEND_MESSAGE, false, data.Length, data);
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
            return _installer.SendPacketAsync(SEND_MESSAGE, true, data.Length, data);
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
                Console.WriteLine(new MessagesResolveException(ClientVersion, unresolved));
            }
        }
        private async Task HandleInstallerDataAsync()
        {
            await Task.Yield();
            try
            {
                HPacket packet = await _installer.ReceivePacketAsync().ConfigureAwait(true);
                if (packet == null) { OnCriticalError("Empty packet input"); return; }

                Task handleInstallerDataTask = HandleInstallerDataAsync();
                if (_extensionEvents.TryGetValue(packet.Id, out Action<HPacket> handler))
                {
                    handler(packet);
                }
            }
            catch { OnCriticalError("Wrong packet input"); return; }
        }
        private void HandleGameObjects(HPacket packet, bool isOutgoing)
        {
            packet.Position = 0;
            if (!isOutgoing)
            {
                if (packet.Id == In.Users)
                {
                    HEntity[] entities = HEntity.Parse(packet);
                    foreach (HEntity entity in entities)
                    {
                        _entities[entity.Index] = entity;
                    }
                    _container.OnEntitiesLoaded(entities.Length);
                }
                else if (packet.Id == In.Items)
                {
                    HWallItem[] wallItems = HWallItem.Parse(packet);
                    foreach (HWallItem wallItem in wallItems)
                    {
                        _wallItems[wallItem.Id] = wallItem;
                    }
                    _container.OnWallItemsLoaded(wallItems.Length);
                }
                else if (packet.Id == In.Objects)
                {
                    HFloorItem[] floorItems = HFloorItem.Parse(packet);
                    foreach (HFloorItem floorItem in floorItems)
                    {
                        _floorItems[floorItem.Id] = floorItem;
                    }
                    _container.OnFloorItemsLoaded(floorItems.Length);
                }
                else if (packet.Id == In.FloorHeightMap)
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
        public void Dispose()
        {
            try { Dispose(true); } catch { Console.WriteLine("WARNING: Dispose event failed."); }
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _installer.Dispose();
            }
            _container.OnDisconnected(null);
            _unknownDataAttributes.Clear();
        }
    }
}