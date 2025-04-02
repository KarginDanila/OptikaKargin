using System;
using System.Data;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing.Drawing2D;
using System.Drawing;

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
        private string captchaText;

        /// <summary>
        /// Обработчик кнопки входа в систему
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Получаем логин и пароль из полей ввода
            string UserLogin = textBoxLogin.Text.Trim();
            string UserPassword = textBoxPassword.Text.Trim();

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
                            else if (Properties.Settings.Default.login == textBoxLogin.Text && Properties.Settings.Default.password == textBoxPassword.Text)
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
                            Captha();
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
        private void Captha()
        {
            CaptchaTo();
            button2.Visible = true;
            pictureBox2.Visible = true;
            textBox3.Visible = true;
            button1.Enabled = false;
            pictureBox1.Enabled = false;
            textBoxLogin.Enabled = false;
            textBoxPassword.Enabled = false;
            textBoxLogin.Text = null;
            textBoxPassword.Text = null;
            this.Width = 642;
        }
        private void CaptchaTo()
        {
            Random random = new Random();
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            captchaText = "";
            for (int i = 0; i < 5; i++)
            {
                captchaText += chars[random.Next(chars.Length)];
            }
            Bitmap bmp = new Bitmap(pictureBox2.Width, pictureBox2.Height);
            Graphics graphics = Graphics.FromImage(bmp);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.Clear(Color.White);
            Font font = new Font("Arial", 20, FontStyle.Bold);
            for (int i = 0; i < 5; i++)
            {
                PointF point = new PointF(i * 20, 0);
                graphics.TranslateTransform(10, 10);
                graphics.RotateTransform(random.Next(-10, 10));
                graphics.DrawString(captchaText[i].ToString(), font, Brushes.Black, point);
                graphics.ResetTransform();
            }
            for (int i = 0; i < 10; i++)
            {
                Pen pen = new Pen(Color.Black, random.Next(2, 5));
                int x1 = random.Next(pictureBox2.Width);
                int y1 = random.Next(pictureBox2.Height);
                int x2 = random.Next(pictureBox2.Width);
                int y2 = random.Next(pictureBox2.Height);
                graphics.DrawLine(pen, x1, y1, x2, y2);
            }
            pictureBox2.Image = bmp;
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