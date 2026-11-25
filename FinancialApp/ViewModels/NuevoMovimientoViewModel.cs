using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace FinancialApp.ViewModels
{
    [QueryProperty(nameof(UsuarioId), "UsuarioId")]
    [QueryProperty(nameof(TipoSeleccionado), "TipoSeleccionado")]
    public partial class NuevoMovimientoViewModel : ObservableObject
    {
        [ObservableProperty]
        private int usuarioId;

        [ObservableProperty]
        private string tipoSeleccionado = "Ingreso";

        [ObservableProperty]
        private string descripcion = string.Empty;

        [ObservableProperty]
        private string montoTexto = string.Empty;

        [ObservableProperty]
        private Categoria? categoriaSeleccionada;

        [ObservableProperty]
        private DateTime fechaMovimiento = DateTime.Now;

        [ObservableProperty]
        private string fuenteIngreso = "Nómina / Transferencia";

        [ObservableProperty]
        private bool esGastoFijo;

        [ObservableProperty]
        private string metaObjetivoTexto = "5000";

        [ObservableProperty]
        private string mensajeError = string.Empty;

        [ObservableProperty]
        private bool estaCargando;

  
        public bool EsIngreso => TipoSeleccionado == "Ingreso";
        public bool EsGasto => TipoSeleccionado == "Gasto";
        public bool EsAhorro => TipoSeleccionado == "Ahorro";
        public string TituloFormulario => $"Nuevo {TipoSeleccionado}";

        public ObservableCollection<Categoria> CategoriasDisponibles { get; } = new();

        partial void OnTipoSeleccionadoChanged(string value)
        {
            OnPropertyChanged(nameof(EsIngreso));
            OnPropertyChanged(nameof(EsGasto));
            OnPropertyChanged(nameof(EsAhorro));
            OnPropertyChanged(nameof(TituloFormulario));
        }

        [RelayCommand]
        public async Task CargarCategoriasAsync()
        {
            try
            {
                using var db = new AppDbContext();
                var lista = await db.Categorias
                    .Where(c => c.Tipo == TipoSeleccionado)
                    .ToListAsync();

                CategoriasDisponibles.Clear();
                foreach (var cat in lista)
                    CategoriasDisponibles.Add(cat);

                CategoriaSeleccionada = CategoriasDisponibles.FirstOrDefault();
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al cargar categorías: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task GuardarMovimientoAsync()
        {
            MensajeError = string.Empty;

            if (!decimal.TryParse(MontoTexto, out decimal monto) || monto <= 0)
            {
                MensajeError = "Por favor ingresa un monto válido.";
                return;
            }

            if (CategoriaSeleccionada == null)
            {
                MensajeError = "Por favor selecciona una categoría.";
                return;
            }

            TransaccionBase movimiento;

            if (TipoSeleccionado == "Ingreso")
            {
                movimiento = new Ingreso
                {
                    FuenteIngreso = FuenteIngreso
                };
            }
            else if (TipoSeleccionado == "Gasto")
            {
                movimiento = new Gasto
                {
                    EsGastoFijo = EsGastoFijo
                };
            }
            else
            {
                decimal.TryParse(MetaObjetivoTexto, out decimal meta);
                movimiento = new AhorroMeta
                {
                    MetaObjetivo = meta > 0 ? meta : 1000m
                };
            }

            movimiento.Descripcion = Descripcion;
            movimiento.Monto = monto;
            movimiento.FechaMovimiento = FechaMovimiento;
            movimiento.UsuarioId = UsuarioId;
            movimiento.CategoriaId = CategoriaSeleccionada.Id;

            if (!movimiento.Validar())
            {
                MensajeError = movimiento.ObtenerMensajeError();
                return;
            }

            try
            {
                EstaCargando = true;
                using var db = new AppDbContext();
                db.Transacciones.Add(movimiento);
                await db.SaveChangesAsync();
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al guardar: {ex.Message}";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task CancelarAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}