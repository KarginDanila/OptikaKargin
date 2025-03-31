using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления сотрудниками (просмотр, добавление, редактирование, удаление)
    /// </summary>
    public partial class FormSotrydniki : Form
    {
        /// <summary>
        /// Конструктор формы
        /// </summary>
        public FormSotrydniki()
        {
            InitializeComponent();
        }
        //Строка подключения к базе данных
        private string con = Connection.myConnection;
        // Список сотрудников
        private List<Sotrydniki> sotrydnikis = new List<Sotrydniki>();
        // Текст-заполнитель для поля поиска
        private const string PlaceholderText = "Поиск";
        // Обработчик нажатия кнопки "Назад" - закрытие формы
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик загрузки формы - инициализация данных
        /// </summary>
        private void FormSotrydniki_Load(object sender, EventArgs e)
        {
            LoadSotrydnikiFromDatabase(); // Загрузка данных из БД
            ConfigureDataGridView();      // Настройка таблицы
            ConfigureSearchBox();        // Настройка поля поиска
        }

        /// <summary>
        /// Настройка DataGridView (таблицы сотрудников)
        /// </summary>
        private void ConfigureDataGridView()
        {
            // Базовые настройки таблицы
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoGenerateColumns = false;

            AddDataGridViewColumns();    // Добавление колонок
            dataGridView1.DataSource = sotrydnikis; // Привязка данных
            CenterAllColumnsContent();   // Центрирование содержимого
        }

        /// <summary>
        /// Добавление колонок в DataGridView
        /// </summary>
        private void AddDataGridViewColumns()
        {
            // Колонка "Имя"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Имя",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Колонка "Адрес"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Адрес",
                DataPropertyName = "Address",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Колонка "Телефон"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Телефон",
                DataPropertyName = "Phone",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Колонка "Логин"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Логин",
                DataPropertyName = "Login",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Колонка "Пароль"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Пароль",
                DataPropertyName = "Password",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Колонка "Роль"
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Роль",
                DataPropertyName = "RoleName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
        }

        /// <summary>
        /// Центрирование содержимого всех колонок
        /// </summary>
        private void CenterAllColumnsContent()
        {
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        /// <summary>
        /// Настройка поля поиска
        /// </summary>
        private void ConfigureSearchBox()
        {
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;
            textBoxPoick.TextChanged += textBoxPoick_TextChanged; // Обработчик изменения текста
            textBoxPoick.Enter += TextBoxPoick_Enter;             // Обработчик получения фокуса
            textBoxPoick.Leave += TextBoxPoick_Leave;             // Обработчик потери фокуса
            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;       // Обработчик нажатия клавиш
        }

        /// <summary>
        /// Загрузка данных о сотрудниках из базы данных
        /// </summary>
        private void LoadSotrydnikiFromDatabase()
        {
            sotrydnikis.Clear();
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                string query = @"SELECT 
                    p.UserId AS Id,
                    p.UserName AS Name,
                    p.UserAddress AS Address,
                    p.UserPhone AS Phone,
                    p.UserLogin AS Login,
                    p.UserPassword AS Password,
                    s.RoleName AS RoleName

                FROM user p
 LEFT JOIN role s ON p.UserRoleId = s.RoleId"
;

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (
                        reader["Id"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Address"] != DBNull.Value &&
                        reader["Phone"] != DBNull.Value &&
                        reader["Login"] != DBNull.Value &&
                        reader["Password"] != DBNull.Value &&
                        reader["RoleName"] != DBNull.Value)
                    {
                        sotrydnikis.Add(new Sotrydniki
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Address = reader["Address"].ToString(),
                            Phone = reader["Phone"].ToString(),
                            Login = reader["Login"].ToString(),
                            Password = reader["Password"].ToString(),
                            RoleName = reader["RoleName"].ToString()
                        });
                    }
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Проверка данных из Reader на NULL значения
        /// </summary>
        private bool ValidateReaderData(MySqlDataReader reader)
        {
            return reader["Id"] != DBNull.Value &&
                   reader["Name"] != DBNull.Value &&
                   reader["Address"] != DBNull.Value &&
                   reader["Phone"] != DBNull.Value &&
                   reader["Login"] != DBNull.Value &&
                   reader["Password"] != DBNull.Value &&
                   reader["RoleName"] != DBNull.Value;
        }

        /// <summary>
        /// Создание объекта сотрудника из данных Reader
        /// </summary>
        private Sotrydniki CreateSotrydnikFromReader(MySqlDataReader reader)
        {
            return new Sotrydniki
            {
                Id = Convert.ToInt32(reader["Id"]),
                Name = reader["Name"].ToString(),
                Address = reader["Address"].ToString(),
                Phone = reader["Phone"].ToString(),
                Login = reader["Login"].ToString(),
                Password = reader["Password"].ToString(),
                RoleName = reader["RoleName"].ToString()
            };
        }

        /// <summary>
        /// Обновление данных в DataGridView
        /// </summary>
        private void UpdateDataGridView(IEnumerable<Sotrydniki> data)
        {
            dataGridView1.DataSource = data.ToList();
        }

        /// <summary>
        /// Обработчик получения фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Enter(object sender, EventArgs e)
        {
            if (textBoxPoick.Text == PlaceholderText)
            {
                textBoxPoick.Text = string.Empty;
                textBoxPoick.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Обработчик потери фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(textBoxPoick.Text))
            {
                textBoxPoick.Text = PlaceholderText;
                textBoxPoick.ForeColor = Color.Black;
                dataGridView1.DataSource = sotrydnikis;
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш в поле поиска
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы и пробелы
            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        private void textBoxPoick_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxPoick.Text.Trim();

            if (searchText == PlaceholderText || searchText.Length < 3)
            {
                UpdateDataGridView(sotrydnikis);
                return;
            }

            var filteredClients = sotrydnikis
                .Where(c => c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            UpdateDataGridView(filteredClients);
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - открытие формы добавления
        /// </summary>
        private void buttonadd_Click(object sender, EventArgs e)
        {
            FormSotrydnikiAdd form = new FormSotrydnikiAdd();
            form.ShowDialog();
            LoadSotrydnikiFromDatabase(); // Обновление данных после закрытия формы
        }

        /// <summary>
        /// Получение выбранного сотрудника из DataGridView
        /// </summary>
        private Sotrydniki GetSelectedProduct()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                return dataGridView1.SelectedRows[0].DataBoundItem as Sotrydniki;
            }
            return null;
        }

        /// <summary>
        /// Обработчик кнопки "Удалить" - удаление выбранного сотрудника
        /// </summary>
        private void buttondelete_Click(object sender, EventArgs e)
        {
            Sotrydniki selected = GetSelectedProduct();
            if (selected == null)
            {
                MessageBox.Show("Пожалуйста, выберите сотрудника для удаления.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ConfirmDelete(selected.Name))
            {
                DeleteEmployee(selected.Name);
            }
        }

        /// <summary>
        /// Подтверждение удаления сотрудника
        /// </summary>
        private bool ConfirmDelete(string employeeName)
        {
            return MessageBox.Show($"Удалить сотрудника '{employeeName}'?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        /// <summary>
        /// Удаление сотрудника из базы данных
        /// </summary>
        private void DeleteEmployee(string userName)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM user WHERE UserName = @UserName";

                    using (MySqlCommand command = new MySqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@UserName", userName);
                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Сотрудник удален.", "Успех",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadSotrydnikiFromDatabase(); // Обновление данных
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать" - открытие формы редактирования
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Выберите сотрудника для редактирования.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selected = (Sotrydniki)dataGridView1.SelectedRows[0].DataBoundItem;
            FormSotrydnikiEdit form = new FormSotrydnikiEdit(selected);
            form.ShowDialog();
            LoadSotrydnikiFromDatabase(); // Обновление данных после закрытия формы
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }

    /// <summary>
    /// Класс, представляющий сотрудника
    /// </summary>
    public class Sotrydniki
    {
        public int Id { get; set; }         // ID сотрудника
        public string Name { get; set; }    // Имя сотрудника
        public string Address { get; set; } // Адрес сотрудника
        public string Phone { get; set; }   // Телефон сотрудника
        public string Login { get; set; }   // Логин сотрудника
        public string Password { get; set; }// Пароль сотрудника
        public string RoleName { get; set; }// Название роли сотрудника
    }
}