using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для редактирования существующих заказов
    /// </summary>
    public partial class FormEditOrder : Form
    {
        // Строка подключения к базе данных
        public string con = Connection.myConnection;

        // Текущий редактируемый заказ
        private Order order;

        /// <summary>
        /// Класс для элементов ComboBox пользователей
        /// </summary>
        public class UserComboItem
        {
            public int Id { get; set; } // ID пользователя
            public string Name { get; set; } // Имя пользователя
            public string Surname { get; set; } // Фамилия пользователя
            // Отображение в ComboBox
            public override string ToString() => $"{Name}";
        }

        /// <summary>
        /// Класс для элементов ComboBox клиентов
        /// </summary>
        public class ClientComboItem
        {
            public int Id { get; set; } // ID клиента
            public string Name { get; set; } // Имя клиента
            public string Surname { get; set; } // Фамилия клиента
            // Отображение в ComboBox
            public override string ToString() => $"{Surname} {Name}";
        }

        /// <summary>
        /// Конструктор формы редактирования заказа
        /// </summary>
        /// <param name="order">Заказ для редактирования</param>
        public FormEditOrder(Order order)
        {
            InitializeComponent();
            this.order = order; // Сохраняем переданный заказ

            // Настройка DataGridView
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false; // Запрещаем добавление строк
            dataGridView1.AllowUserToDeleteRows = false; // Запрещаем удаление строк
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Выделение всей строки
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false; // Отключаем авто-генерацию колонок
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Автоматическое заполнение ширины

            InitializeForm(); // Инициализация формы
            dateTimePicker1.MaxDate = DateTime.Today;
        }

        /// <summary>
        /// Инициализация формы
        /// </summary>
        private void InitializeForm()
        {
            try
            {
                LoadUsers(); // Загрузка списка пользователей
                LoadClients(); // Загрузка списка клиентов

                // Добавляем стандартные статусы заказа
                comboBox2.Items.AddRange(new object[] { "Принят", "Отклонен" });
                // Устанавливаем дату заказа
                dateTimePicker1.Value = order.Data;

                // Устанавливаем выбранные значения
                SetSelectedUser();
                SetSelectedClient();
                SetOrderStatus();

                // Инициализируем таблицу товаров
                InitializeDataGridView();
                // Загружаем товары заказа
                LoadOrderProducts();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при инициализации формы: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Установка выбранного пользователя в ComboBox
        /// </summary>
        private void SetSelectedUser()
        {
            foreach (UserComboItem item in comboBoxUsers.Items)
            {
                if (item.Id == order.UserId)
                {
                    comboBoxUsers.SelectedItem = item;
                    return;
                }
            }
            MessageBox.Show("Сотрудник не найден в списке.", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Установка выбранного клиента в ComboBox
        /// </summary>
        private void SetSelectedClient()
        {
            foreach (ClientComboItem item in comboBox1.Items)
            {
                if (item.Id == order.ClientId)
                {
                    comboBox1.SelectedItem = item;
                    return;
                }
            }
            MessageBox.Show("Клиент не найден в списке.", "Ошибка",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        /// <summary>
        /// Установка статуса заказа
        /// </summary>
        private void SetOrderStatus()
        {
            // Если статуса нет в списке, добавляем его
            if (!comboBox2.Items.Contains(order.Status))
            {
                comboBox2.Items.Add(order.Status);
                MessageBox.Show("Статус заказа был добавлен в список.", "Информация",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            // Устанавливаем текущий статус
            comboBox2.SelectedItem = order.Status;
        }

        /// <summary>
        /// Инициализация DataGridView
        /// </summary>
        private void InitializeDataGridView()
        {
            // Настройка внешнего вида таблицы
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.ReadOnly = true; // Только для чтения
            dataGridView1.RowHeadersVisible = false; // Скрываем заголовки строк
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Очищаем таблицу
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();

            // Добавляем колонки
            dataGridView1.Columns.Add("Name", "Название товара");
            dataGridView1.Columns.Add("Quantity", "Количество");
            dataGridView1.Columns.Add("Price", "Цена за единицу");
            dataGridView1.Columns.Add("Discount", "Скидка (%)");
            dataGridView1.Columns.Add("TotalPrice", "Общая стоимость");

            // Настраиваем форматирование колонок
            dataGridView1.Columns["Quantity"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Price"].DefaultCellStyle.Format = "C2"; // Формат валюты
            dataGridView1.Columns["Price"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["Discount"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            dataGridView1.Columns["TotalPrice"].DefaultCellStyle.Format = "C2";
            dataGridView1.Columns["TotalPrice"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // Добавляем кнопки действий
            DataGridViewButtonColumn addToCartButton = new DataGridViewButtonColumn
            {
                HeaderText = "Добавить",
                Text = "Добавить",
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(addToCartButton);

            DataGridViewButtonColumn delToCartButton = new DataGridViewButtonColumn
            {
                HeaderText = "Удалить",
                Text = "Удалить",
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(delToCartButton);

            // Подписываемся на событие клика по ячейке
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
        }

        /// <summary>
        /// Загрузка списка пользователей из БД
        /// </summary>
        private void LoadUsers()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    string query = "SELECT UserId, UserName FROM user";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBoxUsers.Items.Clear();
                        while (reader.Read())
                        {
                            comboBoxUsers.Items.Add(new UserComboItem
                            {
                                Id = reader.GetInt32("UserId"),
                                Name = reader.GetString("UserName"),
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загрузка списка клиентов из БД
        /// </summary>
        private void LoadClients()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    string query = "SELECT ClientId, ClientName, ClientSurname FROM client";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        comboBox1.Items.Clear();
                        while (reader.Read())
                        {
                            comboBox1.Items.Add(new ClientComboItem
                            {
                                Id = reader.GetInt32("ClientId"),
                                Name = reader.GetString("ClientName"),
                                Surname = reader.GetString("ClientSurname")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Загрузка товаров заказа из БД
        /// </summary>
        private void LoadOrderProducts()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();
                    string query = @"
                        SELECT 
                            p.ProductName AS Name,
                            oi.OrderItemQuentity AS Quantity,
                            p.ProductPrice AS Price,
                            p.ProductDiscount AS Discount,
                            oi.OrderItemTotalPrice AS TotalPrice
                        FROM 
                            orderitem oi
                        JOIN 
                            product p ON oi.OrderItemProductArticle = p.ProductArticle
                        WHERE 
                            oi.OrderItemOrderId = @OrderId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@OrderId", order.OrderId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Добавляем строки в таблицу
                                dataGridView1.Rows.Add(
                                    reader["Name"].ToString(),
                                    reader.GetInt32("Quantity"),
                                    reader.GetDecimal("Price"),
                                    reader.GetInt32("Discount"),
                                    reader.GetDecimal("TotalPrice")
                                );
                            }
                        }
                    }
                }
                // Обновляем общую стоимость
                UpdateTotalCost();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке товаров заказа: {ex.Message}",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Обновление отображения общей стоимости заказа
        /// </summary>
        private void UpdateTotalCost()
        {
            decimal totalCost = 0;
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.Cells["TotalPrice"].Value != null &&
                    decimal.TryParse(row.Cells["TotalPrice"].Value.ToString(), out decimal rowTotal))
                {
                    totalCost += rowTotal;
                }
            }
            // Форматируем как валюту
            label3.Text = $"Общая стоимость: {totalCost:C}";
        }

        /// <summary>
        /// Обработчик клика по ячейкам DataGridView
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Проверяем, что клик был по ячейке с данными
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var column = dataGridView1.Columns[e.ColumnIndex];

                // Проверяем, что клик был по кнопке
                if (column is DataGridViewButtonColumn)
                {
                    var row = dataGridView1.Rows[e.RowIndex];
                    string productName = row.Cells["Name"].Value.ToString();
                    int quantity = Convert.ToInt32(row.Cells["Quantity"].Value);

                    // Получаем данные о товаре
                    decimal price = GetProductPrice(productName);
                    int discount = GetProductDiscount(productName);
                    // Рассчитываем стоимость с учетом скидки
                    decimal discountedCostPerUnit = CalculateDiscountedCost(price, discount);

                    if (column.HeaderText == "Добавить")
                    {
                        // Проверяем доступное количество на складе
                        int availableQuantity = GetAvailableQuantity(productName);
                        if (quantity < availableQuantity)
                        {
                            // Увеличиваем количество
                            quantity++;
                            row.Cells["Quantity"].Value = quantity;
                            // Пересчитываем общую стоимость
                            row.Cells["TotalPrice"].Value = discountedCostPerUnit * quantity;
                        }
                        else
                        {
                            MessageBox.Show($"Не хватает товаров на складе. Доступное количество: {availableQuantity}",
                                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else if (column.HeaderText == "Удалить")
                    {
                        if (quantity > 1)
                        {
                            // Уменьшаем количество
                            quantity--;
                            row.Cells["Quantity"].Value = quantity;
                            // Пересчитываем общую стоимость
                            row.Cells["TotalPrice"].Value = discountedCostPerUnit * quantity;
                        }
                        else
                        {
                            // Удаляем товар из заказа
                            dataGridView1.Rows.Remove(row);
                        }
                    }

                    // Обновляем общую стоимость
                    UpdateTotalCost();
                }
            }
        }

        /// <summary>
        /// Расчет стоимости с учетом скидки
        /// </summary>
        private decimal CalculateDiscountedCost(decimal cost, int discount)
        {
            return cost * (100 - discount) / 100;
        }

        /// <summary>
        /// Обработчик кнопки сохранения изменений
        /// </summary>
        private void button1_Click_1(object sender, EventArgs e)
        {
            // Проверяем, есть ли товары в заказе
            if (dataGridView1.Rows.Count == 0)
            {
                MessageBox.Show("Корзина пуста. Изменение невозможно.", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Проверяем заполнение обязательных полей
            StringBuilder errors = new StringBuilder();
            if (comboBoxUsers.SelectedItem == null) errors.AppendLine("Не выбран пользователь!");
            if (comboBox1.SelectedItem == null) errors.AppendLine("Не выбран клиент!");
            if (comboBox2.SelectedItem == null) errors.AppendLine("Не выбран статус заказа!");

            if (errors.Length > 0)
            {
                MessageBox.Show($"Ошибка изменении заказа:\n{errors}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Запрашиваем подтверждение
            if (MessageBox.Show("Вы уверены, что хотите изменить заказ?", "Подтверждение",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
            {
                return;
            }

            // Сохраняем изменения в БД
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                try
                {
                    // Обновляем данные заказа
                    MySqlCommand command = new MySqlCommand(@"
                        UPDATE `order`
                        SET OrderData = @OrderData, OrderUserId = @OrderUserId, 
                            OrderClientId = @OrderClientId, OrderStatus = @OrderStatus
                        WHERE OrderId = @OrderId", connection);

                    command.Parameters.AddWithValue("@OrderData", dateTimePicker1.Value);
                    command.Parameters.AddWithValue("@OrderUserId", ((UserComboItem)comboBoxUsers.SelectedItem).Id);
                    command.Parameters.AddWithValue("@OrderClientId", ((ClientComboItem)comboBox1.SelectedItem).Id);
                    command.Parameters.AddWithValue("@OrderStatus", comboBox2.SelectedItem.ToString());
                    command.Parameters.AddWithValue("@OrderId", order.OrderId);
                    command.ExecuteNonQuery();

                    // Удаляем старые товары заказа
                    command = new MySqlCommand("DELETE FROM orderitem WHERE OrderItemOrderId = @OrderId", connection);
                    command.Parameters.AddWithValue("@OrderId", order.OrderId);
                    command.ExecuteNonQuery();

                    // Добавляем новые товары заказа
                    foreach (DataGridViewRow row in dataGridView1.Rows)
                    {
                        string productName = row.Cells["Name"].Value?.ToString();
                        command = new MySqlCommand(@"
                            INSERT INTO orderitem (OrderItemQuentity, OrderItemTotalPrice, OrderItemOrderId, OrderItemProductArticle)
                            VALUES (@Quantity, @TotalPrice, @OrderId, @ProductArticle)", connection);

                        command.Parameters.AddWithValue("@Quantity", Convert.ToInt32(row.Cells["Quantity"].Value));
                        command.Parameters.AddWithValue("@TotalPrice", Convert.ToDecimal(row.Cells["TotalPrice"].Value));
                        command.Parameters.AddWithValue("@OrderId", order.OrderId);
                        command.Parameters.AddWithValue("@ProductArticle", GetProductArticle(productName));
                        command.ExecuteNonQuery();

                        // Обновляем остатки на складе
                        command = new MySqlCommand(@"
                            UPDATE product SET ProductStock = ProductStock - @QuantityChange
                            WHERE ProductArticle = @ProductArticle", connection);

                        command.Parameters.AddWithValue("@QuantityChange", Convert.ToInt32(row.Cells["Quantity"].Value));
                        command.Parameters.AddWithValue("@ProductArticle", GetProductArticle(productName));
                        command.ExecuteNonQuery();
                    }

                    // Устанавливаем результат и закрываем форму
                    this.DialogResult = DialogResult.OK;
                    MessageBox.Show("Заказ успешно обновлен!", "Успех",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при обновлении заказа: {ex.Message}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// Получение артикула товара по названию
        /// </summary>
        private string GetProductArticle(string productName)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT ProductArticle FROM product WHERE ProductName = @ProductName",
                    connection);
                command.Parameters.AddWithValue("@ProductName", productName);
                return command.ExecuteScalar()?.ToString();
            }
        }

        /// <summary>
        /// Получение цены товара по названию
        /// </summary>
        private decimal GetProductPrice(string productName)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT ProductPrice FROM product WHERE ProductName = @ProductName",
                    connection);
                command.Parameters.AddWithValue("@ProductName", productName);
                return Convert.ToDecimal(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Получение скидки на товар по названию
        /// </summary>
        private int GetProductDiscount(string productName)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT ProductDiscount FROM product WHERE ProductName = @ProductName",
                    connection);
                command.Parameters.AddWithValue("@ProductName", productName);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Получение доступного количества товара на складе
        /// </summary>
        private int GetAvailableQuantity(string productName)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(
                    "SELECT ProductStock FROM product WHERE ProductName = @ProductName",
                    connection);
                command.Parameters.AddWithValue("@ProductName", productName);
                return Convert.ToInt32(command.ExecuteScalar());
            }
        }

        /// <summary>
        /// Обработчик кнопки отмены
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            this.Close(); // Закрываем форму без сохранения
        }
    }
}