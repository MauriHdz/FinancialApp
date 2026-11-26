using FinancialApp.Views;

namespace FinancialApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(RegistroPage), typeof(RegistroPage));
            Routing.RegisterRoute(nameof(RegistroPage), typeof(RegistroPage));
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
            Routing.RegisterRoute(nameof(NuevoMovimientoPage), typeof(NuevoMovimientoPage));
            Routing.RegisterRoute(nameof(SaludFinancieraPage), typeof(SaludFinancieraPage));
        }
    }
}