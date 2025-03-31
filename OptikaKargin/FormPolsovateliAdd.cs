using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для добавления нового клиента в систему
    /// </summary>
    public partial class FormPolsovateliAdd : Form
    {
        public FormPolsovateliAdd()
        {
            InitializeComponent();
            // Подписываем обработчики событий для валидации ввода
            textBox1.KeyPress += textBox1_KeyPress;    // Имя
            textBox2.KeyPress += textBox2_KeyPress;    // Фамилия
            textBox3.KeyPress += textBox3_KeyPress;    // Отчество
            textBox4.KeyPress += textBox4_KeyPress;    // Email
            textBox5.KeyPress += textBox5_KeyPress;    // Адрес
            maskedTextBox1.KeyPress += textBox6_KeyPress;    // Телефон
        }

        /// <summary>
        /// Обработчик загрузки формы - инициализация полей
        /// </summary>
        private void FormPolsovateliAdd_Load(object sender, EventArgs e)
        {
            // Очистка полей и установка маски телефона
            textBox1.Text = "";
            textBox2.Text = "";
            textBox3.Text = "";
            textBox4.Text = "";
            textBox5.Text = "";
            maskedTextBox1.Mask = "+7 (000) 000-00-00";  // Начальный формат номера телефона
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - валидация и сохранение клиента
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Создание объекта клиента с данными из формы
                Client newClient = new Client
                {
                    Name = textBox1.Text.Trim(),        // Имя (макс. 45 символов)
                    Surname = textBox2.Text.Trim(),     // Фамилия (макс. 50 символов)
                    Patronymic = textBox3.Text.Trim(),  // Отчество (макс. 50 символов)
                    Email = textBox4.Text.Trim(),       // Email (макс. 30 символов)
                    Address = textBox5.Text.Trim(),     // Адрес (макс. 50 символов)
                    Phone = maskedTextBox1.Text.Trim()        // Телефон (макс. 13 символов)
                };

                // Проверка заполнения обязательных полей
                if (string.IsNullOrEmpty(newClient.Name) ||
                    string.IsNullOrEmpty(newClient.Surname) ||
                    string.IsNullOrEmpty(newClient.Patronymic) ||
                    string.IsNullOrEmpty(newClient.Email) ||
                    string.IsNullOrEmpty(newClient.Address) ||
                    string.IsNullOrEmpty(newClient.Phone))
                {
                    MessageBox.Show("Ошибка заполнения данных!", "Заполните все поля!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины полей в соответствии с БД
                if (newClient.Name.Length > 45 ||
                    newClient.Surname.Length > 50 ||
                    newClient.Patronymic.Length > 50 ||
                    newClient.Email.Length > 30 ||
                    newClient.Address.Length > 50 ||
                    newClient.Phone.Length > 18)
                {
                    MessageBox.Show("Превышена максимальная длина одного из полей!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка формата email
                if (!IsValidEmail(newClient.Email))
                {
                    MessageBox.Show("Некорректный формат email!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AddProductToDatabase(newClient);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка!", "Ошибка при добавлении клиента: " + ex.Message, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Проверка валидности email
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

        /// <summary>
        /// Добавление клиента в базу данных
        /// </summary>
        private void AddProductToDatabase(Client client)
        {
            try
            {
                string connect = Connection.myConnection;
                // SQL-запрос с учетом структуры таблицы client
                string insertQuery = @"
                INSERT INTO client (
                    ClientName,        -- varchar(45)
                    ClientSurname,     -- varchar(50)
                    ClientPatronymic,  -- varchar(50)
                    ClientEmail,       -- varchar(30)
                    ClientAddress,     -- varchar(50)
                    ClientPhone        -- varchar(13)
                ) VALUES (
                    @ClientName, 
                    @ClientSurname, 
                    @ClientPatronymic, 
                    @ClientEmail, 
                    @ClientAddress, 
                    @ClientPhone
                )";

                using (MySqlConnection connection = new MySqlConnection(connect))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        // Параметризованный запрос для безопасности
                        command.Parameters.AddWithValue("@ClientName", client.Name);
                        command.Parameters.AddWithValue("@ClientSurname", client.Surname);
                        command.Parameters.AddWithValue("@ClientPatronymic", client.Patronymic);
                        command.Parameters.AddWithValue("@ClientEmail", client.Email);
                        command.Parameters.AddWithValue("@ClientAddress", client.Address);
                        command.Parameters.AddWithValue("@ClientPhone", client.Phone);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Клиент успешно добавлен в базу данных.");
                            FormPolsovateli form = new FormPolsovateli();
                            form.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении клиента в базу данных.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при добавлении клиента в базу данных: " + e.Message);
            }
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
        /// Валидация ввода для Телефона (ClientPhone varchar(13))
        /// </summary>
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Защищаем начало номера "+7 "
            if (maskedTextBox1.Text == "+7 " && (e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Delete))
            {
                e.Handled = true;
                return;
            }

            // Разрешаем только цифры и пробелы
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != ' ')
            {
                e.Handled = true;
            }

            // Ограничение длины 18 символов (включая "+7 ")
            if (maskedTextBox1.Text.Length >= 18 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
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