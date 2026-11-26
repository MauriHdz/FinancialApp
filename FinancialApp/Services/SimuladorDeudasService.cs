using FinancialApp.Models;

namespace FinancialApp.Services
{
    public class ResultadoSimulacion
    {
        public int MesesAvalancha { get; set; }
        public decimal InteresesAvalancha { get; set; }
        public int MesesBolaNieve { get; set; }
        public decimal InteresesBolaNieve { get; set; }
        public string EstrategiaRecomendada { get; set; } = string.Empty;
        public string DiagnosticoSalud { get; set; } = string.Empty;
        public decimal AhorroEnIntereses => Math.Max(0, InteresesBolaNieve - InteresesAvalancha);
    }

    public class SimuladorDeudasService
    {
     
        private readonly SemaphoreSlim _semaforo = new(1, 1);

        public async Task<ResultadoSimulacion> SimularEstrategiasAsync(
            List<Deuda> deudasOriginales,
            decimal ingresosMensuales,
            decimal gastosMensuales,
            IProgress<int>? progreso = null,
            CancellationToken cancellationToken = default)
        {
            await _semaforo.WaitAsync(cancellationToken);

            try
            {
                return await Task.Run(async () =>
                {
                    progreso?.Report(10);
                    decimal excedenteDisponible = Math.Max(0, ingresosMensuales - gastosMensuales);

                    if (deudasOriginales == null || deudasOriginales.Count == 0)
                    {
                        progreso?.Report(100);
                        return new ResultadoSimulacion
                        {
                            EstrategiaRecomendada = "Sin deudas activas",
                            DiagnosticoSalud = "¡Excelente! No tienes deudas registradas. Destina tu excedente a tus metas de ahorro."
                        };
                    }

                    // método Avalancha 
                    cancellationToken.ThrowIfCancellationRequested();
                    progreso?.Report(35);
                    await Task.Delay(350, cancellationToken); 
                    var (mesesAvalancha, intAvalancha) = CalcularProyeccion(
                        deudasOriginales.OrderByDescending(d => d.TasaInteresAnual).ToList(),
                        excedenteDisponible,
                        cancellationToken);

                    // método Bola de Nieve 
                    cancellationToken.ThrowIfCancellationRequested();
                    progreso?.Report(70);
                    await Task.Delay(350, cancellationToken);
                    var (mesesBola, intBola) = CalcularProyeccion(
                        deudasOriginales.OrderBy(d => d.SaldoPendiente).ToList(),
                        excedenteDisponible,
                        cancellationToken);

                    progreso?.Report(90);

                    // Evaluar ratio de endeudamiento
                    decimal totalPagosMinimos = deudasOriginales.Sum(d => d.PagoMinimoMensual);
                    decimal ratioDeuda = ingresosMensuales > 0 ? (totalPagosMinimos / ingresosMensuales) * 100m : 100m;

                    string diagnostico = ratioDeuda switch
                    {
                        <= 20m => $"Salud Óptima ({ratioDeuda:F1}% de tus ingresos comprometidos). Tienes margen para liquidar rápido.",
                        <= 35m => $"Salud Estable ({ratioDeuda:F1}% de tus ingresos en deudas). Evita adquirir nuevos créditos.",
                        _ => $"Alerta Financiera ({ratioDeuda:F1}% de tus ingresos comprometidos). Prioriza reducir gastos variables."
                    };

                    string recomendacion = intAvalancha < intBola
                        ? $"Método Avalancha: Terminas en {mesesAvalancha} meses y te ahorras {intBola - intAvalancha:C} en intereses."
                        : $"Método Bola de Nieve: Liquidas tu primera deuda rápido y terminas todo en {mesesBola} meses.";

                    progreso?.Report(100);

                    return new ResultadoSimulacion
                    {
                        MesesAvalancha = mesesAvalancha,
                        InteresesAvalancha = Math.Round(intAvalancha, 2),
                        MesesBolaNieve = mesesBola,
                        InteresesBolaNieve = Math.Round(intBola, 2),
                        EstrategiaRecomendada = recomendacion,
                        DiagnosticoSalud = diagnostico
                    };
                }, cancellationToken);
            }
            finally
            {
                _semaforo.Release();
            }
        }

        private (int meses, decimal interesesTotales) CalcularProyeccion(
            List<Deuda> ordenDeudas,
            decimal abonoExtraMensual,
            CancellationToken token)
        {
            var saldos = ordenDeudas.Select(d => new
            {
                Saldo = d.SaldoPendiente,
                TasaMensual = (d.TasaInteresAnual / 100m) / 12m,
                Minimo = d.PagoMinimoMensual
            }).ToList();

            var saldosActuales = saldos.Select(s => s.Saldo).ToList();
            int meses = 0;
            decimal interesesAcumulados = 0m;
            int limiteSeguridadMeses = 360; 
            while (saldosActuales.Any(s => s > 0.01m) && meses < limiteSeguridadMeses)
            {
                token.ThrowIfCancellationRequested();
                meses++;
                decimal bolaExtra = abonoExtraMensual;

                for (int i = 0; i < saldosActuales.Count; i++)
                {
                    if (saldosActuales[i] <= 0)
                    {
                        bolaExtra += saldos[i].Minimo;
                        continue;
                    }

                    decimal interesMes = saldosActuales[i] * saldos[i].TasaMensual;
                    interesesAcumulados += interesMes;
                    saldosActuales[i] += interesMes;

                    decimal pago = Math.Min(saldosActuales[i], saldos[i].Minimo);
                    saldosActuales[i] -= pago;
                }

                for (int i = 0; i < saldosActuales.Count && bolaExtra > 0; i++)
                {
                    if (saldosActuales[i] > 0)
                    {
                        decimal abono = Math.Min(saldosActuales[i], bolaExtra);
                        saldosActuales[i] -= abono;
                        bolaExtra -= abono;
                    }
                }
            }

            return (meses, interesesAcumulados);
        }
    }
}