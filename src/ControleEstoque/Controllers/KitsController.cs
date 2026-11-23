using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleEstoque.Controllers;

/// <summary>
/// Montagem do kit (mestre: kit do colaborador; detalhe: produtos e quantidades).
/// </summary>
public class KitsController : PadraoController<Kit>
{
    public KitsController()
    {
        DAO = new KitDAO();
        GeraProximoId = false; // o Id do kit é identity
    }

    public override IActionResult Edit(int id)
    {
        Kit kit = DAO.Consulta(id);
        if (kit != null && !kit.PodeAlterar)
        {
            TempData["Erro"] = "Um kit já entregue não pode ser alterado.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }
        return base.Edit(id);
    }

    public IActionResult Detalhes(int id)
    {
        Kit kit = DAO.Consulta(id);
        if (kit == null)
            return RedirectToAction(nameof(Index));

        ViewBag.Colaborador = new ColaboradorDAO().Consulta(kit.ColaboradorId);
        return View(kit);
    }

    [HttpPost]
    public IActionResult Entregar(int id)
    {
        Tentar(() =>
        {
            ((KitDAO)DAO).Entregar(id);
            return $"Kit {id} marcado como entregue.";
        });
        return RedirectToAction(nameof(Index));
    }

    protected override void PreencheDadosParaView(string Operacao, Kit model)
    {
        base.PreencheDadosParaView(Operacao, model);

        // no combo só aparecem os colaboradores que ainda não têm kit (e o dono deste kit, na alteração)
        var comKit = DAO.Listagem().Where(k => k.Id != model.Id).Select(k => k.ColaboradorId).ToList();
        var colaboradores = new List<SelectListItem> { new SelectListItem("Selecione o colaborador...", "0") };
        foreach (var c in new ColaboradorDAO().Listagem().Where(c => !comKit.Contains(c.Id)))
            colaboradores.Add(new SelectListItem($"{c.Nome} ({c.Funcional}) - {c.NomeDepartamento}", c.Id.ToString()));

        ViewBag.Colaboradores = colaboradores;
        ViewBag.Produtos = HelperControllers.ComboProdutos("Selecione o produto...");
    }

    protected override void ValidaDados(Kit model, string operacao)
    {
        // não chamamos o base: o Id do kit é gerado pelo banco (identity)
        ModelState.Clear();
        TryValidateModel(model);

        if (operacao == "A" && DAO.Consulta(model.Id) == null)
            ModelState.AddModelError("Id", "Este kit não existe!");

        if (model.ColaboradorId > 0)
        {
            Kit kitDoColaborador = ((KitDAO)DAO).ConsultaPorColaborador(model.ColaboradorId);
            if (kitDoColaborador != null && kitDoColaborador.Id != model.Id)
                ModelState.AddModelError("ColaboradorId", "Este colaborador já tem um kit.");
        }

        // validações do detalhe
        if (model.Itens.Count == 0)
            ModelState.AddModelError("Itens", "Adicione ao menos um produto ao kit.");
        if (model.Itens.Exists(i => i.ProdutoId <= 0 || i.Quantidade <= 0))
            ModelState.AddModelError("Itens", "Todos os itens precisam de um produto e de uma quantidade maior que zero.");
        if (model.Itens.GroupBy(i => i.ProdutoId).Any(g => g.Count() > 1))
            ModelState.AddModelError("Itens", "O mesmo produto foi adicionado mais de uma vez. Altere a quantidade dele.");
    }

    // AJAX

    /// <summary>AJAX: dados do produto escolhido (quantidade em estoque e foto) para montar o item do kit.</summary>
    public IActionResult EstoqueProduto(int id)
    {
        Produto produto = new ProdutoDAO().Consulta(id);
        if (produto == null)
            return Json(new { erro = true, msg = "Produto não encontrado." });

        return Json(new
        {
            nome = produto.Nome,
            categoria = produto.Categoria,
            quantidade = produto.Estoque.Quantidade,
            foto = produto.ImagemEmBase64
        });
    }

    /// <summary>AJAX: dados do colaborador escolhido, exibidos ao lado do combo.</summary>
    public IActionResult DadosColaborador(int id)
    {
        Colaborador colaborador = new ColaboradorDAO().Listagem().Find(c => c.Id == id);
        if (colaborador == null)
            return Json(new { erro = true, msg = "Colaborador não encontrado." });

        return Json(new
        {
            nome = colaborador.Nome,
            funcional = colaborador.Funcional,
            email = colaborador.Email,
            departamento = colaborador.NomeDepartamento,
            admissao = colaborador.DataAdmissao.ToString("dd/MM/yyyy"),
            foto = colaborador.ImagemEmBase64
        });
    }
}
