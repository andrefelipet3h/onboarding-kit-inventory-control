using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

/// <summary>
/// Classe ancestral de todas as DAOs: gera as operações básicas de um CRUD.
/// Pressupostos: a chave primária se chama "Id" e as procedures de insert e update
/// se chamam spInsert_[NomeDaTabela] e spUpdate_[NomeDaTabela].
/// </summary>
public abstract class PadraoDAO<T> where T : PadraoViewModel
{
    public PadraoDAO()
    {
        SetTabela();
    }

    protected string Tabela { get; set; }
    protected string NomeSpListagem { get; set; } = "spListagem";
    protected string OrdemListagem { get; set; } = "1"; // 1 é o primeiro campo da tabela
    protected bool ChaveIdentity { get; set; } = false;

    protected abstract SqlParameter[] CriaParametros(T model);
    protected abstract T MontaModel(DataRow registro);
    protected abstract void SetTabela();

    public virtual int Insert(T model)
    {
        return HelperDAO.ExecutaProc("spInsert_" + Tabela, CriaParametros(model), ChaveIdentity);
    }

    public virtual void Update(T model)
    {
        HelperDAO.ExecutaProc("spUpdate_" + Tabela, CriaParametros(model));
    }

    public virtual void Delete(int id)
    {
        var p = new SqlParameter[]
        {
            new SqlParameter("id", id),
            new SqlParameter("tabela", Tabela)
        };
        HelperDAO.ExecutaProc("spDelete", p);
    }

    public virtual T Consulta(int id)
    {
        var p = new SqlParameter[]
        {
            new SqlParameter("id", id),
            new SqlParameter("tabela", Tabela)
        };
        var tabela = HelperDAO.ExecutaProcSelect("spConsulta", p);
        if (tabela.Rows.Count == 0)
            return null;
        else
            return MontaModel(tabela.Rows[0]);
    }

    public virtual int ProximoId()
    {
        var p = new SqlParameter[]
        {
            new SqlParameter("tabela", Tabela)
        };
        var tabela = HelperDAO.ExecutaProcSelect("spProximoId", p);
        return Convert.ToInt32(tabela.Rows[0][0]);
    }

    public virtual List<T> Listagem()
    {
        var p = new SqlParameter[]
        {
            new SqlParameter("tabela", Tabela),
            new SqlParameter("ordem", OrdemListagem)
        };
        var tabela = HelperDAO.ExecutaProcSelect(NomeSpListagem, p);
        return MontaLista(tabela);
    }

    /// <summary>Transforma todos os registros de um DataTable em uma lista de models.</summary>
    protected List<T> MontaLista(DataTable tabela)
    {
        List<T> lista = new List<T>();
        foreach (DataRow registro in tabela.Rows)
            lista.Add(MontaModel(registro));
        return lista;
    }
}
