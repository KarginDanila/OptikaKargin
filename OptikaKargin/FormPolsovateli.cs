using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Mysqlx.Datatypes.Scalar.Types;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления клиентами (пользователями)
    /// </summary>
    public partial class FormPolsovateli : Form
    {
        public FormPolsovateli()
        {
            InitializeComponent();
        }

        string con = Connection.myConnection; // Строка подключения к БД
        private List<Polsovateli> polsovatelis = new List<Polsovateli>(); // Список клиентов
        private const string PlaceholderText = "Поиск"; // Текст-заглушка для поиска

        /// <summary>
        /// Обработчик кнопки возврата
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Загрузка формы и инициализация компонентов
        /// </summary>
        private void FormPolsovateli_Load(object sender, EventArgs e)
        {
            LoadPolsovateliFromDatabase(); // Загрузка клиентов из БД

            // Настройка DataGridView
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк
            dataGridView1.AllowUserToDeleteRows = false; // Запрет удаления строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоподбор ширины колонок
            dataGridView1.AutoGenerateColumns = false; // Отключение авто-генерации колонок

            // Настройка поля поиска
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;

            // Подписка на события поля поиска
            textBoxPoick.TextChanged += textBoxPoick_TextChanged;
            textBoxPoick.Enter += TextBoxPoick_Enter;
            textBoxPoick.Leave += TextBoxPoick_Leave;

            // Добавление колонок в DataGridView
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Имя", DataPropertyName = "Name" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Фамилия", DataPropertyName = "Surname" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Отчество", DataPropertyName = "Patronymic" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Почта", DataPropertyName = "Email" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Адрес", DataPropertyName = "Address" });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Телефон", DataPropertyName = "Phone" });

            dataGridView1.DataSource = polsovatelis; // Привязка данных

            // Центрирование текста в колонках
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            textBoxPoick.KeyPress += TextBoxPoick_KeyPress; // Обработка ввода
        }

        /// <summary>
        /// Загрузка клиентов из базы данных
        /// </summary>
        private void LoadPolsovateliFromDatabase()
        {
            polsovatelis.Clear();
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                // SQL-запрос для получения клиентов
                string query = @"SELECT 
                    ClientId AS Id,
                    ClientName AS Name,
                    ClientSurname AS Surname,
                    ClientPatronymic AS Patronymic,
                    ClientEmail AS Email,
                    ClientAddress AS Address,
                    ClientPhone AS Phone
                FROM client";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    // Проверка на NULL значения
                    if (reader["Id"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Surname"] != DBNull.Value &&
                        reader["Patronymic"] != DBNull.Value &&
                        reader["Email"] != DBNull.Value &&
                        reader["Address"] != DBNull.Value &&
                        reader["Phone"] != DBNull.Value)
                    {
                        // Добавление клиента в список
                        polsovatelis.Add(new Polsovateli
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = reader["Name"].ToString(),
                            Surname = reader["Surname"].ToString(),
                            Patronymic = reader["Patronymic"].ToString(),
                            Email = reader["Email"].ToString(),
                            Address = reader["Address"].ToString(),
                            Phone = reader["Phone"].ToString()
                        });
                    }
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Обновление данных в DataGridView
        /// </summary>
        private void UpdateDataGridView(IEnumerable<Polsovateli> data)
        {
            dataGridView1.DataSource = data.ToList();
        }

        /// <summary>
        /// Обработчик получения фокуса полем поиска
        /// </summary>
        private void TextBoxPoick_Enter(object sender, EventArgs e)
        {
            // Очистка placeholder-текста при получении фокуса
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
            // Восстановление placeholder-текста если поле пустое
            if (string.IsNullOrWhiteSpace(textBoxPoick.Text))
            {
                textBoxPoick.Text = PlaceholderText;
                textBoxPoick.ForeColor = Color.Black;
                dataGridView1.DataSource = polsovatelis;
            }
        }

        /// <summary>
        /// Обработчик ввода символов в поле поиска
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы и пробел
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
                UpdateDataGridView(polsovatelis);
                return;
            }

            var filteredClients = polsovatelis
                .Where(c => c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            UpdateDataGridView(filteredClients);

        }

        /// <summary>
        /// Обработчик кнопки добавления клиента
        /// </summary>
        private void buttonadd_Click(object sender, EventArgs e)
        {
            FormPolsovateliAdd form = new FormPolsovateliAdd();
            form.ShowDialog();
            LoadPolsovateliFromDatabase(); // Обновление списка после закрытия формы
        }

        /// <summary>
        /// Обработчик кнопки редактирования клиента
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Polsovateli selectedClient = (Polsovateli)dataGridView1.SelectedRows[0].DataBoundItem;
                FormPolsovateliEdit form = new FormPolsovateliEdit(selectedClient);
                form.ShowDialog();
                LoadPolsovateliFromDatabase(); // Обновление списка после закрытия формы
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Получение выбранного клиента
        /// </summary>
        private Polsovateli GetSelectedProduct()
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                return dataGridView1.SelectedRows[0].DataBoundItem as Polsovateli;
            }
            return null;
        }

        /// <summary>
        /// Обработчик кнопки удаления клиента
        /// </summary>
        private void buttondelete_Click(object sender, EventArgs e)
        {
            Polsovateli selectedClient = GetSelectedProduct();

            if (selectedClient != null)
            {
                // Подтверждение удаления
                DialogResult result = MessageBox.Show(
                    $"Вы уверены, что хотите удалить клиента '{selectedClient.Name}'?",
                    "Подтверждение удаления",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection connection = new MySqlConnection(con))
                        {
                            connection.Open();

                            // SQL-запрос для удаления
                            string deleteQuery = "DELETE FROM client WHERE ClientId = @ClientId";

                            MySqlCommand command = new MySqlCommand(deleteQuery, connection);
                            command.Parameters.AddWithValue("@ClientId", selectedClient.Id);

                            int rowsAffected = command.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                MessageBox.Show("Клиент успешно удален.", "Успех",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadPolsovateliFromDatabase(); // Обновление списка
                            }
                            else
                            {
                                MessageBox.Show("Ошибка при удалении клиента.", "Ошибка",
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для удаления.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>
    /// Класс для хранения информации о клиенте
    /// </summary>
    public class Polsovateli
    {
        public int Id { get; set; } // ID клиента
        public string Name { get; set; } // Имя
        public string Surname { get; set; } // Фамилия
        public string Patronymic { get; set; } // Отчество
        public string Email { get; set; } // Электронная почта
        public string Address { get; set; } // Адрес
        public string Phone { get; set; } // Телефон
    }
}