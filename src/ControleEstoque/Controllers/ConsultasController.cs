using ControleEstoque.DAO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

/// <summary>Telas de consulta avançada dos kits montados. Os resultados chegam via AJAX.</summary>
[Authorize(Roles = Perfis.Admin)]
public class ConsultasController : BaseController
{
    public IActionResult Kits()
    {
        ViewBag.Departamentos = HelperControllers.ComboDepartamentos("Todos");
        ViewBag.Situacoes = HelperControllers.ComboSituacoesKit("Todas");
        return View();
    }

    public IActionResult ObtemDadosConsultaKits(string colaborador, int departamentoId, string situacao,
                                                DateTime dataInicial, DateTime dataFinal)
    {
        try
        {
            AjustaPeriodo(ref dataInicial, ref dataFinal);
            var lista = new KitDAO().ConsultaAvancada((colaborador ?? "").Trim(), departamentoId, situacao ?? "",
                                                      dataInicial, dataFinal);
            return PartialView("_GridKits", lista);
        }
        catch (Exception erro)
        {
            return Json(new { erro = true, msg = erro.Message });
        }
    }

    public IActionResult Itens()
    {
        ViewBag.Produtos = HelperControllers.ComboProdutos("Todos");
        ViewBag.Categorias = HelperControllers.ComboCategorias("Todas");
        ViewBag.Departamentos = HelperControllers.ComboDepartamentos("Todos");
        return View();
    }

    public IActionResult ObtemDadosConsultaItens(int produtoId, string categoria, int departamentoId,
                                                 DateTime dataInicial, DateTime dataFinal)
    {
        try
        {
            AjustaPeriodo(ref dataInicial, ref dataFinal);
            var lista = new ConsultaDAO().ConsultaItens(produtoId, categoria ?? "", departamentoId,
                                                        dataInicial, dataFinal);
            return PartialView("_GridItens", lista);
        }
        catch (Exception erro)
        {
            return Json(new { erro = true, msg = erro.Message });
        }
    }

    /// <summary>Datas em branco viram "sem limite"; a data final vale até o fim do dia.</summary>
    private static void AjustaPeriodo(ref DateTime dataInicial, ref DateTime dataFinal)
    {
        if (dataInicial == DateTime.MinValue)
            dataInicial = new DateTime(1900, 1, 1);
        if (dataFinal == DateTime.MinValue)
            dataFinal = new DateTime(9999, 12, 31);
        else
            dataFinal = dataFinal.Date.AddDays(1).AddSeconds(-1);

        if (dataFinal < dataInicial)
            throw new Exception("A data final deve ser maior que a data inicial.");
    }
}
