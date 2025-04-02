using System;
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
    /// <summary>
    /// Форма администратора - главное меню для управления системой
    /// </summary>
    public partial class FormAdmin : Form
    {
        /// <summary>
        /// Инициализация формы администратора
        /// </summary>
        public FormAdmin()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обработчик кнопки выхода в меню авторизации
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization(); // Создаем форму авторизации
            form.Show(); // Показываем форму авторизации
            Hide(); // Скрываем текущую форму администратора
        }

        /// <summary>
        /// Обработчик ссылки на форму управления товарами
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormProduct form = new FormProduct(); // Создаем форму товаров
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        /// <summary>
        /// Обработчик ссылки на форму управления поставщиками
        /// </summary>
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormSupplier form = new FormSupplier(); // Создаем форму поставщиков
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        /// <summary>
        /// Обработчик ссылки на форму управления сотрудниками
        /// </summary>
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormSotrydniki form = new FormSotrydniki(); // Создаем форму сотрудников
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        /// <summary>
        /// Обработчик ссылки на форму управления пользователями
        /// </summary>
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormPolsovateli form = new FormPolsovateli(); // Создаем форму пользователей
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        /// <summary>
        /// Обработчик загрузки формы администратора
        /// </summary>
        private void FormAdmin_Load(object sender, EventArgs e)
        {
            // Отображаем ФИО текущего пользователя из статического класса ValuesFIO
            label3.Text = ValuesFIO.UserFIO;
        }

        /// <summary>
        /// Обработчик ссылки на форму управления категориями
        /// </summary>
        private void linkLabel7_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormCategory form = new FormCategory(); // Создаем форму категорий
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        /// <summary>
        /// Обработчик ссылки на форму отчетов
        /// </summary>
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormOtchet form = new FormOtchet(); // Создаем форму отчетов
            form.Show(); // Показываем форму отчетов
            Hide(); // Скрываем текущую форму администратора
        }

        /// <summary>
        /// Обработчик ссылки на форму управления ролями
        /// </summary>
        private void linkLabel6_LinkClicked_1(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormRole form = new FormRole(); // Создаем форму ролей
            form.ShowDialog(); // Открываем форму в модальном режиме
        }

        private void button2_Click(object sender, EventArgs e)
        {
            RecoveryAdmin form = new RecoveryAdmin(); // Создаем форму ролей
            form.Show();
            Hide();// Открываем форму в модальном режиме
        }
    }
}