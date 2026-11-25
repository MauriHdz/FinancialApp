using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        [ObservableProperty]
        private string correo = string.Empty;

        [ObservableProperty]
        private string contrasena = string.Empty;

        [ObservableProperty]
        private bool estaCargando;

        [ObservableProperty]
        private string mensajeError = string.Empty;

        [RelayCommand]
        private async Task IniciarSesionAsync()
        {
            MensajeError = string.Empty;

            var usuarioTemp = new Usuario
            {
                Correo = Correo,
                Contrasena = Contrasena
            };

            if (!usuarioTemp.Validar())
            {
                MensajeError = usuarioTemp.ObtenerMensajeError();
                return;
            }

            try
            {
                EstaCargando = true;

                using var db = new AppDbContext();
                await db.Database.EnsureCreatedAsync();

                var usuarioEncontrado = await db.Usuarios
                    .FirstOrDefaultAsync(u => u.Correo == usuarioTemp.Correo && u.Contrasena == usuarioTemp.Contrasena);

                if (usuarioEncontrado != null)
                {
                    var parametros = new Dictionary<string, object>
    {
        { "UsuarioId", usuarioEncontrado.Id },
        { "NombreUsuario", usuarioEncontrado.Nombre }
    };

                    await Shell.Current.GoToAsync(nameof(Views.DashboardPage), parametros);
                }
                else
                {
                    MensajeError = "Credenciales incorrectas. Verifica tu correo o contraseña.";
                }
            }
            catch (Exception ex)
            {
                MensajeError = $"Error de conexión con SQL Server: {ex.Message}";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task IrARegistroAsync()
        {
            await Shell.Current.GoToAsync(nameof(Views.RegistroPage));
}
    }
}