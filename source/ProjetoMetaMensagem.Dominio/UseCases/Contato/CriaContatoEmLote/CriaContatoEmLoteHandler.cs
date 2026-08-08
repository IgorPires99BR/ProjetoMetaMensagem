using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContatoEmLote
{
    public class CriaContatoEmLoteHandler : IRequestHandler<CriaContatoEmLoteCommand, Response<CriaContatoEmLoteResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaContatoEmLoteHandler> _logger;

        public CriaContatoEmLoteHandler(IUnitOfWork unitOfWork, ILogger<CriaContatoEmLoteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaContatoEmLoteResult>> Handle(CriaContatoEmLoteCommand command)
        {
            var response = new Response<CriaContatoEmLoteResult>();

            // 1. Validação simples do lote básico
            if (command.Contatos == null || !command.Contatos.Any())
            {
                response.AddErro("A lista de contatos não pode estar vazia.");
                return response;
            }

            if (command.IdEmpresa == Guid.Empty)
            {
                response.AddErro("O Id da Empresa é obrigatório para importação em lote.");
                return response;
            }

            // Contato nao tem EmpresaId proprio: pertence a empresa do UsuarioId gravado nele.
            // O EmpresaAccessFilter ja garante que IdEmpresa bate com o token de quem chama (ou
            // libera admin), mas nao tem como saber que UsuarioId aqui precisa ser da MESMA
            // empresa -- sem essa checagem, o lote inteiro cai na empresa dona do UsuarioId
            // informado, nao necessariamente a IdEmpresa declarada.
            var usuarioDoLote = await _unitOfWork.Usuario.ObterPorId(command.UsuarioId);
            if (usuarioDoLote == null || usuarioDoLote.EmpresaId != command.IdEmpresa)
            {
                response.AddErro("Usuário não encontrado.");
                return response;
            }

            var contatosSalvar = new List<Entidades.Contato>();

            // 2. Mapeia e valida individualmente as strings se necessário
            foreach (var item in command.Contatos)
            {
                if (string.IsNullOrWhiteSpace(item.Telefone))
                    continue; // Ignora registros inválidos vazios ou adiciona erro no response

                // Instancia a sua entidade base de contatos passando as informações dinamicamente
                var novoContato = new Entidades.Contato
                {
                    UsuarioId = command.UsuarioId,
                    Nome = item.Nome,
                    Telefone = item.Telefone,
                    // Hora local, igual ao resto do sistema (a criacao de contato individual
                    // usa DateTime.Now): misturar os dois deixava a data do contato importado
                    // adiantada em relacao ao cadastrado pela tela.
                    DataCriacao = DateTime.Now
                };

                contatosSalvar.Add(novoContato);
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 3. Salva em lote no banco usando a UnitOfWork
                foreach (var contato in contatosSalvar)
                {
                    await _unitOfWork.Contato.Incluir(contato);
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();

                response.AddErroServico(ex, _logger, nameof(CriaContatoEmLoteHandler));
            }

            return response;
        }
    }
}

