using System.Security.Claims;
using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

/// <summary>Área do cliente: o colaborador acompanha o próprio kit de onboarding.</summary>
[Authorize(Roles = Perfis.Colaborador)]
public class AreaColaboradorController : BaseController
{
    /// <summary>Home da área do colaborador.</summary>
    public IActionResult Index()
    {
        Colaborador colaborador = new ColaboradorDAO().ConsultaPorFuncional(User.FindFirstValue("Funcional"));
        if (colaborador == null)
            return RedirectToAction("Sair", "Login");

        ViewBag.Kit = new KitDAO().ConsultaPorColaborador(colaborador.Id);
        return View(colaborador);
    }

    [HttpPost]
    public IActionResult ConfirmarRecebimento()
    {
        int colaboradorId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        Tentar(() =>
        {
            var dao = new KitDAO();
            Kit kit = dao.ConsultaPorColaborador(colaboradorId)
                ?? throw new EstoqueException("Você ainda não tem um kit montado.");
            dao.Entregar(kit.Id);
            return "Recebimento confirmado. Boas-vindas!";
        });
        return RedirectToAction(nameof(Index));
    }
}
