using System.Data;
using System.Transactions;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

public static class HelperDAO
{
    /// <summary>
    /// Executa uma stored procedure que não retorna registros (insert, update, delete).
    /// </summary>
    /// <param name="nomeProc">nome da stored procedure</param>
    /// <param name="parametros">parâmetros da procedure (ou null)</param>
    /// <param name="consultaUltimoIdentity">quando true, retorna o último id gerado em campos identity</param>
    public static int ExecutaProc(string nomeProc, SqlParameter[] parametros, bool consultaUltimoIdentity = false)
    {
        using (SqlConnection conexao = ConexaoBD.GetConexao())
        {
            using (SqlCommand comando = new SqlCommand(nomeProc, conexao))
            {
                comando.CommandType = CommandType.StoredProcedure;
                if (parametros != null)
                    comando.Parameters.AddRange(parametros);
                comando.ExecuteNonQuery();

                if (consultaUltimoIdentity)
                {
                    comando.CommandType = CommandType.Text;
                    comando.CommandText = "select isnull(@@IDENTITY, 0)";
                    comando.Parameters.Clear();
                    return Convert.ToInt32(comando.ExecuteScalar());
                }
                else
                    return 0;
            }
        }
    }

    /// <summary>Executa uma stored procedure de consulta.</summary>
    /// <returns>DataTable com os registros retornados</returns>
    public static DataTable ExecutaProcSelect(string nomeProc, SqlParameter[] parametros)
    {
        using (SqlConnection conexao = ConexaoBD.GetConexao())
        {
            using (SqlDataAdapter adapter = new SqlDataAdapter(nomeProc, conexao))
            {
                if (parametros != null)
                    adapter.SelectCommand.Parameters.AddRange(parametros);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                DataTable tabela = new DataTable();
                adapter.Fill(tabela);
                return tabela;
            }
        }
    }

    /// <summary>Converte o null do C# para o DBNull, que é o nulo aceito pelo banco.</summary>
    public static object NullAsDbNull(object valor) => valor ?? DBNull.Value;

    /// <summary>
    /// Abre uma transação: todas as conexões abertas até o Complete() fazem parte dela.
    /// Sem o Complete() (por erro), o Dispose desfaz tudo.
    /// </summary>
    public static TransactionScope NovaTransacao()
    {
        var opcoes = new TransactionOptions { IsolationLevel = System.Transactions.IsolationLevel.ReadCommitted };
        return new TransactionScope(TransactionScopeOption.Required, opcoes);
    }
}
