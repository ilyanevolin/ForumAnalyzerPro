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
using System.Windows.Shapes;

namespace ForumAnalyzerPro
{

    public partial class LicensingForm : MetroWindow
    {

        private Http request;
        public bool MayContinue { get; private set; }

        public LicensingForm()
        {
            InitializeComponent();

            this.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            this.Title = "Registration";
            this.Topmost = true;
            lblStatus.Text = "";

            request = new Http();
        }

        private async void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string TID = txtLicense.Text.Trim().Replace("'", "").Replace("?", "").Replace("&", "").Replace(":", "");
            if (TID.Length == 0)
            {
                SetMessage("License ID is empty.", Colors.Red);
                txtLicense.Focus();
                return;
            }

            try
            {
                btnSubmit.IsEnabled = false;
                SetMessage("Validating License...", Colors.Red);
                Licensing.LicenseResponse resp = Licensing.CheckLicense(request, TID);
                if (resp.IsValid)
                {
                    MayContinue = true;
                    SetMessage("Starting...", Colors.Green);
                    await Task.Factory.StartNew(() => { Thread.Sleep(5000); });
                    {
                        MetroWindow next = new MainWindow();
                        App.Current.MainWindow = next;
                        this.Close();
                        next.Show();
                    }
                }
                else
                {
                    SetMessage(resp.Message, resp.color);
                }
            }
            catch { }
            finally { btnSubmit.IsEnabled = true; }
        }

        private void SetMessage(string msg, Color color)
        {
            lblStatus.Text = msg;
            lblStatus.Foreground = new SolidColorBrush(color);
        }

        private void lnkBuy_MouseDown(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start("https://healzer.com/forumanalyzer/");
        }
    }
}
