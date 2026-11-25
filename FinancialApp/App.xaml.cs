using FinancialApp.Data;

namespace FinancialApp
{
    public partial class App : Application
    {
        //public App()
        //{
        //    InitializeComponent();

        //    using (var db = new AppDbContext())
        //    {
        //        db.Database.EnsureCreated();
        //    }

        //    MainPage = new AppShell();
        //}
        public App()
        {
            InitializeComponent();
            MainPage = new AppShell();
        }

    }
}
