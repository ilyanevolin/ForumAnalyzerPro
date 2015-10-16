using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumAnalyzerPro.Helpers
{
    public static class UserProperties
    {
        public static void SetProperty(string name, object value)
        {
            Properties.Settings.Default[name] = value;
            Properties.Settings.Default.Save();
        }

        public static object GetProperty(string name)
        {

            return Properties.Settings.Default[name];
        }

        public static void OnLoad()
        {
            Properties.Settings.Default.Upgrade();
            Properties.Settings.Default.Save();
        }
    }
}
