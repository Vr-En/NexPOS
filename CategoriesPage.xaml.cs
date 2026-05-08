using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NexPOS
{
    public partial class CategoriesPage : UserControl
    {
        private int editingCategoryID = 0;

        public CategoriesPage()
        {
            InitializeComponent();
            RefreshCategories();
        }

        private void RefreshCategories()
        {
            categoryListPanel.Children.Clear();

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var categories = db.tbl_Categories
                    .OrderBy(c => c.CategoryName)
                    .ToList();

                txtCategoryCount.Text = categories.Count + " CATEGORIES";

                foreach (var category in categories)
                {
                    int productCount = db.tbl_Products
                        .Count(p => p.CategoryID == category.CategoryID);

                    categoryListPanel.Children.Add(
                        CreateCategoryRow(
                            category.CategoryID,
                            category.CategoryName,
                            productCount
                        )
                    );
                }
            }
        }

        private Border CreateCategoryRow(int categoryID, string categoryName, int productCount)
        {
            Border row = new Border();
            row.Background = Brushes.White;
            row.Padding = new Thickness(20, 14, 20, 14);
            row.BorderBrush = new SolidColorBrush(Color.FromRgb(241, 245, 249));
            row.BorderThickness = new Thickness(0, 0, 0, 1);

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            Border iconBox = new Border();
            iconBox.Width = 32;
            iconBox.Height = 32;
            iconBox.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231));
            iconBox.CornerRadius = new CornerRadius(9);
            iconBox.Margin = new Thickness(0, 0, 12, 0);

            TextBlock icon = new TextBlock();
            icon.Text = "◇";
            icon.Foreground = new SolidColorBrush(Color.FromRgb(0, 166, 81));
            icon.FontSize = 16;
            icon.HorizontalAlignment = HorizontalAlignment.Center;
            icon.VerticalAlignment = VerticalAlignment.Center;

            iconBox.Child = icon;

            StackPanel textPanel = new StackPanel();

            TextBlock name = new TextBlock();
            name.Text = categoryName;
            name.FontWeight = FontWeights.Bold;
            name.FontSize = 14;
            name.Foreground = new SolidColorBrush(Color.FromRgb(17, 24, 39));

            TextBlock count = new TextBlock();
            count.Text = productCount == 1 ? "1 product" : productCount + " products";
            count.FontSize = 12;
            count.Foreground = new SolidColorBrush(Color.FromRgb(148, 163, 184));
            count.Margin = new Thickness(0, 3, 0, 0);

            textPanel.Children.Add(name);
            textPanel.Children.Add(count);

            StackPanel actionPanel = new StackPanel();
            actionPanel.Orientation = Orientation.Horizontal;
            actionPanel.VerticalAlignment = VerticalAlignment.Center;

            Button editButton = new Button();
            editButton.Content = "✎";
            editButton.Width = 30;
            editButton.Height = 30;
            editButton.Background = Brushes.Transparent;
            editButton.BorderThickness = new Thickness(0);
            editButton.Foreground = new SolidColorBrush(Color.FromRgb(37, 99, 235));
            editButton.FontSize = 16;
            editButton.Cursor = System.Windows.Input.Cursors.Hand;
            editButton.Tag = categoryID;
            editButton.Click += EditCategory_Click;

            Button deleteButton = new Button();
            deleteButton.Content = "🗑";
            deleteButton.Width = 30;
            deleteButton.Height = 30;
            deleteButton.Background = Brushes.Transparent;
            deleteButton.BorderThickness = new Thickness(0);
            deleteButton.Foreground = Brushes.Red;
            deleteButton.FontSize = 14;
            deleteButton.Cursor = System.Windows.Input.Cursors.Hand;
            deleteButton.Tag = categoryID;
            deleteButton.Click += DeleteCategory_Click;

            actionPanel.Children.Add(editButton);
            actionPanel.Children.Add(deleteButton);

            Grid.SetColumn(iconBox, 0);
            Grid.SetColumn(textPanel, 1);
            Grid.SetColumn(actionPanel, 2);

            grid.Children.Add(iconBox);
            grid.Children.Add(textPanel);
            grid.Children.Add(actionPanel);

            row.Child = grid;

            return row;
        }

        private void SaveCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = txtCategoryName.Text.Trim();

            if (categoryName == "")
            {
                MessageBox.Show("Please enter category name.");
                return;
            }

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                bool alreadyExists = db.tbl_Categories.Any(c =>
                    c.CategoryName.ToLower() == categoryName.ToLower() &&
                    c.CategoryID != editingCategoryID
                );

                if (alreadyExists)
                {
                    MessageBox.Show("Category already exists.");
                    return;
                }

                if (editingCategoryID == 0)
                {
                    tbl_Category newCategory = new tbl_Category();
                    newCategory.CategoryName = categoryName;

                    db.tbl_Categories.InsertOnSubmit(newCategory);
                    db.SubmitChanges();

                    MessageBox.Show("Category added successfully.");
                }
                else
                {
                    var categoryToUpdate = db.tbl_Categories
                        .FirstOrDefault(c => c.CategoryID == editingCategoryID);

                    if (categoryToUpdate == null)
                    {
                        MessageBox.Show("Category not found.");
                        return;
                    }

                    categoryToUpdate.CategoryName = categoryName;
                    db.SubmitChanges();

                    MessageBox.Show("Category updated successfully.");
                }
            }

            ClearForm();
            RefreshCategories();
        }

        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            int categoryID = Convert.ToInt32(button.Tag);

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var category = db.tbl_Categories
                    .FirstOrDefault(c => c.CategoryID == categoryID);

                if (category == null)
                {
                    MessageBox.Show("Category not found.");
                    return;
                }

                editingCategoryID = category.CategoryID;
                txtCategoryName.Text = category.CategoryName;

                txtFormTitle.Text = "Edit Category";
                btnSaveCategory.Content = "Update";
                btnCancelEdit.Visibility = Visibility.Visible;
            }
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            if (button == null)
            {
                return;
            }

            int categoryID = Convert.ToInt32(button.Tag);

            using (NexPOSDataDataContext db = new NexPOSDataDataContext())
            {
                var category = db.tbl_Categories
                    .FirstOrDefault(c => c.CategoryID == categoryID);

                if (category == null)
                {
                    MessageBox.Show("Category not found.");
                    return;
                }

                int productCount = db.tbl_Products
                    .Count(p => p.CategoryID == categoryID);

                if (productCount > 0)
                {
                    MessageBox.Show(
                        "This category cannot be deleted because it still has " + productCount + " product(s).",
                        "Delete Category",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning
                    );

                    return;
                }

                MessageBoxResult result = MessageBox.Show(
                    "Are you sure you want to delete " + category.CategoryName + "?",
                    "Delete Category",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    db.tbl_Categories.DeleteOnSubmit(category);
                    db.SubmitChanges();

                    MessageBox.Show("Category deleted successfully.");
                }
            }

            ClearForm();
            RefreshCategories();
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            editingCategoryID = 0;

            txtCategoryName.Text = "";
            txtFormTitle.Text = "Add New Category";
            btnSaveCategory.Content = "+  Add";
            btnCancelEdit.Visibility = Visibility.Collapsed;
        }
    }
}