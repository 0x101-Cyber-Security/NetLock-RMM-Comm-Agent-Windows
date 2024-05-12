using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using NetLock_Agent.Helper;
using System.Globalization;

namespace NetLock_RMM_Comm_Agent_Windows.Microsoft_Defender_Antivirus
{
    internal class Scan_Jobs_Scheduler
    {
        public class Scan_Job
        {
            public string id { get; set; }
            public string name { get; set; }
            public string description { get; set; }
            public string date { get; set; }
            public string last_run { get; set; }
            public int time_scheduler_type { get; set; }
            public int time_scheduler_seconds { get; set; }
            public int time_scheduler_minutes { get; set; }
            public int time_scheduler_hours { get; set; }
            public string time_scheduler_time { get; set; }
            public string time_scheduler_date { get; set; }
            public bool time_scheduler_monday { get; set; }
            public bool time_scheduler_tuesday { get; set; }
            public bool time_scheduler_wednesday { get; set; }
            public bool time_scheduler_thursday { get; set; }
            public bool time_scheduler_friday { get; set; }
            public bool time_scheduler_saturday { get; set; }
            public bool time_scheduler_sunday { get; set; }
            public int scan_type { get; set; }
            public int scan_settings_cpu_usage { get; set; }
            public bool scan_settings_scan_on_battery { get; set; }
            public bool scan_settings_network_drives { get; set; }
            public bool scan_settings_removable_disks { get; set; }
            public bool scan_settings_update_signatures { get; set; }
            public List<Antivirus_Scan_Job_Directories> scan_directories_json { get; set; }
        }

        public class Antivirus_Scan_Job_Directories
        {
            public string date { get; set; } = String.Empty;
            public string directory { get; set; } = String.Empty;
            public string description { get; set; } = String.Empty;
        }

        public static void Check_Execution()
        {
            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Check scan job execution", "Start");

            Initialization.Health.Check_Directories();

            try
            {
                List<Scan_Job> scan_jobItems = JsonSerializer.Deserialize<List<Scan_Job>>(Service.policy_antivirus_scan_jobs_json);

                // Write each scan job to disk if not already exists
                foreach (var job in scan_jobItems)
                {
                    Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Check if scan job exists on disk", "Job: " + job.name + " Job id: " + job.id);

                    string job_json = JsonSerializer.Serialize(job);
                    string job_path = Application_Paths.program_data_microsoft_defender_antivirus_scan_jobs + "\\" + job.id + ".json";

                    if (!File.Exists(job_path))
                    {
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Check if scan job exists on disk", "false");
                        File.WriteAllText(job_path, job_json);
                    }
                }

                // Clean up old scan jobs not existing anymore
                foreach (string file in Directory.GetFiles(Application_Paths.program_data_microsoft_defender_antivirus_scan_jobs))
                {
                    Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Clean old scan jobs", "Job: " + file);

                    string file_name = Path.GetFileName(file);
                    string file_id = file_name.Replace(".json", "");

                    bool found = false;

                    foreach (var job in scan_jobItems)
                    {
                        if (job.id == file_id)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Clean old scan jobs", "Delete job: " + file);
                        File.Delete(file);
                    }
                }

                // Now read & consume each scan job
                foreach (var job in Directory.GetFiles(Application_Paths.program_data_microsoft_defender_antivirus_scan_jobs))
                {
                    string job_json = File.ReadAllText(job);
                    Scan_Job job_item = JsonSerializer.Deserialize<Scan_Job>(job_json);

                    Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Check scan job execution", "Job: " + job_item.name);

                    bool execute = false;

                    // Check if last run is empty, if so set it to now
                    if (String.IsNullOrEmpty(job_item.last_run))
                        job_item.last_run = DateTime.Now.ToString();

                    if (job_item.time_scheduler_type == 0) // system boot
                    {
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "System boot", "name: " + job_item.name + " id: " + job_item.id + " last_run: " + DateTime.Parse(job_item.last_run) + " Last boot: " + (DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount)));
                        if (DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMilliseconds(Environment.TickCount))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 1) // date & time
                    {
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "date & time", "name: " + job_item.name + " id: " + job_item.id + " last_run: " + DateTime.Parse(job_item.last_run) + " Last boot: DateTime.Parse($\"{job_item.time_scheduler_date} {job_item.time_scheduler_time}\"" + DateTime.ParseExact($"{job_item.time_scheduler_date} {job_item.time_scheduler_time}", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture));

                        DateTime scheduledDateTime = DateTime.ParseExact($"{job_item.time_scheduler_date} {job_item.time_scheduler_time}", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime lastRunDateTime = DateTime.Parse(job_item.last_run);

                        if (DateTime.Now.Date >= scheduledDateTime.Date && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 2) // all x seconds
                    {
                        if (DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 3) // all x minutes
                    {
                        if (DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 4) // all x hours
                    {
                        if (DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 5) // date, all x seconds
                    {
                        if (DateTime.Now.Date == DateTime.Parse(job_item.time_scheduler_date).Date && DateTime.Now >= DateTime.Parse(job_item.time_scheduler_date) && DateTime.Now >= DateTime.Parse(job_item.last_run) + TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 6) // date, all x minutes
                    {
                        if (DateTime.Now.Date == DateTime.Parse(job_item.time_scheduler_date).Date && DateTime.Now.TimeOfDay >= TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Now >= DateTime.Parse(job_item.last_run) + TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 7) // date, all x hours
                    {
                        if (DateTime.Now.Date == DateTime.Parse(job_item.time_scheduler_date).Date && DateTime.Now.TimeOfDay >= TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Now >= DateTime.Parse(job_item.last_run) + TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 8) // following days at X time
                    {
                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && job_item.time_scheduler_monday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && job_item.time_scheduler_tuesday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && job_item.time_scheduler_wednesday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && job_item.time_scheduler_thursday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && job_item.time_scheduler_friday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && job_item.time_scheduler_saturday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && job_item.time_scheduler_sunday && DateTime.Now.TimeOfDay > TimeSpan.Parse(job_item.time_scheduler_time) && DateTime.Parse(job_item.last_run) <= DateTime.Parse(job_item.time_scheduler_date).Add(TimeSpan.Parse(job_item.time_scheduler_time)))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 9) // following days, x seconds
                    {
                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && job_item.time_scheduler_monday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && job_item.time_scheduler_tuesday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && job_item.time_scheduler_wednesday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && job_item.time_scheduler_thursday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && job_item.time_scheduler_friday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && job_item.time_scheduler_saturday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && job_item.time_scheduler_sunday && DateTime.Parse(job_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(job_item.time_scheduler_seconds))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 10) // following days, x minutes
                    {
                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && job_item.time_scheduler_monday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && job_item.time_scheduler_tuesday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && job_item.time_scheduler_wednesday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && job_item.time_scheduler_thursday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && job_item.time_scheduler_friday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && job_item.time_scheduler_saturday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && job_item.time_scheduler_sunday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(job_item.time_scheduler_minutes))
                            execute = true;
                    }
                    else if (job_item.time_scheduler_type == 11) // following days, x hours
                    {
                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && job_item.time_scheduler_monday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && job_item.time_scheduler_tuesday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && job_item.time_scheduler_wednesday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && job_item.time_scheduler_thursday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && job_item.time_scheduler_friday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && job_item.time_scheduler_saturday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && job_item.time_scheduler_sunday && DateTime.Parse(job_item.last_run) < DateTime.Now - TimeSpan.FromHours(job_item.time_scheduler_hours))
                            execute = true;
                    }

                    // Execute if needed
                    if (execute)
                    {
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Execute scan job", "name: " + job_item.name + " id: " + job_item.id);

                        //Set Options for scan job
                        PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Jobs_Scheduler.Check_Execution.Set_CPU_Load", "Set-MpPreference -ScanAvgCPULoadFactor " + job_item.scan_settings_cpu_usage, 30);
                        PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Jobs_Scheduler.Check_Execution.Set_On_Battery", "Set-MpPreference -EnableFullScanOnBatteryPower $" + job_item.scan_settings_scan_on_battery, 30);

                        if (job_item.scan_settings_network_drives)
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Scan_Mapped_Network_Drives", "Set-MpPreference -DisableScanningMappedNetworkDrivesForFullScan $false", 30);
                        else
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Scan_Mapped_Network_Drives", "Set-MpPreference -DisableScanningMappedNetworkDrivesForFullScan $true", 30);

                        if (job_item.scan_settings_removable_disks)
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Scan_Removable_Drives", "Set-MpPreference -DisableRemovableDriveScanning $false", 30);
                        else
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Scan_Removable_Drives", "Set-MpPreference -DisableRemovableDriveScanning $true", 30);

                        // Check sigs
                        if (job_item.scan_settings_update_signatures)
                        {
                            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_Antivirus.Check_Hourly_Sig_Updates", "Execute", "Before");
                            PowerShell.Execute_Command("Microsoft_Defender_Antivirus.Check_Hourly_Sig_Updates", "Update-MpSignature", 60);
                            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_Antivirus.Check_Hourly_Sig_Updates", "Execute", "After");
                        }

                        //Check scanjob type
                        if (job_item.scan_type == 0) // quick scan
                        {
                            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution", "Execute quickscan.", job_item.name);
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Execute_Quickscan", "Start-MpScan -ScanType QuickScan", 30);
                        }
                        else if (job_item.scan_type == 1) // full scan
                        {
                            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution", "Execute fullscan.", job_item.name);
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Execute_Fullscan", "Start-MpScan -ScanType FullScan", 30);
                        }
                        else if (job_item.scan_type == 2) // custom scan
                        {
                            Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution", "Execute fullscan.", job_item.name);
                            PowerShell.Execute_Command("Microsoft_Defender_AntiVirus.Scan_Job_Scheduler.Check_Execution.Execute_Fullscan", "Start-MpScan -ScanType FullScan", 30);
                        }

                        // Update last run
                        job_item.last_run = DateTime.Now.ToString();
                        string updated_job_json = JsonSerializer.Serialize(job_item);
                        File.WriteAllText(job, updated_job_json);

                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Execution finished", "name: " + job_item.name + " id: " + job_item.id);
                    }
                    else
                        Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Scan job will not be executed", "name: " + job_item.name + " id: " + job_item.id);
                }

                Logging.Handler.Microsoft_Defender_Antivirus("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "Check scan job execution", "Stop");
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Microsoft_Defender_AntiVirus.Scan_Jobs.Check_Execution", "General Error", ex.ToString());
            }
        }
    }
}
