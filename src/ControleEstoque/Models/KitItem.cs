namespace ControleEstoque.Models;

/// <summary>
/// Detalhe do kit: um produto e a quantidade dele no kit.
/// </summary>
public class KitItem : PadraoViewModel
{
    public int KitId { get; set; }
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }

    // campos preenchidos pelo join com Produtos (não são salvos na tabela KitItens)
    public string NomeProduto { get; set; }
    public string Categoria { get; set; }
    public byte[] FotoProduto { get; set; }

    public string FotoProdutoBase64 => FotoProduto != null ? Convert.ToBase64String(FotoProduto) : string.Empty;
}
