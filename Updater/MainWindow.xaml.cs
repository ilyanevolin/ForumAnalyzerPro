using ForumAnalyzerPro.Common;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
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

namespace Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly string mainprogram = "ForumAnalyzerPro.exe";
        private Http request = new Http();

        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += Updater_Load;
            lblStatus.Content = "";
        }

        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Task t;
            t = Task.Factory.StartNew(() =>
            {
                update();
                if (checkv())
                {
                    Dispatcher.Invoke(new Action(delegate()
                        {
                            Thread.Sleep(1000);
                            try
                            {
                                lblStatus.Content = "Starting ForumAnalyzerPro...";
                                lblStatus.Foreground = new SolidColorBrush(Colors.Green);
                                Process.Start(AppDomain.CurrentDomain.BaseDirectory + mainprogram);
                                Thread.Sleep(1500);
                            }
                            catch { }
                            this.Close();
                        }));
                }
            });
        }
        private void Updater_Load(object sender, EventArgs e)
        {
            Task t;
            if (!File.Exists(AppDomain.CurrentDomain.BaseDirectory + mainprogram))
            {
                btnUpdate.IsEnabled = false;
                lblStatus.Content = "Downloading...";
                t = Task.Factory.StartNew(() =>
                {
                    using (WebClient client = new WebClient())
                    {
                        client.DownloadFile("https://healzer.com/forumanalyzer/ForumAnalyzerPro.exe", "./" + mainprogram);
                    }
                })
                .ContinueWith((prev_task) =>
                {
                    if (prev_task.Exception != null)
                    {
                        Dispatcher.Invoke(new Action(delegate()
                        {
                            btnUpdate.IsEnabled = true;
                            lblStatus.Content = "Download failed \n Try again or contact support.";
                            lblStatus.Foreground = new SolidColorBrush(Colors.Red);
                        }));
                    }
                    else
                    {
                        Dispatcher.Invoke(new Action(delegate()
                        {
                            btnUpdate.IsEnabled = false;
                            lblStatus.Content = "Please wait...";
                        }));
                    }
                })
                .ContinueWith((prev_task) =>
                {
                    t = Task.Factory.StartNew(() =>
                    {
                        checkv();
                    });
                });
            }
            else
            {
                t = Task.Factory.StartNew(() => { checkv(); });
            }

        }

        private void update()
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate()
                {
                    btnUpdate.IsEnabled = false;
                    lblStatus.Content = "Please wait...";
                }));

                int attempts = 0;
                while (File.Exists(AppDomain.CurrentDomain.BaseDirectory + mainprogram))
                {
                    try
                    {
                        foreach (var process in Process.GetProcessesByName(mainprogram.Replace(".exe", "")))
                        {
                            process.Kill();
                        }
                        File.Delete(AppDomain.CurrentDomain.BaseDirectory + mainprogram);
                        ++attempts;
                    }
                    catch (Exception ex)
                    {
                        string msg = "Error U117." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                        Logging.Log("user", "updater", msg);
                    }
                    if (attempts > 5) throw new Exception();
                }
                using (WebClient client = new WebClient())
                {
                    client.DownloadFile("https://healzer.com/forumanalyzer/ForumAnalyzerPro.exe", "./" + mainprogram);
                }

            }
            catch (Exception ex)
            {
                string msg = "Error U147." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "updater", msg);
                MessageBox.Show("Something went wrong. Please try again or contact support.", "ERROR"); Environment.Exit(Environment.ExitCode);
            }
        }
        private bool checkv()
        {
            bool equal = false;
            try
            {
                Dispatcher.Invoke(new Action(delegate()
                {
                    btnUpdate.IsEnabled = false;
                }));

                string pv = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + mainprogram).ProductVersion;
                string cpv = request.GET("https://healzer.com/forumanalyzer/update.txt", "", null, null);

                equal = pv.Equals(cpv);

                Dispatcher.Invoke(new Action(delegate()
                {
                    lblStatus.Content = "Your version: " + pv + "\nLatest version: " + cpv; ;
                    lblStatus.Foreground = new SolidColorBrush(Colors.Green);
                    btnUpdate.IsEnabled = false;
                }));
                if (int.Parse(cpv.Replace(".", "")) > int.Parse(pv.Replace(".", "")))
                {
                    Dispatcher.Invoke(new Action(delegate()
                    {
                        lblStatus.Foreground = new SolidColorBrush(Colors.Red);
                        btnUpdate.IsEnabled = true;
                    }));
                }
            }
            catch (Exception ex)
            {
                string msg = "Error U184." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "updater", msg);
                MessageBox.Show("Something went wrong. Please try again or contact support.", "ERROR"); Environment.Exit(Environment.ExitCode);
                equal = false;
            }

            return equal;

        }
    }
}
