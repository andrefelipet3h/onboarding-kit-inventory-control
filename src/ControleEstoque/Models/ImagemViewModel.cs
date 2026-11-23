namespace ControleEstoque.Models;

/// <summary>
/// Classe ancestral dos cadastros que têm foto (produto e colaborador).
/// </summary>
public abstract class ImagemViewModel : PadraoViewModel
{
    /// <summary>Imagem recebida do formulário.</summary>
    public IFormFile Imagem { get; set; }

    /// <summary>Imagem em bytes, pronta para ser salva no banco.</summary>
    public byte[] ImagemEmByte { get; set; }

    /// <summary>Imagem no formato usado pelo &lt;img src="data:image/jpeg;base64,..."&gt;.</summary>
    public string ImagemEmBase64 => ImagemEmByte != null ? Convert.ToBase64String(ImagemEmByte) : string.Empty;
}
