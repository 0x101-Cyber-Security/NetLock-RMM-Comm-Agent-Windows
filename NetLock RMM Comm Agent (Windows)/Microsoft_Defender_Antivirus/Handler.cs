using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLock_RMM_Comm_Agent_Windows.Microsoft_Defender_Antivirus
{
    internal class Handler
    {
        public static void Init()
        {
            //If enabled, apply managed settings
            try
            {
                if (Service.microsoft_defender_antivirus_management_enabled)
                {
                    //Check if tray icon should be displayed
                    if (NetLock_Agent_Service.microsoft_defender_antivirus_g_ui_security_center_tray == false)
                        Helper.Helper.kill_security_center_tray_icon();

                    Set_Settings.Set_Settings.Do();
                    Scan_Job_Scheduler.Scan_Job_Scheduler.Check_Exececution();
                    Eventlog_Crawler.Eventlog_Crawler.Do();
                }
                else //If not, restore windows defender standard config
                {

                }
            }
            catch (Exception ex)
            {
                Logging.Logging.Error("Microsoft_Defender_AntiVirus.Handler.Init", "General Error", ex.Message);
            }
        }
    }
}
