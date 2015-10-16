using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using ForumAnalyzerPro.Algorithms;
using ForumAnalyzerPro.Common;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.ComponentModel;
using ForumAnalyzerPro.Helpers;
using System.IO;
using System.Net;
using System.Diagnostics;
using System.Windows.Threading;
using System.Reflection;

namespace ForumAnalyzerPro
{

    public partial class MainWindow : MetroWindow
    {
        private IList<Uri> sites;
        private BackgroundWorker worker;
        private IList<ForumSigType> result = new List<ForumSigType>();
        private Http http;
        private Http app_http;
        private Settings settings;
        private About about;
        private DispatcherTimer tmrUpdate;

        public MainWindow()
        {
            InitializeComponent();
            this.Title = "Forum Analyzer Pro" + "  " + Assembly.GetEntryAssembly().GetName().Version;
            app_http = new Http();
            Init();
            LoadBgWorker();
        }
        
        private void LoadBgWorker()
        {
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.WorkerSupportsCancellation = true;

        }
        private void Init()
        {
            
            UserProperties.OnLoad();
            settings = new Settings();
            about = new About();
            sites = new List<Uri>();
            mnuUpdate.Visibility = System.Windows.Visibility.Collapsed;
            ((Panel)step2.Parent).Children.Remove(step2);
            dgv.ItemsSource = result;

            tmrUpdate = new DispatcherTimer();
            tmrUpdate.Interval = new TimeSpan(0, 5, 0); ;
            tmrUpdate.Tick += tmrUpdate_Elapsed;
            tmrUpdate.Start();
        }
        private void MetroWindow_Closing(object sender, CancelEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #region step1
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ClearDGV();
            lblStatus.Text = ""; sites.Clear();
            if (txtSites.Text.Length < 1)
            { lblStatus.Text = "Please enter URL(s) first."; return; }
            if (FillSites())
                Calculate();
        }
        private bool FillSites()
        {
            try
            {
                int i = 0;
                IList<string> invalid_sites = new List<string>();
                foreach (string s in txtSites.Text.Split('\n'))
                {
                    ++i; if (s.Length <= 4 && s.Length > 0) { invalid_sites.Add(s); continue; } string v = s.Trim(); CheckUrl(ref v); var arr = v.Split('/');
                    if (Regex.IsMatch(arr[arr.Length - 1], "\\.(\\d|.){3}$"))
                    { arr[arr.Length - 1] = ""; v = String.Join("/", arr); }
                    if (Http.ValidUri(v) && Http.ValidUrl(v)) sites.Add(new Uri(v)); else invalid_sites.Add(s);
                }
                if (invalid_sites.Count > 0)
                { InvalidSites(ref invalid_sites); return false; }
                else { txtSites.Text = String.Join("\n", sites.Select(x => x.AbsoluteUri).Cast<string>().ToArray()); return true; }
            }
            catch (Exception ex)
            {
                string msg = "Error MW211." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                Logging.Log("user", "MW", msg);
                return false;
            }
        }
        private void CheckUrl(ref string v)
        {
            if (v[v.Length - 1].ToString() != "/") v += "/";
            if (!v.Contains("http") && v.Contains("www")) v = "http://" + v;
        }
        private void InvalidSites(ref IList<string> invalid_sites)
        {
            lblStatus.Text = "Invalid URL: " + invalid_sites[0];
        }
        private void Calculate()
        {
            transitioning.Content = step2;
            step2.Visibility = System.Windows.Visibility.Visible;
            btnReturnStep1.Visibility = System.Windows.Visibility.Collapsed;
            btnStop.Visibility = System.Windows.Visibility.Visible;
            mnuSettings.IsEnabled = false;

            lblStatus.Text = "Analyzing...";
            worker.RunWorkerAsync();
        }
        #endregion

        #region step2
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            http = new Http();
            foreach (Uri s in sites)
            {
                try
                {
                    worker.ReportProgress((int)((double)result.Count * 100 / (double)sites.Count));
                    ForumScrape fs = new ForumScrape(http);
                    ForumSigType set = fs.GetSignaturesFromForum(s, settings.MaxRequests, settings.MinSigs, settings.Threads);
                    if (((BackgroundWorker)sender).CancellationPending) return;
                    if (http.Aborted) break;
                    result.Add(set);
                    worker.ReportProgress((int)((double)result.Count * 100 / (double)sites.Count));
                    Dispatcher.Invoke(new Action(delegate()
                    { dgv.Items.Refresh(); }));
                }
                catch (Exception ex)
                {
                    string msg = "Error MW256." + Environment.NewLine + ex.Message + Environment.NewLine + (ex.InnerException != null ? ex.InnerException.Message : "");
                    Logging.Log("user", "MW", msg);
                }

            }
        }
        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            lblStatus.Text = "Analyzing... (" + e.ProgressPercentage + "%)";

        }
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lblStatus.Text = "Done!";
            btnReturnStep1.Visibility = System.Windows.Visibility.Visible;
            btnStop.Visibility = System.Windows.Visibility.Collapsed;
            mnuSettings.IsEnabled = true;
        }
        private void btnReturnStep1_Click(object sender, RoutedEventArgs e)
        {
            lblStatus.Text = "";
            ClearDGV();
            transitioning.Content = step1;
        }
        private void ClearDGV()
        {
            result.Clear(); dgv.Items.Refresh();
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            worker.CancelAsync();
            http.Abort();
            btnStop.IsEnabled = false;
        }
        #endregion

        #region menu
        private void mnuSettings_Click(object sender, RoutedEventArgs e)
        {
            settings.Show();
        }
        private void mnuAbout_Click(object sender, RoutedEventArgs e)
        {
            about.ShowDialog();
        }
        private void mnuExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void mnuSave_Click(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        private void tmrUpdate_Elapsed(object sender, EventArgs e)
        {
            string msg = "";
            if (Licensing.LoadCheckUpdate(app_http, ref msg, true))
            {
                mnuUpdate.Visibility = System.Windows.Visibility.Visible;
                tmrUpdate.Stop();
            }
        }
        private void mnuUpdate_Click(object sender, RoutedEventArgs e)
        {
            string msg = "";
            Licensing.LoadCheckUpdate(app_http, ref msg);
        }


    }
}
