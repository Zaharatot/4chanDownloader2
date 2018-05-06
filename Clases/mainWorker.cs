using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using _4chanDownloader2.Clases;
using _4chanDownloader2.Clases.Wrappers;
using System.IO;

namespace _4chanDownloader2.Clases
{
    /// <summary>
    /// основной рабочий класс
    /// </summary>
    class mainWorker
    {
        /// <summary>
        /// Делегат метода, обновления таблицы тредов
        /// </summary>
        /// <param name="threads">Список тредов</param>
        public delegate void updateThreadListEventhandler(List<threadWrapper> threads);
        /// <summary>
        /// Ивент обновления таблицы
        /// </summary>
        public event updateThreadListEventhandler updateTable;

        /// <summary>
        /// Делегат метода, обновления прогрессбаров
        /// </summary>
        public delegate void updateProgressEventhandler(int selectCount, int selectDownload, int fullCount, int fillComplete);
        /// <summary>
        /// Ивент обновления таблицы
        /// </summary>
        public event updateProgressEventhandler updateProgressBars;


        /// <summary>
        /// Ивент догрузки всех тредов
        /// </summary>
        public event EventHandler threadsLoadComplete;


        /// <summary>
        /// Список потоков
        /// </summary>
        List<boardThread> threadList;
        /// <summary>
        /// Класс вытягивающий из буфера обмена адреса
        /// </summary>
        urlWorker uw;
        /// <summary>
        /// Класс, выполняющий загрузку всех картинок треда
        /// </summary>
        imageDownloader id;

        /// <summary>
        /// Флаг загрузки. Проставляется когда началась догрузка файлов
        /// </summary>
        bool downloadFlag;
        /// <summary>
        /// Путь сохранения картинок
        /// </summary>
        string savePath;
        /// <summary>
        /// Идентификтаор текущего выбранного треда
        /// </summary>
        int selectThread;

        /// <summary>
        /// Инициализация основного рабочего класса
        /// </summary>
        public mainWorker()
        {
            //Инициализируем всё
            initVariables();
            initClases();
            initEvents();

            //Запускаем проверку адресов
            uw.start();
        }

        /// <summary>
        /// Инициализация классов
        /// </summary>
        private void initClases()
        {
            uw = new urlWorker();
            //Инициализиоруем загрузчик тредов
            id = new imageDownloader(savePath);
        }

        /// <summary>
        /// Инициализация переменных
        /// </summary>
        private void initVariables()
        {
            //Получаем путь к текущей папке
            savePath = Environment.CurrentDirectory + @"\Downloads\";
            threadList = new List<boardThread>();
            downloadFlag = false;
        }


        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void initEvents()
        {
            uw.findUrl += Uw_findUrl;
            id.imagesLoaded += Id_imagesLoaded;
            id.threadInfoLoaded += Id_threadInfoLoaded;
        }


        /// <summary>
        /// Ивент догрузки инфы о треде
        /// </summary>
        /// <param name="name">Имя треда</param>
        /// <param name="countImages">Общее количество картинок в треде</param>
        private void Id_threadInfoLoaded(string name, int countImages)
        {
            //Обновляем инфу о треде
            threadList[selectThread].threadName = name;
            threadList[selectThread].countImages = countImages;

            //Обновляем таблицу на форме
            updateThreadsTable();
        }

        /// <summary>
        /// Ивент догрузки картинок
        /// </summary>
        /// <param name="count">Id текущей загруженной картинки</param>
        private void Id_imagesLoaded(int count)
        {
            //Обновляем инфу о треде
            threadList[selectThread].countDownloadImages = count + 1;
            //Если все картинки были слиты, то
            if (threadList[selectThread].testLoadThread())
                //Пытаемся загрузить новый тред
                loadNextThread();
            else
            {
                //Обновляем таблицу на форме
                updateThreadsTable();
                //Обновляем прогрессбары
                updateProgressBarsData();
            }
        }

        /// <summary>
        /// Событие нахождения адреса в буфере обмена
        /// </summary>
        /// <param name="url">Вдрес</param>
        private void Uw_findUrl(string url)
        {
            //Если мы всё ещё не грузим файлы, и если в нашем списке нету такого треда
            if (!downloadFlag && (threadList.Count(t => (t.url.Equals(url))) == 0))
            {
                //Добавляем тред
                threadList.Add(new boardThread(url));
                //Обновляем таблицу на форме
                updateThreadsTable();
            }
        }

        /// <summary>
        /// Запускаем работу
        /// </summary>
        public void startWork()
        {
            //Если у нас есть хоть один тред, для загрузки
            if (threadList.Count > 0)
            {
                downloadFlag = true;
                selectThread = 0;
                loadThread();
            }
        }

        /// <summary>
        /// Завершение работы
        /// </summary>
        /// <param name="exit">False - Если завершаем работу</param>
        public void stopWork()
        {
            //Говорим, что можно снова набирать адреса
            downloadFlag = false;

            //Обновляем таблицу на форме
            updateThreadsTable();
            //Обновляем таблицу на форме
            updateThreadsTable();
            //Обновляем прогрессбары
            updateProgressBarsData();
            //Вызываем ивент завершения догрузки тредов
            threadsLoadComplete?.Invoke(null, new EventArgs());
        }

        /// <summary>
        /// Завершаем работу
        /// </summary>
        public void closeWork()
        {
            uw.stop();
            downloadFlag = false;
            if(id != null)
                id.Close();
        }

        /// <summary>
        /// Выполняем загрузку следующего треда
        /// </summary>
        private void loadNextThread()
        {
            //Указываем, что тред загружен
            threadList[selectThread].state = downloadStatus.Завершён;
            //Переходим к новому треду
            selectThread++;
            //Если треды под загрузку ещё есть
            if (selectThread < threadList.Count)
                //Грузим следующий
                loadThread();
            //Иначе - завершаем работу
            else
                stopWork();
        }

        /// <summary>
        /// Запускаем загрузку треда
        /// </summary>
        private void loadThread()
        {
            //Запускаем загрузку текущего треда
            threadList[selectThread].state = downloadStatus.Загрузка;
            id.initDownloadThread(threadList[selectThread].url);
            //Обновляем таблицу на форме
            updateThreadsTable();
            //Обновляем прогрессбары
            updateProgressBarsData();
        }

        /// <summary>
        /// Обновляем таблицу, со всеми записанными тредами
        /// </summary>
        /// <returns>Таблица тредов</returns>
        private void updateThreadsTable()
        {
            List<threadWrapper> ex = new List<threadWrapper>();

            //Получаем строку, для таблицы, из всех тредов
            lock (threadList)
                foreach (var th in threadList)
                    ex.Add(th.getTableString());

            //Вызываем ивент
            updateTable?.Invoke(ex);
        }

        /// <summary>
        /// Обновляем прогрессбары
        /// </summary>
        private void updateProgressBarsData()
        {
            //Если треды под загрузку ещё есть
            if (selectThread < threadList.Count)
            {
                var select = threadList[selectThread];
                //Считаем количество завершённых
                int ct = threadList.Count(th => (th.state == downloadStatus.Завершён));
                //Вызываем ивент обновления прогрессбаров
                updateProgressBars?.Invoke(select.countImages, select.countDownloadImages, threadList.Count, ct);
            }
        }
    }
}
