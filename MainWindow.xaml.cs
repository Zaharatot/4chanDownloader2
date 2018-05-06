using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using _4chanDownloader2.Clases;
using _4chanDownloader2.Clases.Wrappers;

namespace _4chanDownloader2
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Основной рабочий класс
        /// </summary>
        mainWorker mw;

        public MainWindow()
        {
            InitializeComponent();
            initClases();
            initEvents();
        }

        /// <summary>
        /// Инициализация классов
        /// </summary>
        private void initClases()
        {
            mw = new mainWorker();
        }

        /// <summary>
        /// Инициализация событий
        /// </summary>
        private void initEvents()
        {
            mw.threadsLoadComplete += Mw_threadsLoadComplete;
            mw.updateTable += Mw_updateTable;
            mw.updateProgressBars += Mw_updateProgressBars;
            this.Closed += MainWindow_Closed;
        }

        /// <summary>
        /// Ивент обновления прогрессбаров
        /// </summary>
        /// <param name="selectCount">Максимум прогрессбара текущего</param>
        /// <param name="selectDownload">Значение текущего прогрессбара</param>
        /// <param name="fullCount">Максимум общего прогрессбара</param>
        /// <param name="fillComplete">Текущее значение общего прогрессбара</param>
        private void Mw_updateProgressBars(int selectCount, int selectDownload, int fullCount, int fillComplete)
        {
            this.Dispatcher.BeginInvoke(new Action(delegate ()
            {
                if (mainProgrsssBar.Value > fullCount)
                    mainProgrsssBar.Value = fullCount;
                if (threadProgrsssBar.Value > selectCount)
                    threadProgrsssBar.Value = selectCount;

                mainProgrsssBar.Maximum = fullCount;
                threadProgrsssBar.Maximum = selectCount;

                threadProgrsssBar.Value = selectDownload;
                mainProgrsssBar.Value = fillComplete;
            }));
        }

        /// <summary>
        /// Событие закрытия формы
        /// </summary>
        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //Завершаем работу
            mw.closeWork();
        }

        /// <summary>
        /// Событие обновления таблицы тредов
        /// </summary>
        /// <param name="threads">Список тредов</param>
        private void Mw_updateTable(List<threadWrapper> threads)
        {
            try
            {
                this.Dispatcher.BeginInvoke(new Action(delegate ()
                {
                    //Отгружаем всё в таблицу
                    threadsTable.ItemsSource = threads;
                }));
            }
            catch { }
        }

        /// <summary>
        /// Событие догрузки всех тредов
        /// </summary>
        private void Mw_threadsLoadComplete(object sender, EventArgs e)
        {
            MessageBox.Show("Download complete!");
        }


        /// <summary>
        /// Клик по кнопке начала загрузки тредов
        /// </summary>
        private void startWorkButton_Click(object sender, RoutedEventArgs e)
        {
            //Запускаем загрузку тредов
            mw.startWork();
        }
    }
}
