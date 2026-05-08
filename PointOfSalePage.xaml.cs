using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NexPOS
{
    public partial class PointOfSalePage : UserControl
    {
        private List<CartItem> cartItems = new List<CartItem>();
        private string selectedCategory = "All Items";

        private TransactionRecord lastTransaction = null;
        private List<CartItem> lastSoldItems = new List<CartItem>();

        public PointOfSalePage()
        {
            InitializeComponent();

            LoadCategoryButtons();
            LoadProducts();
            RefreshCart();
        }

        private void LoadCategoryButtons()
        {
            categoryPanel.Children.Clear();

            AddCategoryButton("All Items");

            foreach (string category in DataStore.Categories)
            {
                AddCategoryButton(category);
            }
        }

        private void AddCategoryButton(string categoryName)
        {
            Button button = new Button();
            button.Content = categoryName;
            button.Height = 34;
            button.Padding = new Thickness(16, 0, 16, 0);
            button.Margin = new Thickness(0, 0, 10, 0);
            button.BorderThickness = new Thickness(0);
            button.FontWeight = FontWeights.SemiBold;
            button.Cursor = System.Windows.Input.Cursors.Hand;
            button.Click += Category_Click;

            if (categoryName == selectedCategory)
            {
                button.Background = new SolidColorBrush(Color.FromRgb(0, 166, 81));
                button.Foreground = Brushes.White;
            }
            else
            {
                button.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246));
                button.Foreground = new SolidColorBrush(Color.FromRgb(51, 65, 85));
            }

            categoryPanel.Children.Add(button);
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            selectedCategory = button.Content.ToString();

            LoadCategoryButtons();
            LoadProducts();
        }

        private void SearchProduct_Changed(object sender, TextChangedEventArgs e)
        {
            LoadProducts();
        }

        private void LoadProducts()
        {
            productPanel.Children.Clear();

            string search = txtSearchProduct.Text.ToLower();

            var products = DataStore.Products.Where(product =>
                product.Status == "Active" &&
                product.StockQuantity > 0 &&
                (selectedCategory == "All Items" || product.Category == selectedCategory) &&
                (product.ProductName.ToLower().Contains(search) ||
                 product.ProductCode.ToLower().Contains(search))
            ).ToList();

            foreach (Product product in products)
            {
                productPanel.Children.Add(CreateProductCard(product));
            }
        }

        private Border CreateProductCard(Product product)
        {
            CartItem existingCartItem = cartItems.FirstOrDefault(item => item.Product.ProductID == product.ProductID);
            int cartQuantity = existingCartItem == null ? 0 : existingCartItem.Quantity;

            Border card = new Border();
            card.Width = 230;
            card.Height = 145;
            card.Background = Brushes.White;
            card.CornerRadius = new CornerRadius(14);
            card.Padding = new Thickness(18);
            card.Margin = new Thickness(0, 0, 14, 14);
            card.Cursor = System.Windows.Input.Cursors.Hand;

            if (cartQuantity > 0)
            {
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 200, 83));
                card.BorderThickness = new Thickness(2);
            }
            else
            {
                card.BorderBrush = new SolidColorBrush(Color.FromRgb(229, 231, 235));
                card.BorderThickness = new Thickness(1);
            }

            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            StackPanel topPanel = new StackPanel();

            TextBlock code = new TextBlock();
            code.Text = product.ProductCode;
            code.FontSize = 11;
            code.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));

            TextBlock name = new TextBlock();
            name.Text = product.ProductName;
            name.FontSize = 15;
            name.FontWeight = FontWeights.Bold;
            name.Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42));
            name.Margin = new Thickness(0, 4, 0, 0);
            name.TextWrapping = TextWrapping.Wrap;

            TextBlock category = new TextBlock();
            category.Text = product.Category;
            category.FontSize = 13;
            category.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            category.FontWeight = FontWeights.SemiBold;
            category.Margin = new Thickness(0, 4, 0, 0);

            topPanel.Children.Add(code);
            topPanel.Children.Add(name);
            topPanel.Children.Add(category);

            Grid bottomGrid = new Grid();
            bottomGrid.Margin = new Thickness(0, 12, 0, 0);
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition());
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            TextBlock price = new TextBlock();
            price.Text = "₱" + product.UnitPrice.ToString("0.00");
            price.FontSize = 16;
            price.FontWeight = FontWeights.Bold;
            price.Foreground = new SolidColorBrush(Color.FromRgb(0, 166, 81));

            StackPanel rightPanel = new StackPanel();
            rightPanel.HorizontalAlignment = HorizontalAlignment.Right;

            TextBlock stock = new TextBlock();
            stock.Text = GetStockText(product);
            stock.Foreground = GetStockColor(product);
            stock.FontSize = 13;
            stock.HorizontalAlignment = HorizontalAlignment.Right;

            rightPanel.Children.Add(stock);

            if (cartQuantity > 0)
            {
                TextBlock cartText = new TextBlock();
                cartText.Text = "+" + cartQuantity + " in cart";
                cartText.Foreground = new SolidColorBrush(Color.FromRgb(0, 166, 81));
                cartText.FontWeight = FontWeights.SemiBold;
                cartText.FontSize = 13;
                cartText.HorizontalAlignment = HorizontalAlignment.Right;
                rightPanel.Children.Add(cartText);
            }

            Grid.SetColumn(price, 0);
            Grid.SetColumn(rightPanel, 1);

            bottomGrid.Children.Add(price);
            bottomGrid.Children.Add(rightPanel);

            Grid.SetRow(topPanel, 0);
            Grid.SetRow(bottomGrid, 1);

            grid.Children.Add(topPanel);
            grid.Children.Add(bottomGrid);

            card.Child = grid;

            card.MouseLeftButtonUp += (s, e) =>
            {
                AddToCart(product);
            };

            return card;
        }

        private string GetStockText(Product product)
        {
            if (product.StockQuantity <= product.ReorderLevel)
            {
                return "⚠ " + product.StockQuantity;
            }

            return product.StockQuantity + " left";
        }

        private Brush GetStockColor(Product product)
        {
            if (product.StockQuantity <= product.ReorderLevel)
            {
                return new SolidColorBrush(Color.FromRgb(234, 88, 12));
            }

            return new SolidColorBrush(Color.FromRgb(100, 116, 139));
        }

        private void AddToCart(Product product)
        {
            CartItem existingItem = cartItems.FirstOrDefault(item => item.Product.ProductID == product.ProductID);

            if (existingItem != null)
            {
                if (existingItem.Quantity >= product.StockQuantity)
                {
                    MessageBox.Show("Not enough stock available.");
                    return;
                }

                existingItem.Quantity++;
            }
            else
            {
                CartItem item = new CartItem();
                item.Product = product;
                item.Quantity = 1;
                cartItems.Add(item);
            }

            LoadProducts();
            RefreshCart();
        }

        private void RefreshCart()
        {
            cartPanel.Children.Clear();

            int totalQuantity = cartItems.Sum(item => item.Quantity);

            if (totalQuantity > 0)
            {
                cartCountBadge.Visibility = Visibility.Visible;
                txtCartCount.Text = totalQuantity + " items";
            }
            else
            {
                cartCountBadge.Visibility = Visibility.Collapsed;
            }

            if (cartItems.Count == 0)
            {
                StackPanel emptyPanel = new StackPanel();
                emptyPanel.Margin = new Thickness(0, 110, 0, 0);

                TextBlock icon = new TextBlock();
                icon.Text = "🛒";
                icon.FontSize = 34;
                icon.Foreground = new SolidColorBrush(Color.FromRgb(203, 213, 225));
                icon.HorizontalAlignment = HorizontalAlignment.Center;

                TextBlock empty = new TextBlock();
                empty.Text = "Cart is empty";
                empty.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                empty.HorizontalAlignment = HorizontalAlignment.Center;
                empty.Margin = new Thickness(0, 8, 0, 0);

                TextBlock hint = new TextBlock();
                hint.Text = "Tap a product to add";
                hint.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
                hint.HorizontalAlignment = HorizontalAlignment.Center;

                emptyPanel.Children.Add(icon);
                emptyPanel.Children.Add(empty);
                emptyPanel.Children.Add(hint);

                cartPanel.Children.Add(emptyPanel);
            }
            else
            {
                foreach (CartItem item in cartItems)
                {
                    cartPanel.Children.Add(CreateCartRow(item));
                }
            }

            UpdateTotal();
        }

        private Border CreateCartRow(CartItem item)
        {
            Border row = new Border();
            row.Background = new SolidColorBrush(Color.FromRgb(248, 250, 252));
            row.CornerRadius = new CornerRadius(14);
            row.Padding = new Thickness(16);
            row.Margin = new Thickness(0, 0, 0, 12);

            StackPanel main = new StackPanel();

            Grid header = new Grid();
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel namePanel = new StackPanel();

            TextBlock name = new TextBlock();
            name.Text = item.Product.ProductName;
            name.FontWeight = FontWeights.Bold;
            name.FontSize = 14;
            name.Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42));

            TextBlock each = new TextBlock();
            each.Text = "₱" + item.Product.UnitPrice.ToString("0.00") + " each";
            each.Foreground = new SolidColorBrush(Color.FromRgb(100, 116, 139));
            each.FontSize = 12;
            each.Margin = new Thickness(0, 3, 0, 0);

            namePanel.Children.Add(name);
            namePanel.Children.Add(each);

            Button remove = new Button();
            remove.Content = "🗑";
            remove.Width = 26;
            remove.Height = 26;
            remove.Background = Brushes.Transparent;
            remove.BorderThickness = new Thickness(0);
            remove.Foreground = Brushes.Red;
            remove.Cursor = System.Windows.Input.Cursors.Hand;
            remove.Click += (s, e) =>
            {
                cartItems.Remove(item);
                LoadProducts();
                RefreshCart();
            };

            Grid.SetColumn(namePanel, 0);
            Grid.SetColumn(remove, 1);

            header.Children.Add(namePanel);
            header.Children.Add(remove);

            Grid footer = new Grid();
            footer.Margin = new Thickness(0, 14, 0, 0);
            footer.ColumnDefinitions.Add(new ColumnDefinition());
            footer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            StackPanel qtyPanel = new StackPanel();
            qtyPanel.Orientation = Orientation.Horizontal;

            Button minus = CreateQtyButton("-");
            minus.Click += (s, e) =>
            {
                if (item.Quantity > 1)
                {
                    item.Quantity--;
                }
                else
                {
                    cartItems.Remove(item);
                }

                LoadProducts();
                RefreshCart();
            };

            TextBlock quantity = new TextBlock();
            quantity.Text = item.Quantity.ToString();
            quantity.Width = 34;
            quantity.TextAlignment = TextAlignment.Center;
            quantity.VerticalAlignment = VerticalAlignment.Center;
            quantity.FontSize = 15;
            quantity.FontWeight = FontWeights.Bold;

            Button plus = CreateQtyButton("+");
            plus.Click += (s, e) =>
            {
                if (item.Quantity >= item.Product.StockQuantity)
                {
                    MessageBox.Show("Not enough stock available.");
                    return;
                }

                item.Quantity++;
                LoadProducts();
                RefreshCart();
            };

            qtyPanel.Children.Add(minus);
            qtyPanel.Children.Add(quantity);
            qtyPanel.Children.Add(plus);

            TextBlock subtotal = new TextBlock();
            subtotal.Text = "₱" + item.Subtotal.ToString("0.00");
            subtotal.FontWeight = FontWeights.Bold;
            subtotal.FontSize = 15;
            subtotal.Foreground = new SolidColorBrush(Color.FromRgb(15, 23, 42));
            subtotal.VerticalAlignment = VerticalAlignment.Center;

            Grid.SetColumn(qtyPanel, 0);
            Grid.SetColumn(subtotal, 1);

            footer.Children.Add(qtyPanel);
            footer.Children.Add(subtotal);

            main.Children.Add(header);
            main.Children.Add(footer);

            row.Child = main;

            return row;
        }

        private Button CreateQtyButton(string text)
        {
            Button button = new Button();
            button.Content = text;
            button.Width = 34;
            button.Height = 34;
            button.Background = Brushes.White;
            button.BorderBrush = new SolidColorBrush(Color.FromRgb(226, 232, 240));
            button.BorderThickness = new Thickness(1);
            button.FontSize = 16;
            button.FontWeight = FontWeights.Bold;
            button.Cursor = System.Windows.Input.Cursors.Hand;

            return button;
        }

        private decimal GetTotal()
        {
            return cartItems.Sum(item => item.Subtotal);
        }

        private void UpdateTotal()
        {
            decimal total = GetTotal();
            txtTotal.Text = "₱" + total.ToString("0.00");
        }

        private void ProcessPayment_Click(object sender, RoutedEventArgs e)
        {
            if (cartItems.Count == 0)
            {
                MessageBox.Show("Cart is empty.");
                return;
            }

            decimal total = GetTotal();

            txtAmountDue.Text = "₱" + total.ToString("0.00");
            txtCashTendered.Text = total.ToString("0.00");
            txtPaymentChange.Text = "₱0.00";

            PaymentOverlay.Visibility = Visibility.Visible;
        }

        private void ClosePayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentOverlay.Visibility = Visibility.Collapsed;
        }

        private void CashTendered_Changed(object sender, TextChangedEventArgs e)
        {
            decimal total = GetTotal();
            decimal cash;

            if (decimal.TryParse(txtCashTendered.Text, out cash))
            {
                decimal change = cash - total;

                if (change < 0)
                {
                    txtPaymentChange.Text = "₱0.00";
                    btnCompleteTransaction.IsEnabled = false;
                    btnCompleteTransaction.Background = new SolidColorBrush(Color.FromRgb(134, 209, 160));
                }
                else
                {
                    txtPaymentChange.Text = "₱" + change.ToString("0.00");
                    btnCompleteTransaction.IsEnabled = true;
                    btnCompleteTransaction.Background = new SolidColorBrush(Color.FromRgb(0, 166, 81));
                }
            }
            else
            {
                txtPaymentChange.Text = "₱0.00";
                btnCompleteTransaction.IsEnabled = false;
                btnCompleteTransaction.Background = new SolidColorBrush(Color.FromRgb(134, 209, 160));
            }
        }

        private void QuickAmount_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            txtCashTendered.Text = button.Tag.ToString();
        }

        private void CompleteTransaction_Click(object sender, RoutedEventArgs e)
        {
            decimal total = GetTotal();
            decimal cash;

            if (!decimal.TryParse(txtCashTendered.Text, out cash))
            {
                MessageBox.Show("Please enter a valid cash amount.");
                return;
            }

            if (cash < total)
            {
                MessageBox.Show("Cash tendered is not enough.");
                return;
            }

            lastSoldItems.Clear();

            foreach (CartItem item in cartItems)
            {
                CartItem sold = new CartItem();
                sold.Product = item.Product;
                sold.Quantity = item.Quantity;
                lastSoldItems.Add(sold);
            }

            foreach (CartItem item in cartItems)
            {
                item.Product.StockQuantity -= item.Quantity;
            }

            TransactionRecord record = new TransactionRecord();
            record.ReceiptNo = "TXN-" + (DataStore.Transactions.Count + 1).ToString("0000");
            record.TransactionDate = DateTime.Now;
            record.Cashier = "Store Owner";
            record.TotalItems = cartItems.Sum(item => item.Quantity);
            record.TotalAmount = total;
            record.PaymentAmount = cash;
            record.ChangeAmount = cash - total;

            DataStore.Transactions.Add(record);
            lastTransaction = record;

            PaymentOverlay.Visibility = Visibility.Collapsed;

            BuildReceiptText();

            ReceiptOverlay.Visibility = Visibility.Visible;
        }

        private void BuildReceiptText()
        {
            if (lastTransaction == null)
            {
                return;
            }

            StringBuilder receipt = new StringBuilder();

            receipt.AppendLine("        FRESHMART GROCERY");
            receipt.AppendLine("    123 Rizal Street, Quezon City");
            receipt.AppendLine("       Tel: (02) 8123-4567");
            receipt.AppendLine("       TIN: 000-000-000-000");
            receipt.AppendLine("--------------------------------------");
            receipt.AppendLine("Receipt #:              " + lastTransaction.ReceiptNo);
            receipt.AppendLine("Date:                   " + lastTransaction.TransactionDate.ToString("M/d/yyyy"));
            receipt.AppendLine("Time:                   " + lastTransaction.TransactionDate.ToString("h:mm:ss tt"));
            receipt.AppendLine("Cashier:                " + lastTransaction.Cashier);
            receipt.AppendLine("--------------------------------------");

            foreach (CartItem item in lastSoldItems)
            {
                receipt.AppendLine(item.Product.ProductName);
                receipt.AppendLine(item.Quantity + " x ₱" +
                                   item.Product.UnitPrice.ToString("0.00") +
                                   "                    ₱" +
                                   item.Subtotal.ToString("0.00"));
            }

            receipt.AppendLine("--------------------------------------");
            receipt.AppendLine("TOTAL                  ₱" + lastTransaction.TotalAmount.ToString("0.00"));
            receipt.AppendLine("--------------------------------------");
            receipt.AppendLine("Cash Tendered          ₱" + lastTransaction.PaymentAmount.ToString("0.00"));
            receipt.AppendLine("Change                 ₱" + lastTransaction.ChangeAmount.ToString("0.00"));
            receipt.AppendLine("--------------------------------------");
            receipt.AppendLine();
            receipt.AppendLine("   Thank you for shopping at FreshMart!");
            receipt.AppendLine("          Please come again");
            receipt.AppendLine();
            receipt.AppendLine("        *** Official Receipt ***");

            txtReceiptText.Text = receipt.ToString();
        }

        private void PrintReceipt_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();

            bool? result = printDialog.ShowDialog();

            if (result == true)
            {
                printDialog.PrintVisual(receiptPrintArea, "NexPOS Receipt");
            }
        }

        private void NewSale_Click(object sender, RoutedEventArgs e)
        {
            ReceiptOverlay.Visibility = Visibility.Collapsed;

            cartItems.Clear();
            lastSoldItems.Clear();
            lastTransaction = null;

            LoadProducts();
            RefreshCart();
        }
    }
}