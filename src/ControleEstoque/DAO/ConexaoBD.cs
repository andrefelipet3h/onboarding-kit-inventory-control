using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

/// <summary>
/// Cria as conexões com o banco. A string de conexão vem do appsettings.json.
/// </summary>
public static class ConexaoBD
{
    public static string StringConexao { get; set; }

    /// <summary>Devolve uma conexão aberta. Quem chamar fecha a conexão (using).</summary>
    public static SqlConnection GetConexao()
    {
        SqlConnection conexao = new SqlConnection(StringConexao);
        conexao.Open();
        return conexao;
    }
}
