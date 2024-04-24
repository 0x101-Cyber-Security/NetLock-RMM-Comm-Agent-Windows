using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime;
using System.Security.Policy;
using System.Text.RegularExpressions;

namespace NetLock_RMM_Comm_Agent_Windows.Initialization
{
    internal class Check_Connection
    {
        public static async Task<bool> Communication_Server()
        {
            string communication_server_regex = @"(?<=://)(.*?)(?=:|$)";
            string communication_server_port_regex = @"(?<=:)(\d+)";

            // Extract server name
            Match main_communication_server_match = Regex.Match(Service.main_communication_server, communication_server_regex);
            Match main_communication_server_port_match = Regex.Match(Service.main_communication_server, communication_server_port_regex);
            Match fallback_communication_server_match = Regex.Match(Service.fallback_communication_server, communication_server_regex);
            Match fallback_communication_server_port_match = Regex.Match(Service.fallback_communication_server, communication_server_port_regex);

            // Check main communication server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if main communication server is set
                    if (main_communication_server_match.Success && main_communication_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Communication_Server (main)", $"Server: {main_communication_server_match.Groups[0].Value} Port: {main_communication_server_port_match.Groups[0].Value}");

                        // Connect to the main communication server
                        await client.ConnectAsync(main_communication_server_match.Groups[0].Value, Convert.ToInt32(main_communication_server_port_match.Groups[0].Value));
                        
                        Service.communication_server = Service.main_communication_server;

                        Logging.Handler.Debug("Check_Connection", "Communication_Server (main)", "Connection to communication server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Communication_Server (main)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Communication_Server (main)", ex.ToString());
            }

            // Check fallback communication server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if fallback communication server is set
                    if (fallback_communication_server_match.Success && fallback_communication_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Communication_Server (fallback)", $"Server: {fallback_communication_server_match.Groups[0].Value} Port: {fallback_communication_server_port_match.Groups[0].Value}");

                        // Connect to the fallback communication server
                        await client.ConnectAsync(fallback_communication_server_match.Groups[0].Value, Convert.ToInt32(fallback_communication_server_port_match.Groups[0].Value));

                        Service.communication_server = Service.fallback_communication_server;

                        Logging.Handler.Debug("Check_Connection", "Communication_Server (fallback)", "Connection to communication server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Communication_Server (fallback)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Communication_Server (fallback)", ex.ToString());
            }

            return false;
        }

        public static async Task<bool> Trust_Server()
        {
            string trust_server_regex = @"(?<=://)(.*?)(?=:|$)";
            string trust_server_port_regex = @"(?<=:)(\d+)";

            // Extract server name
            Match main_trust_server_match = Regex.Match(Service.main_trust_server, trust_server_regex);
            Match main_trust_server_port_match = Regex.Match(Service.main_trust_server, trust_server_port_regex);
            Match fallback_trust_server_match = Regex.Match(Service.fallback_trust_server, trust_server_regex);
            Match fallback_trust_server_port_match = Regex.Match(Service.fallback_trust_server, trust_server_port_regex);

            // Check main trust server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if main trust server is set
                    if (main_trust_server_match.Success && main_trust_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Trust_Server (main)", $"Server: {main_trust_server_match.Groups[0].Value} Port: {main_trust_server_port_match.Groups[0].Value}");

                        // Connect to the main trust server
                        await client.ConnectAsync(main_trust_server_match.Groups[0].Value, Convert.ToInt32(main_trust_server_port_match.Groups[0].Value));

                        Service.trust_server = Service.main_trust_server;

                        Logging.Handler.Debug("Check_Connection", "Trust_Server (main)", "Connection to trust server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Trust_Server (main)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Trust_Server (main)", ex.ToString());
            }

            // Check fallback trust server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if fallback trust server is set
                    if (fallback_trust_server_match.Success && fallback_trust_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Trust_Server (fallback)", $"Server: {fallback_trust_server_match.Groups[0].Value} Port: {fallback_trust_server_port_match.Groups[0].Value}");

                        // Connect to the fallback trust server
                        await client.ConnectAsync(fallback_trust_server_match.Groups[0].Value, Convert.ToInt32(fallback_trust_server_port_match.Groups[0].Value));

                        Service.trust_server = Service.fallback_trust_server;

                        Logging.Handler.Debug("Check_Connection", "Trust_Server (fallback)", "Connection to trust server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Trust_Server (fallback)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Trust_Server (fallback)", ex.ToString());
            }

            return false;
        }

        public static async Task<bool> Update_Server()
        {
            string update_server_regex = @"(?<=://)(.*?)(?=:|$)";
            string update_server_port_regex = @"(?<=:)(\d+)";

            // Extract server name
            Match main_update_server_match = Regex.Match(Service.main_update_server, update_server_regex);
            Match main_update_server_port_match = Regex.Match(Service.main_update_server, update_server_port_regex);
            Match fallback_update_server_match = Regex.Match(Service.fallback_update_server, update_server_regex);
            Match fallback_update_server_port_match = Regex.Match(Service.fallback_update_server, update_server_port_regex);

            // Check main update server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if main update server is set
                    if (main_update_server_match.Success && main_update_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Update_Server (main)", $"Server: {main_update_server_match.Groups[0].Value} Port: {main_update_server_port_match.Groups[0].Value}");

                        // Connect to the main update server
                        await client.ConnectAsync(main_update_server_match.Groups[0].Value, Convert.ToInt32(main_update_server_port_match.Groups[0].Value));

                        Service.update_server = Service.main_update_server;

                        Logging.Handler.Debug("Check_Connection", "Update_Server (main)", "Connection to update server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Update_Server (main)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Update_Server (main)", ex.ToString());
            }

            // Check fallback update server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if fallback update server is set
                    if (fallback_update_server_match.Success && fallback_update_server_port_match.Success)
                    {
                        Logging.Handler.Debug("Check_Connection", "Update_Server (fallback)", $"Server: {fallback_update_server_match.Groups[0].Value} Port: {fallback_update_server_port_match.Groups[0].Value}");

                        // Connect to the fallback update server
                        await client.ConnectAsync(fallback_update_server_match.Groups[0].Value, Convert.ToInt32(fallback_update_server_port_match.Groups[0].Value));

                        Service.update_server = Service.fallback_update_server;

                        Logging.Handler.Debug("Check_Connection", "Update_Server (fallback)", "Connection to update server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Debug("Check_Connection", "Update_Server (fallback)", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Check_Connection", "Update_Server (fallback)", ex.ToString());
            }

            return false;
        }
    }
}
