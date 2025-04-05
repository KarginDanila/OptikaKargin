using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для работы с клиентами (просмотр, поиск, добавление, редактирование)
    /// </summary>
    public partial class FormClient : Form
    {
        public FormClient()
        {
            InitializeComponent();
        }

        private string con = Connection.myConnection; // Строка подключения к БД
        private List<Client> clients = new List<Client>(); // Список клиентов
        private const string PlaceholderText = "Поиск"; // Текст-заполнитель для поля поиска

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик загрузки формы
        /// </summary>
        private void FormClient_Load(object sender, EventArgs e)
        {
            // Загрузка данных о клиентах из БД
            LoadClientFromDatabase();

            // Настройка DataGridView
            dataGridView1.AllowUserToAddRows = false; // Запрет добавления строк
            dataGridView1.AllowUserToDeleteRows = false; // Запрет удаления строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize; // Автоподбор высоты заголовков
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоподбор ширины столбцов
            dataGridView1.AutoGenerateColumns = false; // Запрет автосоздания столбцов
            dataGridView1.ReadOnly = true; // Только для чтения
            dataGridView1.Columns.Clear();

            // Настройка поля поиска
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;
            textBoxPoick.TextChanged += textBoxPoick_TextChanged; // Подписка на изменение текста
            textBoxPoick.Enter += TextBoxPoick_Enter; // Подписка на событие получения фокуса
            textBoxPoick.Leave += TextBoxPoick_Leave; // Подписка на событие потери фокуса

            // Добавление столбцов в DataGridView
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Имя",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Фамилия",
                DataPropertyName = "Surname",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Отчество",
                DataPropertyName = "Patronymic",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Почта",
                DataPropertyName = "Email",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Адрес",
                DataPropertyName = "Address",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Телефон",
                DataPropertyName = "Phone",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Привязка данных
            dataGridView1.DataSource = clients;

            // Центрирование текста в ячейках
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            // Ограничение ввода в поле поиска
            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;
        }

        /// <summary>
        /// Загрузка клиентов из базы данных
        /// </summary>
        private void LoadClientFromDatabase()
        {
            clients.Clear();

            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
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
                    if (reader["Id"] != DBNull.Value &&
                        reader["Name"] != DBNull.Value &&
                        reader["Surname"] != DBNull.Value &&
                        reader["Patronymic"] != DBNull.Value &&
                        reader["Email"] != DBNull.Value &&
                        reader["Address"] != DBNull.Value &&
                        reader["Phone"] != DBNull.Value)
                    {
                        string originalName = reader["Name"].ToString();
                        string originalSurname = reader["Surname"].ToString();
                        string originalPatronymic = reader["Patronymic"].ToString();
                        string originalEmail = reader["Email"].ToString();
                        string originalAddress = reader["Address"].ToString();
                        string originalPhone = reader["Phone"].ToString();

                        string name = originalName;
                        string surname = originalSurname;
                        string patronymic = originalPatronymic;
                        string email = originalEmail;
                        string address = originalAddress;
                        string phone = originalPhone;

                        if (name.Length > 2)
                            name = name.Substring(0, name.Length - 2) + new string('*', 2);
                        if (surname.Length > 3)
                            surname = surname.Substring(0, surname.Length - 3) + new string('*', 3);
                        if (patronymic.Length > 3)
                            patronymic = patronymic.Substring(0, patronymic.Length - 3) + new string('*', 3);
                        if (phone.Length > 5)
                            phone = phone.Substring(0, phone.Length - 5) + new string('*', 5);
                        if (address.Length > 5)
                            address = address.Substring(0, address.Length - 5) + new string('*', 5);

                        clients.Add(new Client
                        {
                            Id = Convert.ToInt32(reader["Id"]),
                            Name = name,               
                            Surname = surname,         
                            Patronymic = patronymic,    
                            Email = email,              
                            Address = address,         
                            Phone = phone,             
                            OriginalName = originalName,
                            OriginalSurname = originalSurname,
                            OriginalPatronymic = originalPatronymic,
                            OriginalEmail = originalEmail,
                            OriginalAddress = originalAddress,
                            OriginalPhone = originalPhone
                        });
                    }
                }
            }
            dataGridView1.DataSource = null;
            dataGridView1.DataSource = clients;
        }

        /// <summary>
        /// Обновление данных в DataGridView
        /// </summary>
        private void UpdateDataGridView(IEnumerable<Client> data)
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
                dataGridView1.DataSource = clients;
            }
        }

        /// <summary>
        /// Обработчик ввода символов в поле поиска
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы, пробелы и управляющие символы
            if (!char.IsControl(e.KeyChar) && // Управляющие символы (Backspace и т.д.)
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') && // Русские строчные
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') && // Русские заглавные
                e.KeyChar != ' ') // Пробел
            {
                e.Handled = true; // Блокируем ввод
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
                UpdateDataGridView(clients);
                return;
            }

            var filteredClients = clients
                .Where(c => c.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                .ToList();

            UpdateDataGridView(filteredClients);
        }

        /// <summary>
        /// Обработчик кнопки добавления нового клиента
        /// </summary>
        private void buttonadd_Click(object sender, EventArgs e)
        {
            FormClientAdd form = new FormClientAdd();
            form.ShowDialog(); // Открываем форму добавления в модальном режиме
            LoadClientFromDatabase();
        }

        /// <summary>
        /// Обработчик кнопки редактирования клиента
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Получаем выбранного клиента
                Client selectedProduct = (Client)dataGridView1.SelectedRows[0].DataBoundItem;
                FormClientEdit form = new FormClientEdit(selectedProduct);
                form.ShowDialog(); // Открываем форму редактирования
                LoadClientFromDatabase(); // Обновляем список после закрытия формы
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите клиента для редактирования.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Client selectedClient = (Client)dataGridView1.SelectedRows[0].DataBoundItem;
                FormClientDetails detailsForm = new FormClientDetails(selectedClient);
                detailsForm.ShowDialog();
            }
        }
    }

    /// <summary>
    /// Класс для хранения данных о клиенте
    /// </summary>
    public class Client
    {
        public int Id { get; set; } // ID клиента
        public string Name { get; set; } // Имя
        public string Surname { get; set; } // Фамилия
        public string Patronymic { get; set; } // Отчество
        public string Email { get; set; } // Электронная почта
        public string Address { get; set; } // Адрес
        public string Phone { get; set; } // Телефон
        public string OriginalName { get; set; }
        public string OriginalSurname { get; set; }
        public string OriginalPatronymic { get; set; }
        public string OriginalEmail { get; set; }
        public string OriginalAddress { get; set; }
        public string OriginalPhone { get; set; }
    }

}