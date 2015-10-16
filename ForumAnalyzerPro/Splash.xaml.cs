using ForumAnalyzerPro.Common;
using ForumAnalyzerPro.Helpers;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
using System.Windows.Shapes;

namespace ForumAnalyzerPro
{
    public partial class Splash : MetroWindow
    {
        Http request;
        Licensing.LicenseResponse resp;

        private bool isValid;
        public bool IsValid { get { return isValid; } private set { isValid = value; } }
        public bool MayContinue { get; private set; }

        public Splash()
        {
            InitializeComponent();

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Title = "Forum Analyzer Pro" + "  " + Assembly.GetEntryAssembly().GetName().Version;
            this.Topmost = true;
            lblStatus.Text = "Loading...";

            request = new Http();
        }

        private async void SplashForm_Load(object sender, EventArgs e)
        {
            string msg ="";
            //=================================
            MayContinue = Licensing.LoadCheckUpdater(request, ref msg);
            if (!MayContinue) { MessageBox.Show(msg); Close(); return; }
            await Task.Factory.StartNew(() => { Thread.Sleep(500); });
            //=================================
            MayContinue = Licensing.LoadCheckUpdate(request, ref msg);
            if (!MayContinue) { MessageBox.Show(msg); Close(); return; }
            await Task.Factory.StartNew(() => { Thread.Sleep(500); });
            //=================================
            MayContinue = Licensing.LoadCheckLicense(request, ref msg, ref resp, ref isValid);
            if (!MayContinue) { MessageBox.Show(msg); Close(); return; }
            await Task.Factory.StartNew(() => { Thread.Sleep(500); });

            MetroWindow next = null;
            if (!IsValid)
                next = new LicensingForm();
            else
                next= new MainWindow();

            App.Current.MainWindow = next;
            this.Close();       
            next.Show();            
            
                             
            
            
        }

    }
}
