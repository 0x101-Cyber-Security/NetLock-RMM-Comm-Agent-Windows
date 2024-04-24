using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;

namespace NetLock_RMM_Comm_Agent_Windows.Initialization
{
    internal class Server_Config_Handler
    {
        public async static Task<bool> Load()
        {
            try
            {
                string server_config_json = File.ReadAllText(Application_Paths.program_data_server_config_json);
                Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (server_config_json)", server_config_json);

                // Parse the JSON
                using (JsonDocument document = JsonDocument.Parse(server_config_json))
                {
                    // Get the main communication server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("main_communication_server");
                        Service.main_communication_server = element.ToString();

                        Service.communication_server = Service.main_communication_server;
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (main_communication_server)", Service.main_communication_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (main_communication_server) - Parsing", ex.ToString());
                    }
                    
                    // Get the fallback communication server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("fallback_communication_server");
                        Service.fallback_communication_server = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (fallback_communication_server)", Service.fallback_communication_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (fallback_communication_server) - Parsing", ex.ToString());
                    }
                    
                    // Get the main update server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("main_update_server");
                        Service.main_update_server = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (main_update_server)", Service.main_update_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (main_update_server) - Parsing", ex.ToString());
                    }

                    // Get the fallback update server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("fallback_update_server");
                        Service.fallback_update_server = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (fallback_update_server)", Service.fallback_update_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (fallback_update_server) - Parsing", ex.ToString());
                    }

                    // Get the main trust server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("main_trust_server");
                        Service.main_trust_server = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (main_trust_server)", Service.main_trust_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (main_trust_server) - Parsing", ex.ToString());
                    }

                    // Get the fallback trust server
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("fallback_trust_server");
                        Service.fallback_trust_server = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (fallback_trust_server)", Service.fallback_trust_server);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (fallback_trust_server) - Parsing", ex.ToString());
                    }

                    // Get the tenant name
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("tenant_name");
                        Service.tenant_name = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (tenant_name)", Service.tenant_name);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (tenant_name) - Parsing", ex.ToString());
                    }

                    // Get the location name
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("location_name");
                        Service.location_name = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (location_name)", Service.location_name);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (location_name) - Parsing", ex.ToString());
                    }

                    // Get the access key
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("access_key");
                        Service.access_key = element.ToString();
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (access_key)", Service.access_key);
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (access_key) - Parsing", ex.ToString());
                    }

                    // Get the authorized status
                    try
                    {
                        JsonElement element = document.RootElement.GetProperty("authorized");
                        Service.authorized = element.ToString() == "1" ? true : false;
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load (authorized)", Service.authorized.ToString());
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Error("Server_Config_Handler", "Server_Config_Handler.Load (authorized) - Parsing", ex.ToString());
                    }

                    // Check if the access key is valid
                    if (Service.access_key == String.Empty)
                    {
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load", "Access key is empty");

                        // Generate a new access key
                        Service.access_key = await Randomizer.Handler.Generate_Access_Key(32);

                        // Write the new access key to the server config file
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
                    }
                    else
                    {
                        Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load", "Access key is not empty");
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Server_Config_Handler", "Server_Config_Handler.Load", ex.ToString());
                return false;
            }
        }
    }
}
