using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для работы с товарами (просмотр, добавление, редактирование, удаление)
    /// </summary>
    public partial class FormProduct : Form
    {
        public FormProduct()
        {
            InitializeComponent();

            // Подписка на события кнопок
            button1.Click += PrevButton_Click;
            button2.Click += NextButton_Click;

        }

        // Строка подключения к базе данных
        string con = Connection.myConnection;
        private const int MinStockThreshold = 5;
        // Настройки пагинации
        private int currentPage = 1;
        private int pageSize = 5;
        private int totalPages = 1;
        // Список товаров
        private List<Products> products = new List<Products>();
        private List<Products> filteredProducts = new List<Products>();

        // Текст-заполнитель для поля поиска
        private const string PlaceholderText = "Поиск";

        /// <summary>
        /// Загрузка формы - инициализация данных и элементов управления
        /// </summary>
        private void FormProduct_Load(object sender, EventArgs e)
        {
            LoadProductsFromDatabase();  // Загрузка товаров из БД
            InitializeDataGridView();   // Настройка таблицы товаров
            InitializeSearchBox();      // Настройка поля поиска
            InitializeComboBoxes();     // Настройка выпадающих списков
            LoadCategories();           // Загрузка категорий для фильтрации
            UpdatePagination();
            UpdateRecordsCount();
        }

        /// <summary>
        /// Инициализация DataGridView (таблицы товаров)
        /// </summary>
        private void InitializeDataGridView()
        {
            // Настройка основных свойств таблицы
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.RowPrePaint += DataGridView1_RowPrePaint;

            // Очистка существующих колонок
            dataGridView1.Columns.Clear();

            // Добавление колонки с изображением
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
            {
                HeaderText = "Изображение",
                ImageLayout = DataGridViewImageCellLayout.Stretch,
                DataPropertyName = "Image"
            };
            dataGridView1.Columns.Add(imageColumn);

            // Добавление текстовых колонок
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Артикул", DataPropertyName = "Article" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Name" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена", DataPropertyName = "Price" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Скидка (%)", DataPropertyName = "Discount" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена со скидкой", DataPropertyName = "DiscountedPrice" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Количество", DataPropertyName = "Stock" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "CategoryName" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Поставщик", DataPropertyName = "SupplierName" });

            // Центрирование содержимого всех колонок
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }
        private void DataGridView1_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dataGridView1.Rows.Count)
                return;

            var row = dataGridView1.Rows[e.RowIndex];
            var product = row.DataBoundItem as Products;

            if (product != null)
            {
                // Сброс стилей к значениям по умолчанию
                row.DefaultCellStyle.BackColor = dataGridView1.DefaultCellStyle.BackColor;
                row.DefaultCellStyle.ForeColor = dataGridView1.DefaultCellStyle.ForeColor;

                // Подсветка товаров с низким запасом
                if (product.Stock < MinStockThreshold)
                {
                    row.DefaultCellStyle.BackColor = Color.LightCoral;
                }
                // Подсветка товаров с большой скидкой
                else if (product.Discount >= 20)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGreen;
                }
                // Подсветка отсутствующих товаров
                else if (product.Stock == 0)
                {
                    row.DefaultCellStyle.BackColor = Color.LightGray;
                    row.DefaultCellStyle.ForeColor = Color.DarkRed;
                }
            }
        }
        /// <summary>
        /// Инициализация поля поиска
        /// </summary>
        private void InitializeSearchBox()
        {
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;
            textBoxPoick.TextChanged += textBoxPoick_TextChanged_1;
            textBoxPoick.Enter += TextBoxPoick_Enter;
            textBoxPoick.Leave += TextBoxPoick_Leave;
            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;
        }

        /// <summary>
        /// Инициализация выпадающих списков (сортировка и фильтрация)
        /// </summary>
        private void InitializeComboBoxes()
        {
            comboBoxSort.Items.Add("По возрастанию");
            comboBoxSort.Items.Add("По убыванию");
            comboBoxSort.SelectedIndex = -1;
            comboBoxSort.SelectedIndexChanged += comboBoxSort_SelectedIndexChanged;
            comboBoxSort.SelectedIndexChanged += comboBoxFiltr_SelectedIndexChanged;
        }

        /// <summary>
        /// Загрузка товаров из базы данных
        /// </summary>
        private void LoadProductsFromDatabase()
        {
            products.Clear();
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                string query = @"SELECT 
                    p.ProductArticle AS Article,
                    p.ProductName AS Name,
                    p.ProductDescription AS Description,
                    p.ProductPrice AS Price,
                    p.ProductStock AS Stock,
                    p.ProductDiscount AS Discount,
                    c.CategoryName AS CategoryName,
                    s.SupplierName AS SupplierName,
                    p.ProductPhoto AS ProductPhoto
                FROM product p
                LEFT JOIN supplier s ON p.ProductSupplierId = s.SupplierId
                LEFT JOIN category c ON p.ProductCategoryId = c.CategoryId";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string imagePath = reader["ProductPhoto"].ToString();
                    Image image = LoadProductImage(imagePath);

                    if (reader["Article"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Description"] != DBNull.Value &&
                        reader["Price"] != DBNull.Value &&
                        reader["Stock"] != DBNull.Value &&
                        reader["Discount"] != DBNull.Value)
                    {
                        string article = reader["Article"].ToString();
                        string name = reader["Name"].ToString();
                        string description = reader["Description"].ToString();
                        decimal price = Convert.ToDecimal(reader["Price"]);
                        int stock = Convert.ToInt32(reader["Stock"]);
                        int discount = Convert.ToInt32(reader["Discount"]);
                        decimal discountedPrice = price * (100 - discount) / 100;
                        string category = reader["CategoryName"]?.ToString() ?? "Не указана";
                        string supplier = reader["SupplierName"]?.ToString() ?? "Не указан";

                        products.Add(new Products
                        {
                            Article = article,
                            Name = name,
                            Description = description,
                            Price = price,
                            Discount = discount,
                            DiscountedPrice = discountedPrice,
                            Stock = stock,
                            CategoryName = category,
                            SupplierName = supplier,
                            Image = image
                        });
                    }
                }
                reader.Close();
            }

            filteredProducts = new List<Products>(products);
            UpdatePagination();
        }


        /// <summary>
        /// Загрузка изображения товара
        /// </summary>
        /// <param name="imagePath">Путь к изображению</param>
        /// <returns>Объект Image</returns>
        private Image LoadProductImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imagePath);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        return Image.FromFile(fullPath);
                    }
                    catch
                    {
                        // В случае ошибки загрузки изображения
                    }
                }
            }
            // Возвращаем изображение-заглушку, если не удалось загрузить
            return Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "zaglushka.jpg"));
        }
        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - открытие формы добавления товара
        /// </summary>
        private void buttonadd_Click(object sender, EventArgs e)
        {
            FormAdd form = new FormAdd();
            form.ShowDialog();

            // Обновление списка товаров после закрытия формы добавления
            LoadProductsFromDatabase();
        }
        /// <summary>
        /// Обработчик получения фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Enter(object sender, EventArgs e)
        {
            // Очищаем текст-заполнитель при получении фокуса
            if (textBoxPoick.Text == PlaceholderText)
            {
                textBoxPoick.Text = string.Empty;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Обработчик потери фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Leave(object sender, EventArgs e)
        {
            // Восстанавливаем текст-заполнитель если поле пустое
            if (string.IsNullOrWhiteSpace(textBoxPoick.Text))
            {
                textBoxPoick.Text = PlaceholderText;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Обработчик ввода в поле поиска - разрешаем только русские буквы и пробелы
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != ' ')
            {
                e.Handled = true; // Игнорируем недопустимые символы
            }
        }

        /// <summary>
        /// Обработчик изменения сортировки
        /// </summary>
        private void comboBoxSort_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }

        /// <summary>
        /// Обработчик изменения фильтра по категориям
        /// </summary>
        private void comboBoxFiltr_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilterAndSort();
        }
        private void UpdateRecordsCount()
        {
            int totalRecords = products.Count; // Общее количество записей в базе
            int filteredRecords = filteredProducts.Count; // Количество записей после фильтрации
            int displayedRecords = Math.Min(pageSize, filteredRecords - (currentPage - 1) * pageSize); // Количество записей на текущей странице

            // Обновляем Label с информацией о записях
            label2.Text = $"Показано: {displayedRecords} из {filteredRecords} (всего: {totalRecords})";
        }

        // Модифицируем метод UpdatePagination
        private void UpdatePagination()
        {
            // Рассчитываем общее количество страниц
            totalPages = (int)Math.Ceiling((double)filteredProducts.Count / pageSize);

            // Обновляем состояние кнопок навигации
            button1.Enabled = currentPage > 1;
            button2.Enabled = currentPage < totalPages;

            // Обновляем Label с информацией о странице
            label1.Text = $"Страница {currentPage} из {totalPages}";

            // Обновляем отображаемые данные
            DisplayCurrentPage();

            // Обновляем информацию о количестве записей
            UpdateRecordsCount();
        }
        private void DisplayCurrentPage()
        {
            var pagedProducts = filteredProducts
                .Skip((currentPage - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            dataGridView1.DataSource = pagedProducts;

            // Принудительно обновляем форматирование
            dataGridView1.Refresh();
        }
        private void GoToPage(int pageNumber)
        {
            if (pageNumber >= 1 && pageNumber <= totalPages)
            {
                currentPage = pageNumber;
                UpdatePagination();
            }
        }

        private void PrevButton_Click(object sender, EventArgs e)
        {
            GoToPage(currentPage - 1);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            GoToPage(currentPage + 1);
        }
        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        private void textBoxPoick_TextChanged_1(object sender, EventArgs e)
        {
            string searchText = textBoxPoick.Text.Trim();

            // Игнорируем текст-заполнитель
            if (searchText == PlaceholderText)
            {
                filteredProducts = new List<Products>(products);
                dataGridView1.DataSource = filteredProducts;
                UpdatePagination();
                return;
            }

            // Поиск при вводе от 3 символов
            if (searchText.Length >= 3)
            {
                filteredProducts = products
                    .Where(p => p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                dataGridView1.DataSource = filteredProducts;
                UpdatePagination();
            }
            else
            {
                filteredProducts = new List<Products>(products);
                dataGridView1.DataSource = filteredProducts;
                UpdatePagination();
            }
        }
        /// <summary>
        /// Применение фильтров и сортировки к списку товаров
        /// </summary>
        private void ApplyFilterAndSort()
        {
            string searchText = textBoxPoick.Text.ToLower();

            if (searchText == PlaceholderText.ToLower())
            {
                searchText = "";
            }

            string selectedCategory = comboBoxFiltr.SelectedItem?.ToString();

            filteredProducts = products;

            // Фильтрация по поисковому запросу
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                filteredProducts = filteredProducts
                    .Where(p => p.Name.ToLower().Contains(searchText))
                    .ToList();
            }

            // Фильтрация по категории
            if (selectedCategory != null && selectedCategory != "Все категории")
            {
                filteredProducts = filteredProducts
                    .Where(p => p.CategoryName == selectedCategory)
                    .ToList();
            }

            // Сортировка по цене
            if (comboBoxSort.SelectedItem?.ToString() == "По возрастанию")
            {
                filteredProducts = filteredProducts
                    .OrderBy(p => p.Price)
                    .ToList();
            }
            else if (comboBoxSort.SelectedItem?.ToString() == "По убыванию")
            {
                filteredProducts = filteredProducts
                    .OrderByDescending(p => p.Price)
                    .ToList();
            }

            // Сбрасываем на первую страницу и обновляем пагинацию
            currentPage = 1;
            UpdatePagination();
        }


        /// <summary>
        /// Загрузка категорий для фильтрации
        /// </summary>
        private void LoadCategories()
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT CategoryName FROM category", connection);
                MySqlDataReader reader = command.ExecuteReader();

                comboBoxFiltr.Items.Clear();
                comboBoxFiltr.Items.Add("Все категории");

                while (reader.Read())
                {
                    comboBoxFiltr.Items.Add(reader["CategoryName"].ToString());
                }

                reader.Close();
            }

            comboBoxFiltr.SelectedIndex = 0;
        }

        /// <summary>
        /// Получение выбранного товара
        /// </summary>
        /// <returns>Выбранный товар или null</returns>
        private Products GetSelectedProduct()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                var selectedRow = dataGridView1.SelectedRows[0];
                return selectedRow.DataBoundItem as Products;
            }
            return null;
        }

        /// <summary>
        /// Обработчик кнопки "Удалить" - удаление выбранного товара
        /// </summary>
        private void buttondelete_Click(object sender, EventArgs e)
        {
            Products selectedProduct = GetSelectedProduct();

            if (selectedProduct != null)
            {
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить продукт '{selectedProduct.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(con))
                        {
                            connection.Open();

                            // 1. Сначала удаляем все связанные записи из orderitem
                            string deleteOrderItemsQuery = "DELETE FROM orderitem WHERE OrderItemProductArticle = @ProductArticle";
                            MySqlCommand deleteOrderItemsCommand = new MySqlCommand(deleteOrderItemsQuery, connection);
                            deleteOrderItemsCommand.Parameters.AddWithValue("@ProductArticle", selectedProduct.Article);
                            deleteOrderItemsCommand.ExecuteNonQuery();

                            // 2. Затем удаляем сам продукт
                            string deleteProductQuery = "DELETE FROM product WHERE ProductArticle = @ProductArticle";
                            MySqlCommand deleteProductCommand = new MySqlCommand(deleteProductQuery, connection);
                            deleteProductCommand.Parameters.AddWithValue("@ProductArticle", selectedProduct.Article);

                            int rowsAffected = deleteProductCommand.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Продукт успешно удален.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadProductsFromDatabase();
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при удалении продукта.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении продукта: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите продукт для удаления.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать" - открытие формы редактирования товара
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Products selectedProduct = (Products)dataGridView1.SelectedRows[0].DataBoundItem;
                FormEdit form = new FormEdit(selectedProduct);

                form.ShowDialog();
                // Обновление списка товаров после редактирования
                LoadProductsFromDatabase();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите продукт для редактирования.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Класс для хранения информации о товаре
    /// </summary>
    public class Products
    {
        public string Article { get; set; }         // Артикул товара
        public string Name { get; set; }           // Название товара
        public string Description { get; set; }     // Описание товара
        public decimal Price { get; set; }          // Цена товара
        public int Discount { get; set; }           // Скидка в процентах
        public decimal DiscountedPrice { get; set; } // Цена со скидкой
        public int Stock { get; set; }             // Количество на складе
        public Image Image { get; set; }           // Изображение товара
        public string CategoryName { get; set; }    // Название категории
        public string SupplierName { get; set; }    // Название поставщика
        public string Manufacterer { get; set; }    // Производитель
    }
}