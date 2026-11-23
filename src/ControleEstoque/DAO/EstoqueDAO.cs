using ControleEstoque.Models;

namespace ControleEstoque.DAO;

/// <summary>Entrada de unidades no estoque de um produto (a saída acontece na montagem do kit).</summary>
public class EstoqueDAO
{
    private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();

    public void Entrada(int produtoId, int quantidade) =>
        AlteraEstoque(produtoId, estoque => estoque.Entrada(quantidade));

    private void AlteraEstoque(int produtoId, Action<Estoque> regra)
    {
        using (var transacao = HelperDAO.NovaTransacao())
        {
            Produto produto = _produtoDAO.LeEstoqueParaAlterar(produtoId);
            regra(produto.Estoque);
            _produtoDAO.SalvaEstoque(produto);
            transacao.Complete();
        }
    }
}
