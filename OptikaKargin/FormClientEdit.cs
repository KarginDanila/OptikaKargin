using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для редактирования данных клиента
    /// </summary>
    public partial class FormClientEdit : Form
    {
        private Client selectedProduct; // Выбранный клиент для редактирования
        private List<Client> clients = new List<Client>(); // Список клиентов
        string conn = Connection.myConnection; // Строка подключения к БД

        /// <summary>
        /// Конструктор формы редактирования клиента
        /// </summary>
        /// <param name="clients">Объект клиента для редактирования</param>
        public FormClientEdit(Client clients)
        {
            InitializeComponent();
            selectedProduct = clients;

            // Инициализация формы и загрузка данных
            FormClientEdit_Load();

            // Подписка на события проверки ввода
            textBox1.KeyPress += textBox1_KeyPress;       // Имя
            textBox2.KeyPress += textBox2_KeyPress;       // Фамилия
            textBox3.KeyPress += textBox3_KeyPress;       // Отчество
            textBox4.KeyPress += textBox4_KeyPress;       // Email
            textBox5.KeyPress += textBox5_KeyPress;       // Адрес
            maskedTextBox1.KeyPress += textBox6_KeyPress;       // Телефон

            // Установка максимальной длины полей согласно структуре таблицы
            textBox1.MaxLength = 45;    // ClientName varchar(45)
            textBox2.MaxLength = 50;    // ClientSurname varchar(50)
            textBox3.MaxLength = 50;    // ClientPatronymic varchar(50)
            textBox4.MaxLength = 30;     // ClientEmail varchar(30)
            textBox5.MaxLength = 50;     // ClientAddress varchar(50)
            maskedTextBox1.MaxLength = 18;     // ClientPhone varchar(13)
        }

        /// <summary>
        /// Загрузка данных выбранного клиента в форму
        /// </summary>
        private void FormClientEdit_Load()
        {
            textBox1.Text = selectedProduct.Name;
            textBox2.Text = selectedProduct.Surname;
            textBox3.Text = selectedProduct.Patronymic;
            textBox4.Text = selectedProduct.Email;
            textBox5.Text = selectedProduct.Address;
            maskedTextBox1.Text = selectedProduct.Phone;
            maskedTextBox1.Mask = "+7 (000) 000-00-00";

        }

        /// <summary>
        /// Обновление данных клиента в базе данных
        /// </summary>
        /// <param name="clients">Объект клиента с обновленными данными</param>
        private void UpdateProductInDatabase(Client clients)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();

                    // SQL-запрос для обновления данных клиента
                    string updateQuery = @"
                    UPDATE client
                    SET 
                        ClientName = @ClientName,
                        ClientSurname = @ClientSurname,
                        ClientPatronymic = @ClientPatronymic,
                        ClientEmail = @ClientEmail,
                        ClientAddress = @ClientAddress,
                        ClientPhone = @ClientPhone
                    WHERE 
                        ClientId = @ClientId";

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Параметризованный запрос для защиты от SQL-инъекций
                        command.Parameters.AddWithValue("@ClientId", clients.Id);
                        command.Parameters.AddWithValue("@ClientName", clients.Name);
                        command.Parameters.AddWithValue("@ClientSurname", clients.Surname);
                        command.Parameters.AddWithValue("@ClientPatronymic", clients.Patronymic);
                        command.Parameters.AddWithValue("@ClientEmail", clients.Email);
                        command.Parameters.AddWithValue("@ClientAddress", clients.Address);
                        command.Parameters.AddWithValue("@ClientPhone", clients.Phone);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Клиент успешно обновлен в базе данных.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные клиента.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении клиента: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить" - обновление данных клиента
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                    string.IsNullOrWhiteSpace(textBox2.Text) ||
                    string.IsNullOrWhiteSpace(textBox3.Text) ||
                    string.IsNullOrWhiteSpace(textBox4.Text) ||
                    string.IsNullOrWhiteSpace(textBox5.Text) ||
                    string.IsNullOrWhiteSpace(maskedTextBox1.Text))
                {
                    MessageBox.Show("Все поля должны быть заполнены!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка валидности email
                if (!IsValidEmail(textBox4.Text))
                {
                    MessageBox.Show("Введите корректный email адрес!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                // Обновление данных клиента
                selectedProduct.Name = textBox1.Text.Trim();
                selectedProduct.Surname = textBox2.Text.Trim();
                selectedProduct.Patronymic = textBox3.Text.Trim();
                selectedProduct.Email = textBox4.Text.Trim();
                selectedProduct.Address = textBox5.Text.Trim();
                selectedProduct.Phone = maskedTextBox1.Text.Trim();

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

        /// <summary>
        /// Проверка валидности email адреса
        /// </summary>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Обработчики ввода с учетом ограничений полей БД:
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
        /// Обработчик ввода для поля Фамилия (только русские буквы и пробелы)
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';
            bool isSpace = e.KeyChar == ' ';
            bool isControl = char.IsControl(e.KeyChar);

            if (!isRussianLetter && !isSpace && !isControl)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик изменения текста для поля Фамилия (делает первую букву заглавной)
        /// </summary>
        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox2.Text) && textBox2.SelectionStart == 1)
            {
                textBox2.Text = char.ToUpper(textBox2.Text[0]) + textBox2.Text.Substring(1);
                textBox2.SelectionStart = textBox2.Text.Length;
            }
        }

        /// <summary>
        /// Обработчик ввода для поля Отчество (только русские буквы и пробелы)
        /// </summary>
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';
            bool isSpace = e.KeyChar == ' ';
            bool isControl = char.IsControl(e.KeyChar);

            if (!isRussianLetter && !isSpace && !isControl)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик изменения текста для поля Отчество (делает первую букву заглавной)
        /// </summary>
        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(textBox3.Text) && textBox3.SelectionStart == 1)
            {
                textBox3.Text = char.ToUpper(textBox3.Text[0]) + textBox3.Text.Substring(1);
                textBox3.SelectionStart = textBox3.Text.Length;
            }
        }

        /// <summary>
        /// Обработчик ввода для поля Email (только английские буквы, цифры, @, ., _, -)
        /// </summary>
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Проверяем на русские буквы
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';

            // Разрешенные символы для email
            bool isEnglishLetter = (e.KeyChar >= 'a' && e.KeyChar <= 'z') ||
                                 (e.KeyChar >= 'A' && e.KeyChar <= 'Z');
            bool isDigit = char.IsDigit(e.KeyChar);
            bool isAllowedSymbol = e.KeyChar == '.' || e.KeyChar == '@' ||
                                 e.KeyChar == '_' || e.KeyChar == '-';
            bool isControl = char.IsControl(e.KeyChar);

            // Блокируем русские буквы и другие неразрешенные символы
            if (isRussianLetter || (!isEnglishLetter && !isDigit &&
                                   !isAllowedSymbol && !isControl))
            {
                e.Handled = true;
            }
        }

        
        /// <summary>
        /// Валидация ввода для Адреса (ClientAddress varchar(50))
        /// Разрешает: русские буквы, цифры, пробелы, запятые, точки, дефисы
        /// </summary>
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
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
            if (textBox5.Text.Length >= 50 && !isControl)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик ввода для поля Телефон (только цифры и пробелы, начинается с +7)
        /// </summary>
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Запрещаем удаление +7
            if (maskedTextBox1.Text == "+7 " && (e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Delete))
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только цифры, пробелы и управляющие символы
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
            }

            // Автоматическое добавление +7 при начале ввода
            if (maskedTextBox1.Text.Length == 0 && e.KeyChar != '+')
            {
                maskedTextBox1.Mask = "+7 (000) 000-00-00";
                maskedTextBox1.SelectionStart = maskedTextBox1.Text.Length;
            }
        }
    }
}