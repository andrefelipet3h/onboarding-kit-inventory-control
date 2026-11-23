using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

public class DepartamentoDAO : PadraoDAO<Departamento>
{
    protected override SqlParameter[] CriaParametros(Departamento model)
    {
        SqlParameter[] parametros =
        {
            new SqlParameter("id", model.Id),
            new SqlParameter("nome", model.Nome.Trim()),
            new SqlParameter("centroCusto", HelperDAO.NullAsDbNull(model.CentroCusto))
        };
        return parametros;
    }

    protected override Departamento MontaModel(DataRow registro)
    {
        return new Departamento
        {
            Id = Convert.ToInt32(registro["Id"]),
            Nome = registro["Nome"].ToString(),
            CentroCusto = registro["CentroCusto"].ToString()
        };
    }

    protected override void SetTabela()
    {
        Tabela = "Departamentos";
        OrdemListagem = "Nome";
    }
}
