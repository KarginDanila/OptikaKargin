using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    public partial class FormProductManager : Form
    {
        public FormProductManager()
        {
            InitializeComponent();
        }

        // Строка подключения к базе данных
        string con = Connection.myConnection;

        // Список для хранения продуктов
        private List<ProductsManager> products = new List<ProductsManager>();

        // Текст-заполнитель для поля поиска
        private const string PlaceholderText = "Поиск";

        // Кэш всех продуктов для быстрого поиска
        private List<ProductsManager> allProducts = new List<ProductsManager>();

        // Обработчик загрузки формы
        private void FormProductManager_Load(object sender, EventArgs e)
        {
            LoadProductsFromDatabase();  // Загрузка данных из БД
            InitializeDataGridView();    // Настройка DataGridView
            InitializeSearchBox();       // Настройка поля поиска
        }

        // Инициализация DataGridView (таблицы для отображения продуктов)
        private void InitializeDataGridView()
        {
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;       // Запрет добавления строк
            dataGridView1.AllowUserToDeleteRows = false;    // Запрет удаления строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false;      // Ручное определение колонок

            dataGridView1.Columns.Clear();

            // Колонка для изображения товара
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
            {
                HeaderText = "Изображение",
                ImageLayout = DataGridViewImageCellLayout.Stretch, // Растягивание изображения
                DataPropertyName = "Image" // Связь с свойством Image класса ProductsManager
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

        // Инициализация поля поиска
        private void InitializeSearchBox()
        {
            textBoxPoick.Text = PlaceholderText; // Установка текста-заполнителя
            textBoxPoick.ForeColor = Color.Black;

            // Подписка на события
            textBoxPoick.TextChanged += textBoxPoick_TextChanged; // Изменение текста
            textBoxPoick.Enter += TextBoxPoick_Enter;            // Получение фокуса
            textBoxPoick.Leave += TextBoxPoick_Leave;            // Потеря фокуса
            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;      // Нажатие клавиши
        }

        // Загрузка продуктов из базы данных
        private void LoadProductsFromDatabase()
        {
            products.Clear();     // Очистка текущего списка
            allProducts.Clear();  // Очистка кэша

            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();

                // SQL-запрос для получения данных о продуктах
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
                    // Загрузка изображения товара
                    string imagePath = reader["ProductPhoto"].ToString();
                    Image image = LoadProductImage(imagePath);

                    // Проверка на NULL значения обязательных полей
                    if (reader["Article"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Description"] != DBNull.Value &&
                        reader["Price"] != DBNull.Value &&
                        reader["Stock"] != DBNull.Value &&
                        reader["Discount"] != DBNull.Value)
                    {
                        // Чтение данных из результата запроса
                        string article = reader["Article"].ToString();
                        string name = reader["Name"].ToString();
                        string description = reader["Description"].ToString();
                        decimal price = Convert.ToDecimal(reader["Price"]);
                        int stock = Convert.ToInt32(reader["Stock"]);
                        int discount = Convert.ToInt32(reader["Discount"]);

                        // Расчет цены со скидкой
                        decimal discountedPrice = price * (100 - discount) / 100;

                        // Чтение категории и поставщика (может быть NULL)
                        string category = reader["CategoryName"]?.ToString() ?? "Не указана";
                        string supplier = reader["SupplierName"]?.ToString() ?? "Не указан";

                        // Создание объекта продукта
                        var product = new ProductsManager
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
                        };

                        // Добавление в списки
                        products.Add(product);
                        allProducts.Add(product); // Сохраняем в кэш для поиска
                    }
                }
                reader.Close();
            }

            // Привязка данных к DataGridView
            dataGridView1.DataSource = products;

            // Применение цветового выделения строк
            ApplyRowColoring();
        }

        // Загрузка изображения товара
        private Image LoadProductImage(string imagePath)
        {
            if (!string.IsNullOrEmpty(imagePath))
            {
                // Формирование полного пути к изображению
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imagePath);

                if (File.Exists(fullPath))
                {
                    try
                    {
                        return Image.FromFile(fullPath);
                    }
                    catch
                    {
                        // Если не удалось загрузить изображение, вернем заглушку
                    }
                }
            }
            // Возврат изображения-заглушки, если оригинальное не найдено
            return Image.FromFile("zaglushka.jpg");
        }

        // Применение цветового выделения строк
        private void ApplyRowColoring()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.DataBoundItem is ProductsManager product)
                {
                    // Выделение строк с большой скидкой (>15%)
                    if (product.Discount > 15)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 204, 153);
                    }
                }
            }
        }

        // Обработчик изменения текста в поле поиска
        private void textBoxPoick_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxPoick.Text.Trim();

            // Если текст равен заполнителю - показываем все товары
            if (searchText == PlaceholderText)
            {
                dataGridView1.DataSource = allProducts;
                return;
            }

            // Поиск при вводе от 3 символов
            if (searchText.Length >= 3)
            {
                // Фильтрация продуктов по названию (без учета регистра)
                var filteredProducts = allProducts
                    .Where(p => p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                dataGridView1.DataSource = filteredProducts;
            }
            else
            {
                // Показываем все товары, если введено меньше 3 символов
                dataGridView1.DataSource = allProducts;
            }
        }

        // Обработчик получения фокуса полем поиска
        private void TextBoxPoick_Enter(object sender, EventArgs e)
        {
            // Очистка поля, если там текст-заполнитель
            if (textBoxPoick.Text == PlaceholderText)
            {
                textBoxPoick.Text = string.Empty;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        // Обработчик потери фокуса полем поиска
        private void TextBoxPoick_Leave(object sender, EventArgs e)
        {
            // Восстановление текста-заполнителя, если поле пустое
            if (string.IsNullOrWhiteSpace(textBoxPoick.Text))
            {
                textBoxPoick.Text = PlaceholderText;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        // Обработчик нажатия клавиш в поле поиска
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Можно добавить обработку специальных клавиш
        }

        // Обработчик кнопки "Назад"
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрытие формы
        }
    }

    // Класс для представления информации о продукте
    public class ProductsManager
    {
        public string Article { get; set; }          // Артикул товара
        public string Name { get; set; }            // Название товара
        public string Description { get; set; }     // Описание товара
        public decimal Price { get; set; }          // Цена товара
        public int Discount { get; set; }          // Скидка в процентах
        public decimal DiscountedPrice { get; set; } // Цена со скидкой
        public int Stock { get; set; }              // Количество на складе
        public Image Image { get; set; }            // Изображение товара
        public string CategoryName { get; set; }    // Название категории
        public string SupplierName { get; set; }    // Название поставщика
        public string Manufacterer { get; set; }    // Производитель (не используется в текущей реализации)
    }
}