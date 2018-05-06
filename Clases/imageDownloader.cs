using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using _4chanDownloader2.Clases;
using _4chanDownloader2.Clases.Wrappers;

namespace _4chanDownloader2.Clases
{
    /// <summary>
    /// Класс загрузки картинок из треда
    /// </summary>
    class imageDownloader
    {
        /// <summary>
        /// Путь сохранения картинок, данного треда
        /// </summary>
        string savePath;


        /// <summary>
        /// Делегат функции возврата информации о треде
        /// </summary>
        public delegate void threadInfoLoadedEventhandler(string name, int countImages);

        /// <summary>
        /// Делегат функции возврата информации о статусе загрузки
        /// </summary>
        public delegate void imageLoadEventhandler(int count);

        /// <summary>
        /// Объявляем событие класса - окончание догрузки изображений
        /// </summary>
        public event imageLoadEventhandler imagesLoaded;
        /// <summary>
        /// Объявляем событие класса - окончание догрузки информации о треде
        /// </summary>
        public event threadInfoLoadedEventhandler threadInfoLoaded;

        /// <summary>
        /// Класс, работающий с авесомиумом
        /// </summary>
        AwesomiumCore aw;

        /// <summary>
        /// Поток загрузки
        /// </summary>
        Thread downloadThread;

        /// <summary>
        /// Инициализация класса загрузки картинок из треда
        /// </summary>
        /// <param name="savePath">Путь сохранения картинок, данного треда</param>
        public imageDownloader(string savePath)
        {
            //Инициализируем всё
            initClases();
            initVariables(savePath);
            initEvents();
        }

        /// <summary>
        /// Инициализация классов
        /// </summary>
        private void initClases()
        {
            aw = new AwesomiumCore();
        }

        /// <summary>
        /// Инициализация переменных
        /// </summary>
        private void initVariables(string savePath)
        {
            this.savePath = savePath;
            downloadThread = new Thread(startDownloadThread);
        }


        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void initEvents()
        {
            aw.browserPageLoaded += Aw_browserPageLoaded;
        }


        /// <summary>
        /// Событие завершения догрузки страницы
        /// </summary>
        private void Aw_browserPageLoaded(object sender, EventArgs e)
        {
            //Запускаем работу, в отдельном потоке
            downloadThread.Start();
        }

        /// <summary>
        /// Запускаем загрузку треда
        /// </summary>
        /// <param name="url">Путь к треду</param>
        public void initDownloadThread(string url)
        {
            try
            {
                new Thread(delegate ()
                {
                    //Запускаем загрузку страницы
                    aw.Invoke(new Action(delegate ()
                    {
                        aw.wv.Source = new Uri(url);
                    }));
                }).Start();
            }
            catch { }
        }

        /// <summary>
        /// Запуск догрузки треда
        /// </summary>
        private void startDownloadThread()
        {
            string downloadPath;
            //Получаем имя треда
            string name = getThreadName();
            //Получаем список всех изображений
            string[] urlList = getAllImagesUrl();


            //Вызываем ивент возврата инфы о треде
            threadInfoLoaded?.Invoke(name, urlList.Length);

            //Формируем путь сохранения
            downloadPath = savePath + name + @"\";
            //Если данного каталога ещё не существует, то создаём его
            Directory.CreateDirectory(downloadPath);

            //Запускаем загрузку картинок
            downloadFilesThread(urlList, downloadPath);
        }

        /// <summary>
        /// Подгружаем имя треда
        /// </summary>
        /// <returns>Название треда</returns>
        private string getThreadName()
        {
            string ex = "";
            // /d/ - traps. any kind of trap - Hentai/Alternative - 4chan

            try
            {
                //Выполняем скрипт запроса заголовка
                string title = aw.returnInvoke(delegate ()
                {
                    return aw.wv.ExecuteJavascriptWithResult(@"
                        document.title                        
                    ").ToString();
                });
                //Делим строку на части
                string[] parts = title.Split(new string[] { "-" }, StringSplitOptions.RemoveEmptyEntries);
                //Выбираем нужный кусок названия
                ex = (parts.Length > 1) ? parts[1] : parts[0];

                //Удаляем теоретически кривые части названия
                ex = ex.Replace(".", "");
                ex = ex.Replace("/", "");
                ex = ex.Replace("\\", "");
            }
            catch { }

            return ex;
        }


        /// <summary>
        /// Загружаем адреса всех картинок со страницы
        /// </summary>
        /// <returns></returns>
        private string[] getAllImagesUrl()
        {
            string[] ex = new string[0];
            string urlsList;
            
            try
            {
                //Выполняем скрипт запроса всех ссылок в основном потоке
                urlsList = aw.returnInvoke(delegate()
                {
                    return aw.wv.ExecuteJavascriptWithResult(@"
                        var elems = document.getElementsByClassName('fileThumb');
                        var elem;
                        var ex = '';
                        for (var i = 0; i < elems.length; i++)
                        {
	                        elem = elems[i];
	                        ex += elem.getAttribute('href') + '||||';
                        }
                        ex
                    ").ToString();
                });

                //Парсим список урлов, на отдельные адреса
                ex = urlsList.Split(new string[] { "||||" }, StringSplitOptions.RemoveEmptyEntries);


                //Удаляем со всех урлов слеши в начале
                for (int i = 0; i < ex.Length; i++)
                    ex[i] = "http:" + ex[i];
            }
            catch { }

            return ex;
        }

        /// <summary>
		/// Грузим файлец
		/// </summary>
		/// <param name="url">Путь к файлу на с ерваке</param>
		/// <param name="path">Путь к сохранению файла</param>
        private void downloadFile(string url, string path)
        {
            //Отправляем запрос на файл
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
       //     request.Proxy = new WebProxy("https://lv-134-87-2.fri-gate.biz:443");
            //Запрашиваем асинхронный ответ
            Task<WebResponse> tsk = request.GetResponseAsync();

            //Сохраняем файл
            using (WebResponse resp = tsk.Result)
                using (Stream stream = resp.GetResponseStream())
                    using (var fs = new FileStream(path, FileMode.Create))
                        stream.CopyTo(fs);                    
        }

       

        /// <summary>
        /// Получаем имя загружаемого файла, из его URL
        /// </summary>
        /// <param name="url">Ссылка на файл на сервере</param>
        /// <returns>Имя файла</returns>
        private string getFileName(string url)
        {
            return Path.GetFileName(new Uri(url).LocalPath);
        }

        /// <summary>
        /// Поток, осуществляющий загрузку всех файлов картинок
        /// </summary>
        private void downloadFilesThread(string[] urlList, string downloadPath)
        {
            string url, filePath;

            //Грузим все файлы по списку
            for(int i = 0; i < urlList.Length; i++)
            {
                try
                {
                    url = urlList[i];
                    filePath = downloadPath + getFileName(url);
                    //Если такого файла ещё не было
                    if (!File.Exists(filePath))
                    {
                        //Запускаем асинхронную загрузку картинки
                        downloadFile(url, filePath);
                        //Ожидание, между подгрузками файлов
                        Thread.Sleep(100);
                    }
                    
                    //Вызываем событие, означающее завершение догрузки картинки
                    imagesLoaded?.Invoke(i);
                }
                catch { }
            }
        }

        /// <summary>
        /// Заканчиваем работу
        /// </summary>
        public void Close()
        {
            aw.Dispose();
            //Закрываем поток загрузки
            if (downloadThread != null)
                downloadThread.Abort();
        }
    }    
}
