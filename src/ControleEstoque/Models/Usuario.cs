using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models;

/// <summary>
/// Usuário administrador. Também é usado na tela de login
/// (o colaborador digita o funcional no campo NomeUsuario).
/// </summary>
public class Usuario : PadraoViewModel
{
    [Required(ErrorMessage = "Informe o usuário ou o funcional.")]
    [StringLength(50, ErrorMessage = "O usuário deve ter no máximo 50 caracteres.")]
    [Display(Name = "Usuário ou funcional")]
    public string NomeUsuario { get; set; }

    /// <summary>Só existe durante o login; não é gravada no banco.</summary>
    [Required(ErrorMessage = "Informe a senha.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; }

    public string SenhaHash { get; set; }

    public DateTime CriadoEm { get; set; }

    public void DefinirSenha(string senha) => SenhaHash = Models.Senha.GerarHash(senha);

    public bool SenhaConfere(string senha) => Models.Senha.Confere(senha, SenhaHash);
}
