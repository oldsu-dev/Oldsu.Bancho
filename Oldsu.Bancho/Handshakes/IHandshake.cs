using System.Threading.Tasks;
using Oldsu.Bancho.Connections;

namespace Oldsu.Bancho.Handshakes
{
    public interface IHandshake
    {
        void Execute(AuthenticatedConnection authenticatedConnection);
    }
}