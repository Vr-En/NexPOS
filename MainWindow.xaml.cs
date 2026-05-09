using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

            LoadDashboard();
        }

        private void SetActiveButton(Button activeButton)
        {
            btnDashboard.Style = (Style)FindResource("MenuButtonStyle");
            btnInventory.Style = (Style)FindResource("MenuButtonStyle");
            //btnPOS.Style = (Style)FindResource("MenuButtonStyle");
            btnTransactions.Style = (Style)FindResource("MenuButtonStyle");
            btnCategories.Style = (Style)FindResource("MenuButtonStyle");
            btnUsers.Style = (Style)FindResource("MenuButtonStyle");

            activeButton.Style = (Style)FindResource("ActiveMenuButtonStyle");
        }

        private void Dashboard_Click(object sender, RoutedEventArgs e)
        {
            LoadDashboard();

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

        //private void POS_Click(object sender, RoutedEventArgs e)
        //{
        //    DashboardPanel.Visibility = Visibility.Collapsed;
        //    PagePanel.Visibility = Visibility.Visible;

        //    MainContent.Content = new PointOfSalePage();

        //    SetActiveButton(btnPOS);
        //}

        private void Transactions_Click(object sender, RoutedEventArgs e)
        {
            //DashboardPanel.Visibility = Visibility.Collapsed;
            //PagePanel.Visibility = Visibility.Visible;

            //MainContent.Content = new TransactionsPage();

            SetActiveButton(btnTransactions);
            MessageBox.Show("Transactions page will be added next.");
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

        public void LoadDashboard()
        {
            using (var db = new NexPOSDataDataContext())
            {
                DateTime today = DateTime.Today;
                DateTime tomorrow = today.AddDays(1);
                DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
                DateTime thirtyDaysFromNow = today.AddDays(30);

                // TODAY SALES
                decimal todaySales = db.tbl_Transactions
                    .Where(t => t.TransactionDate >= today && t.TransactionDate < tomorrow)
                    .Sum(t => (decimal?)t.TotalAmount) ?? 0;

                txtTodaySales.Text = "₱" + todaySales.ToString("N2");


                // ACTIVE PRODUCTS
                int activeProducts = db.tbl_Products
                    .Count(p => p.Status == "Active");

                txtActiveProducts.Text = activeProducts.ToString();

                // MONTHLY SALES
                decimal monthlySales = db.tbl_Transactions
                    .Where(t => t.TransactionDate >= firstDayOfMonth)
                    .Sum(t => (decimal?)t.TotalAmount) ?? 0;

                txtMonthlySales.Text = "₱" + monthlySales.ToString("N2");

                // LOW STOCK ALERTS
                var lowStock = db.tbl_Products
                    .Where(p => p.StockQuantity <= p.ReorderLevel && p.StockQuantity > 0)
                    .OrderBy(p => p.StockQuantity)
                    .ToList();

                txtLowStockCount.Text = lowStock.Count + " items";

                lowStockPanel.Children.Clear();

                if (lowStock.Count == 0)
                {
                    lowStockPanel.Children.Add(CreateEmptyMessage("No low stock items"));
                }
                else
                {
                    foreach (var item in lowStock)
                    {
                        lowStockPanel.Children.Add(CreateAlertItem(
                            item.ProductName,
                            item.StockQuantity + " left",
                            true));
                    }
                }

                // EXPIRING SOON ALERTS
                var expiring = db.tbl_Products
                    .Where(p => p.ExpirationDate.HasValue &&
                                p.ExpirationDate.Value >= today &&
                                p.ExpirationDate.Value <= thirtyDaysFromNow)
                    .OrderBy(p => p.ExpirationDate)
                    .ToList();

                txtExpiringCount.Text = expiring.Count + " items";

                expiringPanel.Children.Clear();

                if (expiring.Count == 0)
                {
                    expiringPanel.Children.Add(CreateEmptyMessage("No products expiring soon"));
                }
                else
                {
                    foreach (var item in expiring)
                    {
                        expiringPanel.Children.Add(CreateAlertItem(
                            item.ProductName,
                            item.ExpirationDate.Value.ToString("MM/dd/yyyy"),
                            false));
                    }
                }

                // RECENT TRANSACTIONS
                var recent = db.tbl_Transactions
                    .OrderByDescending(t => t.TransactionDate)
                    .Take(5)
                    .ToList();

                recentTransactionsPanel.Children.Clear();

                if (recent.Count == 0)
                {
                    recentTransactionsPanel.Children.Add(CreateEmptyMessage("No transactions yet"));
                }
                else
                {
                    foreach (var t in recent)
                    {
                        recentTransactionsPanel.Children.Add(new TextBlock
                        {
                            Text = "TXN-" + t.TransactionID + " - ₱" + t.TotalAmount.ToString("N2"),
                            Margin = new Thickness(5),
                            FontWeight = FontWeights.SemiBold,
                            Foreground = Brushes.Black
                        });
                    }
                }
            }
        }

        private TextBlock CreateEmptyMessage(string message)
        {
            return new TextBlock
            {
                Text = message,
                Foreground = Brushes.Gray,
                Margin = new Thickness(0, 10, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Center
            };
        }

        private Border CreateAlertItem(string title, string subtitle, bool isStock)
        {
            return new Border
            {
                Background = Brushes.White,
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(12),
                Margin = new Thickness(0, 5, 0, 0),
                Child = new Grid
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = title,
                            FontWeight = FontWeights.SemiBold
                        },
                        new TextBlock
                        {
                            Text = subtitle,
                            HorizontalAlignment = HorizontalAlignment.Right,
                            Foreground = isStock ? Brushes.Red : Brushes.OrangeRed,
                            FontWeight = FontWeights.Bold
                        }
                    }
                }
            };
        }
    }
}