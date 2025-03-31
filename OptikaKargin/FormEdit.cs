using MySql.Data.MySqlClient; 
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing; 
using System.IO; 
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OptikaKargin
{
    public partial class FormEdit : Form
    {
        // Поля класса
        private Products selectedProduct; // Текущий редактируемый продукт
        private List<Products> products = new List<Products>(); // Список продуктов (не используется в текущем коде)
        string conn = Connection.myConnection; // Строка подключения к БД

        // Конструктор формы
        public FormEdit(Products products)
        {
            InitializeComponent();
            selectedProduct = products; // Сохраняем переданный продукт

            // Настройка ограничений для полей ввода
            ConfigureInputValidation();

            FormEdit_Load(); // Загружаем данные формы
        }

        /// <summary>
        /// Настройка ограничений для всех полей ввода
        /// </summary>
        private void ConfigureInputValidation()
        {
            // Артикул (макс. 6 символов, только буквы и цифры)
            textBoxArticle.MaxLength = 6;
            textBoxArticle.KeyPress += (s, e) =>
            {
                if (!Regex.IsMatch(e.KeyChar.ToString(), @"^[a-zA-Z0-9]$") && !char.IsControl(e.KeyChar))
                {
                    e.Handled = true;
                }
            };
            textBoxArticle.TextChanged += (s, e) =>
            {
                textBoxArticle.Text = textBoxArticle.Text.ToUpper();
                textBoxArticle.SelectionStart = textBoxArticle.Text.Length;
            };

            // Название товара (макс. 45 символов, запрет специальных символов кроме пробела и дефиса)
            textboxNameProduct.MaxLength = 45;
            textboxNameProduct.KeyPress += (s, e) =>
            {
                if (!char.IsLetterOrDigit(e.KeyChar) &&
                    !char.IsWhiteSpace(e.KeyChar) &&
                    e.KeyChar != '-' &&
                    !char.IsControl(e.KeyChar))
                    e.Handled = true;
            };

            // Цена (только цифры и одна запятая, макс. 2 знака после запятой)
            textboxPrice.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ',')
                    e.Handled = true;

                // Запрет ввода более одной запятой
                if (e.KeyChar == ',' && (textboxPrice.Text.Contains(",") || textboxPrice.Text.Length == 0))
                    e.Handled = true;
            };
            textboxPrice.TextChanged += (s, e) =>
            {
                // Ограничение на 2 знака после запятой
                if (textboxPrice.Text.Contains(","))
                {
                    string[] parts = textboxPrice.Text.Split(',');
                    if (parts.Length > 1 && parts[1].Length > 2)
                    {
                        textboxPrice.Text = parts[0] + "," + parts[1].Substring(0, 2);
                        textboxPrice.SelectionStart = textboxPrice.Text.Length;
                    }
                }
            };

            // Количество на складе (только цифры)
            textBoxProductQuantityInStock.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };

            // Описание товара (макс. 100 символов, запрет опасных символов)
            textBoxProductDescription.MaxLength = 100;
            textBoxProductDescription.KeyPress += (s, e) =>
            {
                // Запрещаем только самые опасные символы, разрешаем буквы, цифры, пробелы и пунктуацию
                char[] forbiddenChars = { '<', '>', ';', '=', '{', '}', '[', ']', '|', '\\', '\'', '"', '!', '@', '"', '#', '№', ';', '%', '$', '&', '?', ':', '^', '*' };
                if (forbiddenChars.Contains(e.KeyChar))
                    e.Handled = true;
            };


            // Выпадающие списки - только выбор из списка
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        // Загрузка данных формы
        private void FormEdit_Load()
        {
            // Настройка отображения изображения (масштабирование с сохранением пропорций)
            pictureBoxImage.SizeMode = PictureBoxSizeMode.Zoom;

            // Заполнение полей данными продукта
            textBoxArticle.Text = selectedProduct.Article;
            textboxNameProduct.Text = selectedProduct.Name;
            textboxPrice.Text = selectedProduct.Price.ToString();
            textBoxProductQuantityInStock.Text = selectedProduct.Stock.ToString();
            textBoxProductDescription.Text = selectedProduct.Description;
            pictureBoxImage.Image = selectedProduct.Image;

            // Загрузка списков категорий и поставщиков
            LoadCategory();
            LoadSupplier();

            // Установка выбранных значений в комбобоксы
            comboBox1.SelectedItem = selectedProduct.CategoryName;
            comboBox3.SelectedItem = selectedProduct.SupplierName;
        }

        // Загрузка категорий из БД
        private void LoadCategory()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();
                    string query = "SELECT CategoryName FROM category ORDER BY CategoryName";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox1.Items.Clear(); // Очищаем список перед загрузкой
                            while (reader.Read())
                            {
                                comboBox1.Items.Add(reader.GetString("CategoryName"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Загрузка поставщиков из БД
        private void LoadSupplier()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM supplier ORDER BY SupplierName";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox3.Items.Clear(); // Очищаем список перед загрузкой
                            while (reader.Read())
                            {
                                comboBox3.Items.Add(reader.GetString("SupplierName"));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Обновление данных продукта в БД
        private void UpdateProductInDatabase(Products products)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();

                    // SQL-запрос для обновления продукта
                    string updateQuery = @"
                    UPDATE product
                    SET 
                        ProductName = @ProductName,
                        ProductDescription = @ProductDescription,
                        ProductPrice = @ProductPrice,
                        ProductStock = @ProductStock,
                        ProductPhoto = @ProductPhoto,
                        ProductCategoryId = (SELECT CategoryID FROM category WHERE CategoryName = @CategoryName),
                        ProductSupplierId = (SELECT SupplierID FROM supplier WHERE SupplierName = @SupplierName)
                    WHERE 
                        ProductArticle = @ProductArticle";

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Добавляем параметры для защиты от SQL-инъекций
                        command.Parameters.AddWithValue("@ProductName", products.Name);
                        command.Parameters.AddWithValue("@ProductDescription", products.Description);
                        command.Parameters.AddWithValue("@ProductPrice", products.Price);
                        command.Parameters.AddWithValue("@ProductStock", products.Stock);
                        command.Parameters.AddWithValue("@ProductArticle", products.Article);
                        command.Parameters.AddWithValue("@CategoryName", products.CategoryName);
                        command.Parameters.AddWithValue("@SupplierName", products.SupplierName);

                        // Обработка изображения
                        if (products.Image != null)
                        {
                            string imageFileName = SaveImageToFile(products.Image, products.Article);
                            command.Parameters.AddWithValue("@ProductPhoto", imageFileName);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@ProductPhoto", DBNull.Value);
                        }

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Продукт успешно обновлен.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить продукт.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении продукта: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Сохранение изображения в файл
        private string SaveImageToFile(Image image, string productArticle)
        {
            if (image == null) return null;

            try
            {
                // Создаем папку Images, если ее нет
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
                Directory.CreateDirectory(directory);

                // Генерируем уникальное имя файла на основе артикула и текущего времени
                string fileName = $"{productArticle}_{DateTime.Now:yyyyMMddHHmmss}.jpg";
                string fullPath = Path.Combine(directory, fileName);

                // Сохраняем изображение в формате JPG
                image.Save(fullPath, System.Drawing.Imaging.ImageFormat.Jpeg);
                return fileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении изображения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        // Обработчик клика по изображению (для загрузки нового изображения)
        private void pictureBoxImage_Click_1(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Загружаем выбранное изображение
                        selectedProduct.Image = Image.FromFile(openFileDialog.FileName);
                        pictureBoxImage.Image = selectedProduct.Image;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        // Обработчик нажатия кнопки "Сохранить"
        private void buttonSave_Click(object sender, EventArgs e)
        {
            // Валидация введенных данных
            if (!ValidateInputs()) return;

            try
            {
                // Обновление данных продукта из полей формы
                selectedProduct.Name = textboxNameProduct.Text.Trim();
                selectedProduct.Article = textBoxArticle.Text.Trim();
                selectedProduct.Price = decimal.Parse(textboxPrice.Text);
                selectedProduct.Stock = int.Parse(textBoxProductQuantityInStock.Text);
                selectedProduct.Description = textBoxProductDescription.Text.Trim();
                selectedProduct.CategoryName = comboBox1.SelectedItem?.ToString();
                selectedProduct.SupplierName = comboBox3.SelectedItem?.ToString();

                // Обновление продукта в БД
                UpdateProductInDatabase(selectedProduct);
                this.Close(); // Закрываем форму после сохранения
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Проверка корректности введенных данных
        /// </summary>
        private bool ValidateInputs()
        {
            // Проверка обязательных полей
            if (string.IsNullOrWhiteSpace(textBoxArticle.Text) ||
                string.IsNullOrWhiteSpace(textboxNameProduct.Text) ||
                string.IsNullOrWhiteSpace(textboxPrice.Text) ||
                string.IsNullOrWhiteSpace(textBoxProductQuantityInStock.Text) ||
                string.IsNullOrWhiteSpace(textBoxProductDescription.Text) ||
                comboBox1.SelectedIndex == -1 ||
                comboBox3.SelectedIndex == -1)
            {
                MessageBox.Show("Заполните все обязательные поля.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка артикула (ровно 6 символов)
            if (textBoxArticle.Text.Length != 6)
            {
                MessageBox.Show("Артикул должен содержать ровно 6 символов.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка цены (должна быть больше 0)
            if (!decimal.TryParse(textboxPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (больше 0).", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            // Проверка количества (должно быть >= 0)
            if (!int.TryParse(textBoxProductQuantityInStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество (0 или больше).", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            return true; // Все проверки пройдены
        }

        // Обработчик нажатия кнопки "Выход"
        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрываем форму без сохранения
        }
    }
}