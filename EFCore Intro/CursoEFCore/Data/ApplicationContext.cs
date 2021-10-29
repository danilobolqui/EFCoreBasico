using CursoEFCore.Configurations;
using CursoEFCore.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace CursoEFCore.Data
{
    public class ApplicationContext : DbContext
    {
        private static readonly ILoggerFactory _logger = LoggerFactory.Create(p=>p.AddConsole());

        public DbSet<Pedido> Pedidos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_logger).
                EnableSensitiveDataLogging().
                    UseSqlServer(@"Data source=.\SQLEXPRESS; Initial Catalog=teste; Integrated Security=true", 
                    //Torna a conexão mais resiliente a quedas de conexão.
                    p=>p.EnableRetryOnFailure(maxRetryCount: 2, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null)
                    //Informa o nome da tabela de migração conforme queira.
                    .MigrationsHistoryTable("cursoEFCoreMigrations"));

            //EnableRetryOnFailure = Tenta 6 vezes reconectar por padrão.
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Configuração via reflection, procurando todas entidades que implementam IEntityTypeConfiguration.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationContext).Assembly);

            //Configuração via arquivo de configuração de cada entidade.
            //modelBuilder.ApplyConfiguration(new ClienteConfiguration());

            //Configuração via FluentAPI centralizado no OnModelCreating.
            // modelBuilder.Entity<Cliente>(p=>
            // {
            //     p.ToTable("Clientes");
            //     p.HasKey(p => p.Id);
            //     p.Property(p => p.Nome).HasColumnType("VARCHAR(80)").IsRequired();
            //     p.Property(p => p.Telefone).HasColumnType("CHAR(11)");
            //     p.Property(p => p.CEP).HasColumnType("CHAR(8)").IsRequired();
            //     p.Property(p => p.Estado).HasColumnType("CHAR(2)").IsRequired();
            //     p.Property(p => p.Cidade).HasMaxLength(60).IsRequired();
            //     p.HasIndex(i => i.Telefone).HasDatabaseName("idx_cliente_telefone");
            // });

            // modelBuilder.Entity<Produto>(p =>
            // {
            //     p.ToTable("Produtos");
            //     p.HasKey(p => p.Id);
            //     p.Property(p => p.CodigoBarras).HasColumnType("VARCHAR(14)").IsRequired();
            //     p.Property(p => p.Descricao).HasColumnType("VARCHAR(60)");
            //     p.Property(p => p.Valor).IsRequired();
            //     p.Property(p => p.TipoProduto).HasConversion<string>();
            // });

            // modelBuilder.Entity<Pedido>(p =>
            // {
            //     p.ToTable("Pedidos");
            //     p.HasKey(p => p.Id);
            //     p.Property(p => p.IniciadoEm).HasDefaultValueSql("GETDATE()").ValueGeneratedOnAdd();
            //     p.Property(p => p.Status).HasConversion<string>();
            //     p.Property(p => p.TipoFrete).IsRequired();
            //     p.Property(p => p.Observacao).HasColumnType("VARCHAR(512)");

            //     p.HasMany(p => p.Itens).WithOne(p => p.Pedido).OnDelete(DeleteBehavior.Cascade);
            // });

            // modelBuilder.Entity<PedidoItem>(p =>
            // {
            //     p.ToTable("PedidoItens");
            //     p.HasKey(p => p.Id);
            //     p.Property(p => p.Quantidade).HasDefaultValue(1).IsRequired();
            //     p.Property(p => p.Valor).IsRequired();
            //     p.Property(p => p.Desconto).IsRequired();
            // });            
        }

        private void MapearPropriedadesEsquecidas(ModelBuilder modelBuilder)
        {
            //Seta max lenght para propriedades string sem max lenght.
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                var properties = entity.GetProperties().Where(p=>p.ClrType == typeof(string));

                foreach (var property in properties)
                {
                    if ((string.IsNullOrEmpty(property.GetColumnType()) && (!property.GetMaxLength().HasValue))
                    {
                        //Formas para setar maxlenght.
                        //property.SetMaxLength(100);
                        property.SetColumnType("VARCHAR(100)");
                    }
                }
            }
        }
    }
}