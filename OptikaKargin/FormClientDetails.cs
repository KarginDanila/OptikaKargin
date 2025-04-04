using System.Windows.Forms;
using System;

namespace OptikaKargin
{
    public partial class FormClientDetails : Form
    {
        public FormClientDetails(Client client)
        {
            InitializeComponent();
            DisplayClientDetails(client);
        }

        private void DisplayClientDetails(Client client)
        {
            label1.Text += client.OriginalName ?? client.Name;
            label2.Text += client.OriginalSurname ?? client.Surname;
            label3.Text += client.OriginalPatronymic ?? client.Patronymic;
            label4.Text += client.OriginalEmail ?? client.Email;
            label5.Text += client.OriginalAddress ?? client.Address;
            label6.Text += client.OriginalPhone ?? client.Phone;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}