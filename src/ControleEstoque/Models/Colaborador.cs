using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models;

/// <summary>
/// Colaborador que recebe o kit. Entra no sistema com o funcional e a senha.
/// </summary>
public class Colaborador : ImagemViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(80, ErrorMessage = "O nome deve ter no máximo 80 caracteres.")]
    public string Nome { get; set; }

    [Required(ErrorMessage = "Informe o funcional do colaborador.")]
    [RegularExpression(@"^\d{7}$", ErrorMessage = "O funcional deve ter exatamente 7 números.")]
    [Display(Name = "Funcional")]
    public string Funcional { get; set; }

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(80, ErrorMessage = "O e-mail deve ter no máximo 80 caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o departamento.")]
    [Display(Name = "Departamento")]
    public int DepartamentoId { get; set; }

    [Display(Name = "Data de admissão")]
    public DateTime DataAdmissao { get; set; }

    /// <summary>Só existe no formulário; no banco fica o SenhaHash.</summary>
    [DataType(DataType.Password)]
    public string Senha { get; set; }

    public string SenhaHash { get; set; }

    /// <summary>Nome do departamento após o join. Não é salvo na tabela Colaboradores.</summary>
    public string NomeDepartamento { get; set; }

    public void DefinirSenha(string senha) => SenhaHash = Models.Senha.GerarHash(senha);

    public bool SenhaConfere(string senha) => Models.Senha.Confere(senha, SenhaHash);
}
