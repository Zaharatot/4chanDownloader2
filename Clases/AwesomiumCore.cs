using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Awesomium.Core;

namespace _4chanDownloader2.Clases
{
    /// <summary>
    /// Класс, работающий с авесомиумом в оконном режиме
    /// </summary>
    class AwesomiumCore
    {
        /// <summary>
        /// Ui браузера
        /// </summary>
        public WebView wv;
        /// <summary>
        /// Просто рандом
        /// </summary>
        Random r;
        /// <summary>
        /// Сессия браузера
        /// </summary>
        WebSession ss;
        /// <summary>
        /// Объявляем событие класса - окончание догрузки страницы
        /// </summary>
        public event EventHandler browserPageLoaded;

        /// <summary>
        /// Ссылка на основной поток
        /// </summary>
        Dispatcher uiThread;

        /// <summary>
        /// Состряпываем юзерагента
        /// </summary>
        /// <returns>Строка юзерагента</returns>
        public string compileUA()
        {
            string[] winVersions = new string[] {
                "Windows NT 10.0; WOW64",
                "Windows NT 6.1; WOW64",
                "Windows NT 6.0",
                "Windows NT 5.1",
                "Macintosh; Intel Mac OS X 10_8_2",
                "Windows NT 6.2; WOW64",
                "Macintosh; Intel Mac OS X 10_7_5",
                "X11; Linux x86_64",
                "Macintosh; Intel Mac OS X; U; en"
            };
            string ver = string.Format("Mozilla/5.0 ({0}) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/55.0.2883.87 Safari/537.36",
                winVersions[r.Next(0, winVersions.Length)]);
            return ver;
        }

        /// <summary>
        /// Инициализация класса, для работы с браузером
        /// </summary>
        public AwesomiumCore()
        {
            initVariables();
            initAwesomium();
            initEvents();
        }

        /// <summary>
        /// Инициализируем переменные
        /// </summary>
        private void initVariables()
        {
            r = new Random();
            //Получаем ссылку на основной поток
            uiThread = Dispatcher.CurrentDispatcher;
        }

        /// <summary>
        /// Инициализируем Awesomium
        /// </summary>
        private void initAwesomium()
        {
            //Инициализируем параметры запуска ядра авесомиума
            WebCore.Initialize(new WebConfig()
            {
                LogLevel = LogLevel.None,
                UserAgent = compileUA()
            });

            //Создаём сессию
            ss = WebCore.CreateWebSession(new WebPreferences
            {
                //   ProxyConfig = "https://lv-134-87-2.fri-gate.biz:443",
                WebSecurity = true,
                LoadImagesAutomatically = false,
                RemoteFonts = false,
                JavascriptApplicationInfo = false
            });


            //Инициализируем браузер, для работы с сайтом.
            wv = WebCore.CreateWebView(
                1680,
                1050,
                ss,
                WebViewType.Offscreen);
        }

        /// <summary>
        /// Инициализируем события
        /// </summary>
        private void initEvents()
        {
            //Добавляем обработчик догрузки страницы
            wv.DocumentReady += Wv_DocumentReady;
        }

        /// <summary>
        /// Обработчик загрузки страницы
        /// </summary>
        private void Wv_DocumentReady(object sender, DocumentReadyEventArgs e)
        {
            //Если страница догрузилась, и не идут действия
            if (e.ReadyState == DocumentReadyState.Loaded)
                //Вызываем событие, по которому контроллер браузера начнёт обрабатывать страницу
                browserPageLoaded?.Invoke(this, new EventArgs());            
        }


        /// <summary>
        /// Завершение класса рабоыт с браузером
        /// </summary>
        public void Dispose()
        {
            try
            {
                //Есби UI существует - закрываем его
                if (wv != null)
                {
                    wv.Stop();
                    wv.Dispose();
                }

                //Закрываем переменную сессии
                if(ss != null)
                    ss.Dispose();
                //Закрываем ядро броузера
                WebCore.Shutdown();
            }
            catch { }
        }



        /// <summary>
        /// Вызов функции, в птоке Авесомиума
        /// </summary>
        /// <param name="act">Событие, которое необходимо выполнить</param>
        /// <returns>True - если всё прошло удачно</returns>
        public bool Invoke(Action act)
        {
            bool ex = false;
            bool finWait = true;

            try
            {
                //Выполняем, если мейн форма существует
                if (uiThread != null)
                {
                    //Вызываем выполнение переданного события в основном потоке
                    uiThread.BeginInvoke(new Action(delegate () {
                        act();
                        finWait = false;
                    }));
                    //Ждём завершения выполнения действия
                    while (finWait) ;

                    ex = true;
                }
               
            }
            catch {  }
            //Возвращаем результат выполнения
            return ex;
        }


        /// <summary>
        /// Вызов функции, в птоке Авесомиума, с возвратом значения
        /// </summary>
        /// <param name="act">Событие, которое необходимо выполнить</param>
        /// <returns>Пустая строка, либо строка с ответом</returns>
        public string returnInvoke(Func<string> act)
        {
            string ex = "";
            bool finWait = true;

            try
            {
                //Выполняем, если мейн форма существует
                if (uiThread != null)
                {
                    //Вызываем выполнение переданного события в основном потоке
                    uiThread.BeginInvoke(new Action(delegate () {
                        ex = act();
                        finWait = false;
                    }));

                    //Ждём завершения выполнения действия
                    while (finWait) ;
                }
            }
            catch { }
            //Возвращаем результат выполнения
            return ex;
        }

    }
}
