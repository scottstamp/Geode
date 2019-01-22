using System.Threading.Tasks;

using Geode.Network.Protocol;

namespace Geode.Network
{
    public interface IHConnection
    {
        Task<int> SendToClientAsync(byte[] data);
        Task<int> SendToClientAsync(HPacket packet);
        Task<int> SendToClientAsync(ushort id, params object[] values);

        Task<int> SendToServerAsync(byte[] data);
        Task<int> SendToServerAsync(HPacket packet);
        Task<int> SendToServerAsync(ushort id, params object[] values);
    }
}