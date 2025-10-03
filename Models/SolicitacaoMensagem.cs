namespace SGSC.Models
{
    public class SolicitacaoMensagem
    {
        public int Id { get; set; }
        public int SolicitacaoId { get; set; }
        public string UserId { get; set; } // Quem enviou
        public string Mensagem { get; set; }
        public DateTime DataEnvio { get; set; }

        public Solicitacao Solicitacao { get; set; }
    }
}
