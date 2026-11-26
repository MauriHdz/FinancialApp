using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.Data
{
    public class AppDbContext : DbContext
    {

        //BDSETS PARA LAS TABLAS DE LA BASE DE DATOS
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<TransaccionBase> Transacciones { get; set; }
        public DbSet<Ingreso> Ingresos { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<AhorroMeta> AhorrosMeta { get; set; }

        public DbSet<Deuda> Deudas { get; set; }

        //CONFIGURACION PARA LA CONEXION A LA BASE DE DATOS 
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = "Server=MHERNANDEZ;Database=FinancialDB;User Id=sa;Password=a21@b1c?;TrustServerCertificate=True;";
                optionsBuilder.UseSqlServer(connectionString);
            }
        }

        //CONFIGURACION DE LA HERENCIA TPH Y SEMILLAS DE CATEGORIAS
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<TransaccionBase>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<Ingreso>("Ingreso")
                .HasValue<Gasto>("Gasto")
                .HasValue<AhorroMeta>("AhorroMeta");

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Salario / Nómina", Tipo = "Ingreso", FechaCreacion = new DateTime(2026, 1, 1) },
                new Categoria { Id = 2, Nombre = "Freelance / Proyectos", Tipo = "Ingreso", FechaCreacion = new DateTime(2026, 1, 1) },
                new Categoria { Id = 3, Nombre = "Alimentación", Tipo = "Gasto", FechaCreacion = new DateTime(2026, 1, 1) },
                new Categoria { Id = 4, Nombre = "Transporte", Tipo = "Gasto", FechaCreacion = new DateTime(2026, 1, 1) },
                new Categoria { Id = 5, Nombre = "Servicios y Renta", Tipo = "Gasto", FechaCreacion = new DateTime(2026, 1, 1) },
                new Categoria { Id = 6, Nombre = "Fondo de Emergencia", Tipo = "Ahorro", FechaCreacion = new DateTime(2026, 1, 1) }
            );
        }
    }
}