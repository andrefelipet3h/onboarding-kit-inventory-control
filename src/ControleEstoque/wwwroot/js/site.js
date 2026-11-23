// Funções gerais

function confirmarExclusao() {
    return confirm('Confirma a exclusão do registro?');
}

// pré-visualização da foto escolhida no formulário
function exibirImagem() {
    var arquivo = document.getElementById('Imagem').files[0];
    if (arquivo == undefined)
        return;

    var leitor = new FileReader();
    leitor.readAsDataURL(arquivo);
    leitor.onload = function (evento) {
        document.getElementById('imgPreview').src = evento.target.result;
    };
}

function fotoOuPadrao(fotoBase64) {
    return fotoBase64 ? 'data:image/jpeg;base64,' + fotoBase64 : '/img/sem-foto.svg';
}

// evita que um texto digitado no cadastro (ex.: nome com "<") seja interpretado como HTML
function escapar(texto) {
    var div = document.createElement('div');
    div.textContent = texto;
    return div.innerHTML.replace(/"/g, '&quot;');
}

// AJAX 1: dados do colaborador na montagem do kit

function carregarDadosColaborador() {
    var id = document.getElementById('ColaboradorId').value;
    var area = document.getElementById('dadosColaborador');

    if (id == 0) {
        area.innerHTML = '';
        return;
    }

    $.ajax({
        url: '/Kits/DadosColaborador',
        data: { id: id },
        success: function (dados) {
            if (dados.erro != undefined) {
                alert(dados.msg);
                return;
            }
            area.innerHTML =
                '<img src="' + fotoOuPadrao(dados.foto) + '" class="foto-mini" alt="" />' +
                '<div><strong>' + escapar(dados.nome) + '</strong><br />' +
                '<span class="dica">Funcional ' + escapar(dados.funcional) + ' &middot; ' + escapar(dados.departamento) +
                ' &middot; admissão em ' + dados.admissao + '</span></div>';
        }
    });
}

// AJAX 2: estoque do produto na montagem do kit

var produtoEscolhido = null;

function carregarEstoqueProduto() {
    var id = document.getElementById('produtoItem').value;
    var area = document.getElementById('infoProduto');
    produtoEscolhido = null;

    if (id == 0) {
        area.innerHTML = '';
        return;
    }

    $.ajax({
        url: '/Kits/EstoqueProduto',
        data: { id: id },
        beforeSend: function () {
            area.innerHTML = '<span class="dica">Consultando o estoque...</span>';
        },
        success: function (dados) {
            if (dados.erro != undefined) {
                alert(dados.msg);
                return;
            }
            produtoEscolhido = dados;
            area.innerHTML =
                '<img src="' + fotoOuPadrao(dados.foto) + '" class="foto-mini" alt="" />' +
                '<span><strong>' + escapar(dados.nome) + '</strong> (' + escapar(dados.categoria) + ')<br />' +
                '<span class="' + (dados.quantidade > 0 ? '' : 'txt-erro') + '">' +
                dados.quantidade + ' unidade(s) em estoque</span></span>';
        }
    });
}

// Itens do kit (detalhe)

function adicionarItemKit() {
    var produtoId = document.getElementById('produtoItem').value;
    var quantidade = parseInt(document.getElementById('quantidadeItem').value);

    if (produtoId == 0 || produtoEscolhido == null) {
        alert('Escolha o produto.');
        return;
    }
    if (isNaN(quantidade) || quantidade <= 0) {
        alert('Informe uma quantidade maior que zero.');
        return;
    }
    if (document.querySelector('#itensKit input[name$=".ProdutoId"][value="' + produtoId + '"]') != null) {
        alert('Este produto já está no kit. Remova-o para trocar a quantidade.');
        return;
    }
    if (quantidade > produtoEscolhido.quantidade)
        alert('Atenção: só há ' + produtoEscolhido.quantidade + ' unidade(s) em estoque. O sistema vai recusar ao salvar.');

    var linha = document.createElement('tr');
    linha.innerHTML =
        '<td><img src="' + fotoOuPadrao(produtoEscolhido.foto) + '" class="foto-mini" alt="" /></td>' +
        '<td><input type="hidden" data-campo="ProdutoId" value="' + produtoId + '" />' +
        '<input type="hidden" data-campo="NomeProduto" value="' + escapar(produtoEscolhido.nome) + '" />' +
        escapar(produtoEscolhido.nome) + '</td>' +
        '<td class="text-end"><input type="hidden" data-campo="Quantidade" value="' + quantidade + '" />' + quantidade + '</td>' +
        '<td class="acoes-linha"><button type="button" class="btn btn-sm btn-outline-danger" onclick="removerItemKit(this)"><i class="bi bi-x-lg"></i></button></td>';
    document.getElementById('itensKit').appendChild(linha);

    document.getElementById('produtoItem').value = 0;
    document.getElementById('quantidadeItem').value = 1;
    document.getElementById('infoProduto').innerHTML = '';
    produtoEscolhido = null;
    renumerarItensKit();
}

function removerItemKit(botao) {
    botao.closest('tr').remove();
    renumerarItensKit();
}

// o ASP.NET só monta a lista se os índices forem 0, 1, 2... sem buracos
function renumerarItensKit() {
    var linhas = document.querySelectorAll('#itensKit tr');
    linhas.forEach(function (linha, indice) {
        linha.querySelectorAll('input[data-campo]').forEach(function (campo) {
            campo.name = 'Itens[' + indice + '].' + campo.dataset.campo;
        });
    });
    document.getElementById('semItens').style.display = linhas.length == 0 ? '' : 'none';
}

// AJAX 3: consulta avançada de kits

function aplicaFiltroConsultaKits() {
    $.ajax({
        url: '/Consultas/ObtemDadosConsultaKits',
        data: {
            colaborador: $('#colaborador').val(),
            departamentoId: $('#departamento').val(),
            situacao: $('#situacao').val(),
            dataInicial: $('#dataInicial').val(),
            dataFinal: $('#dataFinal').val()
        },
        beforeSend: function () {
            $('#resultadoConsulta').html('<p class="dica p-3">Consultando...</p>');
        },
        success: function (dados) {
            if (dados.erro != undefined) {
                alert(dados.msg);
                $('#resultadoConsulta').html('');
            }
            else
                $('#resultadoConsulta').html(dados);
        }
    });
}

// AJAX 4: consulta avançada de produtos usados nos kits

function aplicaFiltroConsultaItens() {
    $.ajax({
        url: '/Consultas/ObtemDadosConsultaItens',
        data: {
            produtoId: $('#produto').val(),
            categoria: $('#categoria').val(),
            departamentoId: $('#departamento').val(),
            dataInicial: $('#dataInicial').val(),
            dataFinal: $('#dataFinal').val()
        },
        beforeSend: function () {
            $('#resultadoConsulta').html('<p class="dica p-3">Consultando...</p>');
        },
        success: function (dados) {
            if (dados.erro != undefined) {
                alert(dados.msg);
                $('#resultadoConsulta').html('');
            }
            else
                $('#resultadoConsulta').html(dados);
        }
    });
}

// AJAX 5: Home administrativa consumindo a API

function carregarProdutosMaisUsados() {
    $.ajax({
        url: '/api/kits/produtos-mais-usados',
        data: { top: 5 },
        dataType: 'json',
        success: function (produtos) {
            if (produtos.length == 0) {
                $('#maisUsados').html('<tr><td colspan="3" class="vazio">Nenhum kit montado ainda.</td></tr>');
                return;
            }
            var html = '';
            for (var n = 0; n < produtos.length; n++) {
                html += '<tr><td>' + escapar(produtos[n].nomeProduto) + '</td>' +
                        '<td class="text-end">' + produtos[n].qtdKits + '</td>' +
                        '<td class="text-end">' + produtos[n].qtdUnidades + '</td></tr>';
            }
            $('#maisUsados').html(html);
        },
        error: function () {
            $('#maisUsados').html('<tr><td colspan="3" class="vazio">Não foi possível carregar.</td></tr>');
        }
    });
}
