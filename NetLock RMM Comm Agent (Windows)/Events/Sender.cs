using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using NetLock_RMM_Comm_Agent_Windows.Device_Information;
using NetLock_RMM_Comm_Agent_Windows.Helper;
using Newtonsoft.Json;
using System.Management;
using System.Net.Http.Headers;
using System.IO;

namespace NetLock_RMM_Comm_Agent_Windows.Events
{
    internal class Sender
    {
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

        public static async Task <bool> Send_Event(string severity, string reported_by, string _event, string details, string type, string lang)
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

                // Create the JSON object
                var jsonObject = new
                {
                    severity = severity,
                    reported_by = reported_by,
                    _event = _event,
                    description = details,
                    type = type,
                    lang = lang,
                };

                // Convert the object into a JSON string
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "json", json);

                // Create a HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Set the content type header
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "communication_server", Service.communication_server + "/Agent/Windows/Verify_Device");

                    // Send the JSON data to the server
                    var response = await httpClient.PostAsync(Service.communication_server + "/Agent/Windows/Events", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful, handle the response
                        var result = await response.Content.ReadAsStringAsync();
                        Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "result", result);

                        // Parse the JSON response
                        if (result == "unauthorized")
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
                        else if (result == "success")
                            return true;
                        else
                        {
                            // Request failed, handle the error
                            Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "request", "Request failed: " + result);
                            return false;
                        }
                    }
                    else
                    {
                        // Request failed, handle the error
                        Logging.Handler.Debug("Online_Mode.Handler.Update_Device_Information", "request", "Request failed: " + response.Content);
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Online_Mode.Handler.Update_Device_Information", "General error", ex.ToString());
                return false;
            }
        }
    }
}
