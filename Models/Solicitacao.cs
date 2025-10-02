using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGSC.Models
{
    public class Solicitacao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Serviço")]
        public int ServicoId { get; set; }  // chave estrangeira

        [ForeignKey("ServicoId")]
        public Servico? Servico { get; set; } // navegação

        [Required]
        [StringLength(500)]
        public string Descricao { get; set; }

        [Required]
        [Display(Name = "Número de Protocolo")]
        public string NumeroProtocolo { get; set; }

        public DateTime DataCriacao { get; set; } = DateTime.Now;

        // Relacionamento com o usuário do Identity (armazenamos o Id)
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public IdentityUser? User { get; set; }
    }
}
