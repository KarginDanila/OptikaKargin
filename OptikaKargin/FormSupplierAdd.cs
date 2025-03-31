using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для добавления нового поставщика в систему
    /// </summary>
    public partial class FormSupplierAdd : Form
    {
        public event Action SupplierAdded;
        public FormSupplierAdd()
        {
            InitializeComponent();
            // Подписка на события валидации ввода
            textBox1.KeyPress += textBox1_KeyPress;    // Название поставщика (SupplierName)
            textBox2.KeyPress += textBox2_KeyPress;    // Адрес поставщика (SupplierAddress)
            maskedTextBox1.KeyPress += textBox3_KeyPress;    // Телефон поставщика (SupplierPhone)
        }

        /// <summary>
        /// Обработчик ввода для поля Имя (только русские буквы и пробелы)
        /// </summary>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Проверяем, является ли символ русской буквой, пробелом или управляющим символом
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';
            bool isSpace = e.KeyChar == ' ';
            bool isControl = char.IsControl(e.KeyChar);

            if (!isRussianLetter && !isSpace && !isControl)
            {
                e.Handled = true;  // Блокируем ввод
            }
        }

        /// <summary>
        /// Обработчик изменения текста для поля Имя (делает первую букву заглавной)
        /// </summary>
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox1.Text) && textBox1.SelectionStart == 1)
            {
                textBox1.Text = char.ToUpper(textBox1.Text[0]) + textBox1.Text.Substring(1);
                textBox1.SelectionStart = textBox1.Text.Length;
            }
        }

        /// <summary>
        /// Валидация ввода для Адреса (SupplierAddress varchar(50))
        /// Разрешает: русские буквы, цифры, пробелы, запятые, точки, дефисы
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Проверяем, является ли символ допустимым
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';
            bool isDigit = char.IsDigit(e.KeyChar);
            bool isSpace = e.KeyChar == ' ';
            bool isAllowedSymbol = e.KeyChar == ',' || e.KeyChar == '.' ||
                                 e.KeyChar == '-' || e.KeyChar == '/';
            bool isControl = char.IsControl(e.KeyChar);

            if (!isRussianLetter && !isDigit && !isSpace &&
                !isAllowedSymbol && !isControl)
            {
                e.Handled = true;  // Блокируем ввод
            }

            // Ограничение длины 50 символов
            if (textBox2.Text.Length >= 50 && !isControl)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Валидация ввода для телефона поставщика (SupplierPhone)
        /// Ограничения: varchar(12), формат +7XXXXXXXXXX
        /// </summary>
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Защищаем начало номера "+7" от удаления
            if (maskedTextBox1.Text == "+7" && (e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Delete))
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только цифры и символ '+' в начале
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) &&
                !(e.KeyChar == '+' && maskedTextBox1.Text.Length == 0))
            {
                e.Handled = true;
            }

            // Ограничение длины (18 символов: +7XXXXXXXXXX)
            if (maskedTextBox1.Text.Length >= 18 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик загрузки формы - инициализация полей
        /// </summary>
        private void FormSupplierAdd_Load(object sender, EventArgs e)
        {
            // Установка начального формата телефона
            maskedTextBox1.Text = "+7";

            // Установка максимальных длин согласно структуре таблицы
            textBox1.MaxLength = 45;    // SupplierName varchar(45)
            textBox2.MaxLength = 45;    // SupplierAddress varchar(45)
            maskedTextBox1.MaxLength = 18;    // SupplierPhone varchar(12)
            maskedTextBox1.Mask = "+7 (000) 000-00-00";
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - сохранение поставщика
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Создание объекта поставщика с данными из формы
                Suppliers newsupplier = new Suppliers
                {
                    Name = textBox1.Text.Trim(),     // Название поставщика
                    Address = textBox2.Text.Trim(),  // Адрес поставщика
                    Phone = maskedTextBox1.Text.Trim()     // Телефон поставщика
                };

                // Проверка заполнения обязательных полей
                if (string.IsNullOrEmpty(newsupplier.Name) ||
                    string.IsNullOrEmpty(newsupplier.Address) ||
                    string.IsNullOrEmpty(newsupplier.Phone))
                {
                    MessageBox.Show("Ошибка заполнения данных!", "Заполните все поля!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины названия поставщика
                if (newsupplier.Name.Length > 45)
                {
                    MessageBox.Show("Название поставщика не должно превышать 45 символов!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины адреса поставщика
                if (newsupplier.Address.Length > 45)
                {
                    MessageBox.Show("Адрес поставщика не должен превышать 45 символов!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка формата и длины телефона
                if (!newsupplier.Phone.StartsWith("+7") || newsupplier.Phone.Length != 18)
                {
                    MessageBox.Show("Телефон должен быть в формате +7XXXXXXXXXX (12 символов)!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Добавление поставщика в базу данных
                AddSupplierToDatabase(newsupplier);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка!", "Ошибка при добавлении поставщика: " + ex.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close();
            }
        }

        /// <summary>
        /// Добавление поставщика в базу данных
        /// </summary>
        private void AddSupplierToDatabase(Suppliers supplier)
        {

            try
            {
                string connect = Connection.myConnection;
                // SQL-запрос для добавления поставщика
                string insertQuery = @"
        INSERT INTO supplier (
            SupplierName,    -- Название поставщика
            SupplierAddress, -- Адрес поставщика
            SupplierPhone    -- Телефон поставщика
        ) VALUES (
            @SupplierName,  
            @SupplierAddress, 
            @SupplierPhone
        )";

                using (MySqlConnection connection = new MySqlConnection(connect))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        // Параметризованный запрос для безопасности
                        command.Parameters.AddWithValue("@SupplierName", supplier.Name);
                        command.Parameters.AddWithValue("@SupplierAddress", supplier.Address);
                        command.Parameters.AddWithValue("@SupplierPhone", supplier.Phone);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Поставщик успешно добавлен в базу данных.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);

                            this.Close(); 
                        }
                        else
                        {
                            MessageBox.Show("Не удалось добавить поставщика.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении поставщика: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}