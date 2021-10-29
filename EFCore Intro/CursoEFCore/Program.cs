using System;
using System.Collections.Generic;
using System.Linq;
using CursoEFCore.Domain;
using CursoEFCore.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace CursoEFCore
{
    class Program
    {
        static void Main(string[] args)
        {
            using var db = new Data.ApplicationContext();

            //Aplica migrações ao subir aplicação.
            //Não é indicado para produção.
            // db.Database.Migrate();

            //Verifica se existe migração pendente.
            var existe = db.Database.GetPendingMigrations().Any();
            if (existe)
            {
                //Faz algo...
            }

            Console.WriteLine("Hello World!");
            
            //InserirProduto();
            //InserirDadosEmMassa();
            //ConsultarDados();
            //CadastrarPedido();
            //ConsultarPedidoCarregamentoAdiantado();
            AtualizarDados();
        }

        private static void InserirProduto()
        {
            var produto = new Produto
            {
                Descricao = "Produto Teste",
                CodigoBarras = "123456",
                Valor = 10m,
                TipoProduto = TipoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            using var db = new Data.ApplicationContext();

            //***Maneiras de incluir dados.
            //Não vai funcionar por conta do ApplyConfigurationsFromAssembly.
            //db.Produtos.Add(produto);
            //db.Set<Produto>().Add(produto);
            //db.Entry(produto).State = EntityState.Added;
            db.Add(produto);

            var registros = db.SaveChanges();
            Console.WriteLine($"Total registros: {registros}");
        }

        private static void InserirDadosEmMassa()
        {
            var produto = new Produto
            {
                Descricao = "Produto Teste",
                CodigoBarras = "123456",
                Valor = 10m,
                TipoProduto = TipoProduto.MercadoriaParaRevenda,
                Ativo = true
            };

            var cliente = new Cliente
            {
                Nome = "Nome cliente",
                CEP = "00000000",
                Cidade = "Osasco",
                Estado = "SP",
                Telefone = "99000000000"
            };

            using var db = new Data.ApplicationContext();
            //Também funciona com lista.
            db.AddRange(produto, cliente);

            var registros = db.SaveChanges();
            Console.WriteLine($"Total registros: {registros}");
        }

        private static void ConsultarDados()
        {
            using var db = new Data.ApplicationContext();

            //AsNoTracking informa ao EF para não rastrear os objetos (funciona com Find(), ele consulta em memória primeiro, se não houver a PK em memória ele vai no BD).
            var consultaPorSintaxe = (from c in db.Set<Cliente>().AsNoTracking() where c.Id > 0 select c).ToList();
            var consultaPorMetodo = db.Set<Cliente>().AsNoTracking().Where(p => p.Id > 0).ToList();

            foreach (var cliente in consultaPorMetodo)
            {
                Console.WriteLine($"Consultando cliente: {cliente.Id}");

                //Consulta pela PK.
                db.Set<Cliente>().Find(cliente.Id);
            }
        }

        private static void CadastrarPedido()
        {
            using var db = new Data.ApplicationContext();

            var cliente = db.Set<Cliente>().FirstOrDefault();
            var produto = db.Set<Produto>().FirstOrDefault();

            var pedido = new Pedido
            {
                ClienteId = cliente.Id,
                IniciadoEm = DateTime.Now,
                FinalizadoEm = DateTime.Now,
                Observacao = "Pedido teste",
                Status = StatusPedido.Analise,
                TipoFrete = TipoFrete.SemFrete,
                Itens = new List<PedidoItem>
                {
                    new PedidoItem { ProdutoId = produto.Id, Desconto = 0, Quantidade = 1, Valor = 10 }
                }
            };

            db.Set<Pedido>().Add(pedido);
            db.SaveChanges();
        }

        private static void ConsultarPedidoCarregamentoAdiantado()
        {
            using var db = new Data.ApplicationContext();

            //Com o include é possível solicitar que o objeto também carregue os itens do pedido.
            //var pedidos = db.Set<Pedido>().Include(p => p.Itens).ToList();

            //Outra forma de solicitar o carregamento dos itens, enviando o nome da propriedade por string.
            //var pedidos = db.Set<Pedido>().Include("Itens").ToList();

            //Include nos itens do pedido, e dentro dos itens os produtos também.
            var pedidos = db.Set<Pedido>().Include(p => p.Itens).ThenInclude(p=>p.Produto).ToList();

            Console.Write(pedidos.Count);
        }

        private static void AtualizarDados()
        {
            using var db = new Data.ApplicationContext();
            var cliente = db.Set<Cliente>().FirstOrDefault();
            cliente.Nome = "Cliente alterado passo 01";

            //Informa de forma explícita que houve alteração, mas não é necessário pq qualquer alteração feita no dbContext o EF já rastreia.
            db.Entry(cliente).State = EntityState.Modified;

            db.Set<Cliente>().Update(cliente);
            db.SaveChanges();
        }

        private static void CadastroClienteDesconectado()
        {
            using var db = new Data.ApplicationContext();

            var cliente = new Cliente
            {
                Id = 1
            };

            //Inclui o o objeto no dbcontext, sem precisar consultar no BD, apenas para atualizar.
            db.Attach(cliente);

            //Variável anônima.
            var clienteDesconectado = new
            {
                Nome = "Cliente desconectado",
                Teqlefone = "4400000000"
            };

            db.Entry(cliente).CurrentValues.SetValues(clienteDesconectado);

            db.SaveChanges();
        }

        private static void RemoverRegistro()
        {
            using var db = new Data.ApplicationContext();

            var cliente = db.Set<Cliente>().Find(1);

            //Opções para remover registros.
            db.Set<Cliente>().Remove(cliente);
            //db.Remove(cliente);
            //db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }

        private static void RemoverRegistroDesconectado()
        {
            using var db = new Data.ApplicationContext();

            var cliente = new Cliente { Id = 1 };

            //Opções para remover registros.
            //db.Set<Cliente>().Remove(cliente);
            //db.Remove(cliente);
            db.Entry(cliente).State = EntityState.Deleted;

            db.SaveChanges();
        }
    }
}
