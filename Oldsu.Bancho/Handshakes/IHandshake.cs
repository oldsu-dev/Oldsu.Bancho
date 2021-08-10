using System.Threading.Tasks;
using Oldsu.Bancho.Connections;

namespace Oldsu.Bancho.Handshakes
{
    public interface IHandshake
    {
        Task Execute(AuthenticatedConnection authenticatedConnection);
    }
}