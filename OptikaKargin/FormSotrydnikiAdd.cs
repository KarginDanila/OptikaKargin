using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using MySqlX.XDevAPI.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using System.Text.RegularExpressions;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для добавления нового сотрудника в систему
    /// </summary>
    public partial class FormSotrydnikiAdd : Form
    {
        public FormSotrydnikiAdd()
        {
            InitializeComponent();
            // Подписка на события валидации ввода
            textBox1.KeyPress += textBox1_KeyPress;    // Имя (UserName)
            textBox2.KeyPress += textBox2_KeyPress;    // Адрес (UserAddress)
            maskedTextBox1.KeyPress += textBox3_KeyPress;    // Телефон (UserPhone)
            textBox4.KeyPress += textBox4_KeyPress;    // Логин (UserLogin)
            textBox5.KeyPress += textBox5_KeyPress;    // Пароль (UserPassword) 
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - валидация и сохранение сотрудника
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Создание объекта сотрудника с данными из формы
                Sotrydniki newsotrydniki = new Sotrydniki
                {
                    Name = textBox1.Text.Trim(),        // Имя
                    Address = textBox2.Text.Trim(),      // Адрес
                    Phone = maskedTextBox1.Text.Trim(),        // Телефон
                    Login = textBox4.Text.Trim(),        // Логин
                    Password = textBox5.Text.Trim(),     // Пароль
                    RoleName = comboBox1.SelectedItem?.ToString(), // Роль
                };

                // Проверка заполнения обязательных полей
                if (string.IsNullOrEmpty(newsotrydniki.Name) ||
                    string.IsNullOrEmpty(newsotrydniki.Address) ||
                    string.IsNullOrEmpty(newsotrydniki.Phone) ||
                    string.IsNullOrEmpty(newsotrydniki.Login) ||
                    string.IsNullOrEmpty(newsotrydniki.Password) ||
                    string.IsNullOrEmpty(newsotrydniki.RoleName))
                {
                    MessageBox.Show("Ошибка заполнения данных!", "Заполните все поля!",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка формата телефона (должен начинаться с +7 и содержать 11 цифр)
                if (!newsotrydniki.Phone.StartsWith("+7") || newsotrydniki.Phone.Length != 18)
                {
                    MessageBox.Show("Некорректный формат телефона!\nТелефон должен быть в формате +7XXXXXXXXXX",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины логина (не менее 4 символов)
                if (newsotrydniki.Login.Length < 4)
                {
                    MessageBox.Show("Логин должен содержать минимум 4 символа!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Проверка длины пароля (не менее 6 символов)
                if (newsotrydniki.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен содержать минимум 6 символов!",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                AddProductToDatabase(newsotrydniki);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка!", "Ошибка при добавлении сотрудника: " + ex.Message,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Добавление сотрудника в базу данных
        /// </summary>
        private void AddProductToDatabase(Sotrydniki sotrydniki)
        {
            try
            {
                string connect = Connection.myConnection;
                // SQL-запрос для добавления сотрудника
                string insertQuery = @"
                INSERT INTO user (
                    UserName,        -- Имя сотрудника
                    UserAddress,     -- Адрес
                    UserPhone,       -- Телефон
                    UserLogin,       -- Логин
                    UserPassword,    -- Пароль
                    UserRoleId       -- ID роли (получаем из подзапроса)
                ) VALUES (
                    @UserName, 
                    @UserAddress, 
                    @UserPhone, 
                    @UserLogin, 
                    @UserPassword, 
                   (SELECT RoleId FROM role WHERE RoleName = @UserRoleId)
                )";

                using (MySqlConnection connection = new MySqlConnection(connect))
                {
                    connection.Open();

                    using (MySqlCommand command = new MySqlCommand(insertQuery, connection))
                    {
                        // Параметризованный запрос для безопасности
                        command.Parameters.AddWithValue("@UserName", sotrydniki.Name);
                        command.Parameters.AddWithValue("@UserAddress", sotrydniki.Address);
                        command.Parameters.AddWithValue("@UserPhone", sotrydniki.Phone);
                        command.Parameters.AddWithValue("@UserLogin", sotrydniki.Login);
                        command.Parameters.AddWithValue("@UserPassword", sotrydniki.Password);
                        command.Parameters.AddWithValue("@UserRoleId", sotrydniki.RoleName);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Сотрудник успешно добавлен в базу данных.");
                            // Открываем форму со списком сотрудников
                            FormSotrydniki form = new FormSotrydniki();
                            form.ShowDialog();
                        }
                        else
                        {
                            MessageBox.Show("Ошибка при добавлении сотрудника в базу данных.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Ошибка при добавлении сотрудника в базу данных: " + e.Message);
            }
        }

        /// <summary>
        /// Загрузка списка ролей из базы данных
        /// </summary>
        private void LoadRole()
        {
            using (MySqlConnection connection = new MySqlConnection(Connection.myConnection))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT RoleName FROM role", connection);
                MySqlDataReader reader = command.ExecuteReader();

                comboBox1.Items.Clear();

                // Заполнение выпадающего списка ролями
                while (reader.Read())
                {
                    comboBox1.Items.Add(reader["RoleName"].ToString());
                }

                reader.Close();
            }

            // Установка первого элемента по умолчанию
            if (comboBox1.Items.Count > 0)
            {
                comboBox1.SelectedIndex = 0;
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
        /// Валидация ввода для адреса (UserAddress)
        /// Ограничения: varchar(45), русские буквы, цифры, пробелы и основные символы
        /// </summary>
        private void textBox2_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Проверяем, является ли символ допустимым
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                 (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                 e.KeyChar == 'ё' || e.KeyChar == 'Ё';
            bool isDigit = char.IsDigit(e.KeyChar);
            bool isSpace = char.IsWhiteSpace(e.KeyChar);
            bool isControl = char.IsControl(e.KeyChar);
            bool isAllowedSymbol = e.KeyChar == ',' || e.KeyChar == '.' ||
                                 e.KeyChar == '-' || e.KeyChar == '/';

            if (!isRussianLetter && !isDigit && !isSpace &&
                !isControl && !isAllowedSymbol)
            {
                e.Handled = true;  // Блокируем ввод
            }

            // Ограничение длины 45 символов
            if (textBox2.Text.Length >= 45 && !isControl)
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Валидация ввода для телефона (UserPhone)
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

            // Разрешаем только цифры
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            // Ограничение длины (18 символов включая "+7")
            if (maskedTextBox1.Text.Length >= 18 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Валидация ввода для логина (UserLogin)
        /// Ограничения: tinytext (255 символов), латинские буквы, цифры и _
        /// </summary>
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только латинские буквы, цифры и _
            if (!char.IsLetterOrDigit(e.KeyChar) &&
                !char.IsControl(e.KeyChar) &&
                e.KeyChar != '_' &&
                !(e.KeyChar >= 'a' && e.KeyChar <= 'z'))
            {
                e.Handled = true;
            }

            // Ограничение длины 255 символов
            if (textBox4.Text.Length >= 255 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }

            // Запрещаем заглавные буквы
            if (char.IsUpper(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Валидация ввода для пароля (UserPassword)
        /// Ограничения: tinytext (255 символов), латинские буквы, цифры и спецсимволы
        /// </summary>
        private void textBox5_KeyPress(object sender, KeyPressEventArgs e)
        {
            bool isRussianLetter = (e.KeyChar >= 'а' && e.KeyChar <= 'я') ||
                                  (e.KeyChar >= 'А' && e.KeyChar <= 'Я') ||
                                  e.KeyChar == 'ё' || e.KeyChar == 'Ё';

            // Если символ не латинская буква, не цифра, не спецсимвол и не управляющий символ - блокируем
            if (!char.IsLetterOrDigit(e.KeyChar) &&
                !char.IsControl(e.KeyChar) &&
                !"!@#$%^&*()-_=+[]{};:'\",.<>/?\\| ".Contains(e.KeyChar) &&
                !(e.KeyChar >= 'a' && e.KeyChar <= 'z') &&
                !(e.KeyChar >= 'A' && e.KeyChar <= 'Z'))
            {
                e.Handled = true;
            }

            // Блокируем русские буквы в любом случае
            if (isRussianLetter)
            {
                e.Handled = true;
            }

            // Ограничение длины 255 символов
            if (textBox5.Text.Length >= 255 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик загрузки формы - инициализация полей
        /// </summary>
        private void FormSotrydnikiAdd_Load(object sender, EventArgs e)
        {
            // Очистка полей и установка начальных значений
            textBox1.Text = "";
            textBox2.Text = "";
            maskedTextBox1.Text = "+7"; // Начальный формат номера телефона
            textBox4.Text = "";
            textBox5.Text = "";
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Запрет ручного ввода
            LoadRole(); // Загрузка списка ролей

            textBox1.MaxLength = 45;    // UserName varchar(45)
            textBox2.MaxLength = 45;    // UserAddress varchar(45)
            maskedTextBox1.MaxLength = 18;    // UserPhone varchar(12)
            textBox4.MaxLength = 255;   // UserLogin tinytext
            textBox5.MaxLength = 255;   // UserPassword tinytext
            maskedTextBox1.Mask = "+7 (000) 000-00-00";
        }
    }
    
}