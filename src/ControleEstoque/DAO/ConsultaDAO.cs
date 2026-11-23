using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

/// <summary>
/// Consultas com dados consolidados gerados pela montagem dos kits
/// (tela de consulta de itens, APIs e Home administrativa).
/// </summary>
public class ConsultaDAO
{
    public List<ConsumoProduto> ConsultaItens(int produtoId, string categoria, int departamentoId,
                                              DateTime dataInicial, DateTime dataFinal)
    {
        SqlParameter[] p =
        {
            new SqlParameter("produtoId", produtoId),
            new SqlParameter("categoria", categoria),
            new SqlParameter("departamentoId", departamentoId),
            new SqlParameter("dataInicial", dataInicial),
            new SqlParameter("dataFinal", dataFinal)
        };
        DataTable tabela = HelperDAO.ExecutaProcSelect("spConsultaAvancadaItens", p);

        var lista = new List<ConsumoProduto>();
        foreach (DataRow registro in tabela.Rows)
        {
            lista.Add(new ConsumoProduto
            {
                NomeProduto = registro["NomeProduto"].ToString(),
                Categoria = registro["Categoria"].ToString(),
                NomeDepartamento = registro["NomeDepartamento"].ToString(),
                QtdKits = Convert.ToInt32(registro["QtdKits"]),
                QtdUnidades = Convert.ToInt32(registro["QtdUnidades"])
            });
        }
        return lista;
    }

    public List<KitsPorDepartamento> KitsPorDepartamento(DateTime dataInicial, DateTime dataFinal)
    {
        SqlParameter[] p =
        {
            new SqlParameter("dataInicial", dataInicial),
            new SqlParameter("dataFinal", dataFinal)
        };
        DataTable tabela = HelperDAO.ExecutaProcSelect("spKitsPorDepartamento", p);

        var lista = new List<KitsPorDepartamento>();
        foreach (DataRow registro in tabela.Rows)
        {
            lista.Add(new KitsPorDepartamento
            {
                DepartamentoId = Convert.ToInt32(registro["DepartamentoId"]),
                NomeDepartamento = registro["NomeDepartamento"].ToString(),
                TotalKits = Convert.ToInt32(registro["TotalKits"]),
                KitsMontados = Convert.ToInt32(registro["KitsMontados"]),
                KitsEntregues = Convert.ToInt32(registro["KitsEntregues"]),
                TotalUnidades = Convert.ToInt32(registro["TotalUnidades"])
            });
        }
        return lista;
    }

    public List<ProdutoMaisUsado> ProdutosMaisUsados(int top)
    {
        var p = new SqlParameter[] { new SqlParameter("top", top) };
        DataTable tabela = HelperDAO.ExecutaProcSelect("spProdutosMaisUsados", p);

        var lista = new List<ProdutoMaisUsado>();
        foreach (DataRow registro in tabela.Rows)
        {
            lista.Add(new ProdutoMaisUsado
            {
                ProdutoId = Convert.ToInt32(registro["ProdutoId"]),
                NomeProduto = registro["NomeProduto"].ToString(),
                Categoria = registro["Categoria"].ToString(),
                QtdKits = Convert.ToInt32(registro["QtdKits"]),
                QtdUnidades = Convert.ToInt32(registro["QtdUnidades"]),
                EmEstoque = Convert.ToInt32(registro["EmEstoque"])
            });
        }
        return lista;
    }

    /// <summary>Uma linha com as quantidades para a Home administrativa.</summary>
    public DataRow ResumoAdmin()
    {
        return HelperDAO.ExecutaProcSelect("spResumoAdmin", null).Rows[0];
    }
}
