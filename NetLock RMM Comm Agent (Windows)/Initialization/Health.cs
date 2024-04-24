using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting.Messaging;
using NetLock_RMM_Comm_Agent_Windows.Helper;

namespace NetLock_RMM_Comm_Agent_Windows.Initialization
{
    internal class Health
    {
        // Check if the directories are in place
        public static void Check_Directories()
        {
            try
            {
                // Program Data
                if (!Directory.Exists(Application_Paths.program_data))
                    Directory.CreateDirectory(Application_Paths.program_data);

                // Logs
                if (!Directory.Exists(Application_Paths.program_data_logs))
                    Directory.CreateDirectory(Application_Paths.program_data_logs);
                
                // Installer
                if (!Directory.Exists(Application_Paths.program_data_installer))
                    Directory.CreateDirectory(Application_Paths.program_data_installer);

                // Updates
                if (!Directory.Exists(Application_Paths.program_data_updates))
                    Directory.CreateDirectory(Application_Paths.program_data_updates);

                // NetLock Temp
                if (!Directory.Exists(Application_Paths.program_data_temp))
                    Directory.CreateDirectory(Application_Paths.program_data_temp);

                // C Temp
                if (!Directory.Exists(Application_Paths.c_temp))
                    Directory.CreateDirectory(Application_Paths.c_temp);
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Health.Directories", "", ex.Message);
            }
        }

        // Check if the registry keys are in place
        public static void Check_Registry()
        { 
            try
            {
                // Check if netlock key exists
                if (!Registry.HKLM_Key_Exists(Application_Paths.netlock_reg_path))
                    Registry.HKLM_Create_Key(Application_Paths.netlock_reg_path);

                // Check if msdav key exists
                if (!Registry.HKLM_Key_Exists(Application_Paths.netlock_microsoft_defender_antivirus_reg_path))
                    Registry.HKLM_Create_Key(Application_Paths.netlock_microsoft_defender_antivirus_reg_path);
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Health.Registry", "", ex.Message);
            }
        }

        // Check if the firewall rules are in place
        public static void Check_Firewall()
        {
            Windows_Defender_Firewall.Handler.NetLock_RMM_Comm_Agent_Rule_Inbound();
            Windows_Defender_Firewall.Handler.NetLock_RMM_Comm_Agent_Rule_Outbound();
            Windows_Defender_Firewall.Handler.NetLock_RMM_Health_Service_Rule();
            Windows_Defender_Firewall.Handler.NetLock_Installer_Rule();
            Windows_Defender_Firewall.Handler.NetLock_Uninstaller_Rule();
        }
    }
}
