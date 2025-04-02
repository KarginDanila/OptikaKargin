using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using MySql.Data.MySqlClient;

namespace OptikaKargin
{
    public partial class RecoveryAdmin : Form
    {
        public RecoveryAdmin()
        {
            InitializeComponent();
            LoadTablesList();
        }

        string con = Connection.myConnection; // Строка подключения к БД

        private void LoadTablesList()
        {
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(new string[] { "category", "client", "order", "orderitem", "product", "role", "supplier", "user" });
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                // Извлекаем параметры подключения
                var builder = new MySqlConnectionStringBuilder(con);
                string databaseName = builder.Database;

                if (string.IsNullOrEmpty(databaseName))
                {
                    MessageBox.Show("Имя базы данных не указано в строке подключения", "Ошибка",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Подключение к серверу без указания базы данных
                builder.Database = "";
                string serverConnectionString = builder.ConnectionString;

                // 1. Удаляем и создаем заново базу данных
                using (var serverConnection = new MySqlConnection(serverConnectionString))
                {
                    serverConnection.Open();

                    // Удаляем базу, если существует
                    using (var cmd = new MySqlCommand($"DROP DATABASE IF EXISTS `{databaseName}`", serverConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }

                    // Создаем новую базу
                    using (var cmd = new MySqlCommand($"CREATE DATABASE `{databaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci", serverConnection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }

                // 2. Восстанавливаем структуру таблиц
                builder.Database = databaseName;
                string dbConnectionString = builder.ConnectionString;

                // SQL-запросы для создания таблиц
                string[] createTablesScript = {
                    // Создание таблицы category
                    @"CREATE TABLE IF NOT EXISTS `category` (
                        `CategoryId` int NOT NULL AUTO_INCREMENT,
                        `CategoryName` varchar(45) DEFAULT NULL,
                        PRIMARY KEY (`CategoryId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы client
                    @"CREATE TABLE IF NOT EXISTS `client` (
                        `ClientId` int NOT NULL AUTO_INCREMENT,
                        `ClientName` varchar(45) DEFAULT NULL,
                        `ClientSurname` varchar(50) DEFAULT NULL,
                        `ClientPatronymic` varchar(50) DEFAULT NULL,
                        `ClientEmail` varchar(30) DEFAULT NULL,
                        `ClientAddress` varchar(50) DEFAULT NULL,
                        `ClientPhone` varchar(18) DEFAULT NULL,
                        PRIMARY KEY (`ClientId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=55 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы role
                    @"CREATE TABLE IF NOT EXISTS `role` (
                        `RoleId` int NOT NULL AUTO_INCREMENT,
                        `RoleName` varchar(45) DEFAULT NULL,
                        PRIMARY KEY (`RoleId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=6 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы supplier
                    @"CREATE TABLE IF NOT EXISTS `supplier` (
                        `SupplierId` int NOT NULL AUTO_INCREMENT,
                        `SupplierName` varchar(45) DEFAULT NULL,
                        `SupplierAddress` varchar(45) DEFAULT NULL,
                        `SupplierPhone` varchar(18) DEFAULT NULL,
                        PRIMARY KEY (`SupplierId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=39 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы user
                    @"CREATE TABLE IF NOT EXISTS `user` (
                        `UserId` int NOT NULL AUTO_INCREMENT,
                        `UserName` varchar(45) DEFAULT NULL,
                        `UserAddress` varchar(45) DEFAULT NULL,
                        `UserPhone` varchar(18) DEFAULT NULL,
                        `UserLogin` tinytext,
                        `UserPassword` tinytext,
                        `UserRoleId` int NOT NULL,
                        PRIMARY KEY (`UserId`),
                        KEY `fk_user_role1_idx` (`UserRoleId`),
                        CONSTRAINT `fk_user_role1` FOREIGN KEY (`UserRoleId`) REFERENCES `role` (`RoleId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы product
                    @"CREATE TABLE IF NOT EXISTS `product` (
                        `ProductArticle` varchar(6) NOT NULL,
                        `ProductName` varchar(45) DEFAULT NULL,
                        `ProductDescription` varchar(100) DEFAULT NULL,
                        `ProductPrice` int DEFAULT NULL,
                        `ProductStock` int DEFAULT NULL,
                        `ProductCategoryId` int NOT NULL,
                        `ProductSupplierId` int NOT NULL,
                        `ProductPhoto` varchar(100) DEFAULT NULL,
                        `ProductDiscount` tinyint DEFAULT NULL,
                        PRIMARY KEY (`ProductArticle`),
                        KEY `fk_product_category1_idx` (`ProductCategoryId`),
                        KEY `fk_product_supplier1_idx` (`ProductSupplierId`),
                        CONSTRAINT `fk_product_category1` FOREIGN KEY (`ProductCategoryId`) REFERENCES `category` (`CategoryId`),
                        CONSTRAINT `fk_product_supplier1` FOREIGN KEY (`ProductSupplierId`) REFERENCES `supplier` (`SupplierId`)
                    ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы order
                    @"CREATE TABLE IF NOT EXISTS `order` (
                        `OrderId` int NOT NULL AUTO_INCREMENT,
                        `OrderData` date DEFAULT NULL,
                        `OrderStatus` varchar(45) DEFAULT NULL,
                        `OrderUserId` int NOT NULL,
                        `OrderClientId` int NOT NULL,
                        PRIMARY KEY (`OrderId`),
                        KEY `fk_order_employees1_idx` (`OrderUserId`),
                        KEY `fk_order_client1_idx` (`OrderClientId`),
                        CONSTRAINT `fk_order_client1` FOREIGN KEY (`OrderClientId`) REFERENCES `client` (`ClientId`),
                        CONSTRAINT `fk_order_user1` FOREIGN KEY (`OrderUserId`) REFERENCES `user` (`UserId`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=83 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;",

                    // Создание таблицы orderitem
                    @"CREATE TABLE IF NOT EXISTS `orderitem` (
                        `OrderItemId` int NOT NULL AUTO_INCREMENT,
                        `OrderItemQuentity` int DEFAULT NULL,
                        `OrderItemTotalPrice` int DEFAULT NULL,
                        `OrderItemOrderId` int NOT NULL,
                        `OrderItemProductArticle` varchar(6) NOT NULL,
                        PRIMARY KEY (`OrderItemId`,`OrderItemOrderId`,`OrderItemProductArticle`),
                        KEY `fk_orderitem_order1_idx` (`OrderItemOrderId`),
                        KEY `fk_orderitem_product1_idx` (`OrderItemProductArticle`),
                        CONSTRAINT `fk_orderitem_order1` FOREIGN KEY (`OrderItemOrderId`) REFERENCES `order` (`OrderId`),
                        CONSTRAINT `fk_orderitem_productarticle` FOREIGN KEY (`OrderItemProductArticle`) REFERENCES `product` (`ProductArticle`)
                    ) ENGINE=InnoDB AUTO_INCREMENT=121 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;"
                };

                // Выполняем SQL-запросы
                ExecuteSqlScript(dbConnectionString, createTablesScript);

                MessageBox.Show("База данных и таблицы успешно восстановлены", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка восстановления: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExecuteSqlScript(string connectionString, string[] scripts)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                foreach (var script in scripts)
                {
                    if (string.IsNullOrWhiteSpace(script)) continue;

                    try
                    {
                        using (var cmd = new MySqlCommand(script, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Ошибка выполнения команды: {script}\n{ex.Message}");
                        throw; // Пробрасываем исключение дальше
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("Выберите таблицу для импорта", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(textBox1.Text))
            {
                MessageBox.Show("Сначала выберите файл для импорта", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string tableName = comboBox1.SelectedItem.ToString();
                int importedRows = ImportCsvToTable(textBox1.Text, tableName);

                MessageBox.Show($"Успешно импортировано {importedRows} записей", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "CSV files (*.csv)|*.csv",
                Title = "Выберите файл для импорта",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
            }
        }

        private int ImportCsvToTable(string filePath, string tableName)
        {
            using (var connection = new MySqlConnection(con))
            {
                connection.Open();

                // Чтение файла построчно
                var lines = new List<string>();
                using (var reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("!"))
                            lines.Add(line.Trim());
                    }
                }

                if (lines.Count < 2) return 0;

                // Определяем разделитель
                char separator = lines[0].Contains(';') ? ';' : ',';

                // Получаем заголовки
                var headers = SplitCsvLine(lines[0], separator);

                // Получаем столбцы таблицы
                var columns = GetTableColumnsSimple(connection, tableName);

                // Проверка соответствия
                if (headers.Length != columns.Count)
                {
                    throw new Exception($"Несоответствие столбцов. Ожидалось {columns.Count}, получено {headers.Length}");
                }

                // Создаем команду для вставки
                var cmdText = $"INSERT INTO {tableName} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", columns.Select(c => $"@{c}"))})";
                var cmd = connection.CreateCommand();
                cmd.CommandText = cmdText;

                int importedRows = 0;
                for (int i = 1; i < lines.Count; i++)
                {
                    var values = SplitCsvLine(lines[i], separator);
                    if (values.Length != columns.Count) continue;

                    cmd.Parameters.Clear();
                    for (int j = 0; j < columns.Count; j++)
                    {
                        var param = cmd.CreateParameter();
                        param.ParameterName = $"@{columns[j]}";
                        param.Value = string.IsNullOrWhiteSpace(values[j]) ? DBNull.Value : (object)values[j].Trim('"');
                        cmd.Parameters.Add(param);
                    }

                    try
                    {
                        cmd.ExecuteNonQuery();
                        importedRows++;
                    }
                    catch (MySqlException ex)
                    {
                        Console.WriteLine($"Ошибка при вставке строки {i}: {ex.Message}");
                    }
                }

                return importedRows;
            }
        }

        private string[] SplitCsvLine(string line, char separator)
        {
            var result = new List<string>();
            var inQuotes = false;
            var current = new StringBuilder();

            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == separator && !inQuotes)
                {
                    result.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }

            result.Add(current.ToString());
            return result.ToArray();
        }

        private List<string> GetTableColumnsSimple(MySqlConnection connection, string tableName)
        {
            var columns = new List<string>();
            using (var cmd = new MySqlCommand($"SHOW COLUMNS FROM {tableName}", connection))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    columns.Add(reader.GetString(0));
                }
            }
            return columns;
        }
    }
}