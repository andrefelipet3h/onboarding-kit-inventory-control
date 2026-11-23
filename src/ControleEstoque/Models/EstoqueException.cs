namespace ControleEstoque.Models;

/// <summary>
/// Erro de regra de negócio. A mensagem é mostrada para o usuário na tela.
/// </summary>
public class EstoqueException : Exception
{
    public EstoqueException(string mensagem) : base(mensagem) { }
}
