using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

/// <summary>Acesso à tabela Usuarios (login do administrador). Somente SQL, sem regras.</summary>
public class UsuarioDAO
{
    /// <summary>Devolve null se o usuário não existir.</summary>
    public Usuario BuscarPorNome(string nomeUsuario)
    {
        var p = new SqlParameter[] { new SqlParameter("nomeUsuario", nomeUsuario) };
        DataTable tabela = HelperDAO.ExecutaProcSelect("spBuscaUsuario", p);
        if (tabela.Rows.Count == 0)
            return null;

        DataRow registro = tabela.Rows[0];
        return new Usuario
        {
            Id = Convert.ToInt32(registro["Id"]),
            NomeUsuario = registro["NomeUsuario"].ToString(),
            SenhaHash = registro["SenhaHash"].ToString(),
            CriadoEm = Convert.ToDateTime(registro["CriadoEm"])
        };
    }

    public int Contar()
    {
        DataTable tabela = HelperDAO.ExecutaProcSelect("spContaUsuarios", null);
        return Convert.ToInt32(tabela.Rows[0][0]);
    }

    public void Inserir(Usuario usuario)
    {
        SqlParameter[] p =
        {
            new SqlParameter("nomeUsuario", usuario.NomeUsuario),
            new SqlParameter("senhaHash", usuario.SenhaHash),
            new SqlParameter("criadoEm", usuario.CriadoEm)
        };
        HelperDAO.ExecutaProc("spInsert_Usuarios", p);
    }
}
