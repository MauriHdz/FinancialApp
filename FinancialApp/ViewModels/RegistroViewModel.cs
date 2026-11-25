using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FinancialApp.Data;
using FinancialApp.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialApp.ViewModels
{
    public partial class RegistroViewModel : ObservableObject
    {
        [ObservableProperty]
        private string nombre = string.Empty;

        [ObservableProperty]
        private string correo = string.Empty;

        [ObservableProperty]
        private string contrasena = string.Empty;

        [ObservableProperty]
        private bool estaCargando;

        [ObservableProperty]
        private string mensajeError = string.Empty;

        [RelayCommand]
        private async Task RegistrarUsuarioAsync()
        {
            MensajeError = string.Empty;

            if (string.IsNullOrWhiteSpace(Nombre))
            {
                MensajeError = "Por favor ingresa tu nombre completo.";
                return;
            }

            var nuevoUsuario = new Usuario
            {
                Nombre = Nombre,
                Correo = Correo,
                Contrasena = Contrasena
            };

            if (!nuevoUsuario.Validar())
            {
                MensajeError = nuevoUsuario.ObtenerMensajeError();
                return;
            }

            try
            {
                EstaCargando = true;

                using var db = new AppDbContext();
                await db.Database.EnsureCreatedAsync();

                bool correoExiste = await db.Usuarios.AnyAsync(u => u.Correo == nuevoUsuario.Correo);
                if (correoExiste)
                {
                    MensajeError = "Ese correo ya está registrado.";
                    return;
                }

                db.Usuarios.Add(nuevoUsuario);
                await db.SaveChangesAsync();

                await Shell.Current.DisplayAlert("Cuenta creada", "Usuario registrado correctamente. Ya puedes iniciar sesión.", "OK");
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                MensajeError = $"Error al registrar: {ex.Message}";
            }
            finally
            {
                EstaCargando = false;
            }
        }

        [RelayCommand]
        private async Task VolverAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}