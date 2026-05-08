using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace NexPOS
{
    public partial class TransactionsPage : UserControl
    {
        public TransactionsPage()
        {
            InitializeComponent();

            LoadCashierFilter();
            RefreshTransactions();
        }

        private void LoadCashierFilter()
        {
            cmbCashier.Items.Clear();
            cmbCashier.Items.Add("All Cashiers");

            var cashiers = DataStore.Transactions
                .Select(transaction => transaction.Cashier)
                .Distinct()
                .ToList();

            foreach (string cashier in cashiers)
            {
                cmbCashier.Items.Add(cashier);
            }

            cmbCashier.SelectedIndex = 0;
        }

        private void RefreshTransactions()
        {
            string search = "";

            if (txtSearch != null)
            {
                search = txtSearch.Text.ToLower();
            }

            string selectedCashier = "All Cashiers";

            if (cmbCashier != null && cmbCashier.SelectedItem != null)
            {
                selectedCashier = cmbCashier.SelectedItem.ToString();
            }

            var filtered = DataStore.Transactions.Where(transaction =>
                (transaction.ReceiptNo.ToLower().Contains(search) ||
                 transaction.Cashier.ToLower().Contains(search)) &&

                (selectedCashier == "All Cashiers" ||
                 transaction.Cashier == selectedCashier)
            )
            .OrderByDescending(transaction => transaction.TransactionDate)
            .ToList();

            gridTransactions.ItemsSource = null;
            gridTransactions.ItemsSource = filtered;

            txtTransactionSubtitle.Text = DataStore.Transactions.Count + " total transactions";

            txtTotalTransactions.Text = DataStore.Transactions.Count.ToString();

            decimal todayRevenue = DataStore.Transactions
                .Where(transaction => transaction.TransactionDate.Date == DateTime.Today)
                .Sum(transaction => transaction.TotalAmount);

            decimal totalRevenue = DataStore.Transactions
                .Sum(transaction => transaction.TotalAmount);

            txtTodayRevenue.Text = "₱" + todayRevenue.ToString("0.00");
            txtTotalRevenue.Text = "₱" + totalRevenue.ToString("0.00");

            if (filtered.Count == 0)
            {
                emptyPanel.Visibility = Visibility.Visible;
            }
            else
            {
                emptyPanel.Visibility = Visibility.Collapsed;
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (gridTransactions != null)
            {
                RefreshTransactions();
            }
        }

        private void ViewTransaction_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            TransactionRecord transaction = button.DataContext as TransactionRecord;

            if (transaction == null)
            {
                return;
            }

            MessageBox.Show(
                "Receipt #: " + transaction.ReceiptNo + "\n" +
                "Date & Time: " + transaction.DateTimeText + "\n" +
                "Cashier: " + transaction.Cashier + "\n" +
                "Items: " + transaction.TotalItems + "\n" +
                "Total: " + transaction.TotalText + "\n" +
                "Change: " + transaction.ChangeText,
                "Transaction Details",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }
}