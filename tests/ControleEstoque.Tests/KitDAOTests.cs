using ControleEstoque.DAO;
using ControleEstoque.Models;
using Microsoft.Data.SqlClient;
using Xunit;

namespace ControleEstoque.Tests;

/// <summary>Testes da montagem do kit usando o banco de testes.</summary>
[Collection("Banco de testes")]
public class KitDAOTests
{
    private readonly KitDAO _kitDAO = new KitDAO();
    private readonly ProdutoDAO _produtoDAO = new ProdutoDAO();
    private readonly EstoqueDAO _estoqueDAO = new EstoqueDAO();

    private const int Notebook = 1;
    private const int Mouse = 2;

    public KitDAOTests(BancoDeTestes banco)
    {
        banco.Zerar();
        NovoProduto(Notebook, "Notebook", 5);
        NovoProduto(Mouse, "Mouse", 10);
        NovoColaborador(1, "1000001", departamentoId: 1);
        NovoColaborador(2, "1000002", departamentoId: 2);
    }

    private void NovoProduto(int id, string nome, int estoque)
    {
        _produtoDAO.Insert(new Produto { Id = id, Nome = nome, Categoria = "Teste" });
        if (estoque > 0)
            _estoqueDAO.Entrada(id, estoque);
    }

    private static void NovoColaborador(int id, string funcional, int departamentoId)
    {
        var c = new Colaborador
        {
            Id = id,
            Nome = "Colaborador " + id,
            Funcional = funcional,
            Email = $"colaborador{id}@teste.com",
            DepartamentoId = departamentoId,
            DataAdmissao = DateTime.Today
        };
        c.DefinirSenha("123456");
        new ColaboradorDAO().Insert(c);
    }

    private static Kit KitCom(int colaboradorId, params (int produtoId, int quantidade)[] itens) => new Kit
    {
        ColaboradorId = colaboradorId,
        Itens = itens.Select(i => new KitItem { ProdutoId = i.produtoId, Quantidade = i.quantidade }).ToList()
    };

    private int EmEstoque(int produtoId) => _produtoDAO.Consulta(produtoId).Estoque.Quantidade;

    // Montagem

    [Fact]
    public void Montar_kit_grava_mestre_e_detalhe_e_baixa_o_estoque_de_cada_produto()
    {
        int id = _kitDAO.Insert(KitCom(1, (Notebook, 1), (Mouse, 2)));

        Kit kit = _kitDAO.Consulta(id);
        Assert.Equal(Kit.Montado, kit.Situacao);
        Assert.Equal(2, kit.Itens.Count);
        Assert.Equal("Mouse", kit.Itens.Single(i => i.ProdutoId == Mouse).NomeProduto);
        Assert.Equal(4, EmEstoque(Notebook));
        Assert.Equal(8, EmEstoque(Mouse));
    }

    [Fact]
    public void Kit_sem_estoque_suficiente_nao_grava_nada()
    {
        // o mouse tem estoque, o notebook não: nada pode ficar gravado, nem a baixa do mouse
        var erro = Assert.Throws<EstoqueException>(() => _kitDAO.Insert(KitCom(1, (Mouse, 2), (Notebook, 6))));

        Assert.Contains("Notebook", erro.Message);
        Assert.Empty(_kitDAO.Listagem());
        Assert.Equal(10, EmEstoque(Mouse));
        Assert.Equal(5, EmEstoque(Notebook));
    }

    [Fact]
    public void Colaborador_so_pode_ter_um_kit_e_o_estoque_nao_e_baixado_de_novo()
    {
        _kitDAO.Insert(KitCom(1, (Mouse, 1)));

        Assert.Throws<EstoqueException>(() => _kitDAO.Insert(KitCom(1, (Mouse, 1))));

        Assert.Single(_kitDAO.Listagem());
        Assert.Equal(9, EmEstoque(Mouse));
    }

    [Fact]
    public void Alterar_kit_devolve_e_baixa_so_a_diferenca()
    {
        int id = _kitDAO.Insert(KitCom(1, (Notebook, 1), (Mouse, 3)));

        // tira o notebook e reduz o mouse de 3 para 1
        Kit alterado = KitCom(1, (Mouse, 1));
        alterado.Id = id;
        _kitDAO.Update(alterado);

        Assert.Equal(5, EmEstoque(Notebook));
        Assert.Equal(9, EmEstoque(Mouse));
        Assert.Single(_kitDAO.Consulta(id).Itens);
    }

    [Fact]
    public void Excluir_kit_devolve_as_unidades_ao_estoque()
    {
        int id = _kitDAO.Insert(KitCom(1, (Notebook, 2), (Mouse, 2)));

        _kitDAO.Delete(id);

        Assert.Null(_kitDAO.Consulta(id));
        Assert.Equal(5, EmEstoque(Notebook));
        Assert.Equal(10, EmEstoque(Mouse));
    }

    [Fact]
    public void Kit_entregue_nao_pode_ser_alterado_nem_excluido()
    {
        int id = _kitDAO.Insert(KitCom(1, (Mouse, 1)));
        _kitDAO.Entregar(id);

        Kit alterado = KitCom(1, (Mouse, 5));
        alterado.Id = id;

        Assert.Throws<EstoqueException>(() => _kitDAO.Update(alterado));
        Assert.Throws<EstoqueException>(() => _kitDAO.Delete(id));
        Assert.Throws<EstoqueException>(() => _kitDAO.Entregar(id));

        Kit kit = _kitDAO.Consulta(id);
        Assert.Equal(Kit.Entregue, kit.Situacao);
        Assert.NotNull(kit.DataEntrega);
        Assert.Equal(9, EmEstoque(Mouse));
    }

    [Fact]
    public void Falha_ao_gravar_os_itens_devolve_as_unidades_ao_estoque()
    {
        // se o insert dos itens falhar, o kit e a baixa do estoque também são desfeitos
        using (var conexao = ConexaoBD.GetConexao())
            BancoDeTestes.Executar(conexao, "create trigger falha on KitItens instead of insert as throw 50000, 'falha simulada', 1;");
        try
        {
            Assert.ThrowsAny<Exception>(() => _kitDAO.Insert(KitCom(1, (Mouse, 2))));
        }
        finally
        {
            using var conexao = ConexaoBD.GetConexao();
            BancoDeTestes.Executar(conexao, "drop trigger falha");
        }

        Assert.Empty(_kitDAO.Listagem());
        Assert.Equal(10, EmEstoque(Mouse));
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("12345a7")]
    public void O_proprio_banco_recusa_funcional_fora_do_padrao(string funcional)
    {
        using var conexao = ConexaoBD.GetConexao();

        Assert.Throws<SqlException>(() => BancoDeTestes.Executar(conexao,
            "insert into Colaboradores (Id, Nome, Funcional, Email, DepartamentoId, DataAdmissao, SenhaHash) " +
            $"values (99, 'X', '{funcional}', 'x@x.com', 1, getdate(), 'hash')"));
    }

    // Concorrência

    [Fact]
    public void Montagens_simultaneas_nunca_passam_do_estoque()
    {
        for (int n = 3; n <= 20; n++)
            NovoColaborador(n, (2000000 + n).ToString(), departamentoId: 1);

        int montados = 0, recusados = 0;

        // 20 colaboradores disputando 5 notebooks.
        var tarefas = Enumerable.Range(1, 20).Select(n => Task.Run(() =>
        {
            try
            {
                _kitDAO.Insert(KitCom(n, (Notebook, 1)));
                Interlocked.Increment(ref montados);
            }
            catch (EstoqueException)
            {
                Interlocked.Increment(ref recusados);
            }
        })).ToArray();

        Task.WaitAll(tarefas);

        Assert.Equal(5, montados);
        Assert.Equal(15, recusados);
        Assert.Equal(5, _kitDAO.Listagem().Count);
        Assert.Equal(0, EmEstoque(Notebook));
    }

    // Consultas e APIs

    [Fact]
    public void Consulta_de_kits_filtra_por_departamento_e_situacao()
    {
        int kit1 = _kitDAO.Insert(KitCom(1, (Mouse, 1)));
        _kitDAO.Insert(KitCom(2, (Mouse, 1)));
        _kitDAO.Entregar(kit1);

        var inicio = new DateTime(2000, 1, 1);
        var fim = DateTime.Now.AddDays(1);

        Assert.Equal(2, _kitDAO.ConsultaAvancada("", 0, "", inicio, fim).Count);
        Assert.Equal(kit1, _kitDAO.ConsultaAvancada("", 1, "", inicio, fim).Single().Id);
        Assert.Equal(kit1, _kitDAO.ConsultaAvancada("", 0, Kit.Entregue, inicio, fim).Single().Id);
        Assert.Single(_kitDAO.ConsultaAvancada("1000002", 0, "", inicio, fim));
        Assert.Empty(_kitDAO.ConsultaAvancada("", 0, "", inicio, inicio.AddDays(1)));
    }

    [Fact]
    public void Dados_consolidados_por_departamento_e_por_produto()
    {
        _kitDAO.Insert(KitCom(1, (Notebook, 1), (Mouse, 2)));
        _kitDAO.Insert(KitCom(2, (Mouse, 1)));

        var consulta = new ConsultaDAO();
        var porDepartamento = consulta.KitsPorDepartamento(new DateTime(2000, 1, 1), DateTime.Now.AddDays(1));
        var maisUsados = consulta.ProdutosMaisUsados(5);

        Assert.Equal(3, porDepartamento.Single(d => d.DepartamentoId == 1).TotalUnidades);
        Assert.Equal(1, porDepartamento.Single(d => d.DepartamentoId == 2).TotalKits);
        Assert.Equal("Mouse", maisUsados.First().NomeProduto);
        Assert.Equal(3, maisUsados.First().QtdUnidades);
        Assert.Equal(2, maisUsados.First().QtdKits);
    }
}
