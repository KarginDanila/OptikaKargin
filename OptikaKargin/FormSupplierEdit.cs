using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для редактирования данных поставщика
    /// </summary>
    public partial class FormSupplierEdit : Form
    {
        // Выбранный поставщик для редактирования
        private Suppliers selectedProduct;

        // Строка подключения к базе данных
        private string conn = Connection.myConnection;

        /// <summary>
        /// Конструктор формы с передачей данных поставщика
        /// </summary>
        /// <param name="supplier">Объект поставщика для редактирования</param>
        public FormSupplierEdit(Suppliers supplier)
        {
            InitializeComponent();
            selectedProduct = supplier;

            // Подписка на события валидации ввода
            textBox1.KeyPress += textBox1_KeyPress;    // Название поставщика (SupplierName)
            textBox2.KeyPress += textBox2_KeyPress;    // Адрес поставщика (SupplierAddress)
            maskedTextBox1.KeyPress += textBox3_KeyPress;    // Телефон поставщика (SupplierPhone)

            FormSupplierEdit_Load(); // Загрузка данных поставщика в форму
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
        /// Валидация ввода для адреса поставщика (SupplierAddress)
        /// Ограничения: varchar(45), буквы, цифры, пробелы и основные символы
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
        /// Ограничения: varchar(18), формат +7XXXXXXXXXX
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
        /// Загрузка данных поставщика в элементы формы
        /// </summary>
        private void FormSupplierEdit_Load()
        {
            // Установка максимальных длин согласно структуре таблицы
            textBox1.MaxLength = 45;    // SupplierName varchar(45)
            textBox2.MaxLength = 45;    // SupplierAddress varchar(45)
            maskedTextBox1.MaxLength = 18;    // SupplierPhone varchar(12)

            // Заполнение полей формы текущими данными поставщика
            textBox1.Text = selectedProduct.Name;     // Название поставщика
            textBox2.Text = selectedProduct.Address;  // Адрес поставщика
            maskedTextBox1.Text = selectedProduct.Phone;    // Телефон поставщика
            maskedTextBox1.Mask = "+7 (000) 000-00-00";
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обновление данных поставщика в базе данных
        /// </summary>
        /// <param name="supplier">Объект поставщика с обновленными данными</param>
        private void UpdateProductInDatabase(Suppliers supplier)
        {
            // Проверка валидности ID поставщика
            if (supplier.Id <= 0)
            {
                MessageBox.Show("Неверный идентификатор поставщика.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (MySqlConnection connection = new MySqlConnection(conn))
            {
                try
                {
                    connection.Open();

                    // SQL-запрос для обновления данных поставщика
                    string updateQuery = @"
                    UPDATE supplier
                    SET 
                        SupplierName = @SupplierName, 
                        SupplierAddress = @SupplierAddress, 
                        SupplierPhone = @SupplierPhone   
                    WHERE 
                        SupplierId = @SupplierId";

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Параметризованный запрос для безопасности
                        command.Parameters.AddWithValue("@SupplierId", supplier.Id);
                        command.Parameters.AddWithValue("@SupplierName", supplier.Name);
                        command.Parameters.AddWithValue("@SupplierAddress", supplier.Address);
                        command.Parameters.AddWithValue("@SupplierPhone", supplier.Phone);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные поставщика успешно обновлены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные поставщика.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    MessageBox.Show($"Ошибка базы данных: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить" - валидация и сохранение изменений
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка заполнения обязательных полей
                if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                    string.IsNullOrWhiteSpace(textBox2.Text) ||
                    string.IsNullOrWhiteSpace(maskedTextBox1.Text))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины названия поставщика (не более 45 символов)
                if (textBox1.Text.Length > 45)
                {
                    MessageBox.Show("Название поставщика не должно превышать 45 символов!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины адреса поставщика (не более 45 символов)
                if (textBox2.Text.Length > 45)
                {
                    MessageBox.Show("Адрес поставщика не должен превышать 45 символов!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка формата и длины телефона (+7XXXXXXXXXX, 12 символов)
                if (!maskedTextBox1.Text.StartsWith("+7") || maskedTextBox1.Text.Length != 18)
                {
                    MessageBox.Show("Телефон должен быть в формате +7XXXXXXXXXX (12 символов)!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Обновление данных поставщика
                selectedProduct.Name = textBox1.Text.Trim();
                selectedProduct.Address = textBox2.Text.Trim();
                selectedProduct.Phone = maskedTextBox1.Text.Trim();

                // Сохранение изменений в базе данных
                UpdateProductInDatabase(selectedProduct);
            }
            catch (FormatException ex)
            {
                MessageBox.Show($"Ошибка формата данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Произошла ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Close();
            }
        }
    }
}