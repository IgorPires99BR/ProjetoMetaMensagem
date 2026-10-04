using ProjetoMetaMensagem.Dominio.Entidades;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Empresa nova nasce disparando pelo numero/WABA da Contact Solution. As credenciais sao
    // COPIADAS para a Empresa (nao lidas da Contact Solution a cada envio): assim o resto do
    // sistema continua lendo da propria empresa e ela ainda pode ganhar conta Meta propria
    // depois (edicao ou "Conectar ao WhatsApp") sem mudar nada no disparo.
    public interface ICredenciaisMetaDaPlataforma
    {
        // So preenche o que veio em branco: credencial digitada no cadastro prevalece.
        Task PreencherEmBranco(Empresa empresa);
    }
}
