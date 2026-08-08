using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;

namespace ProjetoMetaMensagem.Dominio.Help.Error
{
    // Converte exceção técnica em mensagem de negócio.
    // O texto original de uma SqlException traz nome de tabela, coluna e constraint, então
    // ele nunca pode ir para a resposta HTTP: fica só no log do servidor.
    public static class TratamentoErro
    {
        public const string MensagemGenerica = "Não foi possível concluir a operação. Tente novamente.";

        // Traduz a exceção (ou a SqlException aninhada nela) para uma mensagem em português.
        public static string Traduzir(Exception excecao)
        {
            var sql = ObterSqlException(excecao);

            if (sql == null)
                return MensagemGenerica;

            switch (sql.Number)
            {
                // 8152/2628: "String or binary data would be truncated" - valor maior que a coluna.
                case 8152:
                case 2628:
                    return "Um dos campos informados excede o tamanho permitido.";

                // 547: conflito com FOREIGN KEY. No DELETE quer dizer que existem filhos
                // apontando para o registro; nas demais instruções, o vínculo enviado não existe.
                case 547:
                    return sql.Message.IndexOf("DELETE", StringComparison.OrdinalIgnoreCase) >= 0
                        ? "Não é possível excluir: existem registros vinculados a este item."
                        : "Existe um vínculo inválido entre os registros informados.";

                // 2627/2601: violação de chave primária ou de índice único.
                case 2627:
                case 2601:
                    return "Já existe um registro com esses dados.";

                // 515: tentativa de gravar NULL em coluna obrigatória.
                case 515:
                    return "Preencha todos os campos obrigatórios.";

                // 8114/245/241: valor enviado incompatível com o tipo da coluna.
                case 8114:
                case 245:
                case 241:
                    return "Um dos campos informados está em formato inválido.";

                default:
                    return MensagemGenerica;
            }
        }

        // Guarda o detalhe técnico no log do servidor. O logger é opcional porque alguns
        // pontos de chamada ainda não têm um injetado.
        public static void Registrar(ILogger? logger, Exception excecao, string operacao)
        {
            logger?.LogError(excecao, "Falha em {Operacao}", operacao);
        }

        // Atalho para o padrão usado nos handlers e controllers: loga o detalhe e devolve
        // a mensagem que pode ser exibida ao usuário.
        public static string Tratar(Exception excecao, ILogger? logger, string operacao)
        {
            Registrar(logger, excecao, operacao);
            return Traduzir(excecao);
        }

        private static SqlException? ObterSqlException(Exception excecao)
        {
            for (var atual = excecao; atual != null; atual = atual.InnerException)
            {
                if (atual is SqlException sql)
                    return sql;
            }

            return null;
        }
    }
}
