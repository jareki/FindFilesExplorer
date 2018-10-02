using FindFilesExplorer.CS;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
using System.Windows.Threading;

namespace FindFilesExplorer.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Explorer ex;
        DispatcherTimer timer;
        TimeSpan elapsedtime;
        int count;
        public ObservableCollection<Node> TreeNodes;

        public MainWindow()
        {
            InitializeComponent();
            timer = new DispatcherTimer();
            timer.Tick += Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 1);

            TreeNodes = new ObservableCollection<Node>();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            elapsedtime += timer.Interval;
            TimeTxt.Text = $"Прошло времени: {elapsedtime.ToString("hh\\:mm\\:ss")}";
        }

        void ChangeState(string newstate)
        {
            switch (newstate)
            {
                case "Старт":
                    StartPauseBtn.Content = "Пауза";
                    StopBtn.IsEnabled = true;

                    ex = new Explorer(DirTxt.Text,MaskTxt.Text, SearchTxt.Text);
                    ex.FileExploring += Ex_FileExploring;
                    ex.FileFound += Ex_FileFound;
                    ex.SearchEnded += Ex_SearchEnded;

                    count = 0;
                    TreeNodes.Clear();
                    elapsedtime = new TimeSpan(0, 0, 0);
                    timer.Start();
                    ex.Start();

                    break;
                case "Пауза":
                    StartPauseBtn.Content = "Продолжить";
                    timer.Stop();
                    ex.Pause();
                    break;
                case "Продолжить":
                    StartPauseBtn.Content = "Пауза";

                    timer.Start();
                    ex.Resume();
                    break;
                case "Закончить":
                    StartPauseBtn.Content = "Старт";
                    StopBtn.IsEnabled = false;

                    timer.Stop();
                    ex.Stop();
                    break;
            }
        }

        private void Ex_SearchEnded(object sender, EventArgs e)
        {
            ChangeState("Закончить");
        }

        private void Ex_FileFound(object sender, ExplorerEventArgs e)
        {
            string[] dirs = e.FileName.Split('\\');
            Node node=null;
            for (int i=0;i<dirs.Count();i++)
            {
                if (i==0)
                {
                    node = TreeNodes.Where(n => n.Name == dirs[i]).FirstOrDefault();
                    if (node==null)
                    {
                        node = new Node() { Name = dirs[i], Nodes=new ObservableCollection<Node>() };
                        TreeNodes.Add(node);
                    }
                }
                else
                {
                    var temp = node.Nodes.Where(n => n.Name == dirs[i]).FirstOrDefault();
                    if (temp==null)
                    {
                        temp = new Node() { Name = dirs[i], Nodes = new ObservableCollection<Node>() };
                        node.Nodes.Add(temp);
                    }
                    node = temp;
                }
            }
            DirTree.ItemsSource = TreeNodes;
        }

        private void Ex_FileExploring(object sender, ExplorerEventArgs e)
        {
            string filename = e.FileName;
            if (filename.Length>20)
                    filename = $"{filename.Substring(0, filename.IndexOf('\\')+1)}"
                                + "..."
                                + $"{filename.Substring(e.FileName.LastIndexOf('\\'))}";
            StatusTxt.Text = $"Обрабатываем: {filename}, всего обработано: {++count}";
        }

        private void StartPauseBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangeState(((Button)sender).Content.ToString());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            StopBtn.IsEnabled = false;

            MaskTxt.Text = Properties.Settings.Default.Mask;
            DirTxt.Text = Properties.Settings.Default.InitDir;
            SearchTxt.Text = Properties.Settings.Default.Search;
        }

        private void ChooseDirBtn_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dlg = new System.Windows.Forms.FolderBrowserDialog();
            var result = dlg.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
                DirTxt.Text = dlg.SelectedPath;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Mask = MaskTxt.Text;
            Properties.Settings.Default.InitDir = DirTxt.Text;
            Properties.Settings.Default.Search = SearchTxt.Text;

            Properties.Settings.Default.Save();
        }
    }
}
