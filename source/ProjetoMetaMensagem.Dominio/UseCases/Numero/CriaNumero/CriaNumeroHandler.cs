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
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero
{
    public class CriaNumeroHandler : IRequestHandler<CriaNumeroCommand, Response<CriaNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        public CriaNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
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
                // 2. Monta a requisição de domínio e envia para a Meta
                var requisicaoMeta = new CriaNumeroMetaRequisicao
                {
                    Telefone = command.NumeroTelefone,
                    NomeVerificado = command.NomeEmpresa,
                    CodigoPais = "55"
                };

                var respostaMeta = await _metaService.CriarNumeroMetaAsync(requisicaoMeta);

                if (respostaMeta == null || string.IsNullOrEmpty(respostaMeta.Id))
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
                    InstanciaId = respostaMeta.Id,       // O Phone Number ID retornado pela Meta
                    StatusMeta = "PENDING",             // Status inicial padrão de onboarding
                    QualidadeMeta = "UNKNOWN",          // Qualidade inicial até a Meta analisar o chip
                    DataCriacao = DateTime.Now
                };

                // 4. Salva no banco de dados utilizando seu Unit of Work
                await _unitOfWork.Numero.Incluir(novoNumero);

            }
            catch (Exception ex)
            {
                // Captura falhas de comunicação com a API ou erros internos do banco
                response.AddErro($"Falha ao cadastrar número: {ex.Message}");
            }

            return response;
        }
    }
}
