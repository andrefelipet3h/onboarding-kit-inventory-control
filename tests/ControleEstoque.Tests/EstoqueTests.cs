using ControleEstoque.Models;
using Xunit;

namespace ControleEstoque.Tests;

/// <summary>Testes das regras da classe Estoque (sem banco).</summary>
public class EstoqueTests
{
    private static Estoque NovoEstoque(int quantidade = 100) => new Estoque { Quantidade = quantidade };

    [Fact]
    public void Entrada_aumenta_a_quantidade()
    {
        var p = NovoEstoque(10);
        p.Entrada(5);

        Assert.Equal(15, p.Quantidade);
    }

    [Fact]
    public void Montar_kit_leva_as_unidades_do_estoque()
    {
        var p = NovoEstoque(10);
        p.RetirarParaKit(2);

        Assert.Equal(8, p.Quantidade);
    }

    [Fact]
    public void Montar_kit_pode_usar_tudo_o_que_ha_no_estoque()
    {
        var p = NovoEstoque(10);
        p.RetirarParaKit(10);

        Assert.Equal(0, p.Quantidade);
    }

    [Fact]
    public void Montar_kit_sem_estoque_e_recusado_e_nada_muda()
    {
        var p = NovoEstoque(2);

        Assert.Throws<EstoqueException>(() => p.RetirarParaKit(3));

        Assert.Equal(2, p.Quantidade);
    }

    [Fact]
    public void Devolver_do_kit_volta_ao_estoque()
    {
        var p = NovoEstoque(10);
        p.RetirarParaKit(3);
        p.DevolverDoKit(3);

        Assert.Equal(10, p.Quantidade);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Quantidade_precisa_ser_maior_que_zero_em_todas_as_operacoes(int quantidade)
    {
        var p = NovoEstoque();

        Assert.Throws<EstoqueException>(() => p.Entrada(quantidade));
        Assert.Throws<EstoqueException>(() => p.RetirarParaKit(quantidade));
        Assert.Throws<EstoqueException>(() => p.DevolverDoKit(quantidade));

        Assert.Equal(100, p.Quantidade);
    }

    [Fact]
    public void Entrada_que_estouraria_o_limite_e_recusada()
    {
        var p = NovoEstoque(int.MaxValue - 1);

        Assert.Throws<EstoqueException>(() => p.Entrada(2));
        Assert.Equal(int.MaxValue - 1, p.Quantidade);
    }

    [Fact]
    public void Mensagem_de_erro_diz_quanto_ha_no_estoque()
    {
        var p = NovoEstoque(6);

        var erro = Assert.Throws<EstoqueException>(() => p.RetirarParaKit(7));

        Assert.Contains("só há 6 em estoque", erro.Message);
    }
}
