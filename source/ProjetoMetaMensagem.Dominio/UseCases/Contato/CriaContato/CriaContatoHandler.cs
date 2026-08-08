using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoHandler : IRequestHandler<CriaContatoCommand, Response<CriaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaContatoHandler> _logger;

        public CriaContatoHandler(IUnitOfWork unitOfWork, ILogger<CriaContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaContatoResult>> Handle(CriaContatoCommand command)
        {
            var response = new Response<CriaContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new CriaContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var usuario = await _unitOfWork.Usuario.ObterPorId(command.UsuarioId);

                // Contato nao tem EmpresaId proprio: pertence a empresa do UsuarioId informado.
                // Se quem chama nao e admin, esse usuario precisa ser da mesma empresa de quem
                // esta autenticado -- senao o UsuarioId do corpo deixa qualquer um escolher em
                // que empresa o contato cai.
                if (command.EmpresaIdSolicitante.HasValue
                    && (usuario == null || usuario.EmpresaId != command.EmpresaIdSolicitante.Value))
                {
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                // Evita duplicar contato com o mesmo telefone na mesma empresa (ja aconteceu
                // com dados de teste e quebrava o agrupamento de mensagens no chat, deixando
                // conversas do mesmo lead espalhadas em duas linhas diferentes).
                if (usuario != null)
                {
                    var existente = await _unitOfWork.Contato.ObterPorTelefone(usuario.EmpresaId, command.Telefone);
                    if (existente != null)
                    {
                        response.AddErro("Já existe um contato cadastrado com esse telefone.");
                        return response;
                    }
                }

                await _unitOfWork.Contato.Incluir(new Entidades.Contato(command));

                response.AddValue(new CriaContatoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(CriaContatoHandler)));
            }

            return response;
        }
    }
}

