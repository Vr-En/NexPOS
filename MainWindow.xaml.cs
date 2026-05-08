using System.Windows;
using System.Windows.Controls;

namespace NexPOS
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            WindowState = WindowState.Maximized;

            DashboardPanel.Visibility = Visibility.Visible;
            PagePanel.Visibility = Visibility.Collapsed;

            SetActiveButton(btnDashboard);
        }

        private void SetActiveButton(Button activeButton)
        {
            btnDashboard.Style = (Style)FindResource("MenuButtonStyle");
            btnInventory.Style = (Style)FindResource("MenuButtonStyle");
            btnPOS.Style = (Style)FindResource("MenuButtonStyle");
            btnTransactions.Style = (Style)FindResource("MenuButtonStyle");
            btnCategories.Style = (Style)FindResource("MenuButtonStyle");
            btnUsers.Style = (Style)FindResource("MenuButtonStyle");

            activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Visible;
            PagePanel.Visibility = Visibility.Collapsed;
            MainContent.Content = null;

            SetActiveButton(btnDashboard);
        }

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            PagePanel.Visibility = Visibility.Visible;
            MainContent.Content = new InventoryPage();

            SetActiveButton(btnInventory);
        }

        private void POS_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            PagePanel.Visibility = Visibility.Visible;

            MainContent.Content = new PointOfSalePage();

            SetActiveButton(btnPOS);
        }

        private void Transactions_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            PagePanel.Visibility = Visibility.Visible;

            MainContent.Content = new TransactionsPage();

            SetActiveButton(btnTransactions);
        }

        private void Categories_Click(object sender, RoutedEventArgs e)
        {
            DashboardPanel.Visibility = Visibility.Collapsed;
            PagePanel.Visibility = Visibility.Visible;

            MainContent.Content = new CategoriesPage();

            SetActiveButton(btnCategories);
        }
        private void Users_Click(object sender, RoutedEventArgs e)
        {
            SetActiveButton(btnUsers);
            MessageBox.Show("Users page will be added next.");
        }

        private void SignOut_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}