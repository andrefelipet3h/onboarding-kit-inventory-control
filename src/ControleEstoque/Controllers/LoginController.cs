using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ControleEstoque.DAO;
using ControleEstoque.Models;

namespace ControleEstoque.Controllers;

/// <summary>
/// Tela de login (cookie de autenticação) e saída do sistema.
/// O administrador entra com o usuário; o colaborador entra com o funcional (7 números).
/// </summary>
[AllowAnonymous]
public class LoginController : Controller
{
    private readonly ILogger<LoginController> _logger;

    public LoginController(ILogger<LoginController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Index(string returnUrl)
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View(new Usuario());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index([Bind("NomeUsuario,Senha")] Usuario dados, string returnUrl)
    {
        ViewBag.ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return View(dados);

        string login = dados.NomeUsuario.Trim();
        ClaimsIdentity identidade = Regex.IsMatch(login, @"^\d{7}$")
            ? IdentidadeDoColaborador(login, dados.Senha)
            : IdentidadeDoAdministrador(login, dados.Senha);

        // A mesma mensagem para "usuário não existe" e "senha errada": não revela quais usuários existem.
        if (identidade == null)
        {
            _logger.LogWarning("Tentativa de login recusada para o usuário '{Usuario}'.", login);
            ModelState.AddModelError(string.Empty, "Usuário ou senha inválidos.");
            dados.Senha = null;
            return View(dados);
        }

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidade));

        // Só volta para endereços do próprio site (evita redirecionar para um site de fora).
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return LocalRedirect(returnUrl);

        return identidade.HasClaim(ClaimTypes.Role, Perfis.Colaborador)
            ? RedirectToAction("Index", "AreaColaborador")
            : RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Sair()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Index));
    }

    public IActionResult AcessoNegado()
    {
        return View();
    }

    private static ClaimsIdentity IdentidadeDoAdministrador(string nomeUsuario, string senha)
    {
        Usuario usuario = new UsuarioDAO().BuscarPorNome(nomeUsuario);
        if (usuario == null || !usuario.SenhaConfere(senha))
            return null;

        return new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.NomeUsuario),
            new Claim(ClaimTypes.Role, Perfis.Admin)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
    }

    private static ClaimsIdentity IdentidadeDoColaborador(string funcional, string senha)
    {
        Colaborador colaborador = new ColaboradorDAO().ConsultaPorFuncional(funcional);
        if (colaborador == null || !colaborador.SenhaConfere(senha))
            return null;

        return new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, colaborador.Id.ToString()),
            new Claim(ClaimTypes.Name, colaborador.Nome),
            new Claim(ClaimTypes.Role, Perfis.Colaborador),
            new Claim("Funcional", colaborador.Funcional)
        }, CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
