using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;

namespace NetLock_RMM_Comm_Agent_Windows.Microsoft_Defender_Antivirus
{
    internal class Handler
    {
        public static void Initalization()
        {
            //If enabled, apply managed settings
            try
            {
                bool enabled = false;
                bool security_center_tray = false;

                using (JsonDocument document = JsonDocument.Parse(Service.policy_antivirus_settings_json))
                {
                    JsonElement enabled_element = document.RootElement.GetProperty("enabled");
                    enabled = Convert.ToBoolean(enabled_element.ToString());

                    JsonElement security_center_tray_element = document.RootElement.GetProperty("security_center_tray");
                    security_center_tray = Convert.ToBoolean(security_center_tray_element.ToString());
                }

                if (enabled)
                {
                    //Check if tray icon should be displayed
                    if (security_center_tray == false)
                        Kill_Security_Center_Tray_Icon();

                    Set_Settings.Do();
                    //Scan_Job_Scheduler.Scan_Job_Scheduler.Check_Exececution();
                    //Eventlog_Crawler.Eventlog_Crawler.Do();
                }
                else //If not, restore windows defender standard config
                {

                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Microsoft_Defender_AntiVirus.Handler.Initalization", "General Error", ex.ToString());
            }
        }

        public static void Kill_Security_Center_Tray_Icon()
        {
            if (Process.GetProcessesByName("SecurityHealthSystray").Length > 0)
            {
                Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Handler.Initalization", "Check security center tray icon presence", "Is present. Kill it...");

                try
                {
                    Process cmd_process = new Process();
                    cmd_process.StartInfo.UseShellExecute = true;
                    cmd_process.StartInfo.FileName = "cmd.exe";
                    cmd_process.StartInfo.Arguments = "/c taskkill /F /IM SecurityHealthSystray.exe";
                    cmd_process.Start();
                    cmd_process.WaitForExit();

                    Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Handler.Initalization", "Check security center tray icon presence", "Killed successfully.");
                }
                catch (Exception ex)
                {
                    Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Handler.Initalization", "Check security center tray icon presence", "Couldn't kill it: " + ex.Message);
                }
            }
        }
    }
}
