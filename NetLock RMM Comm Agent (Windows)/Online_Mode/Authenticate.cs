using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;

namespace NetLock_RMM_Comm_Agent_Windows.Online_Mode
{
    internal class Authenticate
    {
        public static async Task Authenticate_Online()
        {
            try
            {
                // Create a JSON object to send
                var json = @"
{
  ""device_identity"": {
    ""agent_version"": ""1.0.0.1"",
    ""device_name"": ""PC01"",
    ""location_name"": ""Köln1"",
    ""tenant_name"": ""0x101 Cyber Security"",
    ""access_key"": ""fsefsefsd"",
    ""hwid"": ""abc123"",
    ""ip_address_internal"": ""192.168.1.1"",
    ""operating_system"": ""Windows 10 Pro"",
    ""domain"": ""netlock.de"",
    ""antivirus_solution"": ""Windows Defender"",
    ""firewall_status"": ""Aktiviert"",
    ""architecture"": ""x64"",
    ""last_boot"": ""01.01.2024 00:00:00"",
    ""timezone"": ""UTC oder so"",
    ""cpu"": ""AMD"",
    ""mainboard"": ""Asus"",
    ""gpu"": ""Nvidia"",
    ""ram"": ""RAM"",
    ""tpm"": ""Aktiviert"",
    ""environment_variables"": ""envir""
  }
}";

                // Create a HttpClient instance
                using (var httpClient = new HttpClient())
                {
                    // Set the content type header
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                     
                    Logging.Handler.Debug("Authenticate_Online", "communication_server", Service.communication_server + "/Agent/Windows/Verify_Device");

                    // Send the JSON data to the server
                    var response = await httpClient.PostAsync(Service.communication_server + "/Agent/Windows/Verify_Device", new StringContent(json, Encoding.UTF8, "application/json"));

                    // Check if the request was successful
                    if (response.IsSuccessStatusCode)
                    {
                        // Request was successful, handle the response
                        var result = await response.Content.ReadAsStringAsync();
                        Logging.Handler.Debug("Authenticate_Online", "Authenticate_Online", result);
                    }
                    else
                    {
                        // Request failed, handle the error
                        Logging.Handler.Debug("Authenticate_Online", "Authenticate_Online", "Request failed: " + response.Content);
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Authenticate_Online", "Authenticate_Online", ex.ToString());
            }
        }
    }
}
