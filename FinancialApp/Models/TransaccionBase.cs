using FinancialApp.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialApp.Models
{
    public abstract class TransaccionBase : EntidadBase, IValidable, IExportableResumen
    {
        private decimal _monto;
        private string _descripcion = string.Empty;
        private string _ultimoError = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Monto
        {
            get => _monto;
            set
            {
                if (value < 0)
                    throw new ArgumentException("El monto no puede ser negativo.");
                _monto = value;
            }
        }

        [Required]
        [MaxLength(150)]
        public string Descripcion
        {
            get => _descripcion;
            set => _descripcion = value?.Trim() ?? string.Empty;
        }

        public DateTime FechaMovimiento { get; set; } = DateTime.Now;

        // LLAVE FORANEA HACIA USUARIOS
        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        // LLAVE FORANEA HACIA CATEGORIAS
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public Categoria? Categoria { get; set; }

        public abstract string TipoMovimiento { get; }
        public abstract decimal CalcularImpactoSaldo();

        public virtual string ObtenerDetalleFormateado()
        {
            return $"{FechaMovimiento:dd/MM/yyyy} - {Descripcion}: {CalcularImpactoSaldo():C}";
        }

        public virtual bool Validar()
        {
            if (Monto <= 0)
            {
                _ultimoError = "El monto debe ser mayor a $0.00.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Descripcion))
            {
                _ultimoError = "La descripción es obligatoria.";
                return false;
            }

            if (CategoriaId <= 0)
            {
                _ultimoError = "Debes seleccionar una categoría.";
                return false;
            }

            _ultimoError = string.Empty;
            return true;
        }

        public string ObtenerMensajeError() => _ultimoError;

        public override string ObtenerResumenEntidad() => ObtenerDetalleFormateado();

        public string GenerarLineaReporte()
        {
            return $"[{TipoMovimiento}] {FechaMovimiento:yyyy-MM-dd} | {Descripcion} | Impacto: {CalcularImpactoSaldo():C}";
        }
    }
}