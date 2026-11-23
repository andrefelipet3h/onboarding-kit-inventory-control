using ControleEstoque.DAO;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers;

/// <summary>
/// Cadastro de produtos. As quantidades não são editadas aqui:
/// elas mudam pela entrada e pela montagem de kits.
/// </summary>
public class ProdutosController : PadraoImagemController<Produto>
{
    public ProdutosController()
    {
        DAO = new ProdutoDAO();
        GeraProximoId = true;
    }
}
