namespace ControleEstoque.Models;

/// <summary>Linha da consulta de produtos usados nos kits, por departamento.</summary>
public class ConsumoProduto
{
    public string NomeProduto { get; set; }
    public string Categoria { get; set; }
    public string NomeDepartamento { get; set; }
    public int QtdKits { get; set; }
    public int QtdUnidades { get; set; }
}

/// <summary>Resultado da API: kits montados e entregues de um departamento.</summary>
public class KitsPorDepartamento
{
    public int DepartamentoId { get; set; }
    public string NomeDepartamento { get; set; }
    public int TotalKits { get; set; }
    public int KitsMontados { get; set; }
    public int KitsEntregues { get; set; }
    public int TotalUnidades { get; set; }
}

/// <summary>Resultado da API: produtos mais usados nos kits.</summary>
public class ProdutoMaisUsado
{
    public int ProdutoId { get; set; }
    public string NomeProduto { get; set; }
    public string Categoria { get; set; }
    public int QtdKits { get; set; }
    public int QtdUnidades { get; set; }
    public int EmEstoque { get; set; }
}
