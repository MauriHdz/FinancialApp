using FinancialApp.ViewModels;

namespace FinancialApp.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DashboardViewModel vm)
        {
            await vm.CargarDatosCommand.ExecuteAsync(null);
        }
    }
}