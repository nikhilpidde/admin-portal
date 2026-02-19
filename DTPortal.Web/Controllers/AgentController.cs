using DTPortal.Core.Domain.Services;
using DTPortal.Core.DTOs;
using DTPortal.Core.Utilities;
using DTPortal.Web.ViewModel;
using DTPortal.Web.ViewModel.Agent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DTPortal.Web.Controllers
{
    [Authorize]
    public class AgentController : BaseController
    {
        private readonly IAgentService _agentService;

        public AgentController(IAgentService agentService,
            IServiceDefinitionService serviceDefinitionService,
            ILogClient logClient) : base(logClient)
        {
            _agentService = agentService;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            var AgentList = await _agentService.GetAllOnboardingAgent();

            if (AgentList == null)
            {
                return View();
            }

            AgentListViewModel viewModel = new AgentListViewModel
            {
                Agents = AgentList
            };

            return View(viewModel);

        }

        [HttpGet]
        public IActionResult Add()
        {
            AgentAddViewModel viewModel = new AgentAddViewModel();
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AgentAddViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            List<AgentDTO> agentList = new List<AgentDTO>();

            AgentDTO agent = new AgentDTO()
            {
                idType = viewModel.IdentifierType.ToString(),
                id = viewModel.IdentifierValue
            };
            agentList.Add(agent);

            var response = await _agentService.AddOnboardingAgent(agentList);

            if (!response.Success)
            {
                AlertViewModel alert = new AlertViewModel { Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return View(viewModel);
            }
            else
            {
                AlertViewModel alert = new AlertViewModel { IsSuccess = true, Message = response.Message };
                TempData["Alert"] = JsonConvert.SerializeObject(alert);
                return RedirectToAction("List");
            }

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var agent = await _agentService.GetAgentById(id);

            if (agent == null)
            {
                return NotFound();
            }

            var model = new AgentEditViewModel
            {
                Name = agent.Name,
                Email = agent.Email,
                MobileNumber = agent.MobileNumber
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Status(int id)
        {
            var agent = await _agentService.AgentStatus(id);

            if (!agent.Success)
            {
                return Json(new { Status = "Failed", Title = "delegation", Message = agent.Message });
            }
            else
            {
                return Json(new { Status = "Success", Title = "Delegation", Message = agent.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var agent = await _agentService.DeleteAgent(id);

            if (!agent.Success)
            {
                return Json(new { Status = "Failed", Title = "delegation", Message = agent.Message });
            }
            else
            {
                return Json(new { Status = "Success", Title = "Delegation", Message = agent.Message });
            }
        }
    }
}
