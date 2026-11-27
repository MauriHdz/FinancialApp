using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FinancialApp.ViewModels
{
    [QueryProperty(nameof(UsuarioId), "UsuarioId")]
    [QueryProperty(nameof(NombreUsuario), "NombreUsuario")]
    public partial class DashboardViewModel : ObservableObject

    {

         private readonly RepositorioGenerico<TransaccionBase> _repoMovimientos = new();
        public ObservableCollection<TransaccionBase> Movimientos { get; } = new();
        private readonly RepositorioGenerico<TransaccionBase> _repoTransacciones = new();

        private readonly List<TransaccionBase> _todosLosMovimientos = new();

        [ObservableProperty]
        private int usuarioId;

        [ObservableProperty]
        private string nombreUsuario = "Usuario";

        [ObservableProperty]
        private decimal saldoTotal;

        [ObservableProperty]
        private decimal totalIngresos;

        [ObservableProperty]
        private decimal totalGastos;

        [ObservableProperty]
        private bool estaCargando;

        [ObservableProperty]
        private string textoBusqueda = string.Empty;

        partial void OnTextoBusquedaChanged(string value)
        {
            AplicarFiltro();
        }

        private void AplicarFiltro()
        {
            Movimientos.Clear();

            var filtrados = string.IsNullOrWhiteSpace(TextoBusqueda) ? _todosLosMovimientos : _todosLosMovimientos.Where(m => m.Descripcion.Contains(TextoBusqueda, StringComparison.OrdinalIgnoreCase) || m.TipoMovimiento.Contains(textoBusqueda, StringComparison.OrdinalIgnoreCase));
            foreach (var item in filtrados)
                Movimientos.Add(item);
        }


        [RelayCommand]
        public async Task CargarDatosAsync()
        {
            if (UsuarioId <= 0) return;

            try
            {
                EstaCargando = true;

                using var db = new AppDbContext();
                var lista = await db.Transacciones
                    .Include(t => t.Categoria)
                    .Where(t => t.UsuarioId == UsuarioId)
                    .OrderByDescending(t => t.FechaMovimiento)
                    .ToListAsync();

                _todosLosMovimientos.Clear();
                _todosLosMovimientos.AddRange(lista);

                decimal ingresos = 0;
                decimal gastos = 0;
                decimal balance = 0;

                foreach (var mov in lista)
                {
                    decimal impacto = mov.CalcularImpactoSaldo();
                    balance += impacto;

                    if (mov is Ingreso)
                        ingresos += mov.Monto;
                    else
                        gastos += mov.Monto;
                }

                TotalIngresos = ingresos;
                TotalGastos = gastos;
                SaldoTotal = balance;

                AplicarFiltro();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"No se pudieron cargar los movimientos: {ex.Message}", "OK");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task AbrirNuevoMovimientoAsync (string tipo)
        {
            var parametros = new Dictionary<string, object>
            {
                { "UsuarioId", UsuarioId },
                { "TipoSeleccionado", tipo }
            };
            await Shell.Current.GoToAsync(nameof(Views.NuevoMovimientoPage), parametros);
        }

        [RelayCommand]
        private async Task AbrirSaludFinancieraAsync()
        {
            var parametros = new Dictionary<string, object>
    {
        { "UsuarioId", UsuarioId },
        { "TotalIngresos", TotalIngresos },
        { "TotalGastos", TotalGastos }
    };

            await Shell.Current.GoToAsync(nameof(Views.SaludFinancieraPage), parametros);
        }

        [RelayCommand]
        private async Task OpcionesMovimientoAsync(TransaccionBase movimiento)
        {
            if (movimiento == null) return;

            string accion = await Shell.Current.DisplayActionSheetAsync(
                movimiento.Descripcion,
                "Cancelar",
                "Eliminar",
                "Editar monto y concepto");

            if (accion == "Eliminar")
            {
                bool confirmar = await Shell.Current.DisplayAlertAsync(
                    "Eliminar movimiento",
                    $"¿Seguro que deseas eliminar '{movimiento.Descripcion}' por {movimiento.Monto:C}?",
                    "Sí, eliminar", "Cancelar");

                if (!confirmar) return;

                try
                {
                    await _repoMovimientos.EliminarAsync(movimiento.Id);
                    await CargarDatosAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlertAsync("Error", $"No se pudo eliminar: {ex.Message}", "OK");
                }
            }
            else if (accion == "Editar monto y concepto")
            {
                string nuevaDesc = await Shell.Current.DisplayPromptAsync(
                    "Editar concepto",
                    "Nueva descripción:",
                    initialValue: movimiento.Descripcion);

                if (string.IsNullOrWhiteSpace(nuevaDesc)) return;

                string nuevoMontoStr = await Shell.Current.DisplayPromptAsync(
                    "Editar monto",
                    "Nuevo monto ($):",
                    initialValue: movimiento.Monto.ToString("0.##"),
                    keyboard: Keyboard.Numeric);

                if (!decimal.TryParse(nuevoMontoStr, out decimal nuevoMonto) || nuevoMonto <= 0)
                {
                    await Shell.Current.DisplayAlertAsync("Dato inválido", "El monto debe ser mayor a $0.00.", "OK");
                    return;
                }

                movimiento.Descripcion = nuevaDesc;
                movimiento.Monto = nuevoMonto;

                if (!movimiento.Validar())
                {
                    await Shell.Current.DisplayAlertAsync("Validación", movimiento.ObtenerMensajeError(), "OK");
                    return;
                }

                try
                {
                    await _repoMovimientos.ActualizarAsync(movimiento);
                    await CargarDatosAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlertAsync("Error", $"No se pudo actualizar: {ex.Message}", "OK");
                }
            }
        }


        [RelayCommand]
        private async Task ExportarReporteAsync()
        {
            try
            {
                EstaCargando = true;

                var (rutaArchivo, contenidoVistaPrevia) = await Task.Run(async () =>
                {
                    using var db = new AppDbContext();
                    var deudas = await db.Deudas.Where(d => d.UsuarioId == UsuarioId).ToListAsync();

                    var lineas = new List<string>
                    {
                        "==========================================",
                        $"ESTADO DE CUENTA FINANCIERO - {NombreUsuario.ToUpper()}",
                        $"Fecha de emisión: {DateTime.Now:dd/MM/yyyy HH:mm}",
                        "==========================================",
                        $"Saldo Disponible: {SaldoTotal:C}",
                        $"Total Ingresos:   +{TotalIngresos:C}",
                        $"Total Gastos:     -{TotalGastos:C}",
                        $"Deuda Total:      {deudas.Sum(d => d.SaldoPendiente):C}",
                        "",
                        "--- DETALLE DE MOVIMIENTOS ---"
                    };

                    // Polimorfismo con la interfaz IExportableResumen
                    foreach (var mov in _todosLosMovimientos)
                        lineas.Add(mov.GenerarLineaReporte());

                    if (deudas.Count > 0)
                    {
                        lineas.Add("");
                        lineas.Add("--- DEUDAS ACTIVAS ---");
                        foreach (var deuda in deudas)
                            lineas.Add(deuda.GenerarLineaReporte());
                    }

                    string carpetaDocumentos = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    string nombreArchivo = $"Reporte_Financiero_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                    string ruta = Path.Combine(carpetaDocumentos, nombreArchivo);

                    await File.WriteAllLinesAsync(ruta, lineas);
                    return (ruta, string.Join(Environment.NewLine, lineas));
                });

                bool abrirArchivo = await Shell.Current.DisplayAlertAsync(
                    "Reporte Generado",
                    $"Guardado en Documentos:\n{Path.GetFileName(rutaArchivo)}\n\n{contenidoVistaPrevia}",
                    "Abrir archivo",
                    "Cerrar");

                if (abrirArchivo)
                {
                    await Launcher.Default.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(rutaArchivo)
                    });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error al exportar", ex.Message, "OK");
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task CerrarSesionAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        
    }
}