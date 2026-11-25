using FinancialApp.ViewModels;

namespace FinancialApp.Views;

public partial class NuevoMovimientoPage : ContentPage
{
    public NuevoMovimientoPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is NuevoMovimientoViewModel vm)
        {
            await vm.CargarCategoriasCommand.ExecuteAsync(null);
        }
    }
}