using System.Globalization;
using System.Reflection;
using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// token antifalsificação (CSRF) em todos os POSTs
builder.Services.AddControllersWithViews(opcoes =>
    opcoes.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));

// login por cookie: toda tela exige login, menos as marcadas com [AllowAnonymous]
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(opcoes =>
    {
        opcoes.LoginPath = "/Login";
        opcoes.AccessDeniedPath = "/Login/AcessoNegado";
        opcoes.Cookie.Name = "ControleEstoque.Login";
        opcoes.Cookie.HttpOnly = true;
        opcoes.Cookie.SameSite = SameSiteMode.Lax;
        opcoes.ExpireTimeSpan = TimeSpan.FromHours(8);
        opcoes.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(opcoes =>
    opcoes.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

// Swagger (documentação das APIs)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opcoes =>
{
    opcoes.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Controle de Estoque - API",
        Version = "v1",
        Description = "Dados consolidados dos kits de onboarding montados no sistema."
    });
    opcoes.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
});

ConexaoBD.StringConexao = builder.Configuration.GetConnectionString("DefaultConnection");

var app = builder.Build();

// cria o usuário admin do appsettings.json se ainda não existir nenhum
var usuarioDAO = new UsuarioDAO();
if (usuarioDAO.Contar() == 0)
{
    var padrao = new Usuario
    {
        NomeUsuario = builder.Configuration["Login:UsuarioPadrao"] ?? "admin",
        CriadoEm = DateTime.Now
    };
    padrao.DefinirSenha(builder.Configuration["Login:SenhaPadrao"] ?? "admin123");
    usuarioDAO.Inserir(padrao);
}

if (!app.Environment.IsDevelopment())
    app.UseExceptionHandler("/Home/Error");
app.UseStatusCodePagesWithReExecute("/Home/Error", "?codigo={0}");

// datas e números no formato brasileiro
var cultura = new CultureInfo("pt-BR");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(cultura),
    SupportedCultures = new[] { cultura },
    SupportedUICultures = new[] { cultura }
});

app.UseStaticFiles();

app.UseSwagger();
app.UseSwaggerUI(opcoes => opcoes.SwaggerEndpoint("/swagger/v1/swagger.json", "Controle de Estoque - API v1"));

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("padrao", "{controller=Home}/{action=Index}/{id?}");

app.Run();
