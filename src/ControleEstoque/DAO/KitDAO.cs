using System.Data;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;

namespace ControleEstoque.DAO;

/// <summary>
/// DAO do kit (mestre) e dos itens (detalhe). Incluir, alterar e excluir o kit
/// rodam em uma transação junto com a baixa do estoque dos produtos.
/// </summary>
public class KitDAO : PadraoDAO<Kit>
{
    private readonly KitItemDAO _itemDAO = new KitItemDAO();
    private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();

    protected override SqlParameter[] CriaParametros(Kit model)
    {
        SqlParameter[] parametros =
        {
            new SqlParameter("id", model.Id),
            new SqlParameter("colaboradorId", model.ColaboradorId),
            new SqlParameter("dataMontagem", model.DataMontagem),
            new SqlParameter("situacao", model.Situacao),
            new SqlParameter("observacao", HelperDAO.NullAsDbNull(model.Observacao)),
            new SqlParameter("dataEntrega", SqlDbType.DateTime) { Value = HelperDAO.NullAsDbNull(model.DataEntrega) }
        };
        return parametros;
    }

    protected override Kit MontaModel(DataRow registro)
    {
        var kit = new Kit
        {
            Id = Convert.ToInt32(registro["Id"]),
            ColaboradorId = Convert.ToInt32(registro["ColaboradorId"]),
            DataMontagem = Convert.ToDateTime(registro["DataMontagem"]),
            Situacao = registro["Situacao"].ToString(),
            Observacao = registro["Observacao"].ToString()
        };
        if (registro["DataEntrega"] != DBNull.Value)
            kit.DataEntrega = Convert.ToDateTime(registro["DataEntrega"]);
        if (registro.Table.Columns.Contains("NomeColaborador"))
        {
            kit.NomeColaborador = registro["NomeColaborador"].ToString();
            kit.Funcional = registro["Funcional"].ToString();
            kit.NomeDepartamento = registro["NomeDepartamento"].ToString();
            kit.TotalUnidades = Convert.ToInt32(registro["TotalUnidades"]);
        }
        return kit;
    }

    protected override void SetTabela()
    {
        Tabela = "Kits";
        NomeSpListagem = "spListagem_Kits";
        ChaveIdentity = true;
    }

    /// <summary>Consulta o kit já com os itens.</summary>
    public override Kit Consulta(int id)
    {
        Kit kit = base.Consulta(id);
        if (kit != null)
            kit.Itens = _itemDAO.ListagemPorKit(id);
        return kit;
    }

    public Kit ConsultaPorColaborador(int colaboradorId)
    {
        var p = new SqlParameter[] { new SqlParameter("colaboradorId", colaboradorId) };
        var tabela = HelperDAO.ExecutaProcSelect("spConsultaKitDoColaborador", p);
        if (tabela.Rows.Count == 0)
            return null;
        Kit kit = MontaModel(tabela.Rows[0]);
        kit.Itens = _itemDAO.ListagemPorKit(kit.Id);
        return kit;
    }

    /// <summary>Monta o kit: grava mestre e detalhe e baixa o estoque de cada produto.</summary>
    public override int Insert(Kit kit)
    {
        using (var transacao = HelperDAO.NovaTransacao())
        {
            kit.DataMontagem = DateTime.Now;
            kit.Situacao = Kit.Montado;
            kit.DataEntrega = null;

            AplicaDiferencaNoEstoque(new List<KitItem>(), kit.Itens);
            try
            {
                kit.Id = base.Insert(kit);
            }
            catch (SqlException erro) when (erro.Number == 2627 || erro.Number == 2601) // chave única
            {
                throw new EstoqueException("Este colaborador já tem um kit.");
            }
            GravaItens(kit);

            transacao.Complete();
            return kit.Id;
        }
    }

    /// <summary>Altera o kit: devolve ao estoque o que saiu e baixa o que entrou.</summary>
    public override void Update(Kit kit)
    {
        using (var transacao = HelperDAO.NovaTransacao())
        {
            Kit antigo = Consulta(kit.Id) ?? throw new EstoqueException("Kit não encontrado.");
            if (!antigo.PodeAlterar)
                throw new EstoqueException("Um kit já entregue não pode ser alterado.");

            kit.DataMontagem = antigo.DataMontagem;
            kit.Situacao = antigo.Situacao;
            kit.DataEntrega = antigo.DataEntrega;

            AplicaDiferencaNoEstoque(antigo.Itens, kit.Itens);
            base.Update(kit);
            _itemDAO.ExcluiPorKit(kit.Id);
            GravaItens(kit);

            transacao.Complete();
        }
    }

    /// <summary>Exclui o kit (só se ainda não foi entregue): os itens voltam ao estoque.</summary>
    public override void Delete(int id)
    {
        using (var transacao = HelperDAO.NovaTransacao())
        {
            Kit antigo = Consulta(id) ?? throw new EstoqueException("Kit não encontrado.");
            if (!antigo.PodeAlterar)
                throw new EstoqueException("Um kit já entregue não pode ser excluído.");

            AplicaDiferencaNoEstoque(antigo.Itens, new List<KitItem>());
            _itemDAO.ExcluiPorKit(id);
            base.Delete(id);

            transacao.Complete();
        }
    }

    /// <summary>Marca o kit como entregue ao colaborador. Depois disso ele não muda mais.</summary>
    public void Entregar(int id)
    {
        Kit kit = base.Consulta(id) ?? throw new EstoqueException("Kit não encontrado.");
        if (kit.Situacao == Kit.Entregue)
            throw new EstoqueException("Este kit já foi entregue.");

        kit.Situacao = Kit.Entregue;
        kit.DataEntrega = DateTime.Now;
        base.Update(kit);
    }

    public List<Kit> ConsultaAvancada(string colaborador, int departamentoId, string situacao,
                                      DateTime dataInicial, DateTime dataFinal)
    {
        SqlParameter[] p =
        {
            new SqlParameter("colaborador", colaborador),
            new SqlParameter("departamentoId", departamentoId),
            new SqlParameter("situacao", situacao),
            new SqlParameter("dataInicial", dataInicial),
            new SqlParameter("dataFinal", dataFinal)
        };
        return MontaLista(HelperDAO.ExecutaProcSelect("spConsultaAvancadaKits", p));
    }

    private void GravaItens(Kit kit)
    {
        foreach (KitItem item in kit.Itens)
        {
            item.KitId = kit.Id;
            _itemDAO.Insert(item);
        }
    }

    // Aplica no estoque a diferença entre os itens antigos e os novos.
    // Os produtos são lidos em ordem de Id para evitar deadlock.
    private void AplicaDiferencaNoEstoque(List<KitItem> antigos, List<KitItem> novos)
    {
        var produtos = antigos.Select(i => i.ProdutoId).Union(novos.Select(i => i.ProdutoId)).OrderBy(id => id);

        foreach (int produtoId in produtos)
        {
            int diferenca = novos.Where(i => i.ProdutoId == produtoId).Sum(i => i.Quantidade)
                          - antigos.Where(i => i.ProdutoId == produtoId).Sum(i => i.Quantidade);
            if (diferenca == 0)
                continue;

            Produto produto = _produtoDAO.LeEstoqueParaAlterar(produtoId);
            try
            {
                if (diferenca > 0)
                    produto.Estoque.RetirarParaKit(diferenca);
                else
                    produto.Estoque.DevolverDoKit(-diferenca);
            }
            catch (EstoqueException erro)
            {
                throw new EstoqueException($"{produto.Nome}: {erro.Message}");
            }
            _produtoDAO.SalvaEstoque(produto);
        }
    }
}
