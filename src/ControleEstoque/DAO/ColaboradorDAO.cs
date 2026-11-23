using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

public class ColaboradorDAO : PadraoDAO<Colaborador>
{
    protected override SqlParameter[] CriaParametros(Colaborador model)
    {
        SqlParameter[] parametros =
        {
            new SqlParameter("id", model.Id),
            new SqlParameter("nome", model.Nome.Trim()),
            new SqlParameter("funcional", model.Funcional.Trim()),
            new SqlParameter("email", model.Email.Trim().ToLower()),
            new SqlParameter("departamentoId", model.DepartamentoId),
            new SqlParameter("dataAdmissao", model.DataAdmissao),
            new SqlParameter("senhaHash", model.SenhaHash),
            new SqlParameter("foto", SqlDbType.VarBinary) { Value = HelperDAO.NullAsDbNull(model.ImagemEmByte) }
        };
        return parametros;
    }

    protected override Colaborador MontaModel(DataRow registro)
    {
        var c = new Colaborador
        {
            Id = Convert.ToInt32(registro["Id"]),
            Nome = registro["Nome"].ToString(),
            Funcional = registro["Funcional"].ToString(),
            Email = registro["Email"].ToString(),
            DepartamentoId = Convert.ToInt32(registro["DepartamentoId"]),
            DataAdmissao = Convert.ToDateTime(registro["DataAdmissao"]),
            SenhaHash = registro["SenhaHash"].ToString()
        };
        if (registro["Foto"] != DBNull.Value)
            c.ImagemEmByte = registro["Foto"] as byte[];
        if (registro.Table.Columns.Contains("NomeDepartamento"))
            c.NomeDepartamento = registro["NomeDepartamento"].ToString();
        return c;
    }

    protected override void SetTabela()
    {
        Tabela = "Colaboradores";
        NomeSpListagem = "spListagem_Colaboradores";
    }

    /// <summary>Usado no login do colaborador. Devolve null se o funcional não existir.</summary>
    public Colaborador ConsultaPorFuncional(string funcional)
    {
        var p = new SqlParameter[] { new SqlParameter("funcional", funcional) };
        var tabela = HelperDAO.ExecutaProcSelect("spConsultaColaboradorPorFuncional", p);
        return tabela.Rows.Count == 0 ? null : MontaModel(tabela.Rows[0]);
    }
}
