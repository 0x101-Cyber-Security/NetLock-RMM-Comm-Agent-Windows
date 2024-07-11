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
using System.Threading;

namespace NetLock_RMM_Comm_Agent_Windows.Initialization
{
    internal class Check_Connection
    {
        public static async Task Check_Servers()
        {
            try
            {
                // Check connection to main communication server
                if (await Hostname_IP_Port(Service.main_communication_server, "main_communication_server"))
                {
                    Service.communication_server = Service.main_communication_server;
                    Service.communication_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Main communication server connection successful.", "");
                }
                else if (await Hostname_IP_Port(Service.fallback_communication_server, "fallback_communication_server")) // Check connection to fallback communication server
                {
                    Service.communication_server = Service.fallback_communication_server;
                    Service.communication_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Fallback communication server connection successful.", "");
                }
                else // If both connections fail, exit the program
                {
                    Logging.Handler.Error("Initialization.Check_Connection.Check_Servers", "Main & fallback communication server connection failed.", "");
                    Service.communication_server_status = false;
                }

                // Check connection to main update server
                if (await Hostname_IP_Port(Service.main_update_server, "main_update_server"))
                {
                    Service.update_server = Service.main_update_server;
                    Service.update_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Main update server connection successful.", "");
                }
                else if (await Hostname_IP_Port(Service.fallback_update_server, "fallback_update_server")) // Check connection to fallback update server
                {
                    Service.update_server = Service.fallback_update_server;
                    Service.update_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Fallback update server connection successful.", "");
                }
                else // If both connections fail, exit the program
                {
                    Logging.Handler.Error("Initialization.Check_Connection.Check_Servers", "Main & fallback update server connection failed.", "");
                    Service.update_server_status = false;
                }

                // Check connection to main trust server
                if (await Hostname_IP_Port(Service.main_trust_server, "main_trust_server"))
                {
                    Service.trust_server = Service.main_trust_server;
                    Service.trust_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Main trust server connection successful.", "");
                }
                else if (await Hostname_IP_Port(Service.fallback_trust_server, "fallback_trust_server")) // Check connection to fallback trust server
                {
                    Service.trust_server = Service.fallback_trust_server;
                    Service.trust_server_status = true;
                    Logging.Handler.Debug("Initialization.Check_Connection.Check_Servers", "Fallback trust server connection successful.", "");
                }
                else // If both connections fail, exit the program
                {
                    Logging.Handler.Error("Initialization.Check_Connection.Check_Servers", "Main & fallback trust server connection failed.", "");
                    Service.trust_server_status = false;
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Initialization.Check_Connection.Check_Servers", "Failed", ex.ToString());
            }
        }

        public static async Task<bool> Hostname_IP_Port(string server, string description)
        {
            Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Hostname_IP_Port", "Server: " + server + " Description: " + description);

            string communication_server_regex = @"([a-zA-Z0-9\-\.]+):\d+";
            string communication_server_port_regex = @"(?<=:)(\d+)";

            // Extract server name
            Match server_match = Regex.Match(server, communication_server_regex);
            Match port_match = Regex.Match(server, communication_server_port_regex);

            // Output server name
            if (server_match.Success)
            {
                Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Server", server_match.Groups[0].Value);
            }
            else
            {
                Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Server", "Server could not be extracted.");
            }

            // Output port
            if (port_match.Success)
            {
                Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Port", "" + port_match.Groups[0].Value);
            }
            else
            {
                Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Port", "Port could not be extracted.");
            }

            // Check main communication server
            try
            {
                using (var client = new TcpClient())
                {
                    // Check if main communication server is set
                    if (server_match.Success && port_match.Success)
                    {
                        Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Hostname_IP_Port", "Server: " + server_match.Groups[1].Value + " Port: " + port_match.Groups[1].Value);

                        // Connect to the main communication server
                        await client.ConnectAsync(server_match.Groups[1].Value, Convert.ToInt32(port_match.Groups[0].Value));

                        Logging.Handler.Debug("Initialization.Check_Connection.Hostname_IP_Port", "Hostname_IP_Port", "Connection to communication server successful");

                        return true;
                    }
                    else
                    {
                        Logging.Handler.Error("Initialization.Check_Connection.Hostname_IP_Port", "Hostname_IP_Port", "Server or port empty.");
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Initialization.Check_Connection.Hostname_IP_Port", "Hostname_IP_Port", ex.ToString());
            }

            return false;
        }
    }
}
