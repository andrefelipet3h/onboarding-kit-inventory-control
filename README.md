# Controle de Estoque - Kits de Onboarding

Trabalho de Linguagem de Programação I - Engenharia de Computação - Prof. Rosalem

Alunos:
- Andre Felipe - RA 082240018
- Leticia Marin - RA 082240017

## Sobre o sistema

Sistema para controlar os kits entregues aos novos colaboradores de uma empresa (notebook, mouse, mochila, camiseta...).

O administrador cadastra os departamentos, os produtos e os colaboradores, dá entrada no estoque e monta o kit de cada colaborador. Os itens do kit são retirados do estoque do produto. O colaborador entra com o funcional e a senha para ver o kit dele e confirmar que recebeu.

## Tecnologias

- C# com ASP.NET Core MVC (.NET 9)
- SQL Server com stored procedures
- Bootstrap, JavaScript e jQuery (AJAX)
- Swagger para as APIs
- xUnit para os testes

## Como rodar

1. Abrir o arquivo `BancoDeDados/script.sql` no SQL Server Management Studio e executar (F5). O script cria o banco `ControleEstoqueDB` com as tabelas, as procedures e alguns dados de exemplo.
2. Conferir a string de conexão em `src/ControleEstoque/appsettings.json`. Ela está com `Data Source=localhost` e login do Windows. Se o SQL Server usar usuário e senha, trocar por:
   `Data Source=LOCALHOST; Initial Catalog=ControleEstoqueDB; user id=sa; password=123456; TrustServerCertificate=True`
3. Abrir o `ControleEstoque.sln` no Visual Studio e rodar (F5).

Para rodar o script de novo, é preciso excluir o banco `ControleEstoqueDB` antes.

## Usuários de teste

| Perfil | Usuário | Senha |
|---|---|---|
| Administrador | admin | admin123 |
| Colaborador | funcional, ex.: 1000005 | 123456 |

Os colaboradores de exemplo vão do funcional 1000001 ao 1000006. O 1000005 tem um kit esperando a confirmação de recebimento e o 1000006 ainda não tem kit.

## APIs

- `GET /api/kits/por-departamento` - kits montados e entregues por departamento
- `GET /api/kits/produtos-mais-usados?top=5` - produtos mais usados nos kits

A documentação fica em `/swagger` e a coleção do Postman está na pasta `Postman`.

## Pastas

- `BancoDeDados` - script do banco e fotos usadas nos dados de exemplo
- `Postman` - coleção para testar as APIs
- `src/ControleEstoque` - projeto MVC (Controllers, DAO, Models, Views)
- `tests/ControleEstoque.Tests` - testes
