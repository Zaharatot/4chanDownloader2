using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4chanDownloader2.Clases.Wrappers
{
    /// <summary>
    /// Оболочка, для строки из таблицы, с информацией о треде
    /// </summary>
    public class threadWrapper
    {
        /// <summary>
        /// Путь к треду
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Имя треда
        /// </summary>
        public string ThreadName { get; set; }
        /// <summary>
        /// Статус треда
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// Количество картинок в треде загружено/всего
        /// </summary>
        public string CountImages { get; set; }
    }
}
