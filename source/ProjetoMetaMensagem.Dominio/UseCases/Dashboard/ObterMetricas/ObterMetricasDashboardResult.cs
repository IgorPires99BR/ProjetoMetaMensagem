namespace ProjetoMetaMensagem.Dominio.UseCases.Dashboard.ObterMetricas
{
    public class ObterMetricasDashboardResult
    {
        public int MensagensHoje { get; set; }
        public int MensagensSemana { get; set; }
        public int MensagensMes { get; set; }
        public double TaxaEntrega { get; set; }
        public int LeadsCapturados { get; set; }
        public int ChatsAtivos { get; set; }
        public int NumerosAtivos { get; set; }
        public int NumerosPendentes { get; set; }
        public int NumerosBloqueados { get; set; }
        public int FlowsAtivos { get; set; }
        public List<DisparoRecente> DisparosRecentes { get; set; } = new();
        public List<FlowAtivo> FlowsComExecucoes { get; set; } = new();
        public List<EvolucaoDisparo> EvolucaoDisparos { get; set; } = new();
    }

    public class DisparoRecente
    {
        public string Nome { get; set; }
        public int Enviadas { get; set; }
        public int Total { get; set; }
        public string Status { get; set; }
    }

    public class FlowAtivo
    {
        public string Nome { get; set; }
        public int DisparosHoje { get; set; }
        public bool Ativo { get; set; }
    }

    public class EvolucaoDisparo
    {
        public string Data { get; set; }
        public int Total { get; set; }
    }
}
