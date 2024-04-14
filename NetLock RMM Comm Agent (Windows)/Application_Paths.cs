using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLock_RMM_Comm_Agent_Windows
{
    internal class Application_Paths
    {
        //Paths
        public static string c_temp = @"C:\temp";
        
        //NetLock Paths
        public static string netlock_service_exe = @"C:\Program Files\0x101 Cyber Security\NetLock RMM\Comm Agent\Comm Agent.exe";
        public static string netlock_health_service_exe = @"C:\Program Files\0x101 Cyber Security\NetLock RMM\Health\NetLock RMM Health Agent.exe";
        public static string netlock_installer_exe = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Installer\Installer.exe";
        public static string netlock_uninstaller_exe = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Uninstaller\Uninstaller.exe";

        public static string program_data = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent";
        public static string program_data_logs = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Logs";
        public static string program_data_installer = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Installer";
        public static string program_data_updates = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Updates";
        public static string program_data_temp = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Temp";
        public static string program_data_updates_service_package = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Updates\comm_agent.package";
        public static string program_data_justinstalled = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\clientjustinstalled";
        public static string program_data_server_config_json = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\server_config.json";
        public static string program_data_debug_txt = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\debug.txt";
        
        //Reg Keys
        public static string netlock_reg_path = @"SOFTWARE\WOW6432Node\NetLock";
        public static string netlock_microsoft_defender_antivirus_reg_path = @"SOFTWARE\WOW6432Node\NetLock\Microsoft_Defender_Antivirus";
        public static string netlock_yara_reg_path = @"SOFTWARE\WOW6432Node\NetLock\YARA";
        public static string netlock_sensors_reg_path = @"SOFTWARE\WOW6432Node\NetLock\Sensors";
        public static string netlock_sensor_management_reg_path = @"SOFTWARE\WOW6432Node\NetLock\Sensor_Management";
        public static string netlock_log_connector_reg_path = @"SOFTWARE\WOW6432Node\NetLock\Log_Connector";
        public static string netlock_rustdesk_reg_path = @"SOFTWARE\WOW6432Node\NetLock\RustDesk";
        public static string netlock_support_mode_reg_path = @"SOFTWARE\WOW6432Node\NetLock\Support_Mode";
        public static string hklm_run_directory_reg_path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    }
}
