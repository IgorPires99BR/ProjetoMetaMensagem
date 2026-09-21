using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Estrutura de acesso modular: um Perfil pertence a uma Empresa e define, via Telas
    // (PerfilTela.TelaChave), quais telas do menu os usuarios daquele perfil enxergam.
    // TelaChave bate com o "id" de cada item em shared/menu.ts no Angular.
    public class Perfil
    {
        public Perfil()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public string Nome { get; set; }
        public DateTime DataCriacao { get; set; }

        // Preenchido pelo repositorio junto com o Perfil (consulta separada, Dapper nao
        // faz array/JSON nativo). Vazio = perfil sem nenhuma tela liberada (por engano do
        // formulario, na pratica), nao "sem restricao".
        public List<string> Telas { get; set; } = new();
    }
}
