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

        public CriaContatoEmLoteHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                    DataCriacao = DateTime.UtcNow
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

                response.AddErro($"Falha ao salvar lote de contatos no banco: {ex.Message}");
            }

            return response;
        }
    }
}

