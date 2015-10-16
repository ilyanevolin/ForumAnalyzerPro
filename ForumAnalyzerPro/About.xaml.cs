using ForumAnalyzerPro.Helpers;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using System.Diagnostics;

namespace ForumAnalyzerPro
{

    public partial class About : MetroWindow
    {
        public int MaxRequests { get; set; }
        public int MinSigs { get; set; }
        public int Threads { get; set; }
        public bool Saved { get; private set; }

        public About()
        {
            InitializeComponent();
            this.Closing += Window_Closing;

            string pv = FileVersionInfo.GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "ForumAnalyzerPro.exe").ProductVersion;
            lblName.Content = "File Analyzer Pro v" + pv;
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        } 

        private void lblContact_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Process.Start("https://healzer.com/contactus/");
        }

    }
}
