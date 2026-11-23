using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models;

public class Departamento : PadraoViewModel
{
    [Required(ErrorMessage = "Informe o nome do departamento.")]
    [StringLength(60, ErrorMessage = "O nome deve ter no máximo 60 caracteres.")]
    public string Nome { get; set; }

    [StringLength(20, ErrorMessage = "O centro de custo deve ter no máximo 20 caracteres.")]
    [Display(Name = "Centro de custo")]
    public string CentroCusto { get; set; }
}
