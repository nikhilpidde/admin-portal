using DTPortal.Core.DTOs;
using System.Collections.Generic;

namespace DTPortal.Web.ViewModel.Agent
{
    public class AgentListViewModel
    {
        public IEnumerable<AgentListDTO> Agents { get; set; }

        // public int Id { get; set; }
    }
}
