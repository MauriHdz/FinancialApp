using System.ComponentModel.DataAnnotations;

namespace FinancialApp.Models
{
    public class Ingreso : TransaccionBase
    {
        private string _fuenteIngreso = "Nómina / Transferencia";

        [MaxLength(80)]
        public string FuenteIngreso
        {
            get => _fuenteIngreso;
            set => _fuenteIngreso = string.IsNullOrWhiteSpace(value) ? "General" : value.Trim();
        }

        public override string TipoMovimiento => "Ingreso";

        public override decimal CalcularImpactoSaldo() => Monto;

        public override string ObtenerDetalleFormateado()
        {
            return $"+{Monto:C} | {Descripcion} (Fuente: {FuenteIngreso})";
        }
    }
}