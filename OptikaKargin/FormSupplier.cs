using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления поставщиками (просмотр, добавление, редактирование)
    /// </summary>
    public partial class FormSupplier : Form
    {
        public FormSupplier()
        {
            InitializeComponent();
        }

        // Строка подключения к базе данных
        string con = Connection.myConnection;

        // Список поставщиков
        private List<Suppliers> suppliers = new List<Suppliers>();

        // Текст-заполнитель для поля поиска
        private const string PlaceholderText = "Поиск";

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик загрузки формы - инициализация данных
        /// </summary>
        private void FormSupplier_Load(object sender, EventArgs e)
        {
            LoadSupplierFromDatabase(); // Загрузка данных из БД

            // Настройка DataGridView
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.AutoGenerateColumns = false;

            dataGridView1.Columns.Clear(); 

            // Настройка поля поиска
            textBoxPoick.Text = PlaceholderText;
            textBoxPoick.ForeColor = Color.Black;
            textBoxPoick.TextChanged += textBoxPoick_TextChanged;
            textBoxPoick.Enter += TextBoxPoick_Enter;
            textBoxPoick.Leave += TextBoxPoick_Leave;

            // Добавление колонок в DataGridView
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Имя",
                DataPropertyName = "Name",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Адрес",
                DataPropertyName = "Address",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "Телефон",
                DataPropertyName = "Phone",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            dataGridView1.DataSource = suppliers;

            // Центрирование содержимого колонок
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            }

            textBoxPoick.KeyPress += TextBoxPoick_KeyPress;
        }

        /// <summary>
        /// Загрузка поставщиков из базы данных
        /// </summary>
        private void LoadSupplierFromDatabase()
        {
            suppliers.Clear();
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                string query = "SELECT * FROM supplier";
                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        // Проверка на NULL значения
                        if (reader["SupplierId"] != DBNull.Value &&
                            reader["SupplierName"] != DBNull.Value &&
                            reader["SupplierAddress"] != DBNull.Value &&
                            reader["SupplierPhone"] != DBNull.Value)
                        {
                            suppliers.Add(new Suppliers
                            {
                                Id = Convert.ToInt32(reader["SupplierId"]),
                                Name = reader["SupplierName"].ToString(),
                                Address = reader["SupplierAddress"].ToString(),
                                Phone = reader["SupplierPhone"].ToString(),
                            });
                        }
                    }
                }
            }
            dataGridView1.DataSource = null; 
            dataGridView1.DataSource = suppliers; 
        }


        /// <summary>
        /// Обновление DataGridView с новыми данными
        /// </summary>
        private void UpdateDataGridView(IEnumerable<Suppliers> data)
        {
            dataGridView1.DataSource = data.ToList();
        }

        /// <summary>
        /// Обработчик изменения текста в поле поиска
        /// </summary>
        private void textBoxPoick_TextChanged(object sender, EventArgs e)
        {
            string searchText = textBoxPoick.Text;

            // Поиск при вводе от 3 символов
            if (searchText.Length >= 3)
            {
                var filteredSuppliers = suppliers.Where(p =>
                    p.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0);
                UpdateDataGridView(filteredSuppliers);
            }
            else
            {
                UpdateDataGridView(suppliers);
            }
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
            }
        }

        /// <summary>
        /// Обработчик нажатия клавиш в поле поиска
        /// </summary>
        private void TextBoxPoick_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только русские буквы, пробелы и управляющие символы
            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != ' ')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - открытие формы добавления поставщика
        /// </summary>
        private void buttonadd_Click(object sender, EventArgs e)
        {
            FormSupplierAdd form = new FormSupplierAdd();
             form.ShowDialog();    
             LoadSupplierFromDatabase();
            
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать" - открытие формы редактирования
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                Suppliers selectedSupplier = (Suppliers)dataGridView1.SelectedRows[0].DataBoundItem;
                FormSupplierEdit form = new FormSupplierEdit(selectedSupplier);
                form.ShowDialog();
                LoadSupplierFromDatabase(); // Обновление данных после закрытия формы
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите поставщика для редактирования.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }

    /// <summary>
    /// Класс для хранения данных о поставщике
    /// </summary>
    public class Suppliers
    {
        public int Id { get; set; }         // ID поставщика
        public string Name { get; set; }    // Название поставщика
        public string Address { get; set; } // Адрес поставщика
        public string Phone { get; set; }   // Телефон поставщика
    }
}