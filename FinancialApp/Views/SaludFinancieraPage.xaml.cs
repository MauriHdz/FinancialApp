using FinancialApp.ViewModels;

namespace FinancialApp.Views;

public partial class SaludFinancieraPage : ContentPage
{
    public SaludFinancieraPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SaludFinancieraViewModel vm)
        {
            await vm.CargarYSimularCommand.ExecuteAsync(null);
        }
    }
}