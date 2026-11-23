using ControleEstoque.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

/// <summary>
/// Home do administrador. O colaborador é levado para a área dele.
/// </summary>
public class HomeController : BaseController
{
    public IActionResult Index()
    {
        if (User.IsInRole(Perfis.Colaborador))
            return RedirectToAction("Index", "AreaColaborador");

        var resumo = new ConsultaDAO().ResumoAdmin();
        ViewBag.TotalKits = resumo["TotalKits"];
        ViewBag.KitsMontados = resumo["KitsMontados"];
        ViewBag.TotalColaboradores = resumo["TotalColaboradores"];
        ViewBag.ColaboradoresSemKit = resumo["ColaboradoresSemKit"];

        ViewBag.UltimosKits = new KitDAO().Listagem().Take(5).ToList();
        ViewBag.Produtos = HelperControllers.ComboProdutos("Selecione o produto...", mostrarEstoque: true);

        return View(new ProdutoDAO().Listagem());
    }

    [AllowAnonymous]
    public IActionResult Sobre()
    {
        return View();
    }

    [AllowAnonymous]
    public IActionResult Error(int? codigo)
    {
        ViewBag.Codigo = codigo;
        ViewBag.IdRequisicao = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        return View();
    }
}
