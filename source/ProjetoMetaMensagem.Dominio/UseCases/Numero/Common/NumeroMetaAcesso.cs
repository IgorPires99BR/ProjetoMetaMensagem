using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.Common
{
    // Carrega o numero ja recortado pela empresa do token e resolve o acesso a Meta dele.
    // Compartilhado pelos casos de uso de perfil/nome do numero, que chamam a Meta com o
    // phone_number_id (InstanciaId) -- a checagem de dono tem que vir ANTES dessa chamada.
    public static class NumeroMetaAcesso
    {
        public const string MensagemCoexistencia =
            "Este número usa coexistência com o app WhatsApp Business: o nome exibido é alterado pelo próprio app no celular.";

        public static async Task<(Entidades.Numero? Numero, string? Erro)> CarregarAsync(IUnitOfWork unitOfWork, Guid numeroId, Guid? empresaIdSolicitante)
        {
            var numero = await unitOfWork.Numero.ObterPorIdEEmpresa(numeroId, empresaIdSolicitante);

            if (numero == null)
                return (null, "Número não encontrado.");

            if (string.IsNullOrEmpty(numero.InstanciaId))
                return (null, "Este número ainda não tem o identificador da Meta. Clique em \"Sincronizar Meta\" e tente de novo.");

            return (numero, null);
        }

        // Empresa dona do numero, e nao a do token: a conta de plataforma (escopo nulo) tambem
        // edita numeros de clientes, e o token/App ID tem que ser o da empresa do numero.
        public static async Task<Guid?> EmpresaDoNumeroAsync(IUnitOfWork unitOfWork, Entidades.Numero numero)
        {
            var dono = await unitOfWork.Usuario.ObterPorId(numero.UsuarioId);
            return dono?.EmpresaId;
        }

        // Mesmo criterio da coexistencia: token proprio do numero (Embedded Signup) ou o da empresa.
        public static async Task<string?> ObterTokenAsync(IUnitOfWork unitOfWork, Entidades.Numero numero)
        {
            if (!string.IsNullOrEmpty(numero.SystemUserToken))
                return numero.SystemUserToken;

            var empresaId = await EmpresaDoNumeroAsync(unitOfWork, numero);
            return empresaId.HasValue ? await unitOfWork.Empresa.ObterMetaAccessToken(empresaId.Value) : null;
        }

        public static bool EhCoexistencia(Entidades.Numero numero) => numero.TipoConexao == TipoConexaoNumero.Coexistencia;
    }
}
