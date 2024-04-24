using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Reflection;
using System.Net.NetworkInformation;
using Microsoft.Win32;
using NetLock_RMM_Comm_Agent_Windows.Helper;
using System.Management;
using System.Reflection.Emit;
using NetFwTypeLib;
using System.Diagnostics;
using NetLock_RMM_Comm_Agent_Windows.Client_Information;

namespace NetLock_RMM_Comm_Agent_Windows.Online_Mode
{
    internal class Handler
    {
        //Class
        public class Device_Identity
        {
            public string agent_version { get; set; }
            public string device_name { get; set; }
            public string location_name { get; set; }
            public string tenant_name { get; set; }
            public string access_key { get; set; }
            public string hwid { get; set; }
            public string ip_address_internal { get; set; }
            public string operating_system { get; set; }
            public string domain { get; set; }
            public string antivirus_solution { get; set; }
            public string firewall_status { get; set; }
            public string architecture { get; set; }
            public string last_boot { get; set; }
            public string timezone { get; set; }
            public string cpu { get; set; }
            public string mainboard { get; set; }
            public string gpu { get; set; }
            public string ram { get; set; }
            public string tpm { get; set; }
        }

        public class Processes
        {
            public string name { get; set; }
            public string pid { get; set; }
            public string parent_name { get; set; }
            public string parent_pid { get; set; }
            public string cpu { get; set; }
            public string ram { get; set; }
            public string user { get; set; }
            public string created { get; set; }
            public string path { get; set; }
            public string cmd { get; set; }
            public string handles { get; set; }
            public string threads { get; set; }
            public string read_operations { get; set; }
            public string read_transfer { get; set; }
            public string write_operations { get; set; }
            public string write_transfer { get; set; }
        }

        public class CPU_Information
        {
            public string name { get; set; }
            public string socket_designation { get; set; }
            public string processor_id { get; set; }
            public string revision { get; set; }
            public string usage { get; set; }
            public string voltage { get; set; }
            public string currentclockspeed { get; set; }
            public string processes { get; set; }
            public string threads { get; set; }
            public string handles { get; set; }
            public string maxclockspeed { get; set; }
            public string sockets { get; set; }
            public string cores { get; set; }
            public string logical_processors { get; set; }
            public string virtualization { get; set; }
            public string l1_cache { get; set; }
            public string l2_cache { get; set; }
            public string l3_cache { get; set; }
        }

        public class RAM_Information
        {
            public string name { get; set; }
            public string available { get; set; }
            public string assured { get; set; }
            public string cache { get; set; }
            public string outsourced_pool { get; set; }
            public string not_outsourced_pool { get; set; }
            public string speed { get; set; }
            public string slots { get; set; }
            public string slots_used { get; set; }
            public string form_factor { get; set; }
            public string hardware_reserved { get; set; }
        }

        public class Network_Adapters
        {
            public string name { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public string link_speed { get; set; }
            public string service_name { get; set; }
            public string dns_domain { get; set; }
            public string dns_hostname { get; set; }
            public string dhcp_enabled { get; set; }
            public string dhcp_server { get; set; }
            public string ipv4_address { get; set; }
            public string ipv6_address { get; set; }
            public string subnet_mask { get; set; }
            public string mac_address { get; set; }
            public string sending { get; set; }
            public string receive { get; set; }
        }

        public class Disks
        {
            public string letter { get; set; }
            public string label { get; set; }
            public string model { get; set; }
            public string firmware_revision { get; set; }
            public string serial_number { get; set; }
            public string interface_type { get; set; }
            public string capacity { get; set; }
            public string usage { get; set; }
            public string status { get; set; }
        }

        public class Applications_Installed
        {
            public string name { get; set; }
            public string version { get; set; }
            public string installed_date { get; set; }
            public string installation_path { get; set; }
            public string vendor { get; set; }
            public string uninstallation_string { get; set; }
        }

        public class Applications_Logon
        {
            public string name { get; set; }
            public string path { get; set; }
            public string command { get; set; }
            public string user { get; set; }
            public string user_sid { get; set; }
        }

        public class Application_Scheduled_Tasks
        {
            public string name { get; set; }
            public string status { get; set; }
            public string author { get; set; }
            public string path { get; set; }
            public string folder { get; set; }
            public string user_sid { get; set; }
            public string next_execution { get; set; }
            public string last_execution { get; set; }
        }

        public class Applications_Services
        {
            public string display_name { get; set; }
            public string name { get; set; }
            public string status { get; set; }
            public string start_type { get; set; }
            public string login_as { get; set; }
            public string path { get; set; }
            public string description { get; set; }
        }

        public class Applications_Drivers
        {
            public string display_name { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string status { get; set; }
            public string type { get; set; }
            public string start_type { get; set; }
            public string path { get; set; }
            public string version { get; set; }
        }

        public class Antivirus_Products
        {
            public string display_name { get; set; }
            public string instance_guid { get; set; }
            public string path_to_signed_product_exe { get; set; }
            public string path_to_signed_reporting_exe { get; set; }
            public string product_state { get; set; }
            public string timestamp { get; set; }
        }

        public class Antivirus_Information
        {
            public string amengineversion { get; set; }
            public string amproductversion { get; set; }
            public bool amserviceenabled { get; set; }
            public string amserviceversion { get; set; }
            public bool antispywareenabled { get; set; }
            public string antispywaresignaturelastupdated { get; set; }
            public string antispywaresignatureversion { get; set; }
            public bool antivirusenabled { get; set; }
            public string antivirussignaturelastupdated { get; set; }
            public string antivirussignatureversion { get; set; }
            public bool behaviormonitorenabled { get; set; }
            public bool ioavprotectionenabled { get; set; }
            public bool istamperprotected { get; set; }
            public bool nisenabled { get; set; }
            public string nisengineversion { get; set; }
            public string nissignaturelastupdated { get; set; }
            public string nissignatureversion { get; set; }
            public bool onaccessprotectionenabled { get; set; }
            public bool realtimetprotectionenabled { get; set; }
        }


        public static async Task<string> Authenticate()
        {
            try
            {
                // Get ip_address_internal
                string ip_address_internal = Helper.Network.Get_Local_IP_Address();
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "ip_address_internal", ip_address_internal); 

                // Get Windows version
                string operating_system = Windows.Windows_Version();
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "operating_system", operating_system);

                // Get DOMAIN
                string domain = Environment.UserDomainName;
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "domain", domain);

                // Get Antivirus solution
                string antivirus_solution = WMI.Search("root\\SecurityCenter2", "select * FROM AntivirusProduct", "displayName");
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "antivirus_solution", antivirus_solution);

                // Get Firewall status
                bool firewall_status = Windows_Defender_Firewall.Handler.Status();
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "firewall_status", firewall_status.ToString());

                // Get Architecture
                string architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "architecture", architecture);

                // Get last boot
                string last_boot = WMI.Search("root\\CIMV2", "SELECT LastBootUpTime FROM Win32_OperatingSystem", "LastBootUpTime");
                DateTime last_boot_datetime = ManagementDateTimeConverter.ToDateTime(last_boot);
                last_boot = last_boot_datetime.ToString("dd.MM.yyyy HH:mm:ss");
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "last_boot", last_boot);

                // Get timezone
                string timezone = Globalization.Local_Time_Zone();
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "timezone", timezone);

                // Get CPU
                string cpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_Processor", "Name");
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "cpu", cpu);

                // Get Mainboard
                string mainboard = WMI.Search("root\\CIMV2", "SELECT Product FROM Win32_BaseBoard", "Product");
                string mainboard_manufacturer = WMI.Search("root\\CIMV2", "SELECT Manufacturer FROM Win32_BaseBoard", "Manufacturer");

                mainboard = mainboard + " (" + mainboard_manufacturer + ")";
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "mainboard", mainboard);

                // Get GPU
                string gpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_VideoController", "Name");
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "gpu", gpu);

                // Get RAM
                string ram = WMI.Search("root\\CIMV2", "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory");
                ram = Math.Round(Convert.ToDouble(ram) / 1024 / 1024 / 1024).ToString();
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "ram", ram);

                // Get TPM
                string tpm = string.Empty;
                //string tpm_IsActivated_InitialValue = WMI.Search("root\\CIMV2", "SELECT IsActivated_InitialValue FROM Win32_Tpm", "IsActivated_InitialValue");
                string tpm_IsEnabled_InitialValue = WMI.Search("root\\cimv2\\Security\\MicrosoftTpm", "SELECT IsEnabled_InitialValue FROM Win32_Tpm", "IsEnabled_InitialValue");
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "tpm_IsEnabled_InitialValue", tpm_IsEnabled_InitialValue);
                //string tpm_IsOwned_InitialValue = WMI.Search("root\\CIMV2", "SELECT IsOwned_InitialValue FROM Win32_Tpm", "IsOwned_InitialValue");
                //string tpm_manufacturer = WMI.Search("root\\CIMV2", "SELECT Manufacturer FROM Win32_Tpm", "Manufacturer");
                //string tpm_ManufacturerId = WMI.Search("root\\CIMV2", "SELECT ManufacturerId FROM Win32_Tpm", "ManufacturerId");
                //string tpm_ManufacturerIdTxt = WMI.Search("root\\CIMV2", "SELECT ManufacturerIdTxt FROM Win32_Tpm", "ManufacturerIdTxt");
                //string tpm_ManufacturerVersion = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersion FROM Win32_Tpm", "ManufacturerVersion");
                //string tpm_ManufacturerVersionFull20 = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersionFull20 FROM Win32_Tpm", "ManufacturerVersionFull20");
                //string tpm_ManufacturerVersionInfo = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersionInfo FROM Win32_Tpm", "ManufacturerVersionInfo");
                //string tpm_PhysicalPresenceVersionInfo = WMI.Search("root\\CIMV2", "SELECT PhysicalPresenceVersionInfo FROM Win32_Tpm", "PhysicalPresenceVersionInfo");
                //string tpm_SpecVersion = WMI.Search("root\\CIMV2", "SELECT SpecVersion FROM Win32_Tpm", "SpecVersion");

                //tpm data to much for a one liner. Needs own table in web console and therefore a own json object

                //Create JSON
                Device_Identity identity = new Device_Identity
                {
                    agent_version = Application_Settings.version,
                    device_name = Service.device_name,
                    location_name = Service.location_name,
                    tenant_name = Service.tenant_name,
                    access_key = Service.access_key,
                    hwid = Service.hwid,
                    ip_address_internal = ip_address_internal,
                    operating_system = operating_system,
                    domain = domain,
                    antivirus_solution = antivirus_solution,
                    firewall_status = firewall_status.ToString(),
                    architecture = architecture,
                    last_boot = last_boot,
                    timezone = timezone,
                    cpu = cpu,
                    mainboard = mainboard,
                    gpu = gpu,
                    ram = ram,
                    tpm = tpm_IsEnabled_InitialValue,                };

                // Create the object that contains the device_identity object
                var jsonObject = new { device_identity = identity };

                // Serialize the object to a JSON string
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "json", json);

                // Create a HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Set the content type header
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                     
                    Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "communication_server", Service.communication_server + "/Agent/Windows/Verify_Device");

                    // Send the JSON data to the server
                    var response = await httpClient.PostAsync(Service.communication_server + "/Agent/Windows/Verify_Device", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful, handle the response
                        var result = await response.Content.ReadAsStringAsync();
                        Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "result", result);

                        // Parse the JSON response
                        if (result == "authorized" || result == "synced" || result == "not_synced")
                        {
                            if (!Service.authorized)
                            {
                                // Write the new authorization status to the server config JSON
                                string new_server_config_json = JsonConvert.SerializeObject(new
                                {
                                    main_communication_server = Service.main_communication_server,
                                    fallback_communication_server = Service.fallback_communication_server,
                                    main_update_server = Service.main_update_server,
                                    fallback_update_server = Service.fallback_update_server,
                                    main_trust_server = Service.main_trust_server,
                                    fallback_trust_server = Service.fallback_trust_server,
                                    tenant_name = Service.tenant_name,
                                    location_name = Service.location_name,
                                    access_key = Service.access_key,
                                    authorized = "1",
                                }, Formatting.Indented);

                                // Write the new server config JSON to the file
                                File.WriteAllText(Application_Paths.program_data_server_config_json, new_server_config_json);

                                Service.authorized = true;
                            }
                        }
                        else if (result == "unauthorized")
                        {
                            if (Service.authorized)
                            {
                                // Write the new authorization status to the server config JSON
                                string new_server_config_json = JsonConvert.SerializeObject(new
                                {
                                    main_communication_server = Service.main_communication_server,
                                    fallback_communication_server = Service.fallback_communication_server,
                                    main_update_server = Service.main_update_server,
                                    fallback_update_server = Service.fallback_update_server,
                                    main_trust_server = Service.main_trust_server,
                                    fallback_trust_server = Service.fallback_trust_server,
                                    tenant_name = Service.tenant_name,
                                    location_name = Service.location_name,
                                    access_key = Service.access_key,
                                    authorized = "0",
                                }, Formatting.Indented);

                                // Write the new server config JSON to the file
                                File.WriteAllText(Application_Paths.program_data_server_config_json, new_server_config_json);

                                Service.authorized = false;
                            }
                        }

                        return result;
                    }
                    else
                    {
                        // Request failed, handle the error
                        Logging.Handler.Debug("Online_Mode.Handler.Authenticate", "request", "Request failed: " + response.Content);
                        return "invalid";
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Online_Mode.Handler.Authenticate", "General error", ex.ToString());
                return "invalid";
            }
        }

        public static async Task<string> Update_Device_Information()
        {
            try
            {
                // Get ip_address_internal
                string ip_address_internal = Helper.Network.Get_Local_IP_Address();
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "ip_address_internal", ip_address_internal);

                // Get Windows version
                string operating_system = Windows.Windows_Version();
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "operating_system", operating_system);

                // Get DOMAIN
                string domain = Environment.UserDomainName;
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "domain", domain);

                // Get Antivirus solution
                string antivirus_solution = WMI.Search("root\\SecurityCenter2", "select * FROM AntivirusProduct", "displayName");
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "antivirus_solution", antivirus_solution);

                // Get Firewall status
                bool firewall_status = Windows_Defender_Firewall.Handler.Status();
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "firewall_status", firewall_status.ToString());

                // Get Architecture
                string architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "architecture", architecture);

                // Get last boot
                string last_boot = WMI.Search("root\\CIMV2", "SELECT LastBootUpTime FROM Win32_OperatingSystem", "LastBootUpTime");
                DateTime last_boot_datetime = ManagementDateTimeConverter.ToDateTime(last_boot);
                last_boot = last_boot_datetime.ToString("dd.MM.yyyy HH:mm:ss");
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "last_boot", last_boot);

                // Get timezone
                string timezone = Globalization.Local_Time_Zone();
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "timezone", timezone);

                // Get CPU
                string cpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_Processor", "Name");
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "cpu", cpu);

                // Get Mainboard
                string mainboard = WMI.Search("root\\CIMV2", "SELECT Product FROM Win32_BaseBoard", "Product");
                string mainboard_manufacturer = WMI.Search("root\\CIMV2", "SELECT Manufacturer FROM Win32_BaseBoard", "Manufacturer");

                mainboard = mainboard + " (" + mainboard_manufacturer + ")";
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "mainboard", mainboard);

                // Get GPU
                string gpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_VideoController", "Name");
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "gpu", gpu);

                // Get RAM
                string ram = WMI.Search("root\\CIMV2", "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory");
                ram = Math.Round(Convert.ToDouble(ram) / 1024 / 1024 / 1024).ToString();
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "ram", ram);

                // Get TPM
                string tpm = string.Empty;
                //string tpm_IsActivated_InitialValue = WMI.Search("root\\CIMV2", "SELECT IsActivated_InitialValue FROM Win32_Tpm", "IsActivated_InitialValue");
                string tpm_IsEnabled_InitialValue = WMI.Search("root\\cimv2\\Security\\MicrosoftTpm", "SELECT IsEnabled_InitialValue FROM Win32_Tpm", "IsEnabled_InitialValue");
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "tpm_IsEnabled_InitialValue", tpm_IsEnabled_InitialValue);
                //string tpm_IsOwned_InitialValue = WMI.Search("root\\CIMV2", "SELECT IsOwned_InitialValue FROM Win32_Tpm", "IsOwned_InitialValue");
                //string tpm_manufacturer = WMI.Search("root\\CIMV2", "SELECT Manufacturer FROM Win32_Tpm", "Manufacturer");
                //string tpm_ManufacturerId = WMI.Search("root\\CIMV2", "SELECT ManufacturerId FROM Win32_Tpm", "ManufacturerId");
                //string tpm_ManufacturerIdTxt = WMI.Search("root\\CIMV2", "SELECT ManufacturerIdTxt FROM Win32_Tpm", "ManufacturerIdTxt");
                //string tpm_ManufacturerVersion = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersion FROM Win32_Tpm", "ManufacturerVersion");
                //string tpm_ManufacturerVersionFull20 = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersionFull20 FROM Win32_Tpm", "ManufacturerVersionFull20");
                //string tpm_ManufacturerVersionInfo = WMI.Search("root\\CIMV2", "SELECT ManufacturerVersionInfo FROM Win32_Tpm", "ManufacturerVersionInfo");
                //string tpm_PhysicalPresenceVersionInfo = WMI.Search("root\\CIMV2", "SELECT PhysicalPresenceVersionInfo FROM Win32_Tpm", "PhysicalPresenceVersionInfo");
                //string tpm_SpecVersion = WMI.Search("root\\CIMV2", "SELECT SpecVersion FROM Win32_Tpm", "SpecVersion");

                //tpm data to much for a one liner. Needs own table in web console and therefore a own json object

                // Create the device_identity object
                Device_Identity identity = new Device_Identity
                {
                    agent_version = Application_Settings.version,
                    device_name = Service.device_name,
                    location_name = Service.location_name,
                    tenant_name = Service.tenant_name,
                    access_key = Service.access_key,
                    hwid = Service.hwid,
                    ip_address_internal = ip_address_internal,
                    operating_system = operating_system,
                    domain = domain,
                    antivirus_solution = antivirus_solution,
                    firewall_status = firewall_status.ToString(),
                    architecture = architecture,
                    last_boot = last_boot,
                    timezone = timezone,
                    cpu = cpu,
                    mainboard = mainboard,
                    gpu = gpu,
                    ram = ram,
                    tpm = tpm_IsEnabled_InitialValue,
                };

                // Get the processes list
                Client_Information.Processes.Collect();

                #region cpu_information
                // Create the data for "cpu_information"
                string cpu_information_json = string.Empty;

                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
                    {
                        HashSet<string> uniqueSockets = new HashSet<string>();

                        foreach (ManagementObject obj in searcher.Get())
                        {
                            string socket_designation = obj["SocketDesignation"].ToString();

                            //To determine the number of sockets, you can query the number of unique values for the "SocketDesignation" attribute across all processors. Each unique value for "SocketDesignation" corresponds to a single socket.
                            if (!string.IsNullOrEmpty(socket_designation))
                            {
                                uniqueSockets.Add(socket_designation);
                            }

                            int numberOfSockets = uniqueSockets.Count;

                            CPU_Information cpuInfo = new CPU_Information
                            {
                                name = obj["Name"].ToString(),
                                socket_designation = obj["SocketDesignation"].ToString(),
                                processor_id = obj["ProcessorId"].ToString(),
                                revision = obj["Revision"].ToString(),
                                usage = obj["LoadPercentage"].ToString(),
                                voltage = obj["CurrentVoltage"].ToString(),
                                currentclockspeed = obj["CurrentClockSpeed"].ToString(),
                                processes = Process.GetProcesses().Length.ToString(),
                                threads = Process.GetCurrentProcess().Threads.Count.ToString(),
                                handles = Process.GetCurrentProcess().HandleCount.ToString(),
                                maxclockspeed = obj["MaxClockSpeed"].ToString(),
                                sockets = numberOfSockets.ToString(),
                                cores = obj["NumberOfCores"].ToString(),
                                logical_processors = obj["NumberOfLogicalProcessors"].ToString(),
                                virtualization = obj["VirtualizationFirmwareEnabled"].ToString(),
                                l1_cache = obj["L2CacheSize"].ToString(),
                                l2_cache = obj["L2CacheSize"].ToString(),
                                l3_cache = obj["L3CacheSize"].ToString()
                            };

                            cpu_information_json = JsonConvert.SerializeObject(cpuInfo, Formatting.Indented);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Handler.Error("Online_Mode.Handler.Update_Device_Information", "CPU_Information", "An error occurred while querying for WMI data: " + ex.ToString());
                }
                #endregion

                #region ram_information
                string ram_information_json = string.Empty;

                try
                {
                    using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory"))
                    {
                        foreach (ManagementObject obj in searcher.Get())
                        {
                            RAM_Information ramInfo = new RAM_Information
                            {
                                name = obj["Name"].ToString(),
                                available = obj["Capacity"].ToString(),
                                assured = obj["AssuredSpeed"].ToString(), //n
                                cache = obj["MaxCacheSize"].ToString(), //n
                                outsourced_pool = obj["OutsourcedPool"].ToString(), //n
                                not_outsourced_pool = obj["NonOutsourcedPool"].ToString(), //n
                                speed = obj["Speed"].ToString(),
                                slots = obj["MemoryType"].ToString(), //r
                                slots_used = obj["BankLabel"].ToString(), //r
                                form_factor = obj["FormFactor"].ToString(), //r
                                hardware_reserved = obj["HardwareReserved"].ToString() //n
                            };

                            ram_information_json = JsonConvert.SerializeObject(ramInfo, Formatting.Indented);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logging.Handler.Error("Online_Mode.Handler.Update_Device_Information", "RAM_Information", "An error occurred while querying for WMI data: " + ex.ToString());
                }
                #endregion


                // Create the data for "ram_information"
                RAM_Information ramInformation = new RAM_Information
                {
                    // Hier füge die RAM-Informationen hinzu
                };

                // Create the data for "network_adapters"
                List<Network_Adapters> networkAdapters = new List<Network_Adapters>
                {
                    // Hier füge die Netzwerkadapterdaten hinzu
                };

                // Erstelle die Daten für "disks"
                List<Disks> disks = new List<Disks>
                {
                    // Hier füge die Diskdaten hinzu
                };

                // Erstelle die Daten für "applications_installed"
                List<Applications_Installed> applicationsInstalled = new List<Applications_Installed>
                {
                    // Hier füge die installierten Anwendungsdaten hinzu
                };

                // Erstelle die Daten für "applications_logon"
                List<Applications_Logon> applicationsLogon = new List<Applications_Logon>
                {
                    // Hier füge die Anmeldedaten der Anwendungen hinzu
                };

                // Erstelle die Daten für "applications_scheduled_tasks"
                List<Application_Scheduled_Tasks> applicationsScheduledTasks = new List<Application_Scheduled_Tasks>
                {
                    // Hier füge die Daten der geplanten Tasks hinzu
                };

                // Erstelle die Daten für "applications_services"
                List<Applications_Services> applicationsServices = new List<Applications_Services>
                {
                    // Hier füge die Servicedaten hinzu
                };

                // Erstelle die Daten für "applications_drivers"
                List<Applications_Drivers> applicationsDrivers = new List<Applications_Drivers>
                {
                    // Hier füge die Treiberdaten hinzu
                };

                // Erstelle die Daten für "antivirus_products"
                List<Antivirus_Products> antivirusProducts = new List<Antivirus_Products>
                {
                    // Hier füge die Antivirenprodukte hinzu
                };

                // Erstelle die Daten für "antivirus_information"
                List<Antivirus_Information> antivirusInformation = new List<Antivirus_Information>
                {
                    // Hier füge die Antivirenprodukte hinzu
                };



                // Erstelle das JSON-Objekt
                var jsonObject = new
                {
                    device_identity = identity,
                    processes = Service.processes_list,
                    cpu_information = cpu_information_json,
                    ram_information = ramInformation,
                    network_adapters = networkAdapters,
                    disks = disks,
                    applications_installed = applicationsInstalled,
                    applications_logon = applicationsLogon,
                    applications_scheduled_tasks = applicationsScheduledTasks,
                    applications_services = applicationsServices,
                    applications_drivers = applicationsDrivers,
                    antivirus_products = antivirusProducts,
                    antivirus_information = antivirusInformation,
                };

                // Konvertiere das Objekt in ein JSON-String
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "json", json);

                // Create a HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Set the content type header
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "communication_server", Service.communication_server + "/Agent/Windows/Verify_Device");

                    // Send the JSON data to the server
                    var response = await httpClient.PostAsync(Service.communication_server + "/Agent/Windows/Update_Device_Information", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful, handle the response
                        var result = await response.Content.ReadAsStringAsync();
                        Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "result", result);

                        // Parse the JSON response
                        if (result == "authorized" || result == "synced" || result == "not_synced")
                        {
                            if (!Service.authorized)
                            {
                                // Write the new authorization status to the server config JSON
                                string new_server_config_json = JsonConvert.SerializeObject(new
                                {
                                    main_communication_server = Service.main_communication_server,
                                    fallback_communication_server = Service.fallback_communication_server,
                                    main_update_server = Service.main_update_server,
                                    fallback_update_server = Service.fallback_update_server,
                                    main_trust_server = Service.main_trust_server,
                                    fallback_trust_server = Service.fallback_trust_server,
                                    tenant_name = Service.tenant_name,
                                    location_name = Service.location_name,
                                    access_key = Service.access_key,
                                    authorized = "1",
                                }, Formatting.Indented);

                                // Write the new server config JSON to the file
                                File.WriteAllText(Application_Paths.program_data_server_config_json, new_server_config_json);

                                Service.authorized = true;
                            }
                        }
                        else if (result == "unauthorized")
                        {
                            if (Service.authorized)
                            {
                                // Write the new authorization status to the server config JSON
                                string new_server_config_json = JsonConvert.SerializeObject(new
                                {
                                    main_communication_server = Service.main_communication_server,
                                    fallback_communication_server = Service.fallback_communication_server,
                                    main_update_server = Service.main_update_server,
                                    fallback_update_server = Service.fallback_update_server,
                                    main_trust_server = Service.main_trust_server,
                                    fallback_trust_server = Service.fallback_trust_server,
                                    tenant_name = Service.tenant_name,
                                    location_name = Service.location_name,
                                    access_key = Service.access_key,
                                    authorized = "0",
                                }, Formatting.Indented);

                                // Write the new server config JSON to the file
                                File.WriteAllText(Application_Paths.program_data_server_config_json, new_server_config_json);

                                Service.authorized = false;
                            }
                        }

                        return result;
                    }
                    else
                    {
                        // Request failed, handle the error
                        Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "request", "Request failed: " + response.Content);
                        return "invalid";
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Online_Mode.Handler.Update_Device_Information", "General error", ex.ToString());
                return "invalid";
            }
        }
    }
}
