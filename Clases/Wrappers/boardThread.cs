using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _4chanDownloader2.Clases.Wrappers
{
    /// <summary>
    /// Класс, с тредом с доски
    /// </summary>
    class boardThread
    {
        /// <summary>
        /// Общее количество картинок в треде
        /// </summary>
        public int countImages { get; set; }
        /// <summary>
        /// Количество загруженных картинок в треде
        /// </summary>
        public int countDownloadImages { get; set; }
        /// <summary>
        /// Путь к треду
        /// </summary>
        public string url { get; set; }
        /// <summary>
        /// Имя треда
        /// </summary>
        public string threadName { get; set; }
        /// <summary>
        /// Статус загрузки
        /// </summary>
        public downloadStatus state { get; set; }

        /// <summary>
        /// Инициализация инфы о треде
        /// </summary>
        /// <param name="url">Путь к треду</param>
        public boardThread(string url)
        {
            this.url = url;
            state = downloadStatus.Ожидание;
            threadName = "";
            countImages = countDownloadImages = 0;
        }


        /// <summary>
        /// Возвращаем информацию о треде, в виде строки таблицы
        /// </summary>
        /// <returns>Класс, с инфой о строке таблицы</returns>
        public threadWrapper getTableString()
        {
            return new threadWrapper {
                Url = url,
                ThreadName = (threadName.Length == 0) ? "***" : threadName,
                Status = state.ToString(),
                CountImages = (countImages == 0) ? "***/***" : string.Format("{0}/{1}", countDownloadImages, countImages)
            };
        }

        /// <summary>
        /// Проверяем уровень загрузки треда
        /// </summary>
        /// <returns>True - тред загружен</returns>
        public bool testLoadThread()
        {
            bool ex = false;
            //Количество картинок равно
            if(countImages == countDownloadImages)
            {
                //Меняем статус, если загрузка завершена
                ex = true;
                state = downloadStatus.Завершён;
            }

            return ex;
        }
             
    }
}
