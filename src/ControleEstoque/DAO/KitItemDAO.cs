using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

public class KitItemDAO : PadraoDAO<KitItem>
{
    protected override SqlParameter[] CriaParametros(KitItem model)
    {
        SqlParameter[] parametros =
        {
            new SqlParameter("id", model.Id),
            new SqlParameter("kitId", model.KitId),
            new SqlParameter("produtoId", model.ProdutoId),
            new SqlParameter("quantidade", model.Quantidade)
        };
        return parametros;
    }

    protected override KitItem MontaModel(DataRow registro)
    {
        var item = new KitItem
        {
            Id = Convert.ToInt32(registro["Id"]),
            KitId = Convert.ToInt32(registro["KitId"]),
            ProdutoId = Convert.ToInt32(registro["ProdutoId"]),
            Quantidade = Convert.ToInt32(registro["Quantidade"])
        };
        if (registro.Table.Columns.Contains("NomeProduto"))
        {
            item.NomeProduto = registro["NomeProduto"].ToString();
            item.Categoria = registro["Categoria"].ToString();
            if (registro["FotoProduto"] != DBNull.Value)
                item.FotoProduto = registro["FotoProduto"] as byte[];
        }
        return item;
    }

    protected override void SetTabela()
    {
        Tabela = "KitItens";
        ChaveIdentity = true;
    }

    public List<KitItem> ListagemPorKit(int kitId)
    {
        var p = new SqlParameter[] { new SqlParameter("kitId", kitId) };
        return MontaLista(HelperDAO.ExecutaProcSelect("spListagemItensDoKit", p));
    }

    public void ExcluiPorKit(int kitId)
    {
        var p = new SqlParameter[] { new SqlParameter("kitId", kitId) };
        HelperDAO.ExecutaProc("spDeleteItensDoKit", p);
    }
}
