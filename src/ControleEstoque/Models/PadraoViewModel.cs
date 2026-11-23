namespace ControleEstoque.Models;

/// <summary>
/// Classe ancestral de todas as models. Toda tabela possui um campo inteiro "Id" como chave primária.
/// </summary>
public abstract class PadraoViewModel
{
    public virtual int Id { get; set; }
}
