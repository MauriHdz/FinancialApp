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

        public ObservableCollection<TransaccionBase> Movimientos { get; } = new();

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

                Movimientos.Clear();
                decimal ingresos = 0;
                decimal gastos = 0;
                decimal balance = 0;

                foreach (var mov in lista)
                {
                    Movimientos.Add(mov);

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
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"No se pudieron cargar los movimientos: {ex.Message}", "OK");
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