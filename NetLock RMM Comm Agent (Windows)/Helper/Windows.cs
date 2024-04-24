using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLock_RMM_Comm_Agent_Windows.Helper
{
    internal class Windows
    {
        public static string Windows_Version()
        {
            string operating_system = "-";

            try
            {
                bool windows11 = false;

                string _CurrentBuild = Registry.HKLM_Read_Value("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "CurrentBuild");
                string _ProductName = Registry.HKLM_Read_Value("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "ProductName");
                string _DisplayVersion = Registry.HKLM_Read_Value("SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion", "DisplayVersion");
               
                int CurrentBuild = Convert.ToInt32(_CurrentBuild);

                // If the build number is greater than or equal to 22000, it is Windows 11. Older windows 11 versions had wrong product names, stating they are windows 11. Knowlege from NetLock Legacy
                if (CurrentBuild > 22000 || CurrentBuild == 22000)
                    windows11 = true;

                if (windows11 == true)
                    operating_system = "Windows 11" + " (" + _DisplayVersion + ")";
                else
                    operating_system = _ProductName + " (" + _DisplayVersion + ")";

                if (operating_system == null)
                    operating_system = "";

                return operating_system;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("NetLock_RMM_Comm_Agent_Windows.Helper.Windows.Windows_Version", "Collect windows product name & version", ex.Message);
                return "-";
            }
        }

    }
}
