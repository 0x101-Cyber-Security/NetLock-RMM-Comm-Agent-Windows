using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetLock_RMM_Comm_Agent_Windows
{
    internal class Application_Paths
    {
        // Paths
        public static string c_temp = @"C:\temp";
        
        // NetLock Paths
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
        public static string program_data_server_config_json = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\server_config.json";
        public static string program_data_debug_txt = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\debug.txt";
        public static string program_data_scripts = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Scripts";
        public static string program_data_microsoft_defender_antivirus_eventlog_backup = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Microsoft Defender Antivirus\Microsoft-Windows-Windows Defender Operational.bak";
        public static string program_data_microsoft_defender_antivirus_scan_jobs = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Microsoft Defender Antivirus\Scan Jobs";
        public static string program_data_jobs = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\Microsoft Defender Antivirus\Scan Jobs";

        public static string program_data_netlock_policy_database = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\policy.nlock";
        public static string program_data_netlock_events_database = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\events.nlock";

        // Reg Keys
        public static string netlock_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent";
        public static string netlock_microsoft_defender_antivirus_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\Microsoft Defender Antivirus";
        public static string netlock_yara_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\YARA";
        public static string netlock_sensors_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\Sensors";
        public static string netlock_sensor_management_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\Sensor_Management";
        public static string netlock_log_connector_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\Log_Connector";
        public static string netlock_rustdesk_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\RustDesk";
        public static string netlock_support_mode_reg_path = @"SOFTWARE\WOW6432Node\NetLock RMM\Comm Agent\Support_Mode";
        public static string hklm_run_directory_reg_path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        // Other
        public static string just_installed = @"C:\ProgramData\0x101 Cyber Security\NetLock RMM\Comm Agent\just_installed.txt";
    }
}
