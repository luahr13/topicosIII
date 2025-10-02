using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SGSC.Data;
using SGSC.Models;

namespace SGSC.Controllers
{
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
            var applicationDbContext = _context.Solicitacoes.Include(s => s.Servico);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Solicitacaos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var solicitacao = await _context.Solicitacoes
                .Include(s => s.Servico)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitacao == null)
            {
                return NotFound();
            }

            return View(solicitacao);
        }

        // GET: Solicitacaos/Create
        public IActionResult Create()
        {
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome");
            return View();
        }

        // POST: Solicitacaos/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

            var solicitacao = await _context.Solicitacoes.FindAsync(id);
            if (solicitacao == null)
            {
                return NotFound();
            }
            ViewData["ServicoId"] = new SelectList(_context.Servicos, "Id", "Nome", solicitacao.ServicoId);
            return View(solicitacao);
        }

        // POST: Solicitacaos/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ServicoId,Descricao,NumeroProtocolo,DataCriacao")] Solicitacao solicitacao)
        {
            if (id != solicitacao.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(solicitacao);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SolicitacaoExists(solicitacao.Id))
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

            var solicitacao = await _context.Solicitacoes
                .Include(s => s.Servico)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (solicitacao == null)
            {
                return NotFound();
            }

            return View(solicitacao);
        }

        // POST: Solicitacaos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var solicitacao = await _context.Solicitacoes.FindAsync(id);
            if (solicitacao != null)
            {
                _context.Solicitacoes.Remove(solicitacao);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SolicitacaoExists(int id)
        {
            return _context.Solicitacoes.Any(e => e.Id == id);
        }
    }
}
