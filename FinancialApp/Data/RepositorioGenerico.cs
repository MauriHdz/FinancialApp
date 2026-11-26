using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace FinancialApp.Data
{
    public class RepositorioGenerico<T> where T : EntidadBase
    {
        public async Task<List<T>> ObtenerTodosAsync(Expression<Func<T, bool>>? filtro = null)
        {
            using var db = new AppDbContext();
            IQueryable<T> query = db.Set<T>();

            if (filtro != null)
                query = query.Where(filtro);

            return await query.ToListAsync();
        }

        public async Task AgregarAsync(T entidad)
        {
            using var db = new AppDbContext();
            await db.Set<T>().AddAsync(entidad);
            await db.SaveChangesAsync();
        }

        public async Task ActualizarAsync(T entidad)
        {
            using var db = new AppDbContext();
            db.Set<T>().Update(entidad);
            await db.SaveChangesAsync();
        }

        public async Task EliminarAsync(int id)
        {
            using var db = new AppDbContext();
            var entidad = await db.Set<T>().FindAsync(id);
            if (entidad != null)
            {
                db.Set<T>().Remove(entidad);
                await db.SaveChangesAsync();
            }
        }
    }
}