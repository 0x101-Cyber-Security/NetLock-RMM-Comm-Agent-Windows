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
using System.Security.Cryptography;

namespace NetLock_RMM_Comm_Agent_Windows.Events
{
    internal class Sender
    {
        public class Device_Identity
        {
            public string agent_version { get; set; }
            public string package_guid { get; set; }
            public string device_name { get; set; }
            public string location_guid { get; set; }
            public string tenant_guid { get; set; }
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

        public class Event_Entity
        {
            public string severity { get; set; }
            public string reported_by { get; set; }
            public string _event { get; set; }
            public string description { get; set; }
            public string type { get; set; }
            public string language { get; set; }
        }

        public static async Task <bool> Send_Event(string severity, string reported_by, string _event, string description, string type, string language)
        {
            try
            {
                // Get ip_address_internal
                string ip_address_internal = Helper.Network.Get_Local_IP_Address();
                Logging.Handler.Debug("Events.Sender.Send_Event", "ip_address_internal", ip_address_internal);

                // Get Windows version
                string operating_system = Windows.Windows_Version();
                Logging.Handler.Debug("Events.Sender.Send_Event", "operating_system", operating_system);

                // Get DOMAIN
                string domain = Environment.UserDomainName;
                Logging.Handler.Debug("Events.Sender.Send_Event", "domain", domain);

                // Get Antivirus solution
                string antivirus_solution = WMI.Search("root\\SecurityCenter2", "select * FROM AntivirusProduct", "displayName");
                Logging.Handler.Debug("Events.Sender.Send_Event", "antivirus_solution", antivirus_solution);

                // Get Firewall status
                bool firewall_status = Microsoft_Defender_Firewall.Handler.Status();
                Logging.Handler.Debug("Events.Sender.Send_Event", "firewall_status", firewall_status.ToString());

                // Get Architecture
                string architecture = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                Logging.Handler.Debug("Events.Sender.Send_Event", "architecture", architecture);

                // Get last boot
                string last_boot = WMI.Search("root\\CIMV2", "SELECT LastBootUpTime FROM Win32_OperatingSystem", "LastBootUpTime");
                DateTime last_boot_datetime = ManagementDateTimeConverter.ToDateTime(last_boot);
                last_boot = last_boot_datetime.ToString("dd.MM.yyyy HH:mm:ss");
                Logging.Handler.Debug("Events.Sender.Send_Event", "last_boot", last_boot);

                // Get timezone
                string timezone = Globalization.Local_Time_Zone();
                Logging.Handler.Debug("Events.Sender.Send_Event", "timezone", timezone);

                // Get CPU
                string cpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_Processor", "Name");
                Logging.Handler.Debug("Events.Sender.Send_Event", "cpu", cpu);

                // Get Mainboard
                string mainboard = WMI.Search("root\\CIMV2", "SELECT Product FROM Win32_BaseBoard", "Product");
                string mainboard_manufacturer = WMI.Search("root\\CIMV2", "SELECT Manufacturer FROM Win32_BaseBoard", "Manufacturer");

                mainboard = mainboard + " (" + mainboard_manufacturer + ")";
                Logging.Handler.Debug("Events.Sender.Send_Event", "mainboard", mainboard);

                // Get GPU
                string gpu = WMI.Search("root\\CIMV2", "SELECT Name FROM Win32_VideoController", "Name");
                Logging.Handler.Debug("Events.Sender.Send_Event", "gpu", gpu);

                // Get RAM
                string ram = WMI.Search("root\\CIMV2", "SELECT TotalPhysicalMemory FROM Win32_ComputerSystem", "TotalPhysicalMemory");
                ram = Math.Round(Convert.ToDouble(ram) / 1024 / 1024 / 1024).ToString();
                Logging.Handler.Debug("Events.Sender.Send_Event", "ram", ram);

                // Get TPM
                string tpm = string.Empty;
                //string tpm_IsActivated_InitialValue = WMI.Search("root\\CIMV2", "SELECT IsActivated_InitialValue FROM Win32_Tpm", "IsActivated_InitialValue");
                string tpm_IsEnabled_InitialValue = WMI.Search("root\\cimv2\\Security\\MicrosoftTpm", "SELECT IsEnabled_InitialValue FROM Win32_Tpm", "IsEnabled_InitialValue");
                Logging.Handler.Debug("Events.Sender.Send_Event", "tpm_IsEnabled_InitialValue", tpm_IsEnabled_InitialValue);

                //tpm data to much for a one liner. Needs own table in web console and therefore a own json object

                // Create the identity_object
                Device_Identity identity_object = new Device_Identity
                {
                    agent_version = Application_Settings.version,
                    package_guid = Service.package_guid,
                    device_name = Service.device_name,
                    location_guid = Service.location_guid,
                    tenant_guid = Service.tenant_guid,
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

                // Create the event_object
                Event_Entity event_object = new Event_Entity
                {
                    severity = severity,
                    reported_by = reported_by,
                    _event = _event,
                    description = description,
                    type = type,
                    language = language,
                };

                // Create the JSON object
                var jsonObject = new
                {
                    device_identity = identity_object,
                    _event = event_object,
                };

                // Convert the object into a JSON string
                string json = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
                Logging.Handler.Debug("Events.Sender.Send_Event", "json", json);

                // Create a HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Set the content type header
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    httpClient.DefaultRequestHeaders.Add("Package_Guid", Service.package_guid);

                    Logging.Handler.Debug("Events.Sender.Send_Event", "communication_server", Service.http_https + Service.communication_server + "/Agent/Windows/Events");

                    // Send the JSON data to the server
                    var response = await httpClient.PostAsync(Service.http_https + Service.communication_server + "/Agent/Windows/Events", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful, handle the response
                        var result = await response.Content.ReadAsStringAsync();
                        Logging.Handler.Debug("Events.Sender.Send_Event", "result", result);

                        // Parse the JSON response
                        if (result == "unauthorized")
                        {
                            if (Service.authorized)
                            {
                                // Write the new authorization status to the server config JSON
                                string new_server_config_json = JsonConvert.SerializeObject(new
                                {
                                    ssl = Service.ssl,
                                    package_guid = Service.package_guid,
                                    communication_servers = Service.communication_servers,
                                    remote_servers = Service.remote_servers,
                                    update_servers = Service.update_servers,
                                    trust_servers = Service.trust_servers,
                                    tenant_guid = Service.tenant_guid,
                                    location_guid = Service.location_guid,
                                    access_key = Service.access_key,
                                    authorized = false,
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
                            Logging.Handler.Debug("Events.Sender.Send_Event", "request", "Request failed, result was not success: " + result);
                            return false;
                        }
                    }
                    else
                    {
                        // Request failed, handle the error
                        Logging.Handler.Debug("Events.Sender.Send_Event", "request", "Request failed: " + response.StatusCode + " " + response.Content.ToString());
                        return false;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Events.Sender.Send_Event", "General error", ex.ToString());
                return false;
            }
        }
    }
}
