using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialApp.Data;
using FinancialApp.Models;
using FinancialApp.Services;
using System.Collections.ObjectModel;

namespace FinancialApp.ViewModels
{
    [QueryProperty(nameof(UsuarioId), "UsuarioId")]
    [QueryProperty(nameof(TotalIngresos), "TotalIngresos")]
    [QueryProperty(nameof(TotalGastos), "TotalGastos")]
    public partial class SaludFinancieraViewModel : ObservableObject
    {
        private readonly RepositorioGenerico<Deuda> _repoDeudas = new();
        private readonly SimuladorDeudasService _simulador = new();
        private CancellationTokenSource? _cts;

        [ObservableProperty]
        private int usuarioId;

        [ObservableProperty]
        private decimal totalIngresos;

        [ObservableProperty]
        private decimal totalGastos;

        [ObservableProperty]
        private string acreedor = string.Empty;

        [ObservableProperty]
        private string saldoPendienteTexto = string.Empty;

        [ObservableProperty]
        private string tasaInteresTexto = string.Empty;

        [ObservableProperty]
        private string pagoMinimoTexto = string.Empty;

        [ObservableProperty]
        private string diagnosticoSalud = "Calculando diagnóstico financiero...";

        [ObservableProperty]
        private string estrategiaRecomendada = string.Empty;

        [ObservableProperty]
        private string detalleAvalancha = string.Empty;

        [ObservableProperty]
        private string detalleBolaNieve = string.Empty;

        [ObservableProperty]
        private double progresoSimulacion;

        [ObservableProperty]
        private bool estaSimulando;

        [ObservableProperty]
        private string mensajeError = string.Empty;

        public decimal DeudaTotal => Deudas.Sum(d => d.SaldoPendiente);
        public decimal ExcedenteMensual => Math.Max(0, TotalIngresos - TotalGastos);

        public ObservableCollection<Deuda> Deudas { get; } = new();

        [RelayCommand]
        public async Task CargarYSimularAsync()
        {
            if (UsuarioId <= 0) return;

            try
            {
                var lista = await _repoDeudas.ObtenerTodosAsync(d => d.UsuarioId == UsuarioId);
                Deudas.Clear();
                foreach (var deuda in lista)
                    Deudas.Add(deuda);

                OnPropertyChanged(nameof(DeudaTotal));
                OnPropertyChanged(nameof(ExcedenteMensual));

                await EjecutarSimulacionAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar deudas: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task EjecutarSimulacionAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            var progress = new Progress<int>(porcentaje =>
            {
                ProgresoSimulacion = porcentaje / 100.0;
            });

            try
            {
                EstaSimulando = true;
                ProgresoSimulacion = 0;

                var resultado = await _simulador.SimularEstrategiasAsync(
                    Deudas.ToList(),
                    TotalIngresos,
                    TotalGastos,
                    progress,
                    token);

                DiagnosticoSalud = resultado.DiagnosticoSalud;
                EstrategiaRecomendada = resultado.EstrategiaRecomendada;

                if (Deudas.Count > 0)
                {
                    DetalleAvalancha = $"{resultado.MesesAvalancha} meses | Intereses: {resultado.InteresesAvalancha:C}";
                    DetalleBolaNieve = $"{resultado.MesesBolaNieve} meses | Intereses: {resultado.InteresesBolaNieve:C}";
                }
                else
                {
                    DetalleAvalancha = "Sin deudas activas";
                    DetalleBolaNieve = "Sin deudas activas";
                }
            }
            catch (OperationCanceledException)
            {
                DiagnosticoSalud = "Simulación cancelada por el usuario.";
            }
            catch (Exception ex)
            {
                MensajeError = $"Error en simulación: {ex.Message}";
            }
            finally
            {
                EstaSimulando = false;
            }
        }

        [RelayCommand]
        private void CancelarSimulacion()
        {
            _cts?.Cancel();
        }

        [RelayCommand]
        private async Task AgregarDeudaAsync()
        {
            MensajeError = string.Empty;

            if (!decimal.TryParse(SaldoPendienteTexto, out decimal saldo) ||
                !decimal.TryParse(PagoMinimoTexto, out decimal minimo))
            {
                MensajeError = "Ingresa montos numéricos válidos en saldo y pago mínimo.";
                return;
            }

            decimal.TryParse(TasaInteresTexto, out decimal tasa);

            var nuevaDeuda = new Deuda
            {
                Acreedor = Acreedor,
                SaldoPendiente = saldo,
                TasaInteresAnual = tasa,
                PagoMinimoMensual = minimo,
                UsuarioId = UsuarioId
            };

            if (!nuevaDeuda.Validar())
            {
                MensajeError = nuevaDeuda.ObtenerMensajeError();
                return;
            }

            try
            {
                await _repoDeudas.AgregarAsync(nuevaDeuda);

                Acreedor = string.Empty;
                SaldoPendienteTexto = string.Empty;
                TasaInteresTexto = string.Empty;
                PagoMinimoTexto = string.Empty;

                await CargarYSimularAsync();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar deuda: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task EliminarDeudaAsync(Deuda deuda)
        {
            if (deuda == null) return;

            bool confirmar = await Shell.Current.DisplayAlertAsync(
                "Liquidar / Eliminar",
                $"¿Marcar '{deuda.Acreedor}' como liquidada y eliminarla?",
                "Sí", "No");

            if (!confirmar) return;

            await _repoDeudas.EliminarAsync(deuda.Id);
            await CargarYSimularAsync();
        }

        [RelayCommand]
        private async Task VolverAsync()
        {
            _cts?.Cancel();
            await Shell.Current.GoToAsync("..");
        }
    }
}