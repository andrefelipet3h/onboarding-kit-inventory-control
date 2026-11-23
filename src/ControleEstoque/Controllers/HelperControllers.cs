using ControleEstoque.DAO;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ControleEstoque.Controllers;

/// <summary>Perfis de acesso gravados no cookie de login.</summary>
public static class Perfis
{
    public const string Admin = "Admin";
    public const string Colaborador = "Colaborador";
}

public static class HelperControllers
{
    /// <summary>Converte a imagem recebida no formulário em um vetor de bytes.</summary>
    public static byte[] ConvertImageToByte(IFormFile arquivo)
    {
        if (arquivo == null)
            return null;

        using (var ms = new MemoryStream())
        {
            arquivo.CopyTo(ms);
            return ms.ToArray();
        }
    }

    // Listas para as caixas combo

    public static List<SelectListItem> ComboDepartamentos(string primeiroItem)
    {
        var lista = new List<SelectListItem> { new SelectListItem(primeiroItem, "0") };
        foreach (var d in new DepartamentoDAO().Listagem())
            lista.Add(new SelectListItem(d.Nome, d.Id.ToString()));
        return lista;
    }

    public static List<SelectListItem> ComboProdutos(string primeiroItem, bool mostrarEstoque = false)
    {
        var lista = new List<SelectListItem> { new SelectListItem(primeiroItem, "0") };
        foreach (var p in new ProdutoDAO().Listagem())
        {
            string texto = mostrarEstoque ? $"{p.Nome} (em estoque: {p.Estoque.Quantidade})" : p.Nome;
            lista.Add(new SelectListItem(texto, p.Id.ToString()));
        }
        return lista;
    }

    public static List<SelectListItem> ComboCategorias(string primeiroItem)
    {
        var lista = new List<SelectListItem> { new SelectListItem(primeiroItem, "") };
        foreach (string categoria in new ProdutoDAO().ListaCategorias())
            lista.Add(new SelectListItem(categoria, categoria));
        return lista;
    }

    public static List<SelectListItem> ComboSituacoesKit(string primeiroItem)
    {
        return new List<SelectListItem>
        {
            new SelectListItem(primeiroItem, ""),
            new SelectListItem(Models.Kit.Montado, Models.Kit.Montado),
            new SelectListItem(Models.Kit.Entregue, Models.Kit.Entregue)
        };
    }
}
