using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IMetaService
    {
        Task<bool> EnviarTemplateAsync(string celulcar, string nomeTemplate);
    }
}
