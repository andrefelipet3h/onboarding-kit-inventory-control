namespace ControleEstoque.Models;

/// <summary>Estoque de um produto e as regras de quantidade.</summary>
public class Estoque
{
    /// <summary>Unidades do produto em estoque (podem ir para os kits).</summary>
    public int Quantidade { get; set; }

    /// <summary>Entrada: adiciona unidades ao estoque.</summary>
    public void Entrada(int quantidade)
    {
        ValidarQuantidade(quantidade);

        if (quantidade > int.MaxValue - Quantidade)
            throw new EstoqueException("A quantidade informada ultrapassa o limite do estoque.");

        Quantidade += quantidade;
    }

    /// <summary>Montagem de um kit: o item do kit leva unidades do estoque.</summary>
    public void RetirarParaKit(int quantidade)
    {
        GarantirEstoque(quantidade, "colocar no kit");
        Quantidade -= quantidade;
    }

    /// <summary>Alteração ou exclusão de um kit ainda não entregue: as unidades voltam ao estoque.</summary>
    public void DevolverDoKit(int quantidade) => Entrada(quantidade);

    private static void ValidarQuantidade(int quantidade)
    {
        if (quantidade <= 0)
            throw new EstoqueException("A quantidade deve ser maior que zero.");
    }

    // não deixa retirar mais do que tem no estoque
    private void GarantirEstoque(int quantidade, string acao)
    {
        ValidarQuantidade(quantidade);

        if (quantidade > Quantidade)
            throw new EstoqueException(
                $"Não é possível {acao} {quantidade} unidade(s): só há {Quantidade} em estoque.");
    }
}
