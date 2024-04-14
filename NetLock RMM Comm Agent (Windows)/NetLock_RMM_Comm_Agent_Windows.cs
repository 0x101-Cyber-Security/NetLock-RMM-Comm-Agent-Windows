using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using _x101.HWID_System;
using NetLock_RMM_Comm_Agent_Windows.Initialization;
using NetLock_RMM_Comm_Agent_Windows.Online_Mode;

namespace NetLock_RMM_Comm_Agent_Windows
{
    public partial class Service : ServiceBase
    {
        public static bool debug_mode = false; //enables/disables logging

        // Server config
        public static string communication_server = String.Empty;
        public static string main_communication_server = String.Empty;
        public static string fallback_communication_server = String.Empty;
        public static string main_update_server = String.Empty;
        public static string fallback_update_server = String.Empty;
        public static string main_trust_server = String.Empty;
        public static string fallback_trust_server = String.Empty;
        public static string access_key = String.Empty;

        public static string tenant_name = String.Empty;
        public static string location_name = String.Empty;
        public static string device_name = String.Empty;
        public static string hwid = String.Empty;
        public static bool authorized = false;

        public async void ServiceAsync()
        {
            // Load server config
            if (!await Server_Config_Handler.Load()) // 
            {
                Logging.Handler.Debug("OnStart", "Server_Config_Handler.Load", "Failed to load server config");
                Stop();
            }

            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            Logging.Handler.Debug("Service", "Service started", "Service started");

            hwid = ENGINE.HW_UID;
            device_name = Environment.MachineName;

            Health.Check_Directories();
            Health.Check_Registry();
            Health.Check_Firewall();

            /* Check OS version (legacy code for windows 7. Need to verify its still working & not causing security issues 
            string os_version = Environment.OSVersion.Version.ToString();
            char os_version_char = os_version[0];

            if (os_version_char == '6')
            {
                Logging.Handler.Debug("OnStart", "OS_Version", $"OS ({os_version_char}) is old. Switch to compatibility mode.");
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            }
            else
                Logging.Handler.Debug("OnStart", "OS_Version", $"OS ({os_version_char}) is new.");
            */

            // Load server config
            if (!await Server_Config_Handler.Load()) // 
            {
                Logging.Handler.Debug("OnStart", "Server_Config_Handler.Load", "Failed to load server config");
                Stop();
            }

            communication_server = main_communication_server;
            Logging.Handler.Debug("OnStart", "main_communication_server", main_communication_server);

            // hier weiter

            // Authenticate online
            await Online_Mode.Authenticate.Authenticate_Online();
        }

        protected override void OnStop()
        {
            Logging.Handler.Debug("Service", "Service stopped", "Service stopped");
        }
    }
}
