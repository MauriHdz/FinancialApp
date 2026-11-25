using FinancialApp.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FinancialApp.Models
{
    public class Usuario : EntidadBase, IValidable
    {
        private string _nombre = string.Empty;
        private string _correo = string.Empty;
        private string _contrasena = string.Empty;
        private string _ultimoError = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Nombre
        {
            get => _nombre;
            set => _nombre = value?.Trim() ?? string.Empty;
        }

        [Required]
        [MaxLength(100)]
        public string Correo
        {
            get => _correo;
            set => _correo = value?.Trim().ToLower() ?? string.Empty;
        }

        [Required]
        [MaxLength(255)]
        public string Contrasena
        {
            get => _contrasena;
            set => _contrasena = value ?? string.Empty;
        }

        public bool Validar()
        {
            if (string.IsNullOrWhiteSpace(Correo) || !Regex.IsMatch(Correo, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                _ultimoError = "Ingresa un correo electrónico válido.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(Contrasena) || Contrasena.Length < 4)
            {
                _ultimoError = "La contraseña debe tener al menos 4 caracteres.";
                return false;
            }

            _ultimoError = string.Empty;
            return true;
        }

        public string ObtenerMensajeError() => _ultimoError;

        public override string ObtenerResumenEntidad() => $"Usuario #{Id}: {Nombre} ({Correo})";
    }
}