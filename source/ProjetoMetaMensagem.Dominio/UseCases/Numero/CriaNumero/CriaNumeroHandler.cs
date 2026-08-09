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
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero
{
    public class CriaNumeroHandler : IRequestHandler<CriaNumeroCommand, Response<CriaNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<CriaNumeroHandler> _logger;

        public CriaNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<CriaNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<CriaNumeroResult>> Handle(CriaNumeroCommand command)
        {
            var response = new Response<CriaNumeroResult>();

            var validator = new CriaNumeroValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                _unitOfWork.BeginTransaction();

                // Numero nao tem EmpresaId proprio: pertence a empresa do UsuarioId gravado nele.
                // O EmpresaAccessFilter ja garante que IdEmpresa bate com o token de quem chama
                // (ou libera admin), mas nao tem como saber que UsuarioId aqui precisa ser da
                // MESMA empresa -- sem essa checagem, o numero cai na empresa dona do UsuarioId
                // informado, nao necessariamente a IdEmpresa declarada.
                var usuarioDoNumero = await _unitOfWork.Usuario.ObterPorId(command.UsuarioId);
                if (usuarioDoNumero == null || usuarioDoNumero.EmpresaId != command.IdEmpresa)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Usuário não encontrado.");
                    return response;
                }

                // 2. Envia a criação/vinculação do número para a Meta
                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                var phoneNumberId = await _metaService.CriarNumeroMetaAsync(command.NumeroTelefone, command.NomeEmpresa, "55", wabaId, token);

                if (string.IsNullOrEmpty(phoneNumberId))
                {
                    response.AddErro("A Meta aceitou a requisição, mas não retornou um identificador válido.");
                    return response;
                }

                var novoNumero = new Entidades.Numero
                {
                    Id = Guid.NewGuid(),
                    UsuarioId = command.UsuarioId,
                    Telefone = command.NumeroTelefone,
                    Descricao = command.NomeEmpresa, // Usando o nome verificado como descrição inicial
                    InstanciaId = phoneNumberId,          // O Phone Number ID retornado pela Meta
                    StatusMeta = "PENDING",             // Status inicial padrão de onboarding
                    QualidadeMeta = "UNKNOWN",          // Qualidade inicial até a Meta analisar o chip
                    DataCriacao = DateTime.Now
                };

                // 4. Salva no banco de dados utilizando seu Unit of Work
                await _unitOfWork.Numero.Incluir(novoNumero);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Captura falhas de comunicação com a API ou erros internos do banco
                response.AddErroServico(ex, _logger, nameof(CriaNumeroHandler));
            }

            return response;
        }
    }
}


