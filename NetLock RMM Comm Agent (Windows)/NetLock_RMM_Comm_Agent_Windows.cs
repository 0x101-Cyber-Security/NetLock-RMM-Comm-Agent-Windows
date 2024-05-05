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
using System.IO;
using Microsoft.Win32;
using NetLock_RMM_Comm_Agent_Windows.Events;

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
        public static System.Timers.Timer events_timer;

        // Status
        public static bool connection_status = false;
        public static bool first_sync = true;
        public static bool sync_active = true;
        public static bool events_processing = true; //Tells that the events are currently being processed and tells the Init to wait until its finished
        public static bool process_events = false; //Indicates if events should be processed. Its being locked by the client settings loader

        //Lists
        //public static string processes_list = "[]";

        //Policy
        public static string policy_antivirus_settings_json = String.Empty;
        public static string policy_antivirus_exclusions_json = String.Empty;
        public static string policy_antivirus_scan_jobs_json = String.Empty;
        public static string policy_antivirus_controlled_folder_access_folders_json = String.Empty;
        public static string policy_antivirus_controlled_folder_access_ruleset_json = String.Empty;
        public static string policy_sensors_json = String.Empty;
        public static string policy_jobs_json = String.Empty;

        //Datatables
        public static DataTable events_data_table = new DataTable();

        public void ServiceAsync()
        {
            InitializeComponent();
        }

        protected override async void OnStart(string[] args)
        {
            Logging.Handler.Debug("Service.Service", "Service started", "Service started");

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
            if (!await Server_Config.Load()) // 
            {
                Logging.Handler.Debug("Service.OnStart", "Server_Config_Handler.Load", "Failed to load server config");
                Stop();
            }

            // Setup virtual datatables
            Initialization.Health.Setup_Events_Virtual_Datatable();

            // Setup synchronize timer
            try
            {
                sync_timer = new System.Timers.Timer(600000); //sync 10 minutes
                sync_timer.Elapsed += new ElapsedEventHandler(Initialize);
                sync_timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Service.OnStart", "Start sync_timer", ex.ToString());
            }

            //Start events timer
            try
            {
                events_timer = new System.Timers.Timer(10000);
                events_timer.Elapsed += new ElapsedEventHandler(Process_Events_Timer_Tick);
                events_timer.Enabled = true;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Service.OnStart", "Start Event_Processor_Timer", ex.Message);
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
                Logging.Handler.Debug("Service.OnStart", "Start start_timer", ex.ToString());
            }

            // Authenticate online
            //await Online_Mode.Authenticate.Authenticate_Online();
        }

        protected override void OnStop()
        {
            Logging.Handler.Debug("Service", "Service stopped", "Service stopped");
        }

        private async void Initialize(object sender, ElapsedEventArgs e)
        {
            Logging.Handler.Debug("Service.Initialize", "Initialize", "Start");

            //Disable start timer to prevent concurrent executions
            if (start_timer.Enabled)
                start_timer.Dispose();

            // Check if connection to communication server is available
            connection_status = Check_Connection.Communication_Server().Result;

            // Online mode
            if (connection_status)
            {
                Logging.Handler.Debug("Service.Initialize", "connection_status", "Online mode.");

                bool forced = false;

                //Force client sync if settings are missing
                if (File.Exists(Application_Paths.program_data_netlock_policy_database) == false || File.Exists(Application_Paths.program_data_netlock_events_database) == false)
                {
                    Initialization.Database.NetLock_Events_Setup(); //Create events database if its not existing (cause it was deleted somehow)
                    forced = true;
                }

                //If first run, skip module init (pre boot) and load client settings first
                if (File.Exists(Application_Paths.just_installed) == false && forced == false && first_sync == true) //Enable the Preboot Modules to block shit on system boot
                    Pre_Boot();
                if (File.Exists(Application_Paths.just_installed) && forced == false) //Force the sync & set the config because its the first run (justinstalled.txt)
                    forced = true;
                else if (Helper.Registry.HKLM_Read_Value(Application_Paths.netlock_reg_path, "Synced") == "0" || Helper.Registry.HKLM_Read_Value(Application_Paths.netlock_reg_path, "Synced") == null)
                    forced = true;

                // Check version
                bool up2date = await Initialization.Version.Check_Version();

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
                    if (auth_result == "not_synced" || forced)
                    {
                        // Set synced flag in registry to not synced
                        Helper.Registry.HKLM_Write_Value(Application_Paths.netlock_reg_path, "Synced", "0");

                        // Sync
                        await Online_Mode.Handler.Policy();

                        // Sync done. Set synced flag in registry to prevent re-sync
                        Helper.Registry.HKLM_Write_Value(Application_Paths.netlock_reg_path, "Synced", "1");
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
            else // Offline mode
            {
                Offline_Mode.Handler.Policy();
            }

            // Trigger module handler
            Module_Handler();
        }

        private void Pre_Boot()
        {
            if (authorized)
            {
                Logging.Handler.Debug("Service.Pre_Boot", "", "Authorized.");
                //prepare_rulesets();
                Offline_Mode.Handler.Policy();
                Module_Handler();
            }
            else if (!authorized)
            {
                Logging.Handler.Debug("Service.Pre_Boot", "", "Not authorized.");
            }
        }

        private void Module_Handler()
        {
            if (!authorized)
                return;

            // Antivirus
            Microsoft_Defender_Antivirus.Handler.Initalization();
        }

        private async void Process_Events_Timer_Tick(object source, ElapsedEventArgs e)
        {
            Logging.Handler.Debug("Service.Process_Events_Tick", "Start status", process_events.ToString());

            if (process_events == true) //Check if the events should be processed
            {
                if (events_processing == false)
                {
                    events_processing = true;

                    Logger.Consume_Events();
                    await Logger.Process_Events();
                    
                    events_processing = false;
                }
            }

            Logging.Handler.Debug("Service.Process_Events_Tick", "Stop status", process_events.ToString());
        }
    }
}
