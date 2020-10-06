using System.ComponentModel;
using System.Windows;
using AMStock.WPF.ViewModel;

namespace AMStock.WPF.Views
{
    /// <summary>
    /// Interaction logic for ExpenseLoans.xaml
    /// </summary>
    public partial class ExpenseLoans : Window
    {
        public ExpenseLoans()
        {
            InitializeComponent();
        }

        private void ExpenseLoans_OnClosing(object sender, CancelEventArgs e)
        {
            ExpenseLoanViewModel.CleanUp();
        }
    }
}
