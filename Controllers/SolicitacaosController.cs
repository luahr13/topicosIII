using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGSC.Data;
using SGSC.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGSC.Controllers
{
    [Authorize]
    public class SolicitacaosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SolicitacaosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Solicitacaos
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verifica se o usuário está na role Administrador
            if (User.IsInRole("Administrador"))
            {
                // Admin vê todas as solicitações
                var todasSolicitacoes = _context.Solicitacoes
                                                .Include(s => s.Servico);
                return View(await todasSolicitacoes.ToListAsync());
            }
            else
            {
                // Cidadão vê apenas suas próprias solicitações
                var solicitacoesDoUsuario = _context.Solicitacoes
                                                   .Where(s => s.UserId == userId)
                                                   .Include(s => s.Servico);
                return View(await solicitacoesDoUsuario.ToListAsync());
            }
        }

        // GET: Solicitacaos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var solicitacao = await _context.Solicitacoes
                .Include(s => s.Servico)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (solicitacao == null)
                return NotFound();

            // Carrega mensagens relacionadas
            var mensagens = await _context.SolicitacaoMensagens
                .Where(m => m.SolicitacaoId == id)
                .OrderBy(m => m.DataEnvio)
                .Select(m => new
                {
                    m.Id,
                    m.Mensagem,
                    m.DataEnvio,
                    UserEmail = _context.Users
                           .Where(u => u.Id == m.UserId)
                           .Select(u => u.Email)
                           .FirstOrDefault()
                })
                .ToListAsync();

            ViewBag.Mensagens = mensagens;

            return View(solicitacao);
        }

        // GET: Solicitacaos/Create
        public IActionResult Create()
        {
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome");
            return View();
        }

        // POST: Solicitacaos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ServicoId,Descricao,NumeroProtocolo")] Solicitacao solicitacao)
        {
            if (!ModelState.IsValid)
            {
                var erros = string.Join("; ", ModelState.Values
                                                 .SelectMany(v => v.Errors)
                                                 .Select(e => e.ErrorMessage));
                return Content("ModelState inválido: " + erros);
            }

            if (ModelState.IsValid)
            {
                // 🔹 vincula a solicitação ao usuário logado
                solicitacao.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                solicitacao.DataCriacao = DateTime.Now;

                _context.Add(solicitacao);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", solicitacao.ServicoId);
            return View(solicitacao);
        }

        // GET: Solicitacaos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Pega o ID do usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Busca a solicitação que pertence ao usuário
            var solicitacao = await _context.Solicitacoes
                                            .Where(s => s.UserId == userId)
                                            .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitacao == null)
            {
                return NotFound(); // ou Unauthorized() se preferir
            }

            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", solicitacao.ServicoId);
            return View(solicitacao);
        }

        // POST: Solicitacaos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServicoId,Descricao,NumeroProtocolo,DataCriacao")] Solicitacao solicitacao)
        {
            if (id != solicitacao.Id)
            {
                return NotFound();
            }

            // Pega o ID do usuário logado
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verifica se a solicitação realmente pertence ao usuário
            var solicitacaoExistente = await _context.Solicitacoes
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitacaoExistente == null)
            {
                return Unauthorized(); // não pode editar solicitação de outro usuário
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Mantém o UserId correto
                    solicitacao.UserId = userId;

                    _context.Update(solicitacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Solicitacoes.Any(e => e.Id == solicitacao.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", solicitacao.ServicoId);
            return View(solicitacao);
        }

        // GET: Solicitacaos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacao = await _context.Solicitacoes
                .Include(s => s.Servico)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId); // filtra pelo usuário logado

            if (solicitacao == null)
            {
                return Unauthorized(); // ou NotFound(), dependendo da UX que você deseja
            }

            return View(solicitacao);
        }

        // POST: Solicitacaos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacao = await _context.Solicitacoes
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId); // filtra pelo usuário logado

            if (solicitacao == null)
            {
                return Unauthorized(); // não pode deletar de outro usuário
            }

            _context.Solicitacoes.Remove(solicitacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //POST EEnviar Mensagem
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EnviarMensagem(int solicitacaoId, string mensagem)
        {
            if (string.IsNullOrWhiteSpace(mensagem))
                return RedirectToAction(nameof(Details), new { id = solicitacaoId });

            var conteudo = new SolicitacaoMensagem
            {
                SolicitacaoId = solicitacaoId,
                Mensagem = mensagem,
                DataEnvio = DateTime.Now,
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            };

            _context.Add(conteudo);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = solicitacaoId });
        }
    }
}
