using ControleEstoque.Models;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

public abstract class BaseController : Controller
{
    /// <summary>
    /// Executa a operação e mostra a mensagem de sucesso ou o erro de regra (EstoqueException).
    /// </summary>
    protected bool Tentar(Func<string> operacao)
    {
        try
        {
            TempData["Sucesso"] = operacao();
            return true;
        }
        catch (EstoqueException erro)
        {
            TempData["Erro"] = erro.Message;
            return false;
        }
    }
}
