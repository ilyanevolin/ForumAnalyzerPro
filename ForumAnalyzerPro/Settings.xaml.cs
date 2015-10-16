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

namespace ForumAnalyzerPro
{

    public partial class Settings : MetroWindow
    {
        public int MaxRequests { get; set; }
        public int MinSigs { get; set; }
        public int Threads { get; set; }
        public bool Saved { get; private set; }
        private bool AnyChanges = false;

        public Settings()
        {
            InitializeComponent();
            this.Closing += Window_Closing;

            btnSave_Click(null, null);
            Saved = false; AnyChanges = false;    
        }

        public new void Show()
        {
            numMaxRequests.Value = MaxRequests = (int)UserProperties.GetProperty("MaxRequests");
            numMinSigs.Value = MinSigs = (int)UserProperties.GetProperty("MinSigs");
            numThreads.Value = Threads = (int)UserProperties.GetProperty("Threads");
            btnSave_Click(null, null);
            Saved = false; AnyChanges = false;            
            ShowDialog();
        }


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            if (AnyChanges && !Saved)
            {
                var r = MessageBox.Show("Would you like to save before closing?", "Save", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (r == MessageBoxResult.Yes)
                {
                    btnSave_Click(sender, null);
                    this.Visibility = Visibility.Hidden;
                }
                else if (r == MessageBoxResult.No)
                {
                    this.Visibility = Visibility.Hidden;
                }
                else
                { }
            }
            else
            {
                this.Visibility = Visibility.Hidden;
                AnyChanges = false;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Threads = (int)numThreads.Value; UserProperties.SetProperty("Threads", Threads);
            MinSigs = (int)numMinSigs.Value; UserProperties.SetProperty("MinSigs", MinSigs);
            MaxRequests = (int)numMaxRequests.Value; UserProperties.SetProperty("MaxRequests", MaxRequests);
            Saved = true; AnyChanges = false;
            this.Hide();
        }
        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            numMaxRequests.Value = MaxRequests = 100;
            numMinSigs.Value = MinSigs = 10;
            numThreads.Value = Threads = 5;
        }

        private void num_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double?> e)
        {
            AnyChanges = true;
        }
    }
}
