using Microsoft.Office.Interop.Word;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Word.Application;

namespace OptikaKargin
{
    /// <summary>
    /// Форма для генерации отчетов по заказам
    /// </summary>
    public partial class FormOtchet : Form
    {
        string connect = Connection.myConnection;
        /// <summary>
        /// Конструктор формы отчетов
        /// </summary>
        public FormOtchet()
        {
            InitializeComponent();

            // Устанавливаем начальные значения (текущий месяц)
            dateTimePicker1.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePicker2.Value = DateTime.Now;

            // Подписываемся на события изменения даты
            dateTimePicker1.ValueChanged += dateTimePicker1_ValueChanged;
            dateTimePicker2.ValueChanged += dateTimePicker2_ValueChanged;

            // Устанавливаем максимальную дату - сегодня
            dateTimePicker1.MaxDate = DateTime.Today;
            dateTimePicker2.MaxDate = DateTime.Today;
        }
        
        /// <summary>
        /// Класс для хранения информации о заказе
        /// </summary>
        public class OrderInfo
        {
            public int OrderId { get; set; }
            public DateTime OrderDate { get; set; }
            public string ClientName { get; set; }
            public decimal OrderTotal { get; set; }
            public decimal OrderDiscount { get; set; } // Скидка на весь заказ
            public decimal OrderFinalTotal => OrderTotal * (1 - OrderDiscount / 100m); // Итоговая сумма с учетом скидки
            public List<OrderItem> Items { get; set; } = new List<OrderItem>();
        }
        /// <summary>
        /// Класс для хранения информации о товаре в заказе
        /// </summary>
        public class OrderItem
        {
            public string ProductName { get; set; }
            public int Quantity { get; set; }
            public decimal Price { get; set; }
            public int Discount { get; set; } // Скидка на конкретный товар
            public decimal DiscountedPrice => Price * (1 - Discount / 100m); // Цена со скидкой
            public decimal Total => DiscountedPrice * Quantity; // Итоговая стоимость с учетом скидки
        }
        /// <summary>
        /// Обработчик изменения даты начало периода
        /// </summary>
        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            // Если дата начала изменилась
            if (dateTimePicker1.Value > DateTime.Today)
            {
                dateTimePicker1.Value = DateTime.Today;
                return;
            }

            // Проверяем чтобы дата окончания была не раньше даты начала
            if (dateTimePicker2.Value < dateTimePicker1.Value)
            {
                dateTimePicker2.Value = dateTimePicker1.Value;
            }
        }
        /// <summary>
        /// Обработчик изменения даты окончания периода
        /// </summary>
        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            // Если дата окончания изменилась
            if (dateTimePicker2.Value > DateTime.Today)
            {
                dateTimePicker2.Value = DateTime.Today;
                return;
            }

            // Проверяем чтобы дата окончания была не раньше даты начала
            if (dateTimePicker2.Value < dateTimePicker1.Value)
            {
                MessageBox.Show("Дата окончания не может быть раньше даты начала", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                dateTimePicker2.Value = dateTimePicker1.Value;
            }
        }
        /// <summary>
        /// Обработчик нажатия кнопки генерации отчета
        /// </summary>
        private void buttondelete_Click_1(object sender, EventArgs e)
        {
            DateTime startDate = dateTimePicker1.Value.Date;
            DateTime endDate = dateTimePicker2.Value.Date.AddDays(1).AddSeconds(-1);

            // Дополнительная проверка (хотя обработчики ValueChanged уже должны предотвратить это)
            if (startDate > endDate)
            {
                MessageBox.Show("Дата начала не может быть больше даты окончания", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                List<OrderInfo> acceptedOrders = GetAcceptedOrders(startDate, endDate);
                if (acceptedOrders.Count == 0)
                {
                    MessageBox.Show("Нет принятых заказов за выбранный период", "Информация",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                CreateDetailedReport(startDate, endDate, acceptedOrders);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при формировании отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Получает список принятых заказов за указанный период
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <returns>Список заказов</returns>
        private List<OrderInfo> GetAcceptedOrders(DateTime startDate, DateTime endDate)
        {
            List<OrderInfo> orders = new List<OrderInfo>();

            using (MySqlConnection connection = new MySqlConnection(connect))
            {
                connection.Open();

                // Получаем список заказов (без скидки на заказ, так как ее нет в FormOrderbuy)
                string orderQuery = @"
                    SELECT 
                        o.OrderId, 
                        o.OrderData, 
                        CONCAT(c.ClientSurname, ' ', c.ClientName) AS ClientName
                    FROM `order` o
                    JOIN client c ON o.OrderClientId = c.ClientId
                    WHERE o.OrderData BETWEEN @StartDate AND @EndDate
                    AND o.OrderStatus = 'Принят'
                    ORDER BY o.OrderData DESC";

                using (MySqlCommand orderCommand = new MySqlCommand(orderQuery, connection))
                {
                    orderCommand.Parameters.AddWithValue("@StartDate", startDate);
                    orderCommand.Parameters.AddWithValue("@EndDate", endDate);

                    using (MySqlDataReader orderReader = orderCommand.ExecuteReader())
                    {
                        while (orderReader.Read())
                        {
                            OrderInfo order = new OrderInfo
                            {
                                OrderId = Convert.ToInt32(orderReader["OrderId"]),
                                OrderDate = Convert.ToDateTime(orderReader["OrderData"]),
                                ClientName = orderReader["ClientName"].ToString(),
                                OrderDiscount = 0 // В FormOrderbuy нет скидки на заказ, только на товары
                            };
                            orders.Add(order);
                        }
                    }
                }

                // Для каждого заказа получаем товары с учетом скидки из product
                foreach (OrderInfo order in orders)
                {
                    string itemsQuery = @"
                        SELECT 
                            p.ProductName,
                            oi.OrderItemQuentity AS Quantity,
                            p.ProductPrice AS Price,
                            p.ProductDiscount AS Discount
                        FROM orderitem oi
                        JOIN product p ON oi.OrderItemProductArticle = p.ProductArticle
                        WHERE oi.OrderItemOrderId = @OrderId";

                    using (MySqlCommand itemsCommand = new MySqlCommand(itemsQuery, connection))
                    {
                        itemsCommand.Parameters.AddWithValue("@OrderId", order.OrderId);

                        using (MySqlDataReader itemsReader = itemsCommand.ExecuteReader())
                        {
                            while (itemsReader.Read())
                            {
                                var item = new OrderItem
                                {
                                    ProductName = itemsReader["ProductName"].ToString(),
                                    Quantity = Convert.ToInt32(itemsReader["Quantity"]),
                                    Price = Convert.ToDecimal(itemsReader["Price"]),
                                    Discount = Convert.ToInt32(itemsReader["Discount"])
                                };

                                order.Items.Add(item);
                                order.OrderTotal += item.Total; // Учитываем скидку на товар
                            }
                        }
                    }
                }
            }

            return orders;
        }
        /// <summary>
        /// Создает детализированный отчет в формате Word
        /// </summary>
        /// <param name="startDate">Начальная дата периода</param>
        /// <param name="endDate">Конечная дата периода</param>
        /// <param name="orders">Список заказов для отчета</param>
        private void CreateDetailedReport(DateTime startDate, DateTime endDate, List<OrderInfo> orders)
        {
            Application wordApp = null;
            Document wordDoc = null;

            try
            {
                wordApp = new Application { Visible = false };
                wordDoc = wordApp.Documents.Add();

                // Настройка страницы
                wordDoc.PageSetup.Orientation = WdOrientation.wdOrientPortrait;
                wordDoc.PageSetup.LeftMargin = wordApp.CentimetersToPoints(2f);
                wordDoc.PageSetup.RightMargin = wordApp.CentimetersToPoints(2f);
                wordDoc.PageSetup.TopMargin = wordApp.CentimetersToPoints(2f);
                wordDoc.PageSetup.BottomMargin = wordApp.CentimetersToPoints(2f);

                // Заголовок отчета
                Paragraph title = wordDoc.Content.Paragraphs.Add();
                title.Range.Text = "Отчет по принятым заказам";
                title.Range.Font.Bold = 1;
                title.Range.Font.Size = 16;
                title.Range.Font.Name = "Times New Roman";
                title.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                title.Range.ParagraphFormat.SpaceAfter = 12;
                title.Range.InsertParagraphAfter();

                // Период отчета
                Paragraph period = wordDoc.Content.Paragraphs.Add();
                period.Range.Text = $"Период: с {startDate:dd.MM.yyyy} по {endDate:dd.MM.yyyy}";
                period.Range.Font.Size = 14;
                period.Range.Font.Italic = 1;
                period.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                period.Range.ParagraphFormat.SpaceAfter = 12;
                period.Range.InsertParagraphAfter();

                // Общая статистика
                decimal totalRevenue = orders.Sum(o => o.OrderFinalTotal);
                int totalItems = orders.Sum(o => o.Items.Sum(i => i.Quantity));
                decimal totalDiscount = orders.Sum(o => o.OrderTotal - o.OrderFinalTotal);

                Paragraph stats = wordDoc.Content.Paragraphs.Add();
                stats.Range.Text = $"Всего заказов: {orders.Count}\n" +
                                  $"Всего товаров: {totalItems}\n" +
                                  $"Итоговая сумма: {totalRevenue:C}";
                stats.Range.Font.Size = 12;
                stats.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphLeft;
                stats.Range.ParagraphFormat.SpaceAfter = 18;

                // Добавляем разделительную линию перед списком заказов
                wordDoc.Content.Paragraphs.Add();
                wordDoc.Content.InsertParagraphAfter();

                // Перебираем все заказы
                foreach (OrderInfo order in orders)
                {
                    // Добавляем заголовок заказа
                    Range orderRange = wordDoc.Content;
                    orderRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                    Paragraph orderHeader = orderRange.Paragraphs.Add();
                    orderHeader.Range.Text = $"Заказ №{order.OrderId} от {order.OrderDate:dd.MM.yyyy}";
                    orderHeader.Range.Font.Bold = 1;
                    orderHeader.Range.Font.Size = 14;
                    orderHeader.Range.Font.Color = WdColor.wdColorDarkBlue;
                    orderHeader.Range.ParagraphFormat.SpaceAfter = 6;
                    orderHeader.Range.InsertParagraphAfter();

                    // Информация о клиенте
                    orderRange = wordDoc.Content;
                    orderRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                    Paragraph clientInfo = orderRange.Paragraphs.Add();
                    clientInfo.Range.Text = $"Клиент: {order.ClientName ?? "Не указан"}";
                    clientInfo.Range.Font.Size = 12;
                    clientInfo.Range.ParagraphFormat.SpaceAfter = 6;
                    clientInfo.Range.InsertParagraphAfter();

                    // Создаем таблицу для товаров
                    orderRange = wordDoc.Content;
                    orderRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                    Table orderTable = wordDoc.Tables.Add(
                        orderRange,
                        order.Items.Count + 1, // +1 для заголовка
                        5 // Колонки: Товар, Кол-во, Цена, Скидка, Сумма
                    );

                    // Настройка таблицы
                    orderTable.Borders.Enable = 1;
                    orderTable.Range.ParagraphFormat.SpaceAfter = 6;
                    orderTable.AllowAutoFit = true;
                    orderTable.AutoFitBehavior(WdAutoFitBehavior.wdAutoFitWindow);

                    // Заголовки таблицы
                    orderTable.Cell(1, 1).Range.Text = "Товар";
                    orderTable.Cell(1, 2).Range.Text = "Кол-во";
                    orderTable.Cell(1, 3).Range.Text = "Цена";
                    orderTable.Cell(1, 4).Range.Text = "Скидка";
                    orderTable.Cell(1, 5).Range.Text = "Сумма";

                    // Форматирование заголовков
                    for (int i = 1; i <= 5; i++)
                    {
                        orderTable.Cell(1, i).Range.Font.Bold = 1;
                        orderTable.Cell(1, i).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        orderTable.Cell(1, i).Shading.BackgroundPatternColor = WdColor.wdColorGray15;
                    }

                    // Заполнение таблицы данными
                    for (int i = 0; i < order.Items.Count; i++)
                    {
                        OrderItem item = order.Items[i];
                        orderTable.Cell(i + 2, 1).Range.Text = item.ProductName ?? "Без названия";
                        orderTable.Cell(i + 2, 2).Range.Text = item.Quantity.ToString();
                        orderTable.Cell(i + 2, 3).Range.Text = item.Price.ToString("C");
                        orderTable.Cell(i + 2, 4).Range.Text = $"{item.Discount}%";
                        orderTable.Cell(i + 2, 5).Range.Text = item.Total.ToString("C");

                        // Выравнивание
                        orderTable.Cell(i + 2, 2).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        orderTable.Cell(i + 2, 4).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphCenter;
                        orderTable.Cell(i + 2, 3).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                        orderTable.Cell(i + 2, 5).Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                    }

                    // Итоговая сумма под таблицей
                    orderRange = wordDoc.Content;
                    orderRange.Collapse(WdCollapseDirection.wdCollapseEnd);
                    Paragraph totalParagraph = orderRange.Paragraphs.Add();
                    totalParagraph.Range.Text = $"Итого по заказу: {order.OrderFinalTotal.ToString("C")}";
                    totalParagraph.Range.Font.Bold = 1;
                    totalParagraph.Range.Font.Size = 12;
                    totalParagraph.Range.ParagraphFormat.Alignment = WdParagraphAlignment.wdAlignParagraphRight;
                    totalParagraph.Range.ParagraphFormat.SpaceAfter = 12;
                    totalParagraph.Range.InsertParagraphAfter();

                    // Добавляем разделительную линию между заказами (кроме последнего)
                    if (orders.IndexOf(order) < orders.Count - 1)
                    {
                        wordDoc.Content.Paragraphs.Add();
                        wordDoc.Content.InsertParagraphAfter();
                        wordDoc.Content.InsertParagraphAfter(); // Дополнительный отступ
                    }
                }

                // Сохранение документа
                string reportsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Отчеты");
                Directory.CreateDirectory(reportsFolder);

                string fileName = $"Отчет_по_заказам_{DateTime.Now:yyyyMMdd_HHmmss}.docx";
                string filePath = Path.Combine(reportsFolder, fileName);

                wordDoc.SaveAs(filePath);
                wordApp.Visible = true;

                MessageBox.Show($"Отчет успешно сохранен:\n{filePath}", "Готово",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // Освобождение ресурсов
                if (wordDoc != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wordDoc);
                }

                if (wordApp != null)
                {
                    System.Runtime.InteropServices.Marshal.ReleaseComObject(wordApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        /// <summary>
        /// Обработчик нажатия кнопки возврата в меню администратора
        /// </summary>
        private void buttonBask_Click(object sender, EventArgs e)
        {
            FormAdmin form = new FormAdmin();
            form.Show();
            Hide();
        }
    }
}