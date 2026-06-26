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
            try
            {
                var conversations = await _unitOfWork.ConversationState.ObterPorFlow(flowId);
                return Ok(conversations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { erro = ex.Message, detalhe = ex.InnerException?.Message });
            }
        }
    }
}
