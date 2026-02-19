using DTPortal.Core.Domain.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace DTPortal.Web.Hubs
{
    public class DataHub : Hub
    {
        public async Task SendOrgStats(string data)
        {
            await Clients.All.SendAsync("OrgStats", data);
        }
    }
}
