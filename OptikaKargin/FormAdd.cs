using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    public partial class FormAdd : Form
    {
        public FormAdd()
        {
            InitializeComponent();

            // Настройка обработчиков событий для валидации ввода
            ConfigureInputValidation();
        }


        private void FormAdd_Load(object sender, EventArgs e)
        {
            // Настройка выпадающих списков
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;

            // Очистка и инициализация формы
            ClearForm();
            LoadCategories();
            LoadSuppliers();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            // Выход с подтверждением
            if (ConfirmExit())
            {
                this.Close();
            }
        }

        private void buttonSave_Click_1(object sender, EventArgs e)
        {
            // Сохранение товара
            SaveProduct();
        }

        private void pictureBoxImage_Click_1(object sender, EventArgs e)
        {
            // Выбор изображения для товара
            SelectProductImage();
        }

        private void textBoxArticle_TextChanged_1(object sender, EventArgs e)
        {
            // Автоматическое преобразование артикула в верхний регистр
            textBoxArticle.Text = textBoxArticle.Text.ToUpper();
            textBoxArticle.SelectionStart = textBoxArticle.Text.Length;
        }

        /// <summary>
        /// Настройка валидации полей ввода
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
                char[] forbiddenChars = { '<', '>', ';', '=', '{', '}', '[', ']', '|', '\\', '\'', '"','!', '@', '"', '#', '№', ';', '%', '$', '&', '?', ':', '^', '*' };
                if (forbiddenChars.Contains(e.KeyChar))
                    e.Handled = true;
            };

            // Скидка (только цифры, 0-100)
            textBox1.KeyPress += (s, e) =>
            {
                if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                    e.Handled = true;
            };
            textBox1.TextChanged += (s, e) =>
            {
                if (!string.IsNullOrEmpty(textBox1.Text) && int.TryParse(textBox1.Text, out int discount))
                {
                    if (discount > 100)
                    {
                        textBox1.Text = "100";
                        textBox1.SelectionStart = textBox1.Text.Length;
                    }
                    else if (discount < 0)
                    {
                        textBox1.Text = "0";
                        textBox1.SelectionStart = textBox1.Text.Length;
                    }
                }
            };

            // Выпадающие списки - только выбор из списка
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBox3.DropDownStyle = ComboBoxStyle.DropDownList;
        }

        /// <summary>
        /// Проверка изменений в форме
        /// </summary>
        private bool IsFormChanged()
        {
            return !string.IsNullOrEmpty(textBoxArticle.Text) ||
                   !string.IsNullOrEmpty(textboxNameProduct.Text) ||
                   !string.IsNullOrEmpty(textboxPrice.Text) ||
                   !string.IsNullOrEmpty(textBoxProductQuantityInStock.Text) ||
                   !string.IsNullOrEmpty(textBoxProductDescription.Text) ||
                   textBox1.Text != "0" ||
                   comboBox1.SelectedIndex != -1 ||
                   comboBox3.SelectedIndex != -1 ||
                   pictureBoxImage.Image != Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "zaglushka.jpg"));
        }

        /// <summary>
        /// Подтверждение выхода из формы
        /// </summary>
        private bool ConfirmExit()
        {
            if (!IsFormChanged())
                return true;

            return MessageBox.Show("Вы действительно хотите выйти? Все несохраненные изменения будут потеряны.",
                                "Подтверждение выхода",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Валидация введенных данных
        /// </summary>
        private bool ValidateInputs()
        {
            // Проверка артикула (ровно 6 символов, буквы и цифры)
            if (string.IsNullOrWhiteSpace(textBoxArticle.Text) ||
                !Regex.IsMatch(textBoxArticle.Text, @"^[a-zA-Z0-9]+$"))
            {
                MessageBox.Show("Артикул должен содержать только буквы и цифры (не более 6 символов)!",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxArticle.Focus();
                return false;
            }


            // Проверка названия (не пустое, не более 45 символов)
            if (string.IsNullOrWhiteSpace(textboxNameProduct.Text) || textboxNameProduct.Text.Length > 45)
            {
                MessageBox.Show("Введите название продукта (не более 45 символов)!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textboxNameProduct.Focus();
                return false;
            }

            // Проверка цены (целое число, больше 0)
            if (!int.TryParse(textboxPrice.Text, out int price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену (целое число больше 0)!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textboxPrice.Focus();
                return false;
            }

            // Проверка количества (целое число, 0 или больше)
            if (!int.TryParse(textBoxProductQuantityInStock.Text, out int stock) || stock < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число, 0 или больше)!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxProductQuantityInStock.Focus();
                return false;
            }

            // Проверка описания (не более 45 символов)
            if (textBoxProductDescription.Text.Length > 100)
            {
                MessageBox.Show("Описание не должно превышать 45 символов!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBoxProductDescription.Focus();
                return false;
            }

            // Проверка скидки (0-100)
            if (!byte.TryParse(textBox1.Text, out byte discount) || discount > 100)
            {
                MessageBox.Show("Скидка должна быть числом от 0 до 100%!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                textBox1.Focus();
                return false;
            }

            // Проверка выбора категории
            if (comboBox1.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите категорию из списка!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox1.Focus();
                return false;
            }

            // Проверка выбора поставщика
            if (comboBox3.SelectedIndex <= 0)
            {
                MessageBox.Show("Выберите поставщика из списка!",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                comboBox3.Focus();
                return false;
            }

            return true;
        }


        /// <summary>
        /// Очистка формы
        /// </summary>
        private void ClearForm()
        {
            textBoxArticle.Text = "";
            textboxNameProduct.Text = "";
            textboxPrice.Text = "";
            textBoxProductQuantityInStock.Text = "";
            textBoxProductDescription.Text = "";
            textBox1.Text = "0";
            comboBox1.SelectedIndex = -1;
            comboBox3.SelectedIndex = -1;
            pictureBoxImage.Image = Image.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "zaglushka.jpg"));
        }

        /// <summary>
        /// Загрузка категорий из БД
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection.myConnection))
                {
                    connection.Open();
                    string query = "SELECT CategoryName FROM category ORDER BY CategoryName";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        comboBox1.Items.Add(""); // Пустой элемент

                        while (reader.Read())
                        {
                            comboBox1.Items.Add(reader["CategoryName"].ToString());
                        }

                        comboBox1.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загрузка поставщиков из БД
        /// </summary>
        private void LoadSuppliers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(Connection.myConnection))
                {
                    connection.Open();
                    string query = "SELECT SupplierName FROM supplier ORDER BY SupplierName";
                    MySqlCommand command = new MySqlCommand(query, connection);

                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox3.Items.Clear();
                        comboBox3.Items.Add(""); // Пустой элемент

                        while (reader.Read())
                        {
                            comboBox3.Items.Add(reader["SupplierName"].ToString());
                        }

                        comboBox3.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки поставщиков: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Сохранение товара в БД
        /// </summary>
        private void SaveProduct()
        {
            try
            {
                // Валидация данных
                if (!ValidateInputs())
                    return;

                // Создание объекта продукта
                var newProduct = new Products
                {
                    Article = textBoxArticle.Text.Trim().ToUpper(),
                    Name = textboxNameProduct.Text.Trim(),
                    Price = decimal.Parse(textboxPrice.Text),
                    SupplierName = comboBox3.SelectedItem?.ToString(),
                    CategoryName = comboBox1.SelectedItem?.ToString(),
                    Stock = int.Parse(textBoxProductQuantityInStock.Text),
                    Description = textBoxProductDescription.Text.Trim(),
                    Discount = int.Parse(textBox1.Text)
                };

                // Подтверждение добавления продукта
                DialogResult result = MessageBox.Show(
                    "Вы уверены, что хотите добавить продукт?",
                    "Подтверждение добавления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                // Если пользователь подтвердил добавление
                if (result == DialogResult.Yes)
                {
                    byte[] image = GetImageBytes(pictureBoxImage.Image); // Получаем изображение
                    AddProductToDatabase(newProduct); // Добавляем продукт в базу данных
                    this.Close(); // Закрываем форму
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Конвертация изображения в массив байтов
        private byte[] GetImageBytes(Image image)
        {
            if (image == null) return null;

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    // Создаем копию для избежания GDI+ ошибок
                    using (Bitmap temp = new Bitmap(image))
                    {
                        temp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                    }
                    return ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка конвертации изображения: " + ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Добавление товара в базу данных
        /// </summary>
        private bool AddProductToDatabase(Products product)
        {
            try
            {
                string imageFileName = SaveImageToFile(pictureBoxImage.Image, product.Article);

                using (MySqlConnection connection = new MySqlConnection(Connection.myConnection))
                {
                    connection.Open();

                    // Проверка на дубликат артикула
                    string checkQuery = "SELECT COUNT(*) FROM product WHERE ProductArticle = @ProductArticle";
                    using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@ProductArticle", product.Article);
                        if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
                        {
                            MessageBox.Show("Продукт с таким артикулом уже существует!",
                                          "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    // Добавление продукта
                    string insertQuery = @"
                    INSERT INTO product (
                        ProductArticle,
                        ProductName,
                        ProductDescription,
                        ProductPrice,
                        ProductStock,
                        ProductCategoryId,
                        ProductSupplierId,
                        ProductPhoto,
                        ProductDiscount
                    ) VALUES (
                        @ProductArticle, 
                        @ProductName, 
                        @ProductDescription, 
                        @ProductPrice, 
                        @ProductStock, 
                        (SELECT CategoryID FROM category WHERE CategoryName = @CategoryName), 
                        (SELECT SupplierID FROM supplier WHERE SupplierName = @SupplierName), 
                        @ProductPhoto,
                        @ProductDiscount
                    )";

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ProductArticle", product.Article);
                        command.Parameters.AddWithValue("@ProductName", product.Name);
                        command.Parameters.AddWithValue("@ProductDescription", product.Description);
                        command.Parameters.AddWithValue("@ProductPrice", product.Price);
                        command.Parameters.AddWithValue("@ProductStock", product.Stock);
                        command.Parameters.AddWithValue("@CategoryName", product.CategoryName);
                        command.Parameters.AddWithValue("@SupplierName", product.SupplierName);
                        command.Parameters.AddWithValue("@ProductPhoto", imageFileName);
                        command.Parameters.AddWithValue("@ProductDiscount", product.Discount);

                        return command.ExecuteNonQuery() > 0;
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Number == 1062
                    ? "Продукт с таким артикулом уже существует!"
                    : $"Ошибка MySQL: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// Сохранение изображения товара
        /// </summary>
        private string SaveImageToFile(Image image, string productId)
        {
            if (image == null) return null;

            // Очищаем недопустимые символы в имени файла
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                productId = productId.Replace(c, '_');
            }

            string imageFileName = $"{productId}.jpg";
            string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", imageFileName);

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(imagePath));

                // Удаляем существующий файл, если есть
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }

                // Сохраняем через копию изображения
                using (Bitmap tempImage = new Bitmap(image))
                {
                    tempImage.Save(imagePath, System.Drawing.Imaging.ImageFormat.Jpeg);
                }

                return imageFileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения изображения: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Выбор изображения для товара
        /// </summary>
        private void SelectProductImage()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Изображения (*.jpg; *.jpeg; *.png)|*.jpg; *.jpeg; *.png";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Используем using для освобождения ресурсов
                using (FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                {
                    pictureBoxImage.Image = Image.FromStream(stream);
                }
            }
        }

    }
}