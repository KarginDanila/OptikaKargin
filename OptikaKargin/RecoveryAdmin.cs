﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OptikaKargin
{
    public partial class RecoveryAdmin : Form
    {
        public RecoveryAdmin()
        {
            InitializeComponent();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization(); // Создаем форму авторизации
            form.Show();
            this.Close();
        }
    }
}
