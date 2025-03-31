using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.Remoting.Contexts;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Menu;
using System.Text;
using Microsoft.Office.Interop.Word;
using System.IO;
using System.Linq;
using MySqlX.XDevAPI;
using Application = Microsoft.Office.Interop.Word.Application;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для оформления покупки/заказа в системе "Оптика Каргина"
    /// </summary>
    public partial class FormOrderbuy : Form
    {
        // Строка подключения к базе данных
        string con = Connection.myConnection;

        // Список товаров в корзине с привязкой данных
        private BindingList<Product> cartItems;

        // Родительская форма заказа (для возврата)
        private FormOrder parentForm;

        /// <summary>
        /// Конструктор формы оформления заказа
        /// </summary>
        /// <param name="items">Список товаров для заказа</param>
        /// <param name="parentForm">Родительская форма</param>
        public FormOrderbuy(List<Product> items, FormOrder parentForm)
        {
            InitializeComponent();
            cartItems = new BindingList<Product>(items); // Инициализация корзины
            DisplayCartItems(); // Отображение товаров
            UpdateTotalCost(); // Расчет общей стоимости
            this.parentForm = parentForm; // Сохранение ссылки на родительскую форму
            dateTimePicker1.MaxDate = DateTime.Today;//Устанавливаем максимальную дату - сегодня
        }


        /// <summary>
        /// Отображение товаров в корзине в DataGridView
        /// </summary>
        private void DisplayCartItems()
        {
            // Настройка внешнего вида таблицы
            dataGridView1.Dock = DockStyle.None;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView1.AutoGenerateColumns = false;

            // Добавление колонок, если они еще не добавлены
            if (dataGridView1.Columns.Count == 0)
            {
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Артикул",
                    DataPropertyName = "Article",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Название",
                    DataPropertyName = "Name",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Цена",
                    DataPropertyName = "Price",
                    AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
                });
                // Колонка количества с возможностью редактирования
                var quantityColumn = new DataGridViewTextBoxColumn
                {
                    HeaderText = "Количество",
                    DataPropertyName = "Quantity"
                };
                quantityColumn.ReadOnly = false;
                dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
                {
                    HeaderText = "Цена со скидкой",
                    DataPropertyName = "DiscountedCost"
                });
                dataGridView1.Columns.Add(quantityColumn);
            }

            // Загрузка пользователей и клиентов
            LoadUsers();
            LoadClient();

            // Добавление кнопки "Добавить"
            DataGridViewButtonColumn addToCartButton = new DataGridViewButtonColumn
            {
                HeaderText = "Добавить",
                Text = "Добавить",
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(addToCartButton);

            // Добавление кнопки "Удалить"
            DataGridViewButtonColumn delToCartButton = new DataGridViewButtonColumn
            {
                HeaderText = "Удалить",
                Text = "Удалить",
                UseColumnTextForButtonValue = true
            };
            dataGridView1.Columns.Add(delToCartButton);

            // Добавление статусов заказа
            comboBoxStatus.Items.Add("Принят");
            comboBoxStatus.Items.Add("Отклонен");

            // Привязка данных и подписка на события
            dataGridView1.DataSource = cartItems;
            dataGridView1.CellContentClick += dataGridView1_CellContentClick;
            dataGridView1.CellValidating += DataGridView1_CellValidating;
        }

        /// <summary>
        /// Валидация ввода количества товара
        /// </summary>
        private void DataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            // Проверяем только колонку с количеством (индекс 3)
            if (e.ColumnIndex == 3)
            {
                if (int.TryParse(e.FormattedValue.ToString(), out int newQuantity))
                {
                    // Получаем доступное количество товара
                    int availableQuantity = GetAvailable(cartItems[e.RowIndex].Article);

                    if (newQuantity < 1)
                    {
                        MessageBox.Show("Количество должно быть больше 0.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                    else if (newQuantity > availableQuantity)
                    {
                        MessageBox.Show($"Максимально доступное количество для {cartItems[e.RowIndex].Name}: {availableQuantity}.",
                            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        e.Cancel = true;
                    }
                    else
                    {
                        // Обновляем количество и пересчитываем сумму
                        cartItems[e.RowIndex].Quantity = newQuantity;
                        UpdateTotalCost();
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное число для количества.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Получение доступного количества товара на складе
        /// </summary>
        /// <param name="productArticle">Артикул товара</param>
        /// <returns>Доступное количество</returns>
        private int GetAvailable(string productArticle)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                string query = "SELECT ProductStock FROM product WHERE ProductArticle = @ProductArticle";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductArticle", productArticle);

                var result = command.ExecuteScalar();
                if (result != null && int.TryParse(result.ToString(), out int stock))
                {
                    return stock;
                }
                return 0;
            }
        }

        /// <summary>
        /// Обновление отображения общей стоимости заказа
        /// </summary>
        private void UpdateTotalCost()
        {
            decimal totalCost = cartItems.Sum(p => p.DiscountedCost * p.Quantity);
            label1.Text = $"Общая стоимость: {totalCost:C}";
        }

        /// <summary>
        /// Обработчик кнопки возврата к форме заказов
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            FormOrder form = new FormOrder();
            form.Show();
            Hide();
        }

        /// <summary>
        /// Загрузка списка пользователей в выпадающий список
        /// </summary>
        private void LoadUsers()
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT UserName FROM user", connection);
                MySqlDataReader reader = command.ExecuteReader();

                comboBoxUsers.Items.Clear();

                while (reader.Read())
                {
                    string fullName = $"{reader["UserName"]}";
                    comboBoxUsers.Items.Add(fullName);
                }

                reader.Close();
            }

            comboBoxUsers.SelectedIndex = 0;
        }

        /// <summary>
        /// Загрузка списка клиентов в выпадающий список
        /// </summary>
        private void LoadClient()
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand("SELECT ClientSurname, ClientName FROM client", connection);
                MySqlDataReader reader = command.ExecuteReader();

                comboBoxClient.Items.Clear();

                while (reader.Read())
                {
                    string fullName = $"{reader["ClientSurname"]} {reader["ClientName"]}";
                    comboBoxClient.Items.Add(fullName);
                }

                reader.Close();
            }

            comboBoxClient.SelectedIndex = 0;
        }

        /// <summary>
        /// Обработчик оформления заказа
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            // Проверка заполнения обязательных полей
            StringBuilder errors = new StringBuilder();

            if (comboBoxUsers.SelectedItem == null)
                errors.AppendLine("Не выбран пользователь!");
            if (comboBoxClient.SelectedItem == null)
                errors.AppendLine("Не выбран клиент!");
            if (comboBoxStatus.SelectedItem == null)
                errors.AppendLine("Не выбран статус заказа!");
            if (cartItems.Count == 0)
                errors.AppendLine("Корзина пуста - добавьте товары!");

            if (errors.Length > 0)
            {
                MessageBox.Show($"Ошибка оформления заказа:\n{errors.ToString()}",
                              "Ошибка",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return;
            }

            // Проверка доступности товаров
            foreach (var product in cartItems)
            {
                int available = GetAvailableQuantity(product.Article);
                if (product.Quantity > available)
                {
                    MessageBox.Show($"Товар {product.Name} (арт. {product.Article})\n" +
                                  $"Доступно: {available}, заказано: {product.Quantity}",
                                  "Ошибка количества",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return;
                }
            }

            // Подтверждение заказа
            DialogResult confirmResult = MessageBox.Show("Вы уверены, что хотите оформить заказ?",
                "Подтверждение", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmResult == DialogResult.No)
            {
                return;
            }

            // Сохранение заказа в БД
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();

                string selectedUser = comboBoxUsers.SelectedItem.ToString();
                string selectedClient = comboBoxClient.SelectedItem.ToString();
                string selectedStatus = comboBoxStatus.SelectedItem.ToString();
                DateTime orderDate = dateTimePicker1.Value;

                // Получение ID пользователя и клиента
                int userId = GetUserId(selectedUser, connection);
                int clientId = GetClientId(selectedClient, connection);

                // Вставка основного заказа
                string insertOrderQuery = @"
            INSERT INTO `order` (OrderData, OrderStatus, OrderUserId, OrderClientId)
            VALUES (@OrderData, @OrderStatus, @OrderUserId, @OrderClientId);
            SELECT LAST_INSERT_ID();";

                MySqlCommand orderCommand = new MySqlCommand(insertOrderQuery, connection);
                orderCommand.Parameters.AddWithValue("@OrderData", orderDate);
                orderCommand.Parameters.AddWithValue("@OrderUserId", userId);
                orderCommand.Parameters.AddWithValue("@OrderClientId", clientId);
                orderCommand.Parameters.AddWithValue("@OrderStatus", selectedStatus);

                // Получение ID нового заказа
                int orderId = Convert.ToInt32(orderCommand.ExecuteScalar());

                // Добавление товаров заказа
                foreach (var product in cartItems)
                {
                    // Проверка существования товара
                    string checkProductQuery = "SELECT COUNT(*) FROM product WHERE ProductArticle = @ProductArticle";
                    MySqlCommand checkProductCommand = new MySqlCommand(checkProductQuery, connection);
                    checkProductCommand.Parameters.AddWithValue("@ProductArticle", product.Article);

                    int productExists = Convert.ToInt32(checkProductCommand.ExecuteScalar());

                    if (productExists == 0)
                    {
                        MessageBox.Show($"Продукт с артикулом {product.Article} не найден. Заказ не может быть завершен.");
                        continue;
                    }

                    // Вставка позиции заказа
                    string insertOrderProductQuery = @"
                INSERT INTO orderitem (OrderItemOrderId, OrderItemProductArticle, OrderItemQuentity, OrderItemTotalPrice)
                VALUES (@OrderItemOrderId, @OrderItemProductArticle, @OrderItemQuentity, @OrderItemTotalPrice);";

                    MySqlCommand orderProductCommand = new MySqlCommand(insertOrderProductQuery, connection);
                    orderProductCommand.Parameters.AddWithValue("@OrderItemOrderId", orderId);
                    orderProductCommand.Parameters.AddWithValue("@OrderItemProductArticle", product.Article);
                    orderProductCommand.Parameters.AddWithValue("@OrderItemQuentity", product.Quantity);
                    orderProductCommand.Parameters.AddWithValue("@OrderItemTotalPrice", product.Price * product.Quantity);

                    try
                    {
                        orderProductCommand.ExecuteNonQuery();

                        // Обновление остатков на складе
                        string updateProductQuantityQuery = @"
                    UPDATE product 
                    SET ProductStock = ProductStock - @Quantity 
                    WHERE ProductArticle = @ProductArticle;";

                        MySqlCommand updateProductCommand = new MySqlCommand(updateProductQuantityQuery, connection);
                        updateProductCommand.Parameters.AddWithValue("@Quantity", product.Quantity);
                        updateProductCommand.Parameters.AddWithValue("@ProductArticle", product.Article);

                        updateProductCommand.ExecuteNonQuery();
                    }
                    catch (MySqlException ex)
                    {
                        MessageBox.Show($"Ошибка при добавлении продукта: {ex.Message}");
                    }
                }

                MessageBox.Show("Заказ успешно сохранен!");

                // Генерация чека и переход к списку заказов
                GenerateReceipt(orderId, selectedUser, selectedClient, orderDate);
this.Close();
            }
        }

        /// <summary>
        /// Генерация чека заказа в Word
        /// </summary>
        /// <param name="orderId">ID заказа</param>
        /// <param name="selectedUser">Пользователь</param>
        /// <param name="selectedClient">Клиент</param>
        /// <param name="orderDate">Дата заказа</param>
        private void GenerateReceipt(int orderId, string selectedUser, string selectedClient, DateTime orderDate)
        {
            Application wordApp = null;
            Document document = null;

            try
            {
                // Проверяем установлен ли Word
                try
                {
                    wordApp = new Application { Visible = false };
                }
                catch (COMException)
                {
                    MessageBox.Show("Microsoft Word не установлен или поврежден. Установите Microsoft Office для создания чеков.",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return;
                }

                // Создаем новый документ
                document = wordApp.Documents.Add();
                wordApp.Visible = true;

                // Настройка документа
                document.PageSetup.Orientation = WdOrientation.wdOrientPortrait;
                document.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2f);
                document.PageSetup.RightMargin = wordApp.CentimetersToPoints(2f);
                document.PageSetup.TopMargin = wordApp.CentimetersToPoints(2f);
                document.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2f);

                // Заголовок
                var titleRange = document.Content;
                titleRange.Text = "ЧЕК ЗАКАЗА";
                titleRange.Font.Size = 18;
                titleRange.Font.Bold = 1;
                titleRange.Font.Name = "Times New Roman";
                titleRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                titleRange.InsertParagraphAfter();

                // Разделительная линия
                AddHorizontalLine(document);

                // Информация о заказе
                var orderInfoRange = document.Content;
                orderInfoRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                orderInfoRange.Text = $"Дата: {orderDate:dd.MM.yyyy HH:mm}\n" +
                                    $"Номер заказа: {orderId}\n" +
                                    $"Кассир: {selectedUser}\n" +
                                    $"Клиент: {selectedClient}\n" +
                                    $"Статус: {comboBoxStatus.SelectedItem}";
                orderInfoRange.Font.Size = 12;
                orderInfoRange.ParagraphFormat.SpaceAfter = 12;
                orderInfoRange.InsertParagraphAfter();

                // Разделительная линия
                AddHorizontalLine(document);

                // Создание таблицы с товарами
                var tableRange = document.Content;
                tableRange.Collapse(WdCollapseDirection.wdCollapseEnd);

                // Создаем таблицу с нужным количеством строк (товары + заголовок + итоговая строка)
                var table = document.Tables.Add(tableRange, cartItems.Count + 2, 6);
                table.Borders.Enable = 1;
                table.AllowAutoFit = true;
                table.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);

                // Настройка ширины столбцов
                table.Columns[1].SetWidth(wordApp.CentimetersToPoints(3f), WdRulerStyle.wdAdjustNone); // Артикул
                table.Columns[2].SetWidth(wordApp.CentimetersToPoints(6f), WdRulerStyle.wdAdjustNone); // Название
                table.Columns[3].SetWidth(wordApp.CentimetersToPoints(2.5f), WdRulerStyle.wdAdjustNone); // Цена
                table.Columns[4].SetWidth(wordApp.CentimetersToPoints(2f), WdRulerStyle.wdAdjustNone); // Скидка
                table.Columns[5].SetWidth(wordApp.CentimetersToPoints(2f), WdRulerStyle.wdAdjustNone); // Кол-во
                table.Columns[6].SetWidth(wordApp.CentimetersToPoints(3f), WdRulerStyle.wdAdjustNone); // Сумма

                // Заголовки таблицы (первая строка)
                table.Cell(1, 1).Range.Text = "Артикул";
                table.Cell(1, 2).Range.Text = "Наименование";
                table.Cell(1, 3).Range.Text = "Цена";
                table.Cell(1, 4).Range.Text = "Скидка";
                table.Cell(1, 5).Range.Text = "Кол-во";
                table.Cell(1, 6).Range.Text = "Сумма";

                // Стиль заголовков
                for (int j = 1; j <= 6; j++)
                {
                    var cell = table.Cell(1, j);
                    cell.Range.Font.Bold = 1;
                    cell.Range.Font.Size = 11;
                    cell.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    cell.Shading.BackgroundPatternColor = WdColor.wdColorGray15;
                }

                // Заполнение таблицы данными
                decimal total = 0;
                for (int i = 0; i < cartItems.Count; i++)
                {
                    var product = cartItems[i];
                    decimal itemTotal = product.DiscountedCost * product.Quantity;

                    // Заполняем строки с товарами (начиная со второй строки)
                    table.Cell(i + 2, 1).Range.Text = product.Article;
                    table.Cell(i + 2, 2).Range.Text = product.Name;
                    table.Cell(i + 2, 3).Range.Text = product.Price.ToString("N2");
                    table.Cell(i + 2, 4).Range.Text = $"{product.Discount}%";
                    table.Cell(i + 2, 5).Range.Text = product.Quantity.ToString();
                    table.Cell(i + 2, 6).Range.Text = itemTotal.ToString("N2");
                    total += itemTotal;

                    // Выравнивание содержимого ячеек
                    table.Cell(i + 2, 1).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(i + 2, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                    table.Cell(i + 2, 3).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                    table.Cell(i + 2, 4).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(i + 2, 5).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                    table.Cell(i + 2, 6).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                }

                // Итоговая строка (последняя строка таблицы)
                table.Rows.Add();
                var lastRow1 = table.Rows[table.Rows.Count];
                lastRow1.Cells[1].Merge(lastRow1.Cells[5]);
                lastRow1.Cells[1].Range.Text = $"ИТОГО:";
                lastRow1.Cells[2].Range.Text = $"{total:C}";
                lastRow1.Cells[1].Range.Font.Bold = 1;
                lastRow1.Cells[1].Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;


                // Разделительная линия
                AddHorizontalLine(document);

                // Подпись
                var signatureRange = document.Content;
                signatureRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                signatureRange.InsertParagraphAfter();
                signatureRange.Text = "Спасибо за покупку!\n";
                signatureRange.Font.Size = 12;
                signatureRange.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                signatureRange.InsertParagraphAfter();

                // Сохранение документа
                string receiptsFolder = Path.Combine(System.Windows.Forms.Application.StartupPath, "Чеки");
                if (!Directory.Exists(receiptsFolder))
                    Directory.CreateDirectory(receiptsFolder);

                string fileName = $"Чек_{orderId}_{DateTime.Now:yyyyMMddHHmmss}.docx";
                string filePath = Path.Combine(receiptsFolder, fileName);
                document.SaveAs(filePath);

                // Открытие документа для просмотра
                wordApp.Visible = true;
                wordApp.Activate();
            }
            finally
            {
                // Корректное освобождение ресурсов
                if (document != null)
                {
                    Marshal.ReleaseComObject(document);
                }
                if (wordApp != null)
                {
                    Marshal.ReleaseComObject(wordApp);
                }
            }
        }

        // Метод для добавления горизонтальной линии
        private void AddHorizontalLine(Document document)
        {
            var lineRange = document.Content;
            lineRange.Collapse(WdCollapseDirection.wdCollapseEnd);
            lineRange.ParagraphFormat.Borders[WdBorderType.wdBorderBottom].LineStyle = WdLineStyle.wdLineStyleSingle;
            lineRange.ParagraphFormat.Borders[WdBorderType.wdBorderBottom].LineWidth = WdLineWidth.wdLineWidth050pt;
            lineRange.InsertParagraphAfter();
        }
        /// <summary>
        /// Получение ID пользователя по имени
        /// </summary>
        private int GetUserId(string fullName, MySqlConnection connection)
        {
            string query = "SELECT UserId FROM user WHERE UserName = @Name;";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Name", fullName);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Получение ID клиента по ФИО
        /// </summary>
        private int GetClientId(string fullName, MySqlConnection connection)
        {
            string[] names = fullName.Split(' ');
            string query = "SELECT ClientId FROM client WHERE ClientSurname = @Surname AND ClientName = @Name;";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Surname", names[0]);
            command.Parameters.AddWithValue("@Name", names[1]);
            return Convert.ToInt32(command.ExecuteScalar());
        }

        /// <summary>
        /// Получение доступного количества товара и обновление информации о скидке
        /// </summary>
        private int GetAvailableQuantity(string productArticle)
        {
            using (MySqlConnection connection = new MySqlConnection(con))
            {
                connection.Open();
                string query = @"
            SELECT ProductStock, ProductDiscount 
            FROM product 
            WHERE ProductArticle = @ProductArticle";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@ProductArticle", productArticle);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var product = cartItems.FirstOrDefault(p => p.Article == productArticle);
                        if (product != null)
                        {
                            // Обновление информации о скидке
                            product.Discount = Convert.ToInt32(reader["ProductDiscount"]);
                        }
                        return Convert.ToInt32(reader["ProductStock"]);
                    }
                }
            }
            return 0;
        }

        /// <summary>
        /// Обработчик нажатия кнопок в таблице товаров
        /// </summary>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                var column = dataGridView1.Columns[e.ColumnIndex];

                if (column is DataGridViewButtonColumn)
                {
                    if (column.HeaderText == "Добавить")
                    {
                        // Увеличение количества товара
                        Product selectedProduct = cartItems[e.RowIndex];

                        int availableQuantity = GetAvailableQuantity(selectedProduct.Article);
                        if (selectedProduct.Quantity < availableQuantity)
                        {
                            selectedProduct.Quantity++;
                            dataGridView1.Refresh();
                            UpdateTotalCost();
                        }
                        else
                        {
                            MessageBox.Show($"Не удается добавить больше товара. Доступное количество: {availableQuantity}");
                        }
                    }
                    else if (column.HeaderText == "Удалить")
                    {
                        // Уменьшение количества или удаление товара
                        Product selectedProduct = cartItems[e.RowIndex];
                        if (selectedProduct.Quantity > 1)
                        {
                            selectedProduct.Quantity--;
                        }
                        else
                        {
                            cartItems.RemoveAt(e.RowIndex);
                        }

                        dataGridView1.Refresh();
                        UpdateTotalCost();
                    }
                }
            }
        }
    }
}