using FinancialApp.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialApp.Models
{
    public class Deuda : EntidadBase, IValidable, IExportableResumen
    {
        private string _acreedor = string.Empty;
        private decimal _saldoPendiente;
        private decimal _tasaInteresAnual;
        private decimal _pagoMinimoMensual;
        private string _ultimoError = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Acreedor
        {
            get => _acreedor;
            set => _acreedor = value?.Trim() ?? string.Empty;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SaldoPendiente
        {
            get => _saldoPendiente;
            set => _saldoPendiente = value < 0 ? 0 : value;
        }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TasaInteresAnual
        {
            get => _tasaInteresAnual;
            set => _tasaInteresAnual = value < 0 ? 0 : value;
        }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PagoMinimoMensual
        {
            get => _pagoMinimoMensual;
            set => _pagoMinimoMensual = value < 0 ? 0 : value;
        }

        public int UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public Usuario? Usuario { get; set; }

        public bool Validar()
        {
            if (string.IsNullOrWhiteSpace(Acreedor))
            {
                _ultimoError = "Ingresa el nombre de la deuda o institución (ej. Tarjeta BBVA).";
                return false;
            }
            if (SaldoPendiente <= 0)
            {
                _ultimoError = "El saldo pendiente debe ser mayor a $0.00.";
                return false;
            }
            if (PagoMinimoMensual <= 0)
            {
                _ultimoError = "El pago mínimo mensual debe ser mayor a $0.00.";
                return false;
            }

            _ultimoError = string.Empty;
            return true;
        }

        public string ObtenerMensajeError() => _ultimoError;

        public override string ObtenerResumenEntidad()
        {
            return $"{Acreedor} | Saldo: {SaldoPendiente:C} | Tasa: {TasaInteresAnual}% | Mínimo: {PagoMinimoMensual:C}";
        }

        public string GenerarLineaReporte()
        {
            return $"[DEUDA] {Acreedor} - Pendiente: {SaldoPendiente:C} (CAT/Tasa: {TasaInteresAnual}%, Pago Mín: {PagoMinimoMensual:C})";
        }
    }
}