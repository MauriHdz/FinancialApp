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
        }
    }
}