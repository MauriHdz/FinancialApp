namespace FinancialApp.Models
{
    public class Gasto : TransaccionBase
    {
        public bool EsGastoFijo { get; set; }

        public override string TipoMovimiento => "Gasto";

        public override decimal CalcularImpactoSaldo() => -Monto;

        public override string ObtenerDetalleFormateado()
        {
            string etiqueta = EsGastoFijo ? "Fijo" : "Variable";
            return $"-{Monto:C} | {Descripcion} [{etiqueta}]";
        }
    }
}