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
    public partial class FormPolsovateliEdit : Form
    {
        private Polsovateli selectedProduct;
        private List<Polsovateli> polsovatelis = new List<Polsovateli>();
        string conn = Connection.myConnection;

        // Конструктор формы, принимающий объект Polsovateli для редактирования
        public FormPolsovateliEdit(Polsovateli polsovatelis)
        {
            InitializeComponent();
            selectedProduct = polsovatelis;
            FormPolsovateliEdit_Load();

            // Подписка на события KeyPress для всех текстовых полей
            textBox1.KeyPress += textBox1_KeyPress;    // Имя
            textBox2.KeyPress += textBox2_KeyPress;    // Фамилия
            textBox3.KeyPress += textBox3_KeyPress;    // Отчество
            textBox4.KeyPress += textBox4_KeyPress;    // Email
            textBox5.KeyPress += textBox5_KeyPress;    // Адрес
            maskedTextBox1.KeyPress += textBox6_KeyPress;    // Телефон
        }

        // Загрузка данных выбранного пользователя в поля формы
        private void FormPolsovateliEdit_Load()
        {
            textBox1.Text = selectedProduct.Name;        // Имя (ClientName, varchar(45))
            textBox2.Text = selectedProduct.Surname;     // Фамилия (ClientSurname, varchar(50))
            textBox3.Text = selectedProduct.Patronymic; // Отчество (ClientPatronymic, varchar(50))
            textBox4.Text = selectedProduct.Email;       // Email (ClientEmail, varchar(30))
            textBox5.Text = selectedProduct.Address;     // Адрес (ClientAddress, varchar(50))
            maskedTextBox1.Text = selectedProduct.Phone;// Телефон (ClientPhone, varchar(13))
            maskedTextBox1.Mask = "+7 (000) 000-00-00";
        }

        // Обработчик кнопки сохранения изменений
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                // Обновление свойств объекта из полей формы
                selectedProduct.Name = textBox1.Text;
                selectedProduct.Surname = textBox2.Text;
                selectedProduct.Patronymic = textBox3.Text;
                selectedProduct.Email = textBox4.Text;
                selectedProduct.Address = textBox5.Text;
                selectedProduct.Phone = maskedTextBox1.Text;

                // Проверка на пустые обязательные поля
                if (string.IsNullOrWhiteSpace(selectedProduct.Name) ||
                    string.IsNullOrWhiteSpace(selectedProduct.Surname) ||
                    string.IsNullOrWhiteSpace(selectedProduct.Phone))
                {
                    MessageBox.Show("Поля Имя, Фамилия и Телефон обязательны для заполнения!");
                    return;
                }

                UpdateProductInDatabase(selectedProduct);
            }
            catch (FormatException ex)
            {
                MessageBox.Show("Ошибка формата данных: " + ex.Message);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Произошла ошибка: " + ex.Message);
            }
            finally
            {
                this.Close();
            }
        }

        // Метод обновления данных пользователя в базе данных
        private void UpdateProductInDatabase(Polsovateli polsovatelis)
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

                MySqlCommand command = new MySqlCommand(updateQuery, connection);
                // Параметры запроса с ограничениями длины согласно структуре таблицы
                command.Parameters.AddWithValue("@ClientId", polsovatelis.Id);
                command.Parameters.AddWithValue("@ClientName", polsovatelis.Name.Length > 45 ? polsovatelis.Name.Substring(0, 45) : polsovatelis.Name);
                command.Parameters.AddWithValue("@ClientSurname", polsovatelis.Surname.Length > 50 ? polsovatelis.Surname.Substring(0, 50) : polsovatelis.Surname);
                command.Parameters.AddWithValue("@ClientPatronymic", polsovatelis.Patronymic.Length > 50 ? polsovatelis.Patronymic.Substring(0, 50) : polsovatelis.Patronymic);
                command.Parameters.AddWithValue("@ClientEmail", polsovatelis.Email.Length > 30 ? polsovatelis.Email.Substring(0, 30) : polsovatelis.Email);
                command.Parameters.AddWithValue("@ClientAddress", polsovatelis.Address.Length > 50 ? polsovatelis.Address.Substring(0, 50) : polsovatelis.Address);
                command.Parameters.AddWithValue("@ClientPhone", polsovatelis.Phone.Length > 18 ? polsovatelis.Phone.Substring(0, 13) : polsovatelis.Phone);

                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Клиент успешно обновлен в базе данных.");
                    FormPolsovateli form = new FormPolsovateli();
                    form.ShowDialog();
                }
                else
                {
                    MessageBox.Show("Ошибка при обновлении клиента в базе данных.");
                }
            }
        }

        // Обработчик кнопки отмены
        private void button1_Click_1(object sender, EventArgs e)
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

        // Телефон (ClientPhone, varchar(13))
        private void textBox6_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Запрещаем удаление префикса "+7 "
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

            // Ограничение длины (18 символов - "+7 123456789")
            if (maskedTextBox1.Text.Length >= 18 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
    }
}