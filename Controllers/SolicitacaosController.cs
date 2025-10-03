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

            var solicitacoes = _context.Solicitacoes
                .Include(s => s.Servico)
                .AsQueryable();

            if (!User.IsInRole("Administrador"))
            {
                solicitacoes = solicitacoes.Where(s => s.UserId == userId);
            }

            var lista = await solicitacoes.ToListAsync();

            // Marcar status de nova mensagem
            var solicitacaoIds = lista.Select(s => s.Id).ToList();
            var leituras = await _context.SolicitacaoLeituras
                .Where(l => l.UserId == userId && solicitacaoIds.Contains(l.SolicitacaoId))
                .ToListAsync();

            var novasMensagensIds = await _context.SolicitacaoMensagens
                .Where(m => solicitacaoIds.Contains(m.SolicitacaoId))
                .GroupBy(m => m.SolicitacaoId)
                .Select(g => new
                {
                    SolicitacaoId = g.Key,
                    UltimaMensagem = g.Max(m => m.DataEnvio)
                })
                .ToListAsync();

            ViewBag.NovasMensagens = lista.ToDictionary(
                s => s.Id,
                s =>
                {
                    var ultimaMsg = novasMensagensIds.FirstOrDefault(n => n.SolicitacaoId == s.Id)?.UltimaMensagem;
                    var leitura = leituras.FirstOrDefault(l => l.SolicitacaoId == s.Id);
                    return leitura == null || (ultimaMsg != null && ultimaMsg > leitura.UltimaVisualizacao);
                });

            return View(lista);
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

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Atualiza a última visualização do usuário
            var leitura = await _context.SolicitacaoLeituras
                .FirstOrDefaultAsync(l => l.UserId == userId && l.SolicitacaoId == id);

            if (leitura == null)
            {
                leitura = new SolicitacaoLeitura
                {
                    UserId = userId,
                    SolicitacaoId = id.Value,
                    UltimaVisualizacao = DateTime.Now
                };
                _context.Add(leitura);
            }
            else
            {
                leitura.UltimaVisualizacao = DateTime.Now;
                _context.Update(leitura);
            }

            await _context.SaveChangesAsync();

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

            solicitacao.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            solicitacao.DataCriacao = DateTime.Now;

            _context.Add(solicitacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Solicitacaos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacao = await _context.Solicitacoes
                                            .Where(s => s.UserId == userId)
                                            .FirstOrDefaultAsync(s => s.Id == id);

            if (solicitacao == null)
                return NotFound();

            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", solicitacao.ServicoId);
            return View(solicitacao);
        }

        // POST: Solicitacaos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServicoId,Descricao,NumeroProtocolo,DataCriacao")] Solicitacao solicitacao)
        {
            if (id != solicitacao.Id)
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacaoExistente = await _context.Solicitacoes
                                                    .AsNoTracking()
                                                    .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitacaoExistente == null)
                return Unauthorized();

            if (ModelState.IsValid)
            {
                try
                {
                    solicitacao.UserId = userId;
                    _context.Update(solicitacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Solicitacoes.Any(e => e.Id == solicitacao.Id))
                        return NotFound();
                    else
                        throw;
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
                return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacao = await _context.Solicitacoes
                .Include(s => s.Servico)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (solicitacao == null)
                return Unauthorized();

            return View(solicitacao);
        }

        // POST: Solicitacaos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var solicitacao = await _context.Solicitacoes
                .FirstOrDefaultAsync(s => s.Id == id && s.UserId == userId);

            if (solicitacao == null)
                return Unauthorized();

            _context.Solicitacoes.Remove(solicitacao);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Enviar Mensagem
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

        // POST: Excluir Mensagem (Administrador)
        [HttpPost]
        [Authorize(Roles = "Administrador")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExcluirMensagem(int mensagemId, int solicitacaoId)
        {
            var mensagem = await _context.SolicitacaoMensagens
                                         .FirstOrDefaultAsync(m => m.Id == mensagemId);

            if (mensagem == null)
                return NotFound();

            _context.SolicitacaoMensagens.Remove(mensagem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Details), new { id = solicitacaoId });
        }
    }
}
