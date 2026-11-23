using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ControleEstoque.Controllers;

/// <summary>
/// APIs com dados consolidados dos kits de onboarding (funcionalidade principal do sistema).
/// </summary>
[ApiController]
[AllowAnonymous]
[Route("api/kits")]
[Produces("application/json")]
public class KitsApiController : ControllerBase
{
    /// <summary>
    /// Kits montados por departamento.
    /// </summary>
    /// <remarks>
    /// Consolida os kits de cada departamento: total de kits, quantos ainda estão montados,
    /// quantos já foram entregues e o total de unidades de produtos usadas.
    /// As datas filtram pela data de montagem do kit e são opcionais.
    ///
    /// Exemplo: GET /api/kits/por-departamento?dataInicial=2026-09-01&amp;dataFinal=2026-12-31
    /// </remarks>
    /// <param name="dataInicial">Data inicial (yyyy-MM-dd). Opcional.</param>
    /// <param name="dataFinal">Data final (yyyy-MM-dd). Opcional.</param>
    /// <response code="200">Lista de departamentos com os totais de kits</response>
    /// <response code="400">Período inválido</response>
    [HttpGet("por-departamento")]
    [ProducesResponseType(typeof(List<KitsPorDepartamento>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<List<KitsPorDepartamento>> PorDepartamento(DateTime? dataInicial, DateTime? dataFinal)
    {
        try
        {
            DateTime inicio = dataInicial ?? new DateTime(1900, 1, 1);
            DateTime fim = dataFinal?.Date.AddDays(1).AddSeconds(-1) ?? new DateTime(9999, 12, 31);
            if (fim < inicio)
                return BadRequest(new { erro = true, msg = "A data final deve ser maior que a data inicial." });

            return new ConsultaDAO().KitsPorDepartamento(inicio, fim);
        }
        catch (Exception erro)
        {
            return StatusCode(500, new { erro = true, msg = erro.Message });
        }
    }

    /// <summary>
    /// Produtos mais usados na montagem dos kits.
    /// </summary>
    /// <remarks>
    /// Ranking dos produtos pela quantidade de unidades colocadas nos kits,
    /// com o número de kits em que aparecem e a quantidade em estoque hoje.
    ///
    /// Exemplo: GET /api/kits/produtos-mais-usados?top=5
    /// </remarks>
    /// <param name="top">Quantidade de produtos retornados (1 a 50)</param>
    /// <response code="200">Lista de produtos ordenada pelas unidades usadas</response>
    /// <response code="400">Parâmetro top inválido</response>
    [HttpGet("produtos-mais-usados")]
    [ProducesResponseType(typeof(List<ProdutoMaisUsado>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public ActionResult<List<ProdutoMaisUsado>> ProdutosMaisUsados(int top = 5)
    {
        try
        {
            if (top < 1 || top > 50)
                return BadRequest(new { erro = true, msg = "O parâmetro top deve estar entre 1 e 50." });

            return new ConsultaDAO().ProdutosMaisUsados(top);
        }
        catch (Exception erro)
        {
            return StatusCode(500, new { erro = true, msg = erro.Message });
        }
    }
}
