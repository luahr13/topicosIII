using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SGSC.Models;

namespace SGSC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet para o modelo Serviço
        public DbSet<Servico> Servicos { get; set; } // Tabela Serviços
        public DbSet<Solicitacao> Solicitacoes { get; set; } // Tabela Solicitações
        public DbSet<SolicitacaoMensagem> SolicitacaoMensagens { get; set; } // Tabela Mensagens das Solicitações


    }
}
