using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OptikaKargin
{
    /// <summary>
    /// Класс для работы с параметрами подключения к базе данных
    /// </summary>
    public class Connection
    {
        /// <summary>
        /// Хост (сервер) базы данных
        /// </summary>
        public static string host = Properties.Settings.Default.host;

        /// <summary>
        /// Имя базы данных
        /// </summary>
        public static string database = Properties.Settings.Default.database;

        /// <summary>
        /// Имя пользователя для подключения к базе данных
        /// </summary>
        public static string uid = Properties.Settings.Default.uid;

        /// <summary>
        /// Пароль для подключения к базе данных
        /// </summary>
        public static string pwd = Properties.Settings.Default.pwd;

        /// <summary>
        /// Строка подключения к базе данных в формате MySQL
        /// Формат: "host=хост; uid=пользователь; pwd=пароль; database=база_данных"
        /// </summary>
        public static string myConnection = $@"host={host}; uid={uid}; pwd={pwd}; database={database}";
    }
}