namespace FinancialApp.Interfaces
{
    public interface IValidable
    {
        bool Validar();
        string ObtenerMensajeError();
    }
}