using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace NexPOS
{
    public partial class InventoryPage : UserControl
    {
        private int editingProductID = 0;
        private int currentBatchProductID = 0;
        private int editingBatchID = 0;
        private bool isAutoCompleteChanging = false;

        private class BatchRow
        {
            public int BatchID { get; set; }
            public int ProductID { get; set; }
            public int StockQuantity { get; set; }
            public DateTime ExpirationDate { get; set; }
            public DateTime DateReceived { get; set; }
            public string Status { get; set; }

            public string ExpirationText
            {
                get { return ExpirationDate.ToString("M/d/yyyy"); }
            }

            public string DateReceivedText
            {
                get { return DateReceived.ToString("M/d/yyyy"); }
            }
        }

        public InventoryPage()
        {
            InitializeComponent();

            LoadFilters();
            RefreshInventory();
        }

        private void RefreshDashboard()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

                if (mainWindow != null)
                {
                    mainWindow.LoadDashboard();
                }
            });
        }

        private void LoadFilters()
        {
            cmbCategory.Items.Clear();
            cmbCategory.Items.Add("All Categories");

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var categories = db.tbl_Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                foreach (var category in categories)
                {
                    cmbCategory.Items.Add(category.CategoryName);
                }
            }

            cmbCategory.SelectedIndex = 0;

            cmbStatus.Items.Clear();
            cmbStatus.Items.Add("All Status");
            cmbStatus.Items.Add("Active");
            cmbStatus.Items.Add("Inactive");
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadProductCategoryCombo()
        {
            cmbProductCategory.Items.Clear();

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var categories = db.tbl_Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                foreach (var category in categories)
                {
                    cmbProductCategory.Items.Add(new CategoryComboItem
                    {
                        CategoryID = category.CategoryID,
                        CategoryName = category.CategoryName
                    });
                }
            }

            if (cmbProductCategory.Items.Count > 0)
            {
                cmbProductCategory.SelectedIndex = 0;
            }
        }

        private void RefreshInventory()
        {
            string search = "";

            if (txtSearch != null)
            {
                search = txtSearch.Text.ToLower();
            }

            string selectedCategory = "All Categories";

            if (cmbCategory != null && cmbCategory.SelectedItem != null)
            {
                selectedCategory = cmbCategory.SelectedItem.ToString();
            }

            string selectedStatus = "All Status";

            if (cmbStatus != null && cmbStatus.SelectedItem != null)
            {
                selectedStatus = cmbStatus.SelectedItem.ToString();
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                RefreshAllProductSummaries(db);
                db.SubmitChanges();

                var products = db.tbl_Products
                    .Join(
                        db.tbl_Categories,
                        product => product.CategoryID,
                        category => category.CategoryID,
                        (product, category) => new InventoryProductRow
                        {
                            ProductID = product.ProductID,
                            ProductCode = product.ProductCode,
                            ProductName = product.ProductName,
                            CategoryID = product.CategoryID,
                            CategoryName = category.CategoryName,
                            UnitPrice = product.UnitPrice,
                            StockQuantity = product.StockQuantity,
                            ExpirationDate = product.ExpirationDate,
                            ReorderLevel = product.ReorderLevel,
                            Status = product.Status
                        }
                    )
                    .ToList();

                var filteredProducts = products.Where(product =>
                    (product.ProductCode.ToLower().Contains(search) ||
                     product.ProductName.ToLower().Contains(search)) &&

                    (selectedCategory == "All Categories" ||
                     product.CategoryName == selectedCategory) &&

                    (selectedStatus == "All Status" ||
                     product.Status == selectedStatus)
                ).ToList();

                gridProducts.ItemsSource = null;
                gridProducts.ItemsSource = filteredProducts;

                int totalProducts = products.Count;
                int activeProducts = products.Count(p => p.Status == "Active");
                int lowStockProducts = products.Count(p => p.StockQuantity <= p.ReorderLevel && p.StockQuantity > 0);
                int outOfStockProducts = products.Count(p => p.StockQuantity == 0);

                txtTotalProducts.Text = totalProducts.ToString();
                txtActiveProducts.Text = activeProducts.ToString();
                txtLowStock.Text = lowStockProducts.ToString();
                txtOutOfStock.Text = outOfStockProducts.ToString();
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (gridProducts != null)
            {
                RefreshInventory();
            }
        }

        private void ShowAddProduct_Click(object sender, RoutedEventArgs e)
        {
            editingProductID = 0;

            txtModalTitle.Text = "Add Product / Batch";
            btnSaveProduct.Content = "Save Product";

            txtProductCode.IsEnabled = true;
            txtProductName.IsEnabled = true;
            txtStockQty.IsEnabled = true;
            dpExpirationDate.IsEnabled = true;

            SetEditableComboBoxText(txtProductCode, "");
            SetEditableComboBoxText(txtProductName, "");

            txtUnitPrice.Text = "";
            txtStockQty.Text = "";
            txtReorderLevel.Text = "10";
            dpExpirationDate.SelectedDate = DateTime.Today;
            cmbProductStatus.SelectedIndex = 0;

            LoadProductCategoryCombo();

            ProductOverlay.Visibility = Visibility.Visible;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            InventoryProductRow selectedProduct = button.DataContext as InventoryProductRow;

            if (selectedProduct == null)
            {
                return;
            }

            editingProductID = selectedProduct.ProductID;

            txtModalTitle.Text = "Edit Product";
            btnSaveProduct.Content = "Update Product";

            txtProductCode.IsEnabled = true;
            txtProductName.IsEnabled = true;

            txtStockQty.Text = "";
            txtStockQty.IsEnabled = false;

            dpExpirationDate.SelectedDate = null;
            dpExpirationDate.IsEnabled = false;

            SetEditableComboBoxText(txtProductCode, selectedProduct.ProductCode);
            SetEditableComboBoxText(txtProductName, selectedProduct.ProductName);

            txtUnitPrice.Text = selectedProduct.UnitPrice.ToString("0.00");
            txtReorderLevel.Text = selectedProduct.ReorderLevel.ToString();

            cmbProductStatus.SelectedIndex = selectedProduct.Status == "Active" ? 0 : 1;

            LoadProductCategoryCombo();
            SelectProductCategory(selectedProduct.CategoryID);

            ProductOverlay.Visibility = Visibility.Visible;
        }

        private void ProductCodeAutoComplete_KeyUp(object sender, KeyEventArgs e)
        {
            if (ShouldIgnoreAutoCompleteKey(e.Key) || isAutoCompleteChanging || editingProductID > 0)
            {
                return;
            }

            string typedText = txtProductCode.Text.Trim();

            if (typedText == "")
            {
                txtProductCode.ItemsSource = null;
                txtProductCode.IsDropDownOpen = false;
                return;
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                string searchText = typedText.ToLower();

                List<string> suggestions = db.tbl_Products
                    .Where(p => p.ProductCode.ToLower().Contains(searchText))
                    .OrderBy(p => p.ProductCode)
                    .Select(p => p.ProductCode)
                    .Distinct()
                    .Take(10)
                    .ToList();

                ApplyComboSuggestions(txtProductCode, typedText, suggestions);
            }
        }

        private void ProductNameAutoComplete_KeyUp(object sender, KeyEventArgs e)
        {
            if (ShouldIgnoreAutoCompleteKey(e.Key) || isAutoCompleteChanging || editingProductID > 0)
            {
                return;
            }

            string typedText = txtProductName.Text.Trim();

            if (typedText == "")
            {
                txtProductName.ItemsSource = null;
                txtProductName.IsDropDownOpen = false;
                return;
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                string searchText = typedText.ToLower();

                List<string> suggestions = db.tbl_Products
                    .Where(p => p.ProductName.ToLower().Contains(searchText))
                    .OrderBy(p => p.ProductName)
                    .Select(p => p.ProductName)
                    .Distinct()
                    .Take(10)
                    .ToList();

                ApplyComboSuggestions(txtProductName, typedText, suggestions);
            }
        }

        private bool ShouldIgnoreAutoCompleteKey(Key key)
        {
            return key == Key.Up ||
                   key == Key.Down ||
                   key == Key.Left ||
                   key == Key.Right ||
                   key == Key.Enter ||
                   key == Key.Escape ||
                   key == Key.Tab;
        }

        private void ApplyComboSuggestions(ComboBox comboBox, string typedText, List<string> suggestions)
        {
            bool previousState = isAutoCompleteChanging;
            isAutoCompleteChanging = true;

            comboBox.ItemsSource = suggestions;
            comboBox.IsDropDownOpen = suggestions.Count > 0;
            comboBox.Text = typedText;

            comboBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                MoveComboCaretToEnd(comboBox);
            }), DispatcherPriority.Background);

            isAutoCompleteChanging = previousState;
        }

        private void MoveComboCaretToEnd(ComboBox comboBox)
        {
            comboBox.ApplyTemplate();

            TextBox editableTextBox =
                comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

            if (editableTextBox != null)
            {
                editableTextBox.CaretIndex = editableTextBox.Text.Length;
                editableTextBox.Focus();
            }
        }

        private void SetEditableComboBoxText(ComboBox comboBox, string value)
        {
            string safeValue = value ?? "";

            bool previousState = isAutoCompleteChanging;
            isAutoCompleteChanging = true;

            comboBox.IsDropDownOpen = false;
            comboBox.SelectedIndex = -1;
            comboBox.SelectedItem = null;
            comboBox.ItemsSource = null;
            comboBox.Text = safeValue;

            comboBox.ApplyTemplate();

            TextBox editableTextBox =
                comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

            if (editableTextBox != null)
            {
                editableTextBox.Text = safeValue;
                editableTextBox.CaretIndex = editableTextBox.Text.Length;
            }

            comboBox.Dispatcher.BeginInvoke(new Action(() =>
            {
                bool innerPreviousState = isAutoCompleteChanging;
                isAutoCompleteChanging = true;

                comboBox.IsDropDownOpen = false;
                comboBox.Text = safeValue;

                comboBox.ApplyTemplate();

                TextBox innerTextBox =
                    comboBox.Template.FindName("PART_EditableTextBox", comboBox) as TextBox;

                if (innerTextBox != null)
                {
                    innerTextBox.Text = safeValue;
                    innerTextBox.CaretIndex = innerTextBox.Text.Length;
                }

                isAutoCompleteChanging = innerPreviousState;

            }), DispatcherPriority.Background);

            isAutoCompleteChanging = previousState;
        }

        private void ProductCodeSuggestion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isAutoCompleteChanging || editingProductID > 0)
            {
                return;
            }

            string selectedCode = txtProductCode.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedCode))
            {
                return;
            }

            LoadProductFromSuggestion(selectedCode, true);
        }

        private void ProductNameSuggestion_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isAutoCompleteChanging || editingProductID > 0)
            {
                return;
            }

            string selectedName = txtProductName.SelectedItem as string;

            if (string.IsNullOrWhiteSpace(selectedName))
            {
                return;
            }

            LoadProductFromSuggestion(selectedName, false);
        }

        private void LoadProductFromSuggestion(string value, bool searchByCode)
        {
            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                tbl_Product product = null;

                if (searchByCode)
                {
                    product = db.tbl_Products.FirstOrDefault(p => p.ProductCode == value);
                }
                else
                {
                    product = db.tbl_Products.FirstOrDefault(p => p.ProductName == value);
                }

                if (product == null)
                {
                    return;
                }

                editingProductID = 0;

                txtModalTitle.Text = "Add Stock Batch";
                btnSaveProduct.Content = "Add Batch";

                txtStockQty.IsEnabled = true;
                dpExpirationDate.IsEnabled = true;

                SetEditableComboBoxText(txtProductCode, product.ProductCode);
                SetEditableComboBoxText(txtProductName, product.ProductName);

                txtUnitPrice.Text = product.UnitPrice.ToString("0.00");
                txtStockQty.Text = "";
                txtReorderLevel.Text = product.ReorderLevel.ToString();
                dpExpirationDate.SelectedDate = DateTime.Today;

                cmbProductStatus.SelectedIndex = product.Status == "Active" ? 0 : 1;

                LoadProductCategoryCombo();
                SelectProductCategory(product.CategoryID);
            }
        }

        private void SelectProductCategory(int categoryID)
        {
            foreach (object item in cmbProductCategory.Items)
            {
                CategoryComboItem categoryItem = item as CategoryComboItem;

                if (categoryItem != null && categoryItem.CategoryID == categoryID)
                {
                    cmbProductCategory.SelectedItem = categoryItem;
                    break;
                }
            }
        }

        private string GetBatchStatus(DateTime expirationDate)
        {
            if (expirationDate.Date < DateTime.Today)
            {
                return "Expired";
            }

            return "Active";
        }

        private void InsertOrUpdateProductBatch(
            NexPOSDataDataContext db,
            int productID,
            int stockQty,
            DateTime expirationDate)
        {
            DateTime cleanExpirationDate = expirationDate.Date;
            string batchStatus = GetBatchStatus(cleanExpirationDate);

            var existingBatch = db.tbl_ProductBatches
                .FirstOrDefault(b =>
                    b.ProductID == productID &&
                    b.ExpirationDate == cleanExpirationDate &&
                    b.Status == batchStatus);

            if (existingBatch != null)
            {
                existingBatch.StockQuantity += stockQty;
                existingBatch.DateReceived = DateTime.Now;
            }
            else
            {
                tbl_ProductBatch newBatch = new tbl_ProductBatch();

                newBatch.ProductID = productID;
                newBatch.StockQuantity = stockQty;
                newBatch.ExpirationDate = cleanExpirationDate;
                newBatch.DateReceived = DateTime.Now;
                newBatch.Status = batchStatus;

                db.tbl_ProductBatches.InsertOnSubmit(newBatch);
            }
        }

        private void RefreshAllProductSummaries(NexPOSDataDataContext db)
        {
            var productIDs = db.tbl_Products
                .Select(p => p.ProductID)
                .ToList();

            foreach (int productID in productIDs)
            {
                SyncProductStockSummary(db, productID);
            }
        }

        private void SyncProductStockSummary(NexPOSDataDataContext db, int productID)
        {
            var product = db.tbl_Products
                .FirstOrDefault(p => p.ProductID == productID);

            if (product == null)
            {
                return;
            }

            var productBatches = db.tbl_ProductBatches
                .Where(b => b.ProductID == productID)
                .ToList();

            foreach (var batch in productBatches)
            {
                if (batch.ExpirationDate < DateTime.Today && batch.Status == "Active")
                {
                    batch.Status = "Expired";
                }
            }

            var activeBatches = productBatches
                .Where(b =>
                    b.Status == "Active" &&
                    b.StockQuantity > 0 &&
                    b.ExpirationDate >= DateTime.Today)
                .ToList();

            if (activeBatches.Count == 0)
            {
                product.StockQuantity = 0;
                product.ExpirationDate = null;
                return;
            }

            product.StockQuantity = activeBatches.Sum(b => b.StockQuantity);
            product.ExpirationDate = activeBatches.Min(b => b.ExpirationDate);
        }

        private void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            string productCode = txtProductCode.Text.Trim();
            string productName = txtProductName.Text.Trim();

            decimal unitPrice;
            int reorderLevel;

            if (productCode == "")
            {
                MessageBox.Show("Product code is required.");
                return;
            }

            if (productName == "")
            {
                MessageBox.Show("Product name is required.");
                return;
            }

            if (cmbProductCategory.SelectedItem == null)
            {
                MessageBox.Show("Category is required.");
                return;
            }

            if (cmbProductStatus.SelectedItem == null)
            {
                MessageBox.Show("Status is required.");
                return;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out unitPrice))
            {
                MessageBox.Show("Unit price must be a number.");
                return;
            }

            if (!int.TryParse(txtReorderLevel.Text, out reorderLevel))
            {
                MessageBox.Show("Reorder level must be a number.");
                return;
            }

            CategoryComboItem selectedCategory = cmbProductCategory.SelectedItem as CategoryComboItem;

            if (selectedCategory == null)
            {
                MessageBox.Show("Invalid category selected.");
                return;
            }

            ComboBoxItem selectedStatusItem = cmbProductStatus.SelectedItem as ComboBoxItem;
            string productStatus = selectedStatusItem.Content.ToString();

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                if (editingProductID > 0)
                {
                    bool duplicateCode = db.tbl_Products.Any(p =>
                        p.ProductCode.ToLower() == productCode.ToLower() &&
                        p.ProductID != editingProductID
                    );

                    if (duplicateCode)
                    {
                        MessageBox.Show("Product code already exists.");
                        return;
                    }

                    var productToUpdate = db.tbl_Products
                        .FirstOrDefault(p => p.ProductID == editingProductID);

                    if (productToUpdate == null)
                    {
                        MessageBox.Show("Product not found.");
                        return;
                    }

                    productToUpdate.ProductCode = productCode;
                    productToUpdate.ProductName = productName;
                    productToUpdate.CategoryID = selectedCategory.CategoryID;
                    productToUpdate.UnitPrice = unitPrice;
                    productToUpdate.ReorderLevel = reorderLevel;
                    productToUpdate.Status = productStatus;

                    SyncProductStockSummary(db, productToUpdate.ProductID);

                    db.SubmitChanges();

                    RefreshDashboard();

                    MessageBox.Show("Product information updated successfully.");
                }
                else
                {
                    int stockQty;

                    if (!int.TryParse(txtStockQty.Text, out stockQty))
                    {
                        MessageBox.Show("Stock quantity must be a number.");
                        return;
                    }

                    if (stockQty <= 0)
                    {
                        MessageBox.Show("Stock quantity must be greater than zero.");
                        return;
                    }

                    if (dpExpirationDate.SelectedDate == null)
                    {
                        MessageBox.Show("Expiration date is required.");
                        return;
                    }

                    DateTime expirationDate = dpExpirationDate.SelectedDate.Value.Date;

                    var existingProduct = db.tbl_Products
                        .FirstOrDefault(p => p.ProductCode.ToLower() == productCode.ToLower());

                    if (existingProduct == null)
                    {
                        tbl_Product newProduct = new tbl_Product();

                        newProduct.ProductCode = productCode;
                        newProduct.ProductName = productName;
                        newProduct.CategoryID = selectedCategory.CategoryID;
                        newProduct.UnitPrice = unitPrice;
                        newProduct.StockQuantity = 0;
                        newProduct.ExpirationDate = null;
                        newProduct.ReorderLevel = reorderLevel;
                        newProduct.Status = productStatus;

                        db.tbl_Products.InsertOnSubmit(newProduct);
                        db.SubmitChanges();

                        InsertOrUpdateProductBatch(
                            db,
                            newProduct.ProductID,
                            stockQty,
                            expirationDate);

                        SyncProductStockSummary(db, newProduct.ProductID);

                        db.SubmitChanges();

                        RefreshDashboard();

                        if (expirationDate < DateTime.Today)
                        {
                            MessageBox.Show("Product added, but the batch is marked as expired.");
                        }
                        else
                        {
                            MessageBox.Show("Product and first batch added successfully.");
                        }
                    }
                    else
                    {
                        InsertOrUpdateProductBatch(
                            db,
                            existingProduct.ProductID,
                            stockQty,
                            expirationDate);

                        SyncProductStockSummary(db, existingProduct.ProductID);

                        db.SubmitChanges();

                        RefreshDashboard();

                        if (expirationDate < DateTime.Today)
                        {
                            MessageBox.Show("Expired batch added for existing product.");
                        }
                        else
                        {
                            MessageBox.Show("New batch added for existing product.");
                        }
                    }
                }
            }

            ProductOverlay.Visibility = Visibility.Collapsed;
            editingProductID = 0;

            txtStockQty.IsEnabled = true;
            dpExpirationDate.IsEnabled = true;

            LoadFilters();
            RefreshInventory();
        }

        private void ViewBatches_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            InventoryProductRow selectedProduct = button.DataContext as InventoryProductRow;

            if (selectedProduct == null)
            {
                return;
            }

            currentBatchProductID = selectedProduct.ProductID;
            txtBatchModalTitle.Text = selectedProduct.ProductCode + " - " + selectedProduct.ProductName + " Batches";

            ClearBatchEditor();
            LoadBatches(currentBatchProductID);

            BatchOverlay.Visibility = Visibility.Visible;
        }

        private void LoadBatches(int productID)
        {
            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                SyncProductStockSummary(db, productID);
                db.SubmitChanges();

                var batches = db.tbl_ProductBatches
                    .Where(b => b.ProductID == productID)
                    .OrderBy(b => b.ExpirationDate)
                    .Select(b => new BatchRow
                    {
                        BatchID = b.BatchID,
                        ProductID = b.ProductID,
                        StockQuantity = b.StockQuantity,
                        ExpirationDate = b.ExpirationDate,
                        DateReceived = b.DateReceived,
                        Status = b.Status
                    })
                    .ToList();

                gridBatches.ItemsSource = null;
                gridBatches.ItemsSource = batches;
            }
        }

        private void EditBatch_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            BatchRow selectedBatch = button.DataContext as BatchRow;

            if (selectedBatch == null)
            {
                return;
            }

            editingBatchID = selectedBatch.BatchID;

            txtBatchQuantity.Text = selectedBatch.StockQuantity.ToString();
            dpBatchExpiration.SelectedDate = selectedBatch.ExpirationDate;

            if (selectedBatch.Status == "Active")
            {
                cmbBatchStatus.SelectedIndex = 0;
            }
            else if (selectedBatch.Status == "Expired")
            {
                cmbBatchStatus.SelectedIndex = 1;
            }
            else
            {
                cmbBatchStatus.SelectedIndex = 2;
            }
        }

        private void UpdateBatch_Click(object sender, RoutedEventArgs e)
        {
            int quantity;

            if (editingBatchID == 0)
            {
                MessageBox.Show("Please select a batch to edit.");
                return;
            }

            if (!int.TryParse(txtBatchQuantity.Text, out quantity))
            {
                MessageBox.Show("Quantity must be a number.");
                return;
            }

            if (quantity < 0)
            {
                MessageBox.Show("Quantity cannot be negative.");
                return;
            }

            if (dpBatchExpiration.SelectedDate == null)
            {
                MessageBox.Show("Expiration date is required.");
                return;
            }

            if (cmbBatchStatus.SelectedItem == null)
            {
                MessageBox.Show("Status is required.");
                return;
            }

            DateTime expirationDate = dpBatchExpiration.SelectedDate.Value.Date;

            ComboBoxItem statusItem = cmbBatchStatus.SelectedItem as ComboBoxItem;
            string status = statusItem.Content.ToString();

            if (expirationDate < DateTime.Today)
            {
                status = "Expired";
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var batchToUpdate = db.tbl_ProductBatches
                    .FirstOrDefault(b => b.BatchID == editingBatchID);

                if (batchToUpdate == null)
                {
                    MessageBox.Show("Batch not found.");
                    return;
                }

                int productID = batchToUpdate.ProductID;

                batchToUpdate.StockQuantity = quantity;
                batchToUpdate.ExpirationDate = expirationDate;
                batchToUpdate.Status = status;

                SyncProductStockSummary(db, productID);

                db.SubmitChanges();

                RefreshDashboard();

                LoadBatches(productID);
                RefreshInventory();
                ClearBatchEditor();

                MessageBox.Show("Batch updated successfully.");
            }
        }

        private void DeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            BatchRow selectedBatch = button.DataContext as BatchRow;

            if (selectedBatch == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete this batch?",
                "Delete Batch",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var batchToDelete = db.tbl_ProductBatches
                    .FirstOrDefault(b => b.BatchID == selectedBatch.BatchID);

                if (batchToDelete == null)
                {
                    MessageBox.Show("Batch not found.");
                    return;
                }

                int productID = batchToDelete.ProductID;

                db.tbl_ProductBatches.DeleteOnSubmit(batchToDelete);
                db.SubmitChanges();

                SyncProductStockSummary(db, productID);
                db.SubmitChanges();

                RefreshDashboard();

                LoadBatches(productID);
                RefreshInventory();
                ClearBatchEditor();

                MessageBox.Show("Batch deleted successfully.");
            }
        }

        private void ClearBatchEditor()
        {
            editingBatchID = 0;

            txtBatchQuantity.Text = "";
            dpBatchExpiration.SelectedDate = null;
            cmbBatchStatus.SelectedIndex = 0;
        }

        private void CloseBatchOverlay_Click(object sender, RoutedEventArgs e)
        {
            BatchOverlay.Visibility = Visibility.Collapsed;
            currentBatchProductID = 0;
            ClearBatchEditor();

            RefreshInventory();
        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            InventoryProductRow selectedProduct = button.DataContext as InventoryProductRow;

            if (selectedProduct == null)
            {
                return;
            }

            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to delete " + selectedProduct.ProductName + "?",
                "Delete Product",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                using (NexPOSDataDataContext db = new NexPOSDataDataContext())
                {
                    var productToDelete = db.tbl_Products
                        .FirstOrDefault(p => p.ProductID == selectedProduct.ProductID);

                    if (productToDelete == null)
                    {
                        MessageBox.Show("Product not found.");
                        return;
                    }

                    var batchesToDelete = db.tbl_ProductBatches
                        .Where(b => b.ProductID == selectedProduct.ProductID)
                        .ToList();

                    db.tbl_ProductBatches.DeleteAllOnSubmit(batchesToDelete);
                    db.tbl_Products.DeleteOnSubmit(productToDelete);

                    db.SubmitChanges();

                    RefreshDashboard();
                }

                MessageBox.Show("Product deleted successfully.");

                LoadFilters();
                RefreshInventory();
            }
            catch
            {
                MessageBox.Show(
                    "This product cannot be deleted because it may already be used in a transaction.",
                    "Delete Product",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
            }
        }

        private void CloseProductModal_Click(object sender, RoutedEventArgs e)
        {
            ProductOverlay.Visibility = Visibility.Collapsed;
            editingProductID = 0;

            txtStockQty.IsEnabled = true;
            dpExpirationDate.IsEnabled = true;
        }
    }
}