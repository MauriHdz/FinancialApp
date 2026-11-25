using FinancialApp.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models
{
    public class Categoria : EntidadBase, IValidable
    {
        private string _nombre = string.Empty;
        private string _tipo = "Gasto"; 
        private string _ultimoError = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Nombre
        {
            get => _nombre;
            set => _nombre = value?.Trim() ?? string.Empty;
        }

        [Required]
        [MaxLength(20)]
        public string Tipo
        {
            get => _tipo;
            set => _tipo = value?.Trim() ?? "Gasto";
        }

        public ICollection<TransaccionBase> Transacciones { get; set; } = new List<TransaccionBase>();

        public bool Validar()
        {
            if (string.IsNullOrWhiteSpace(Nombre))
            {
                _ultimoError = "El nombre de la categoría es obligatorio.";
                return false;
            }
            _ultimoError = string.Empty;
            return true;
        }

        public string ObtenerMensajeError() => _ultimoError;

        public override string ObtenerResumenEntidad() => $"{Nombre} ({Tipo})";
    }
}