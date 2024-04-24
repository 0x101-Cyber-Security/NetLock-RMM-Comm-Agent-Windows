using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace NetLock_RMM_Comm_Agent_Windows.Client_Information
{
    public class Process_Data
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

    internal class Processes
    {
        public static void Collect()
        {
            try
            {
                // Create a list of JSON strings for each process
                List<string> processJsonList = new List<string>();

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "select * from Win32_Process"))
                {
                    // Parallel execution of the loop
                    Parallel.ForEach(searcher.Get().OfType<ManagementObject>(), reader =>
                    {
                        try
                        {
                            int pid = Convert.ToInt32(reader["ProcessId"]);
                            Process process_info = Process.GetProcessById(pid);

                            // Collect process information
                            string name = reader["Name"].ToString();
                            int cpu_percentage = 0;
                            int parent_id = Convert.ToInt32(reader["ParentProcessId"]);
                            string parent_name = Process.GetProcessById(parent_id).ProcessName;
                            double ram_usage = 0;
                            string user = Helper.Process_User.Get(process_info) + " (" + process_info.SessionId + ")";
                            string created = process_info.StartTime.ToString();
                            string path = process_info.MainModule.FileName;
                            string commandline = reader["CommandLine"].ToString();
                            int handles = 0, threads = 0, read_operations = 0, read_transfer = 0, write_operations = 0, write_transfer = 0;

                            int.TryParse(reader["HandleCount"].ToString(), out handles);
                            int.TryParse(reader["ThreadCount"].ToString(), out threads);
                            int.TryParse(reader["ReadOperationCount"].ToString(), out read_operations);
                            int.TryParse(reader["WriteOperationCount"].ToString(), out write_operations);
                            double.TryParse(reader["ReadTransferCount"].ToString(), out double readTransfer);
                            double.TryParse(reader["WriteTransferCount"].ToString(), out double writeTransfer);
                            read_transfer = (int)(readTransfer / (1024 * 1024));
                            write_transfer = (int)(writeTransfer / (1024 * 1024));

                            // RAM usage in MB
                            var ram_perf_c = new PerformanceCounter("Process", "Working Set - Private", process_info.ProcessName);
                            ram_usage = Math.Round((double)ram_perf_c.RawValue / (1024 * 1024), 2);

                            // CPU usage in %
                            using (ManagementObjectSearcher searcher_perf = new ManagementObjectSearcher("root\\CIMV2", "select * from Win32_PerfFormattedData_PerfProc_Process WHERE IDProcess = '" + pid + "'"))
                            {
                                foreach (ManagementObject reader_perf in searcher_perf.Get())
                                    cpu_percentage = Convert.ToInt32(reader_perf["PercentProcessorTime"]) / Environment.ProcessorCount;
                            }

                            // Create process object
                            Process_Data processInfo = new Process_Data
                            {
                                name = name,
                                pid = pid.ToString(),
                                parent_name = parent_name,
                                parent_pid = parent_id.ToString(),
                                cpu = cpu_percentage.ToString(),
                                ram = ram_usage.ToString(),
                                user = user,
                                created = created,
                                path = path,
                                cmd = commandline,
                                handles = handles.ToString(),
                                threads = threads.ToString(),
                                read_operations = read_operations.ToString(),
                                read_transfer = read_transfer.ToString(),
                                write_operations = write_operations.ToString(),
                                write_transfer = write_transfer.ToString()
                            };

                            // Serialize the process object into a JSON string and add it to the list
                            string processJson = JsonConvert.SerializeObject(processInfo, Formatting.Indented);
                            processJsonList.Add(processJson);
                        }
                        catch (Exception ex)
                        {
                            Logging.Handler.Client_Information("Client_Information.Process_List.Collect", "Failed.", ex.Message);
                        }
                    });
                }

                // Create and log JSON array
                Service.processes_list = "[" + string.Join("," + Environment.NewLine, processJsonList) + "]";

                Logging.Handler.Client_Information("Client_Information.Process_List.Collect", "Collected the following process information.", Service.processes_list);
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Client_Information.Process_List.Collect", "Failed.", ex.ToString());
            }   
        }
    }
}
