using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    /// <summary>
    /// Форма авторизации пользователей в системе
    /// </summary>
    public partial class Avtorization : Form
    {
        /// <summary>
        /// Инициализация формы авторизации
        /// </summary>
        public Avtorization()
        {
            InitializeComponent();
            textBoxPassword.PasswordChar = '*'; // Устанавливаем символ маскировки пароля
            // Добавляем обработчики событий для проверки вводимых символов
            textBoxLogin.KeyPress += TextBox_KeyPress;
            textBoxPassword.KeyPress += TextBox_KeyPress;
        }

        // Строка подключения к базе данных
        string conn = Connection.myConnection;

        /// <summary>
        /// Обработчик кнопки входа в систему
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем логин и пароль из полей ввода
            string UserLogin = textBoxLogin.Text.Trim();
            string UserPassword = textBoxPassword.Text.Trim();

            string login = Properties.Settings.Default.login;
            string password = Properties.Settings.Default.password;

           
            // Проверка на пустые поля
            if (UserLogin.Length == 0 || UserPassword.Length == 0)
            {
                MessageBox.Show("Введите логин и пароль", "Ошибка авторизации",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Хешируем введенный пароль для сравнения с хранимым в БД
            string hashedPass = HashPassword(UserPassword);

            try
            {
                using (MySqlConnection con = new MySqlConnection(conn))
                {
                    con.Open();
                    // SQL-запрос для проверки учетных данных
                    using (MySqlCommand cmd = new MySqlCommand(
                        "SELECT UserRoleId, UserName FROM user WHERE UserLogin = @UserLogin AND UserPassword = @UserPassword", con))
                    {
                        // Параметризованный запрос для защиты от SQL-инъекций
                        cmd.Parameters.AddWithValue("@UserLogin", UserLogin);
                        cmd.Parameters.AddWithValue("@UserPassword", hashedPass);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Если пользователь найден
                            {
                                string role = reader["UserRoleId"].ToString();
                                // Сохраняем ФИО пользователя в статическом классе
                                ValuesFIO.UserFIO = reader["UserName"].ToString();

                                // В зависимости от роли открываем соответствующую форму
                                Form nextForm = role == "1"
                                    ? (Form)new FormAdmin()
                                    : new FormManager();
                                nextForm.Show();
                                this.Hide(); // Скрываем форму авторизации
                            }
                            else if (login == "admin" && password == "admin")
                            {
                                RecoveryAdmin form = new RecoveryAdmin();
                                form.Show();
                                Hide();

                                return;
                            }
                            else // Если пользователь не найден
                            {
                                MessageBox.Show("Введен неверный логин или пароль",
                                    "Ошибка авторизации",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                                // Очищаем поля ввода
                                textBoxLogin.Clear();
                                textBoxPassword.Clear();
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex)
            {
                MessageBox.Show($"Ошибка базы данных: {ex.Message}",
                    "Ошибка авторизации",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}",
                    "Ошибка авторизации",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Хеширование пароля с использованием SHA256
        /// </summary>
        /// <param name="password">Пароль в открытом виде</param>
        /// <returns>Хешированная строка пароля</returns>
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // Вычисляем хеш
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                // Преобразуем каждый байт хеша в 16-ричную строку
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// Обработчик ввода символов в поля логина и пароля
        /// </summary>
        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только:
            // - английские буквы в нижнем регистре (a-z)
            // - цифры (0-9)
            // - управляющие символы (Backspace, Delete и т.д.)
            bool isLowercaseEnglish = (e.KeyChar >= 'a' && e.KeyChar <= 'z');
            bool isDigit = char.IsDigit(e.KeyChar);
            bool isControl = char.IsControl(e.KeyChar);

            if (!isLowercaseEnglish && !isDigit && !isControl)
            {
                e.Handled = true; // Блокируем ввод
            }
        }

        /// <summary>
        /// Обработчик кнопки выхода из приложения
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            // Запрос подтверждения выхода
            var result = MessageBox.Show("Вы действительно хотите выйти?",
                "Подтверждение выхода",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
    }
}