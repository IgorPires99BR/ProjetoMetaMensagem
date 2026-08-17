namespace ProjetoMetaMensagem.Servico.Configuration
{
    // Links de checkout da Cakto por plano, usados pelo Flow quando o lead escolhe um plano
    // por botao (etapa "Capturar Input" com Botao1/Botao2). Tem default hardcoded porque sao
    // os links publicos da pagina de vendas, nao segredo -- funciona sem precisar configurar
    // nada no Render; PlanosConfiguration:LinkStarter/LinkPro sobrescreve se a oferta mudar.
    public class PlanosConfiguration
    {
        public string LinkStarter { get; set; } = "https://pay.cakto.com.br/pw7sssc_1045806";
        public string LinkPro { get; set; } = "https://pay.cakto.com.br/qftc9dx";
    }
}
