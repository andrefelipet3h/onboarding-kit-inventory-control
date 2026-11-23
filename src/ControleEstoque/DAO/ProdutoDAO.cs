using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

public class ProdutoDAO : PadraoDAO<Produto>
{
    protected override SqlParameter[] CriaParametros(Produto model)
    {
        SqlParameter[] parametros =
        {
            new SqlParameter("id", model.Id),
            new SqlParameter("nome", model.Nome.Trim()),
            new SqlParameter("categoria", model.Categoria.Trim()),
            new SqlParameter("descricao", HelperDAO.NullAsDbNull(model.Descricao)),
            new SqlParameter("foto", SqlDbType.VarBinary) { Value = HelperDAO.NullAsDbNull(model.ImagemEmByte) }
        };
        return parametros;
    }

    protected override Produto MontaModel(DataRow registro)
    {
        var p = new Produto
        {
            Id = Convert.ToInt32(registro["Id"]),
            Nome = registro["Nome"].ToString(),
            Estoque = new Estoque
            {
                Quantidade = Convert.ToInt32(registro["Quantidade"])
            }
        };
        if (registro.Table.Columns.Contains("Categoria"))
        {
            p.Categoria = registro["Categoria"].ToString();
            p.Descricao = registro["Descricao"].ToString();
            if (registro["Foto"] != DBNull.Value)
                p.ImagemEmByte = registro["Foto"] as byte[];
        }
        return p;
    }

    protected override void SetTabela()
    {
        Tabela = "Produtos";
        OrdemListagem = "Nome";
    }

    /// <summary>Lê o estoque do produto com UPDLOCK (trava a linha até o fim da transação).</summary>
    public Produto LeEstoqueParaAlterar(int produtoId)
    {
        var p = new SqlParameter[] { new SqlParameter("id", produtoId) };
        var tabela = HelperDAO.ExecutaProcSelect("spLeEstoqueProduto", p);
        if (tabela.Rows.Count == 0)
            throw new EstoqueException("Selecione um produto válido.");
        return MontaModel(tabela.Rows[0]);
    }

    public void SalvaEstoque(Produto produto)
    {
        SqlParameter[] p =
        {
            new SqlParameter("id", produto.Id),
            new SqlParameter("quantidade", produto.Estoque.Quantidade)
        };
        HelperDAO.ExecutaProc("spSalvaEstoqueProduto", p);
    }

    public List<string> ListaCategorias()
    {
        var tabela = HelperDAO.ExecutaProcSelect("spListagemCategorias", null);
        return tabela.Rows.Cast<DataRow>().Select(r => r["Categoria"].ToString()).ToList();
    }
}
