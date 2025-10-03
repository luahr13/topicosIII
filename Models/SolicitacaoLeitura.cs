namespace SGSC.Models
{
    public class SolicitacaoLeitura
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int SolicitacaoId { get; set; }
        public DateTime UltimaVisualizacao { get; set; }

        public Solicitacao Solicitacao { get; set; }

    }
}
