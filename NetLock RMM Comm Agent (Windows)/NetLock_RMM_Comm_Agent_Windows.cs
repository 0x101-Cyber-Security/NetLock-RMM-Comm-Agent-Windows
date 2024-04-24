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
using System.Timers;
using _x101.HWID_System;
using NetLock_RMM_Comm_Agent_Windows.Initialization;
using NetLock_RMM_Comm_Agent_Windows.Online_Mode;

namespace NetLock_RMM_Comm_Agent_Windows
{
    public partial class Service : ServiceBase
    {
        public static bool debug_mode = false; //enables/disables logging

        // Server config
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

        // Server communication
        public static string communication_server = String.Empty;
        public static string trust_server = String.Empty;
        public static string update_server = String.Empty;

        // Timers
        public static System.Timers.Timer start_timer;
        public static System.Timers.Timer sync_timer;

        // Status
        public static bool connection_status = false;

        //Lists
        public static string processes_list = "[]";


        public void ServiceAsync()
        {
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.Service", "Service started", "Service started");

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
                Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.OnStart", "Server_Config_Handler.Load", "Failed to load server config");
                Stop();
            }

            // Setup synchronize timer
            try
            {
                sync_timer = new System.Timers.Timer(600000); //sync 10 minutes
                sync_timer.Elapsed += new ElapsedEventHandler(Initialize);
                sync_timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("NetLock_RMM_Comm_Agent_Windows.OnStart", "Start sync_timer", ex.ToString());
            }

            //Start Init Timer. We are doing this to get the service instantly running on service manager. Afterwards we will dispose the timer in Synchronize function
            try
            {
                start_timer = new System.Timers.Timer(2500);
                start_timer.Elapsed += new ElapsedEventHandler(Initialize);
                start_timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.OnStart", "Start start_timer", ex.ToString());
            }

            // Authenticate online
            //await Online_Mode.Authenticate.Authenticate_Online();
        }

        protected override void OnStop()
        {
            Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.Service", "Service stopped", "Service stopped");
        }

        private async void Initialize(object sender, ElapsedEventArgs e)
        {
            Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.Initialize", "Initialize", "Start");

            //Disable start timer to prevent concurrent executions
            if (start_timer.Enabled)
                start_timer.Dispose();

            // Check if connection to communication server is available
            connection_status = Check_Connection.Communication_Server().Result;

            // Online mode
            if (connection_status)
            {
                Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.Initialize", "connection_status", "Online mode.");

                // Check version
                bool up2date = await Initialization.Version_Handler.Check_Version();

                if (up2date) // No update required. Continue logic
                {
                    // Authenticate online
                    string auth_result = await Online_Mode.Handler.Authenticate();

                    // Check authorization status
                    //if (auth_result == "authorized" || auth_result == "not_synced" || auth_result == "synced")
                    if (authorized)
                    {
                        // Update device information
                        await Online_Mode.Handler.Update_Device_Information();
                    }

                    // Check sync status
                    if (auth_result == "authorized" || auth_result == "not_synced")
                    {
                        //Sync all settings & rulesets
                    }
                    else if (auth_result == "synced")
                    {
                        // placeholder, nothing to do here right now
                    }
                }
                else // Outdated. Trigger update
                {

                }
            }
        }

        private void Synchronize(bool force)
        {
            Logging.Handler.Debug("NetLock_RMM_Comm_Agent_Windows.Initialize", "Initialize", "Start");

            // Authenticate online

        }
    }
}
