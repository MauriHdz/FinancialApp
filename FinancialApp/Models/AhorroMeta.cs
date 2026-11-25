using System.ComponentModel.DataAnnotations.Schema;

namespace FinancialApp.Models
{
    public class AhorroMeta : TransaccionBase
    {
        private decimal _metaObjetivo = 1000m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal MetaObjetivo
        {
            get => _metaObjetivo;
            set => _metaObjetivo = value <= 0 ? 1000m : value;
        }

        public override string TipoMovimiento => "Ahorro";

        public override decimal CalcularImpactoSaldo() => -Monto;

        public decimal CalcularPorcentajeAvance()
        {
            return MetaObjetivo > 0 ? Math.Min(100m, Math.Round((Monto / MetaObjetivo) * 100m, 2)) : 0m;
        }

        public override string ObtenerDetalleFormateado()
        {
            return $"Meta ({CalcularPorcentajeAvance()}%): {Descripcion} - Apartado: {Monto:C} de {MetaObjetivo:C}";
        }
    }
}