using ControleEstoque.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

/// <summary>Entrada de unidades no estoque de um produto. O formulário fica na tela principal.</summary>
[Authorize(Roles = Perfis.Admin)]
public class EstoqueController : BaseController
{
    private readonly EstoqueDAO _dao = new EstoqueDAO();

    [HttpPost]
    public IActionResult Entrada(int produtoId, int quantidade)
    {
        Tentar(() =>
        {
            _dao.Entrada(produtoId, quantidade);
            return $"Entrada de {quantidade} unidade(s) registrada.";
        });
        return RedirectToAction("Index", "Home");
    }
}
