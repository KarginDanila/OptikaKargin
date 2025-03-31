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
    /// Главная форма менеджера для приложения "Оптика Каргина".
    /// Предоставляет доступ к различным модулям системы.
    /// </summary>
    public partial class FormManager : Form
    {
        /// <summary>
        /// Инициализирует новый экземпляр формы менеджера.
        /// </summary>
        public FormManager()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Обрабатывает событие нажатия кнопки выхода.
        /// Возвращает пользователя к форме авторизации.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            Avtorization form = new Avtorization();
            form.Show();
            Hide();
        }

        /// <summary>
        /// Обрабатывает клик по ссылке "Управление товарами".
        /// Открывает форму управления товарами в модальном режиме.
        /// </summary>
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormProductManager form = new FormProductManager();
            form.ShowDialog();
        }

        /// <summary>
        /// Обрабатывает клик по ссылке "Управление клиентами".
        /// Открывает форму управления клиентами в модальном режиме.
        /// </summary>
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormClient form = new FormClient();
            form.ShowDialog();
        }

        /// <summary>
        /// Обрабатывает событие загрузки формы.
        /// Устанавливает отображение ФИО текущего пользователя.
        /// </summary>
        private void FormManager_Load(object sender, EventArgs e)
        {
            label3.Text = ValuesFIO.UserFIO;
        }

        /// <summary>
        /// Обрабатывает клик по ссылке "Управление заказами".
        /// Открывает форму управления заказами в модальном режиме.
        /// </summary>
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            FormOrd form = new FormOrd();
            form.ShowDialog();
        }
    }
}