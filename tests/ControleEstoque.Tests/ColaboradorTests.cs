using System.ComponentModel.DataAnnotations;
using ControleEstoque.Models;
using Xunit;

namespace ControleEstoque.Tests;

/// <summary>Funcional do colaborador (exatamente 7 números) e hash da senha.</summary>
public class ColaboradorTests
{
    // ---- Funcional: exatamente 7 números ----

    private static IList<ValidationResult> ValidarFuncional(string funcional)
    {
        var colaborador = new Colaborador { Funcional = funcional };
        var resultados = new List<ValidationResult>();
        Validator.TryValidateProperty(funcional,
            new ValidationContext(colaborador) { MemberName = nameof(Colaborador.Funcional) }, resultados);
        return resultados;
    }

    [Theory]
    [InlineData("1234567")]
    [InlineData("0000001")]
    [InlineData("9999999")]
    public void Funcional_com_7_numeros_e_valido(string funcional)
    {
        Assert.Empty(ValidarFuncional(funcional));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("123456")]
    [InlineData("12345678")]
    [InlineData("12345a7")]
    [InlineData("abcdefg")]
    [InlineData("123 567")]
    [InlineData("-123456")]
    public void Funcional_fora_do_padrao_e_invalido(string funcional)
    {
        Assert.NotEmpty(ValidarFuncional(funcional));
    }

    // ---- Senha: só o hash é guardado ----

    [Fact]
    public void Senha_confere_so_com_a_senha_certa()
    {
        var colaborador = new Colaborador();
        colaborador.DefinirSenha("segredo1");

        Assert.DoesNotContain("segredo1", colaborador.SenhaHash);
        Assert.True(colaborador.SenhaConfere("segredo1"));
        Assert.False(colaborador.SenhaConfere("segredo2"));
        Assert.False(colaborador.SenhaConfere(""));
    }

    [Fact]
    public void A_mesma_senha_gera_hashes_diferentes()
    {
        var a = new Colaborador();
        var b = new Colaborador();
        a.DefinirSenha("123456");
        b.DefinirSenha("123456");

        Assert.NotEqual(a.SenhaHash, b.SenhaHash);
    }
}
