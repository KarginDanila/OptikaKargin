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
        }
        string con = Connection.myConnection; 
        private void button4_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            this.Close();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string tableName = comboBox1.Text;
            OpenFileDialog OPF = new OpenFileDialog();
            OPF.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            OPF.Filter = "Файлы csv|*.csv";
            string FileName = string.Empty;
            if (OPF.ShowDialog() == DialogResult.OK)
            {
                FileName = OPF.FileName;
                if (Path.GetFileNameWithoutExtension(FileName) != tableName)
                {
                    MessageBox.Show("Название файла не совпадает с названием таблицы. Пожалуйста, выберите правильный файл.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                MessageBox.Show("Файл не выбран");
                return;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(con))
                {
                    connection.Open();

                    DataTable tables = connection.GetSchema("Tables");
                    comboBox1.DataSource = tables;
                    comboBox1.DisplayMember = "Table_NAME";
                    comboBox1.ValueMember = "Table_NAME";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении списка таблиц: {ex.Message}");
            }
        }
    }
}
