using MySql.Data.MySqlClient;
using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для управления категориями товаров
    /// </summary>
    public partial class FormCategory : Form
    {
        private string con = Connection.myConnection; // Строка подключения к БД
        private DataTable categoriesTable = new DataTable(); // Таблица для хранения категорий
        private int currentCategoryId = -1; // ID текущей выбранной категории
        private bool isFirstLoad = true; // Флаг первой загрузки данных

        public FormCategory()
        {
            InitializeComponent();
            InitializeDataGridView();
            LoadCategories();
            textBoxCategoryName.Clear();

            // Настройка ограничений для поля ввода названия категории
            textBoxCategoryName.MaxLength = 45; // Максимальная длина названия
            textBoxCategoryName.KeyPress += TextBoxCategoryName_KeyPress; // Обработчик ввода
        }

        /// <summary>
        /// Инициализация DataGridView для отображения категорий
        /// </summary>
        private void InitializeDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.Columns.Clear();

            // Колонка для ID категории (скрытая)
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryId",
                DataPropertyName = "CategoryId",
                HeaderText = "ID",
                Visible = false // Скрываем ID от пользователя
            });

            // Колонка для названия категории
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "CategoryName",
                DataPropertyName = "CategoryName",
                HeaderText = "Наименование категории",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            });

            // Подписка на события
            dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
            dataGridView1.DataBindingComplete += DataGridView1_DataBindingComplete;
        }

        /// <summary>
        /// Обработчик события завершения привязки данных
        /// </summary>
        private void DataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (isFirstLoad && dataGridView1.Rows.Count > 0)
            {
                isFirstLoad = false;
                dataGridView1.ClearSelection(); // Снимаем выделение при первой загрузке
            }
        }

        /// <summary>
        /// Загрузка категорий из базы данных
        /// </summary>
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    string query = "SELECT CategoryId, CategoryName FROM category";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    MySqlDataAdapter adapter = new MySqlDataAdapter(command);

                    categoriesTable.Clear();
                    adapter.Fill(categoriesTable);
                    dataGridView1.DataSource = categoriesTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик изменения выделенной строки в DataGridView
        /// </summary>
        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0 &&
                dataGridView1.SelectedRows[0].Cells["CategoryId"].Value != DBNull.Value)
            {
                currentCategoryId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["CategoryId"].Value);
                textBoxCategoryName.Text = dataGridView1.SelectedRows[0].Cells["CategoryName"].Value.ToString();
            }
        }

        /// <summary>
        /// Обработчик кнопки добавления новой категории
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка на пустое название
            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
            {
                MessageBox.Show("Введите название категории", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка на допустимые символы
            if (!Regex.IsMatch(textBoxCategoryName.Text, @"^[а-яА-ЯёЁa]+$"))
            {
                MessageBox.Show("Название может содержать только буквы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();

                    // Проверка на существование категории с таким же именем
                    string checkQuery = "SELECT COUNT(*) FROM category WHERE CategoryName = @CategoryName";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@CategoryName", textBoxCategoryName.Text.Trim());

                    if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Добавление новой категории
                    string insertQuery = "INSERT INTO category (CategoryName) VALUES (@CategoryName)";
                    MySqlCommand command = new MySqlCommand(insertQuery, connection);
                    command.Parameters.AddWithValue("@CategoryName", textBoxCategoryName.Text.Trim());
                    command.ExecuteNonQuery();

                    MessageBox.Show("Категория добавлена успешно", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновление данных и очистка полей
                    LoadCategories();
                    textBoxCategoryName.Clear();
                    dataGridView1.ClearSelection();
                    currentCategoryId = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления категории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки редактирования категории
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            if (currentCategoryId == -1)
            {
                MessageBox.Show("Выберите категорию для редактирования", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrWhiteSpace(textBoxCategoryName.Text))
            {
                MessageBox.Show("Введите новое название категории", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверка на допустимые символы
            if (!Regex.IsMatch(textBoxCategoryName.Text, @"^[a-zA-Zа-яА-Я]+$"))
            {
                MessageBox.Show("Название может содержать только буквы",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();

                    // Проверка на существование другой категории с таким же именем
                    string checkQuery = "SELECT COUNT(*) FROM category WHERE CategoryName = @CategoryName AND CategoryId != @CategoryId";
                    MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection);
                    checkCommand.Parameters.AddWithValue("@CategoryName", textBoxCategoryName.Text.Trim());
                    checkCommand.Parameters.AddWithValue("@CategoryId", currentCategoryId);

                    if (Convert.ToInt32(checkCommand.ExecuteScalar()) > 0)
                    {
                        MessageBox.Show("Категория с таким названием уже существует", "Ошибка",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Обновление категории
                    string updateQuery = "UPDATE category SET CategoryName = @CategoryName WHERE CategoryId = @CategoryId";
                    MySqlCommand command = new MySqlCommand(updateQuery, connection);
                    command.Parameters.AddWithValue("@CategoryName", textBoxCategoryName.Text.Trim());
                    command.Parameters.AddWithValue("@CategoryId", currentCategoryId);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Категория обновлена успешно", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Обновление данных и очистка полей
                    LoadCategories();
                    textBoxCategoryName.Clear();
                    dataGridView1.ClearSelection();
                    currentCategoryId = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка обновления категории: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обработчик кнопки закрытия формы
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Обработчик ввода символов в поле названия категории
        /// </summary>
        private void TextBoxCategoryName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !(e.KeyChar >= 'а' && e.KeyChar <= 'я') &&
                !(e.KeyChar >= 'А' && e.KeyChar <= 'Я') &&
                e.KeyChar != 'ё' && e.KeyChar != 'Ё')
            {
                e.Handled = true; // Блокируем ввод
            }
        }
    }
    
}