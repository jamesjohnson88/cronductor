using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Cronductor
{
    public class LogHub : Hub
    {
        // This hub will be used to push log updates to clients
        public async Task SendLog(string message)
        {
            await Clients.All.SendAsync("ReceiveLog", message);
        }
    }
}

