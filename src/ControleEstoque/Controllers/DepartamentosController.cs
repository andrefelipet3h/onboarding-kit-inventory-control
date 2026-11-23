using ControleEstoque.DAO;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers;

public class DepartamentosController : PadraoController<Departamento>
{
    public DepartamentosController()
    {
        DAO = new DepartamentoDAO();
        GeraProximoId = true;
    }
}
