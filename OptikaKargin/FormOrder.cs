using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для оформления заказов
    /// </summary>
    public partial class FormOrder : Form
    {
        public FormOrder()
        {
            InitializeComponent();
        }

        string con = Connection.myConnection; // Строка подключения к БД
        private List<Product> products = new List<Product>(); // Список товаров
        private const string PlaceholderText = "Поиск"; // Текст-заглушка для поиска
        private Cart cart = new Cart(); // Корзина заказов

        /// <summary>
        /// Загрузка формы и инициализация компонентов
        /// </summary>
        private void FormOrder_Load(object sender, EventArgs e)
        {
            LoadOrderFromDatabase(); // Загрузка товаров из БД

            // Настройка DataGridView
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк
            dataGridView1.AllowUserToDeleteRows = false; // Запрет удаления строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false; // Отключение авто-генерации колонок

            // Настройка поля поиска
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;

            // Подписка на события
            textBoxPoick.TextChanged += textBoxPoick_TextChanged_1;
            textBoxPoick.Enter += TextBoxPoick_Enter;
            textBoxPoick.Leave += TextBoxPoick_Leave;

            // Добавление колонки с изображениями
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
            {
                HeaderText = "Изображение",
                ImageLayout = DataGridViewImageCellLayout.Stretch,
                DataPropertyName = "Image"
            };
            dataGridView1.Columns.Add(imageColumn);

            // Добавление текстовых колонок
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Артикул", DataPropertyName = "Article", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Название", DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Описание", DataPropertyName = "Description", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Цена", DataPropertyName = "Price", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Количество", DataPropertyName = "Stock", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Категория", DataPropertyName = "CategoryName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Поставщик", DataPropertyName = "SupplierName", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });

            dataGridView1.DataSource = products; // Привязка данных

            // Добавление кнопки в корзину
            DataGridViewButtonColumn addToCartButton = new DataGridViewButtonColumn
            {
                HeaderText = "Добавить в корзину",
                Text = "Добавить",
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(addToCartButton);

            // Создание метки для отображения общей суммы
            Label labelTotalPrice = new Label
            {
                AutoSize = true,
                Location = new Point(10, dataGridView1.Bottom + 10)
            };
            this.Controls.Add(labelTotalPrice);

            UpdateTotalPriceDisplay(); // Обновление суммы

            // Центрирование текста в колонках
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;
        }

        /// <summary>
        /// Загрузка товаров из базы данных
        /// </summary>
        private void LoadOrderFromDatabase()
        {
            products.Clear();
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();

                // SQL-запрос для получения товаров
                string query = @"
                    SELECT 
                        p.ProductArticle AS Id,
                        p.ProductName AS Name,
                        p.ProductPrice AS Cost,
                        p.ProductDiscount AS Discount,
                        s.SupplierName AS SupplierName,  
                        c.CategoryName AS CategoryName, 
                        p.ProductStock AS QuantityInStock,
                        p.ProductDescription AS Description,
                        p.ProductPhoto AS ProductPhoto
                    FROM product p
                    LEFT JOIN supplier s ON p.ProductSupplierID = s.SupplierID
                    LEFT JOIN category c ON p.ProductCategoryID = c.CategoryID";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    string imageFileName = reader["ProductPhoto"].ToString();
                    Image image = null;

                    // Загрузка изображения товара
                    if (!string.IsNullOrEmpty(imageFileName))
                    {
                        string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imageFileName);

                        if (File.Exists(imagePath))
                        {
                            image = Image.FromFile(imagePath);
                        }
                        else
                        {
                            image = Image.FromFile("zaglushka.jpg"); // Заглушка, если изображение не найдено
                        }
                    }
                    else
                    {
                        image = Image.FromFile("zaglushka.jpg"); // Заглушка, если нет фото
                    }

                    // Проверка на NULL значения
                    if (reader["Id"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Cost"] != DBNull.Value &&
                        reader["Discount"] != DBNull.Value &&
                        reader["SupplierName"] != DBNull.Value &&
                        reader["CategoryName"] != DBNull.Value &&
                        reader["QuantityInStock"] != DBNull.Value &&
                        reader["Description"] != DBNull.Value)
                    {
                        // Создание объекта товара
                        products.Add(new Product
                        {
                            Article = reader["Id"].ToString(),
                            Name = reader["Name"].ToString(),
                            Price = Convert.ToDecimal(reader["Cost"]),
                            Discount = Convert.ToInt32(reader["Discount"]),
                            SupplierName = reader["SupplierName"].ToString(),
                            CategoryName = reader["CategoryName"].ToString(),
                            Stock = Convert.ToInt32(reader["QuantityInStock"]),
                            Description = reader["Description"].ToString(),
                            Image = image
                        });
                    }
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Обработчик клика по кнопке добавления в корзину
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex >= 0 && e.RowIndex >= 0)
            {
                var column = dataGridView1.Columns[e.ColumnIndex];
                if (column is DataGridViewButtonColumn && column.HeaderText == "Добавить в корзину")
                {
                    Product selectedProduct = products[e.RowIndex];

                    // Проверяем, есть ли уже этот товар в корзине
                    var cartItem = cart.GetItems().FirstOrDefault(p => p.Article == selectedProduct.Article);
                    int alreadyInCart = cartItem?.Quantity ?? 0;

                    // Если пытаемся добавить больше, чем есть на складе
                    if (alreadyInCart >= selectedProduct.Stock)
                    {
                        MessageBox.Show($"Нельзя добавить больше {selectedProduct.Stock} единиц товара!\n" +
                                      $"Уже в корзине: {alreadyInCart}",
                                      "Ошибка",
                                      MessageBoxButtons.OK,
                                      MessageBoxIcon.Warning);
                        return;
                    }

                    cart.AddProduct(selectedProduct); // Добавление товара в корзину
                    UpdateTotalPriceDisplay(); // Обновление суммы
                }
            }
        }
            /// <summary>
            /// Обновление DataGridView с новыми данными
            /// </summary>
            private void UpdateDataGridView(IEnumerable<Product> data)
        {
            dataGridView1.DataSource = data.ToList();
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        private void textBoxPoick_TextChanged_1(object sender, EventArgs e)
        {
            string searchText = textBoxPoick.Text;

            // Поиск при вводе 3+ символов
            if (searchText.Length >= 3)
            {
                var query = products.Where(p => p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                UpdateDataGridView(query);
            }
            else
            {
                UpdateDataGridView(products);
            }
            dataGridView1.DataSource = products;
        }

        /// <summary>
        /// Обработчик получения фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Enter(object sender, EventArgs e)
        {
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
            if (string.IsNullOrWhiteSpace(textBoxPoick.Text))
            {
                textBoxPoick.Text = PlaceholderText;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Обработчик ввода символов в поле поиска
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы и пробел
            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обновление отображения общей суммы
        /// </summary>
        private void UpdateTotalPriceDisplay()
        {
            int totalItems = cart.GetTotalItemsCount();
            labelTotalPrice.Text = $"Общее количество товаров: {totalItems}";
        }

        /// <summary>
        /// Обработчик кнопки отмены
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки оформления заказа
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FormOrderbuy form = new FormOrderbuy(cart.GetItems(), this);
            form.Show();
            this.Hide();
        }
    }

    /// <summary>
    /// Класс корзины для хранения выбранных товаров
    /// </summary>
    public class Cart
    {
        private Dictionary<Product, int> items = new Dictionary<Product, int>();

        /// <summary>
        /// Добавление товара в корзину
        /// </summary>
        public void AddProduct(Product product)
        {
            if (items.ContainsKey(product))
            {
                items[product]++;
                product.Quantity++;
            }
            else
            {
                items[product] = 1;
                product.Quantity = 1;
            }
        }

        /// <summary>
        /// Получение списка товаров в корзине
        /// </summary>
        public List<Product> GetItems()
        {
            return items.Select(i => new Product
            {
                Article = i.Key.Article,
                Name = i.Key.Name,
                Description = i.Key.Description,
                Price = i.Key.Price,
                Discount = i.Key.Discount,
                Stock = i.Key.Stock,
                CategoryName = i.Key.CategoryName,
                SupplierName = i.Key.SupplierName,
                Image = i.Key.Image,
                Quantity = i.Value
            }).ToList();
        }

        /// <summary>
        /// Получение общего количества товаров
        /// </summary>
        public int GetTotalItemsCount()
        {
            return items.Sum(item => item.Value);
        }
    }

    /// <summary>
    /// Класс товара
    /// </summary>
    public class Product
    {
        public string Article { get; set; } // Артикул
        public string Name { get; set; } // Название
        public string Description { get; set; } // Описание
        public decimal Price { get; set; } // Цена
        public int Stock { get; set; } // Количество на складе
        public string CategoryName { get; set; } // Категория
        public string SupplierName { get; set; } // Поставщик
        public Image Image { get; set; } // Изображение
        public int Quantity { get; set; } // Количество в заказе
        public decimal Discount { get; set; } // Скидка

        // Цена со скидкой
        public decimal DiscountedCost
        {
            get => Discount > 0 ? Price * (1 - Discount / 100m) : Price;
        }
    }
}