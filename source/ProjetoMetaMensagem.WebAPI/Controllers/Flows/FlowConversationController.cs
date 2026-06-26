using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Interfaces;

namespace ProjetoMetaMensagem.WebAPI.Controllers.Flows
{
    [ApiController]
    [Route("api/config/flow")]
    public class FlowConversationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public FlowConversationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET api/config/flow/{flowId}/conversations
        // Lista contatos ativos em determinado flow
        [HttpGet("{flowId}/conversations")]
        public async Task<IActionResult> ListarConversations(Guid flowId)
        {
            var conversations = await _unitOfWork.ConversationState.ObterPorFlow(flowId);
            return Ok(conversations);
        }
    }
}
