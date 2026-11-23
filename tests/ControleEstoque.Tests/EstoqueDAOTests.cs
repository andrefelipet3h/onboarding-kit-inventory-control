using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ControleEstoque.Tests;

/// <summary>As classes que usam o banco rodam uma de cada vez e compartilham o mesmo banco de testes.</summary>
[CollectionDefinition("Banco de testes")]
public class ColecaoBancoDeTestes : ICollectionFixture<BancoDeTestes>
{
}

/// <summary>Testes da entrada no estoque usando o banco de testes.</summary>
[Collection("Banco de testes")]
public class EstoqueDAOTests
{
    private readonly EstoqueDAO _dao = new EstoqueDAO();
    private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();

    public EstoqueDAOTests(BancoDeTestes banco)
    {
        banco.Zerar();
    }

    // cadastra um produto e dá entrada no estoque
    private int ProdutoComEstoque(int quantidade, int id = 1)
    {
        _produtoDAO.Insert(new Produto { Id = id, Nome = "Produto " + id, Categoria = "Teste" });
        if (quantidade > 0)
            _dao.Entrada(id, quantidade);
        return id;
    }

    private int QuantidadeDo(int produtoId) => _produtoDAO.Consulta(produtoId).Estoque.Quantidade;

    [Fact]
    public void Produto_novo_comeca_com_estoque_zerado()
    {
        int p = ProdutoComEstoque(0);

        Assert.Equal(0, QuantidadeDo(p));
    }

    [Fact]
    public void Alterar_o_cadastro_do_produto_nao_mexe_no_estoque()
    {
        int p = ProdutoComEstoque(7);

        _produtoDAO.Update(new Produto { Id = p, Nome = "Novo nome", Categoria = "Outra" });

        Assert.Equal(7, QuantidadeDo(p));
    }

    [Fact]
    public void Entradas_somam_na_quantidade()
    {
        int p = ProdutoComEstoque(10);

        _dao.Entrada(p, 5);
        _dao.Entrada(p, 3);

        Assert.Equal(18, QuantidadeDo(p));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Entrada_com_quantidade_zero_ou_negativa_e_recusada(int quantidade)
    {
        int p = ProdutoComEstoque(10);

        Assert.Throws<EstoqueException>(() => _dao.Entrada(p, quantidade));

        Assert.Equal(10, QuantidadeDo(p));
    }

    [Fact]
    public void O_estoque_de_um_produto_nao_mexe_no_outro()
    {
        int mouse = ProdutoComEstoque(10, id: 1);
        int teclado = ProdutoComEstoque(3, id: 2);

        _dao.Entrada(mouse, 4);

        Assert.Equal(14, QuantidadeDo(mouse));
        Assert.Equal(3, QuantidadeDo(teclado));
    }

    [Fact]
    public void Entrada_em_produto_inexistente_e_recusada()
    {
        Assert.Throws<EstoqueException>(() => _dao.Entrada(999, 5));
    }

    [Fact]
    public void O_proprio_banco_recusa_quantidade_negativa()
    {
        int p = ProdutoComEstoque(10);

        using var conexao = ConexaoBD.GetConexao();

        Assert.Throws<SqlException>(() => BancoDeTestes.Executar(conexao, $"update Produtos set Quantidade = -1 where Id = {p}"));
    }

    [Fact]
    public void Entradas_simultaneas_nao_se_perdem()
    {
        int p = ProdutoComEstoque(0);

        // 30 entradas ao mesmo tempo
        var tarefas = Enumerable.Range(1, 30).Select(n => Task.Run(() => _dao.Entrada(p, 1))).ToArray();
        Task.WaitAll(tarefas);

        Assert.Equal(30, QuantidadeDo(p));
    }
}
