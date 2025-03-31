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

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления заказами в системе "Оптика Каргина"
    /// </summary>
    public partial class FormOrd : Form
    {
        // Строка подключения к базе данных
        string con = Connection.myConnection;

        // Список для хранения заказов
        private List<Order> orders = new List<Order>();

        /// <summary>
        /// Конструктор формы управления заказами
        /// </summary>
        public FormOrd()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик события загрузки формы
        /// </summary>
        private void FormOrd_Load(object sender, EventArgs e)
        {
            LoadOrderFromDatabase();  // Загрузка заказов из БД
            InitializeDataGridView(); // Инициализация DataGridView
        }

        /// <summary>
        /// Настройка параметров DataGridView для отображения заказов
        /// </summary>
        private void InitializeDataGridView()
        {
            // Настройка основных свойств таблицы
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;      // Запрет добавления строк
            dataGridView1.AllowUserToDeleteRows = false;   // Запрет удаления строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выбор всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoGenerateColumns = false;     // Запрет авто-генерации колонок

            // Очистка существующих колонок
            dataGridView1.Columns.Clear();

            // Добавление колонок с привязкой к свойствам Order
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Дата",
                DataPropertyName = "Data",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Статус",
                DataPropertyName = "Status",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Сотрудник",
                DataPropertyName = "UserName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Клиент",
                DataPropertyName = "ClientName",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Установка источника данных
            dataGridView1.DataSource = orders;

            // Центрирование содержимого всех ячеек
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }
        }

        /// <summary>
        /// Загрузка списка заказов из базы данных
        /// </summary>
        private void LoadOrderFromDatabase()
        {
            orders.Clear(); // Очистка текущего списка

            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                // SQL-запрос для получения заказов с информацией о сотрудниках и клиентах
                string query = @"SELECT 
                    o.OrderId,
                    o.OrderData AS Data,
                    o.OrderStatus AS Status,
                    o.OrderUserId AS UserId,
                    o.OrderClientId AS ClientId,
                    CONCAT(u.UserName) AS UserName,
                    CONCAT(c.ClientSurname, ' ', c.ClientName) AS ClientName
                FROM `order` o
                LEFT JOIN user u ON o.OrderUserId = u.UserId
                LEFT JOIN client c ON o.OrderClientId = c.ClientId
                ORDER BY o.OrderData DESC";

                MySqlCommand command = new MySqlCommand(query, connection);
                MySqlDataReader reader = command.ExecuteReader();

                // Чтение результатов запроса и заполнение списка заказов
                while (reader.Read())
                {
                    orders.Add(new Order
                    {
                        OrderId = Convert.ToInt32(reader["OrderId"]),
                        Data = Convert.ToDateTime(reader["Data"]),
                        Status = reader["Status"].ToString(),
                        UserId = Convert.ToInt32(reader["UserId"]),
                        ClientId = Convert.ToInt32(reader["ClientId"]),
                        UserName = reader["UserName"].ToString(),
                        ClientName = reader["ClientName"].ToString()
                    });
                }
                reader.Close();
            }
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Добавить заказ"
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            FormOrder form = new FormOrder(); // Создание формы добавления заказа
            form.Show();                      // Открытие формы
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Назад"
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрытие текущей формы
        }

        /// <summary>
        /// Обработчик нажатия кнопки "Редактировать заказ"
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int selectedIndex = dataGridView1.SelectedRows[0].Index;
                if (selectedIndex >= 0 && selectedIndex < orders.Count)
                {
                    // Получение выбранного заказа
                    var selectedOrder = orders[selectedIndex];

                    // Создание формы редактирования с передачей выбранного заказа
                    FormEditOrder editForm = new FormEditOrder(selectedOrder);

                    // Открытие формы в модальном режиме и проверка результата
                    if (editForm.ShowDialog() == DialogResult.OK)
                    {
                        // Обновление данных после редактирования
                        LoadOrderFromDatabase();
                        dataGridView1.DataSource = null;
                        dataGridView1.DataSource = orders;
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите заказ для редактирования.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    /// <summary>
    /// Класс, представляющий заказ в системе
    /// </summary>
    public class Order
    {
        public int OrderId { get; set; }         // ID заказа
        public DateTime Data { get; set; }      // Дата заказа
        public string Status { get; set; }       // Статус заказа
        public string UserName { get; set; }    // Имя сотрудника
        public string ClientName { get; set; }   // Имя клиента
        public int UserId { get; set; }          // ID сотрудника
        public int ClientId { get; set; }        // ID клиента
    }
}