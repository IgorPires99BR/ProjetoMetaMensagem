namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    // Mapeia o id da oferta na Cakto para o plano da plataforma. Configurado em
    // CaktoConfiguration:Ofertas ("B8BcHrY": "PRO"), porque o id é o único dado do evento que
    // não muda quando alguém renomeia a oferta no painel.
    public interface IConfiguracaoOfertasCakto
    {
        string? PlanoDaOferta(string ofertaId);
    }
}
