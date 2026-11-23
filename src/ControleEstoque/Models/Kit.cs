using System.ComponentModel.DataAnnotations;

namespace ControleEstoque.Models;

/// <summary>
/// Kit de onboarding de um colaborador (mestre). Cada colaborador tem no máximo um kit.
/// </summary>
public class Kit : PadraoViewModel
{
    public const string Montado = "Montado";
    public const string Entregue = "Entregue";

    [Range(1, int.MaxValue, ErrorMessage = "Selecione o colaborador.")]
    [Display(Name = "Colaborador")]
    public int ColaboradorId { get; set; }

    public DateTime DataMontagem { get; set; }

    public string Situacao { get; set; } = Montado;

    [StringLength(200, ErrorMessage = "A observação deve ter no máximo 200 caracteres.")]
    [Display(Name = "Observação")]
    public string Observacao { get; set; }

    public DateTime? DataEntrega { get; set; }

    /// <summary>Detalhe: os produtos do kit.</summary>
    public List<KitItem> Itens { get; set; } = new List<KitItem>();

    // campos preenchidos pelos joins (não são salvos na tabela Kits)
    public string NomeColaborador { get; set; }
    public string Funcional { get; set; }
    public string NomeDepartamento { get; set; }
    public int TotalUnidades { get; set; }

    public bool PodeAlterar => Situacao == Montado;
}
