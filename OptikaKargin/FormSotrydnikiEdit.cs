using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Linq;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для редактирования данных сотрудника
    /// </summary>
    public partial class FormSotrydnikiEdit : Form
    {
        private Sotrydniki selectedEmployee; // Выбранный сотрудник для редактирования
        private string conn = Connection.myConnection; // Строка подключения к БД

        /// <summary>
        /// Конструктор формы с передачей данных сотрудника
        /// </summary>
        public FormSotrydnikiEdit(Sotrydniki employee)
        {
            InitializeComponent();
            selectedEmployee = employee;

            // Настройка обработчиков событий для валидации
            textBox1.KeyPress += textBox1_KeyPress;    // Имя (UserName)
            textBox2.KeyPress += textBox2_KeyPress;    // Адрес (UserAddress)
            maskedTextBox1.KeyPress += textBox3_KeyPress;    // Телефон (UserPhone)
            textBox4.KeyPress += textBox4_KeyPress;    // Логин (UserLogin)
            textBox5.KeyPress += textBox5_KeyPress;    // Пароль (UserPassword) 

            FormSotrydnikiEdit_Load(); // Загрузка данных сотрудника в форму
        }

        /// <summary>
        /// Загрузка данных сотрудника в элементы формы
        /// </summary>
        private void FormSotrydnikiEdit_Load()
        {
            // Заполнение полей формы данными сотрудника
            textBox1.Text = selectedEmployee.Name;       // UserName (varchar(45))
            textBox2.Text = selectedEmployee.Address;    // UserAddress (varchar(45))
            maskedTextBox1.Text = selectedEmployee.Phone;      // UserPhone (varchar(12))
            textBox4.Text = selectedEmployee.Login;      // UserLogin (tinytext)
            textBox5.Text = selectedEmployee.Password;   // UserPassword (tinytext)

            LoadRoles(); // Загрузка списка ролей
            comboBox1.SelectedItem = selectedEmployee.RoleName; // Установка текущей роли
            comboBox1.DropDownStyle = ComboBoxStyle.DropDownList; // Запрет ручного ввода

            textBox1.MaxLength = 45;    // UserName varchar(45)
            textBox2.MaxLength = 45;    // UserAddress varchar(45)
            maskedTextBox1.MaxLength = 12;    // UserPhone varchar(12)
            textBox4.MaxLength = 255;   // UserLogin tinytext
            textBox5.MaxLength = 255;   // UserPassword tinytext
            maskedTextBox1.Mask = "+7 (000) 000-00-00";
        }

        /// <summary>
        /// Загрузка списка ролей из базы данных
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();
                    string query = "SELECT RoleName FROM role";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            comboBox1.Items.Clear(); // Очистка списка ролей

                            // Заполнение выпадающего списка
                            while (reader.Read())
                            {
                                string roleName = reader.GetString("RoleName");
                                comboBox1.Items.Add(roleName);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обновление данных сотрудника в базе данных
        /// </summary>
        private void UpdateEmployeeInDatabase()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(conn))
                {
                    connection.Open();

                    // SQL-запрос для обновления данных сотрудника
                    string updateQuery = @"
                    UPDATE user
                    SET 
                        UserName = @UserName,
                        UserAddress = @UserAddress,
                        UserPhone = @UserPhone,
                        UserLogin = @UserLogin,
                        UserPassword = @UserPassword,
                        UserRoleId = (SELECT RoleId FROM role WHERE RoleName = @RoleName)
                    WHERE 
                        UserId = @UserId";

                    using (MySqlCommand command = new MySqlCommand(updateQuery, connection))
                    {
                        // Параметризованный запрос с учетом ограничений полей
                        command.Parameters.AddWithValue("@UserId", selectedEmployee.Id);
                        command.Parameters.AddWithValue("@UserName", selectedEmployee.Name.Length > 45 ?
                            selectedEmployee.Name.Substring(0, 45) : selectedEmployee.Name);
                        command.Parameters.AddWithValue("@UserAddress", selectedEmployee.Address.Length > 45 ?
                            selectedEmployee.Address.Substring(0, 45) : selectedEmployee.Address);
                        command.Parameters.AddWithValue("@UserPhone", selectedEmployee.Phone.Length > 45 ?
                            selectedEmployee.Phone.Substring(0, 45) : selectedEmployee.Phone);
                        command.Parameters.AddWithValue("@UserLogin", selectedEmployee.Login);
                        command.Parameters.AddWithValue("@UserPassword", selectedEmployee.Password);
                        command.Parameters.AddWithValue("@RoleName", selectedEmployee.RoleName);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Данные сотрудника успешно обновлены.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Не удалось обновить данные сотрудника.", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении данных: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Сохранить" - валидация и сохранение изменений
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Проверка заполнения всех обязательных полей
            if (string.IsNullOrWhiteSpace(textBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox2.Text) ||
                string.IsNullOrWhiteSpace(maskedTextBox1.Text) ||
                string.IsNullOrWhiteSpace(textBox4.Text) ||
                string.IsNullOrWhiteSpace(textBox5.Text) ||
                comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Все поля должны быть заполнены!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины имени (не более 45 символов)
            if (textBox1.Text.Length > 45)
            {
                MessageBox.Show("Имя не должно превышать 45 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины адреса (не более 45 символов)
            if (textBox2.Text.Length > 45)
            {
                MessageBox.Show("Адрес не должен превышать 45 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины логина (4-255 символов)
            if (textBox4.Text.Length < 4 || textBox4.Text.Length > 255)
            {
                MessageBox.Show("Логин должен содержать от 4 до 255 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины пароля (6-255 символов)
            if (textBox5.Text.Length < 6 || textBox5.Text.Length > 255)
            {
                MessageBox.Show("Пароль должен содержать от 6 до 255 символов!", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Обновление данных сотрудника
            selectedEmployee.Name = textBox1.Text.Trim();
            selectedEmployee.Address = textBox2.Text.Trim();
            selectedEmployee.Phone = maskedTextBox1.Text.Trim();
            selectedEmployee.Login = textBox4.Text.Trim();
            selectedEmployee.Password = textBox5.Text.Trim();
            selectedEmployee.RoleName = comboBox1.SelectedItem.ToString();

            UpdateEmployeeInDatabase();
            this.Close();
        }

        /// <summary>
        /// Обработчик кнопки "Отмена" - закрытие формы
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
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
    }
}