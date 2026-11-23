using ControleEstoque.DAO;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers;

public class ColaboradoresController : PadraoImagemController<Colaborador>
{
    public ColaboradoresController()
    {
        DAO = new ColaboradorDAO();
        GeraProximoId = true;
    }

    protected override void PreencheDadosParaView(string Operacao, Colaborador model)
    {
        base.PreencheDadosParaView(Operacao, model);
        if (Operacao == "I" && model.DataAdmissao == DateTime.MinValue)
            model.DataAdmissao = DateTime.Today;
        model.Senha = null; // a senha nunca volta para a tela
        ViewBag.Departamentos = HelperControllers.ComboDepartamentos("Selecione o departamento...");
    }

    protected override void ValidaDados(Colaborador model, string operacao)
    {
        base.ValidaDados(model, operacao);

        if (model.DataAdmissao.Year < 2000 || model.DataAdmissao > DateTime.Today.AddMonths(6))
            ModelState.AddModelError("DataAdmissao", "Data de admissão inválida.");

        var outros = DAO.Listagem().Where(c => c.Id != model.Id).ToList();
        if (!string.IsNullOrEmpty(model.Funcional) && outros.Exists(c => c.Funcional == model.Funcional.Trim()))
            ModelState.AddModelError("Funcional", "Já existe um colaborador com este funcional.");
        if (!string.IsNullOrEmpty(model.Email) && outros.Exists(c => c.Email == model.Email.Trim().ToLower()))
            ModelState.AddModelError("Email", "Já existe um colaborador com este e-mail.");

        // senha obrigatória na inclusão; na alteração, em branco mantém a atual
        if (string.IsNullOrEmpty(model.Senha))
        {
            if (operacao == "I")
                ModelState.AddModelError("Senha", "Informe a senha de acesso do colaborador.");
            else if (ModelState.IsValid)
                model.SenhaHash = DAO.Consulta(model.Id).SenhaHash;
        }
        else if (model.Senha.Length < 6)
            ModelState.AddModelError("Senha", "A senha deve ter ao menos 6 caracteres.");
        else
            model.DefinirSenha(model.Senha);
    }
}
