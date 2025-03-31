using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления ролями пользователей
    /// </summary>
    public partial class FormRole : Form
    {
        // Строка подключения к базе данных
        private string con = Connection.myConnection;

        // Таблица для хранения ролей
        private DataTable rolesTable = new DataTable();

        // ID текущей выбранной роли
        private int currentRoleId = -1;

        // Флаг первой загрузки данных
        private bool isFirstLoad = true;

        public FormRole()
        {
            InitializeComponent();
            InitializeDataGridView(); // Настройка таблицы ролей
            LoadRoles();             // Загрузка ролей из БД
            textBox1.Clear();        // Очистка поля ввода
        }

        /// <summary>
        /// Инициализация DataGridView для отображения ролей
        /// </summary>
        private void InitializeDataGridView()
        {
            // Настройка основных свойств таблицы
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Columns.Clear();

            // Добавление колонки ID (скрытой)
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RoleId",
                DataPropertyName = "RoleId",
                HeaderText = "ID",
                Visible = false, // Скрываем колонку ID
            });

            // Добавление колонки с названием роли
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RoleName",
                DataPropertyName = "RoleName",
                HeaderText = "Наименование роли",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill // Автоматическое растягивание
            });

            // Подписка на события
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
        }
        private void TextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != 'ё' && e.KeyChar != 'Ё')
            {
                e.Handled = true; // Блокируем ввод
            }
        }
        /// <summary>
        /// Обработчик завершения привязки данных
        /// </summary>
        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            // При первой загрузке снимаем выделение
            if (isFirstLoad)
            {
                isFirstLoad = false;
                if (dataGridView1.Rows.Count > 0)
                {
                    dataGridView1.ClearSelection();
                }
            }
        }

        /// <summary>
        /// Загрузка ролей из базы данных
        /// </summary>
        private void LoadRoles()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    // SQL-запрос для получения ролей (RoleId, RoleName)
                    string query = "SELECT RoleId, RoleName FROM role";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    rolesTable.Clear();
                    adapter.Fill(rolesTable);
                    dataGridView1.DataSource = rolesTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки ролей: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения выделенной строки в таблице
        /// </summary>
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 &&
                dataGridView1.SelectedRows[0].Cells["RoleId"].Value != DBNull.Value)
            {
                // Сохраняем ID выбранной роли и отображаем её название в текстовом поле
                currentRoleId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["RoleId"].Value);
                textBox1.Text = dataGridView1.SelectedRows[0].Cells["RoleName"].Value.ToString();
            }
        }

        /// <summary>
        /// Обработчик кнопки "Добавить" - добавление новой роли
        /// </summary>
        private void buttonadd_Click_1(object sender, EventArgs e)
        {
            // Проверка на пустое поле (RoleName varchar(45))
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите название роли", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины названия роли (не более 45 символов)
            if (textBox1.Text.Length > 45)
            {
                MessageBox.Show("Название роли не должно превышать 45 символов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    // SQL-запрос для добавления новой роли
                    string insertQuery = "INSERT INTO role (RoleName) VALUES (@RoleName)";
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@RoleName", textBox1.Text.Trim());
                    command.ExecuteNonQuery();

                    MessageBox.Show("Роль добавлена успешно", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновление списка ролей
                    LoadRoles();
                    textBox1.Clear();
                    dataGridView1.ClearSelection();
                    currentRoleId = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Редактировать" - обновление выбранной роли
        /// </summary>
        private void buttonredactirovanie_Click(object sender, EventArgs e)
        {
            // Проверка, что выбрана роль для редактирования
            if (currentRoleId == -1)
            {
                MessageBox.Show("Выберите роль для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Проверка на пустое поле (RoleName varchar(45))
            if (string.IsNullOrWhiteSpace(textBox1.Text))
            {
                MessageBox.Show("Введите новое название роли", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка длины названия роли (не более 45 символов)
            if (textBox1.Text.Length > 45)
            {
                MessageBox.Show("Название роли не должно превышать 45 символов", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    // SQL-запрос для обновления роли
                    string updateQuery = "UPDATE role SET RoleName = @RoleName WHERE RoleId = @RoleId";
                    MySqlCommand command = new MySqlCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@RoleName", textBox1.Text.Trim());
                    command.Parameters.AddWithValue("@RoleId", currentRoleId);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Роль обновлена успешно", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновление списка ролей
                    LoadRoles();
                    textBox1.Clear();
                    dataGridView1.ClearSelection();
                    currentRoleId = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления роли: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки "Назад" - закрытие формы
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}