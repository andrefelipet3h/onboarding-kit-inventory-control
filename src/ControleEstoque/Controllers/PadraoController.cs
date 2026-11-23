using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.Controllers;

/// <summary>
/// Controller padrão que generaliza as operações de um CRUD (listar, incluir, alterar e excluir).
/// Os cadastros só podem ser acessados pelo administrador.
/// </summary>
[Authorize(Roles = Perfis.Admin)]
public abstract class PadraoController<T> : BaseController where T : PadraoViewModel
{
    protected PadraoDAO<T> DAO { get; set; }
    protected bool GeraProximoId { get; set; }
    protected string NomeViewIndex { get; set; } = "Index";
    protected string NomeViewForm { get; set; } = "Form";

    public virtual IActionResult Index()
    {
        return View(NomeViewIndex, DAO.Listagem());
    }

    public virtual IActionResult Create()
    {
        ViewBag.Operacao = "I";
        T model = Activator.CreateInstance<T>();
        PreencheDadosParaView("I", model);
        return View(NomeViewForm, model);
    }

    public virtual IActionResult Edit(int id)
    {
        ViewBag.Operacao = "A";
        T model = DAO.Consulta(id);
        if (model == null)
            return RedirectToAction(NomeViewIndex);

        PreencheDadosParaView("A", model);
        return View(NomeViewForm, model);
    }

    [HttpPost]
    public virtual IActionResult Save(T model, string Operacao)
    {
        ValidaDados(model, Operacao);

        if (ModelState.IsValid)
        {
            try
            {
                if (Operacao == "I")
                    DAO.Insert(model);
                else
                    DAO.Update(model);

                TempData["Sucesso"] = "Registro salvo com sucesso.";
                return RedirectToAction(NomeViewIndex);
            }
            catch (EstoqueException erro)
            {
                ModelState.AddModelError(string.Empty, erro.Message);
            }
        }

        ViewBag.Operacao = Operacao;
        PreencheDadosParaView(Operacao, model);
        return View(NomeViewForm, model);
    }

    [HttpPost]
    public virtual IActionResult Delete(int id)
    {
        try
        {
            ValidaExclusao(id);
            DAO.Delete(id);
            TempData["Sucesso"] = "Registro excluído.";
        }
        catch (EstoqueException erro)
        {
            TempData["Erro"] = erro.Message;
        }
        catch (SqlException erro) when (erro.Number == 547) // 547 = violação de chave estrangeira
        {
            TempData["Erro"] = "Não é possível excluir: este registro está sendo usado em outro cadastro.";
        }
        return RedirectToAction(NomeViewIndex);
    }

    /// <summary>Preenche o que a tela precisa (combos, valores padrão...).</summary>
    protected virtual void PreencheDadosParaView(string Operacao, T model)
    {
        if (GeraProximoId && Operacao == "I")
            model.Id = DAO.ProximoId();
    }

    /// <summary>
    /// Limpa os erros automáticos do ASP.NET (que podem vir em inglês) e valida de novo
    /// com as mensagens em português das DataAnnotations; depois confere o Id.
    /// </summary>
    protected virtual void ValidaDados(T model, string operacao)
    {
        ModelState.Clear();
        TryValidateModel(model);

        if (model.Id <= 0)
            ModelState.AddModelError("Id", "Id inválido!");
        else if (operacao == "I" && DAO.Consulta(model.Id) != null)
            ModelState.AddModelError("Id", "Código já está em uso!");
        else if (operacao == "A" && DAO.Consulta(model.Id) == null)
            ModelState.AddModelError("Id", "Este registro não existe!");
    }

    /// <summary>Regras que impedem a exclusão. Lance EstoqueException com a mensagem para o usuário.</summary>
    protected virtual void ValidaExclusao(int id)
    {
    }
}
