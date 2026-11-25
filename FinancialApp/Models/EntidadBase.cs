using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models
{
    public abstract class EntidadBase
    {
        private int _id;
        private DateTime _fechaCreacion = DateTime.Now;

        [Key]
        public int Id
        {
            get => _id;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El ID no puede ser negativo.");
                _id = value;
            }
        }

        public DateTime FechaCreacion
        {
            get => _fechaCreacion;
            set
            {
                if (value > DateTime.Now.AddDays(1))
                    throw new ArgumentException("La fecha de creación no puede ser futura.");
                _fechaCreacion = value;
            }
        }

        public abstract string ObtenerResumenEntidad();
    }
}