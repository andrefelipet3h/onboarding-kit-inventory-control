using ControleEstoque.Models;

namespace ControleEstoque.Controllers;

/// <summary>Controller padrão para os cadastros que têm foto (produtos e colaboradores).</summary>
public abstract class PadraoImagemController<T> : PadraoController<T> where T : ImagemViewModel
{
    protected override void ValidaDados(T model, string operacao)
    {
        base.ValidaDados(model, operacao);

        if (model.Imagem != null && !model.Imagem.ContentType.StartsWith("image/"))
            ModelState.AddModelError("Imagem", "O arquivo escolhido não é uma imagem.");
        if (model.Imagem != null && model.Imagem.Length / 1024 / 1024 >= 2)
            ModelState.AddModelError("Imagem", "Imagem limitada a 2 MB.");

        if (ModelState.IsValid)
        {
            // na alteração, se não foi escolhida outra imagem, mantém a que já estava salva.
            if (operacao == "A" && model.Imagem == null)
                model.ImagemEmByte = DAO.Consulta(model.Id).ImagemEmByte;
            else
                model.ImagemEmByte = HelperControllers.ConvertImageToByte(model.Imagem);
        }
    }
}
