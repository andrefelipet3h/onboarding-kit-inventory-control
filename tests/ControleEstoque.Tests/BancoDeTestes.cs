using System.Text.RegularExpressions;
using ControleEstoque.DAO;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.Tests;

/// <summary>
/// Cria o banco ControleEstoqueDB_Testes a partir do BancoDeDados/script.sql (no SQL Server local).
/// </summary>
public class BancoDeTestes
{
    public const string NomeBanco = "ControleEstoqueDB_Testes";
    private const string Servidor = @"Data Source=localhost; Integrated Security=True; TrustServerCertificate=True";

    public BancoDeTestes()
    {
        string script = File.ReadAllText(LocalizarScript()).Replace("ControleEstoqueDB", NomeBanco);

        using var conexao = new SqlConnection(Servidor + "; Initial Catalog=master");
        conexao.Open();
        Executar(conexao, $"if db_id('{NomeBanco}') is not null begin " +
                          $"alter database {NomeBanco} set single_user with rollback immediate; drop database {NomeBanco}; end");

        // o script usa "go" para separar os lotes, como no Management Studio
        foreach (string lote in Regex.Split(script, @"^\s*go\s*$", RegexOptions.Multiline | RegexOptions.IgnoreCase))
        {
            if (!string.IsNullOrWhiteSpace(lote))
                Executar(conexao, lote);
        }

        ConexaoBD.StringConexao = Servidor + "; Initial Catalog=" + NomeBanco;
    }

    /// <summary>Apaga os dados de exemplo e deixa só um departamento, para cada teste começar do zero.</summary>
    public void Zerar()
    {
        using var conexao = ConexaoBD.GetConexao();
        Executar(conexao, @"
            delete KitItens; delete Kits; delete Colaboradores; delete Produtos; delete Departamentos;
            insert into Departamentos (Id, Nome, CentroCusto) values (1, 'Tecnologia', 'CC-1');
            insert into Departamentos (Id, Nome, CentroCusto) values (2, 'Comercial', 'CC-2');");
    }

    public static void Executar(SqlConnection conexao, string sql)
    {
        using var comando = new SqlCommand(sql, conexao);
        comando.ExecuteNonQuery();
    }

    private static string LocalizarScript()
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta != null)
        {
            string caminho = Path.Combine(pasta.FullName, "BancoDeDados", "script.sql");
            if (File.Exists(caminho))
                return caminho;
            pasta = pasta.Parent;
        }
        throw new FileNotFoundException("Não encontrei BancoDeDados/script.sql.");
    }
}
