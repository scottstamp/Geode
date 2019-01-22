using System;
using System.Net;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

using Geode.Habbo;
using Geode.Network;
using Geode.Habbo.Messages;
using Geode.Network.Protocol;
using Geode.Extension.Windows.Helpers;

namespace Geode.Extension.Windows
{
    public class ExtensionForm : Form, IExtension, INotifyPropertyChanged
    {
        private readonly GService _service;
        private readonly Dictionary<string, Binding> _bindings;

        public Incoming In => _service.In;
        public Outgoing Out => _service.Out;
        public IHConnection Connection => _service;
        public string Revision => _service.Revision;
        public HotelEndPoint HotelServer => _service.HotelServer;

        public IReadOnlyDictionary<int, HEntity> Entities => _service.Entities;
        public IReadOnlyDictionary<int, HWallItem> WallItems => _service.WallItems;
        public IReadOnlyDictionary<int, HFloorItem> FloorItems => _service.FloorItems;

        public ExtensionForm()
            : this(null)
        { }
        public ExtensionForm(IPEndPoint moduleServer)
        {
            _bindings = new Dictionary<string, Binding>();
            _service = new GService(this, moduleServer);
        }

        public HMessages GetMessages(bool isOutgoing) => _service.GetMessages(isOutgoing);
        public HMessage GetMessage(ushort id, bool isOutgoing) => _service.GetMessage(id, isOutgoing);
        public HMessage GetMessage(string identifier, bool isOutgoing) => _service.GetMessage(identifier, isOutgoing);

        void IExtension.OnEntitiesLoaded(int count)
        {
            _service.OnEntitiesLoaded(count);
            OnEntitiesLoaded(count);
        }
        public virtual void OnEntitiesLoaded(int count)
        { }

        void IExtension.OnWallItemsLoaded(int count)
        {
            _service.OnWallItemsLoaded(count);
            OnWallItemsLoaded(count);
        }
        public virtual void OnWallItemsLoaded(int count)
        { }

        void IExtension.OnFloorItemsLoaded(int count)
        {
            _service.OnFloorItemsLoaded(count);
            OnFloorItemsLoaded(count);
        }
        public virtual void OnFloorItemsLoaded(int count)
        { }

        void IExtension.OnFlagsCheck(HPacket packet)
        {
            _service.OnFlagsCheck(packet);
            OnFlagsCheck(packet);
        }
        public virtual void OnFlagsCheck(HPacket packet)
        { }

        void IExtension.OnDoubleClick(HPacket packet)
        {
            _service.OnDoubleClick(packet);
            OnDoubleClick();
        }
        public virtual void OnDoubleClick()
        {
            BringToFront();
        }

        void IExtension.OnInfoRequest(HPacket packet)
        {
            _service.OnInfoRequest(packet);
            OnInfoRequest();
        }
        public virtual void OnInfoRequest()
        { }

        void IExtension.OnPacketIntercept(HPacket packet)
        {
            _service.OnPacketIntercept(packet);
            OnPacketIntercept(packet);
        }
        public virtual void OnPacketIntercept(HPacket packet)
        { }

        void IExtension.OnInitialized(HPacket packet)
        {
            _service.OnInitialized(packet);
            OnInitialized();
        }
        public virtual void OnInitialized()
        { }

        void IExtension.OnConnected(HPacket packet)
        {
            _service.OnConnected(packet);
            OnConnected();
        }
        public virtual void OnConnected()
        { }

        void IExtension.OnDisconnected(HPacket packet)
        {
            _service.OnDisconnected(packet);
        }
        public virtual void OnDisconnected()
        { }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            if (Owner != null)
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(Owner.Location.X + Owner.Width / 2 - Width / 2, Owner.Location.Y + Owner.Height / 2 - Height / 2);
            }
        }
        protected void Bind(IBindableComponent component, string propertyName, string dataMember, IValueConverter converter = null, DataSourceUpdateMode dataSourceUpdateMode = DataSourceUpdateMode.OnPropertyChanged)
        {
            var binding = new CustomBinding(propertyName, this, dataMember, converter)
            {
                DataSourceUpdateMode = dataSourceUpdateMode,
                ControlUpdateMode = ControlUpdateMode.OnPropertyChanged
            };
            component.DataBindings.Add(binding);
            _bindings[dataMember] = binding;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                FindForm()?.Invoke(handler, this, e);
            }
            if (DesignMode)
            {
                _bindings[e.PropertyName].ReadValue();
            }
        }
        protected void RaiseOnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}