using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models;

/// <summary>
/// Produto que pode ir para um kit (notebook, mouse, mochila...).
/// </summary>
public class Produto : ImagemViewModel
{
    [Required(ErrorMessage = "Informe o nome do produto.")]
    [StringLength(60, ErrorMessage = "O nome deve ter no máximo 60 caracteres.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Informe a categoria.")]
    [StringLength(30, ErrorMessage = "A categoria deve ter no máximo 30 caracteres.")]
    public string Categoria { get; set; }

    [StringLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; }

    /// <summary>Quantidade em estoque do produto.</summary>
    public Estoque Estoque { get; set; } = new Estoque();
}
