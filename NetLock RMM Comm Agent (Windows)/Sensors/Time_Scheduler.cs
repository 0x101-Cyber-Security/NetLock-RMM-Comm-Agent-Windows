using NetLock_Agent.Helper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Diagnostics;
using System.Threading;
using NetLock_RMM_Comm_Agent_Windows.Microsoft_Defender_Antivirus;

namespace NetLock_RMM_Comm_Agent_Windows.Sensors
{
    internal class Time_Scheduler
    {
        public class Sensor
        {
            public string id { get; set; }
            public string name { get; set; }
            public string date { get; set; }
            public string last_run { get; set; }
            public string author { get; set; }
            public string description { get; set; }
            public int severity { get; set; }
            public int category { get; set; }
            public int sub_category { get; set; }
            public int utilization_category { get; set; }
            public int notification_treshold_count { get; set; }
            public int notification_treshold_max { get; set; }
            public int action_treshold_count { get; set; }
            public int action_treshold_max { get; set; }
            public string action_history { get; set; }
            public string script { get; set; }
            public string script_action { get; set; }
            public int cpu_usage { get; set; }
            public int ram_usage { get; set; }
            public int disk_usage { get; set; }
            public int disk_minimum_capacity { get; set; }
            public int disk_category { get; set; }
            public string disk_letters { get; set; }
            public bool include_network_disks { get; set; }
            public bool include_removable_disks { get; set; }
            public string eventlog { get; set; }
            public string eventlog_category { get; set; }
            public string eventlog_event_id { get; set; }
            public string expected_result { get; set; }

            //service sensor
            public string service_name { get; set; }
            public int service_condition { get; set; }
            public int service_action { get; set; }

            //ping sensor
            public string ping_hostname { get; set; }

            //time schedule
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
        }

        public static void Check_Execution()
        {
            Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check sensor execution", "Start");

            Initialization.Health.Check_Directories();

            try
            {
                DateTime os_up_time = ManagementDateTimeConverter.ToDateTime(Helper.WMI.Search("root\\cimv2", "SELECT LastBootUpTime FROM Win32_OperatingSystem", "LastBootUpTime")); // Environment.TickCount is not reliable, use WMI instead

                List<Sensor> scan_sensorItems = JsonSerializer.Deserialize<List<Sensor>>(Service.policy_sensors_json);

                // Write each sensor to disk if not already exists
                foreach (var sensor in scan_sensorItems)
                {
                    Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check if sensor exists on disk", "Sensor: " + sensor.name + " Sensor id: " + sensor.id);

                    string sensor_json = JsonSerializer.Serialize(sensor);
                    string sensor_path = Application_Paths.program_data_sensors + "\\" + sensor.id + ".json";

                    if (!File.Exists(sensor_path))
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check if sensor exists on disk", "false");
                        File.WriteAllText(sensor_path, sensor_json);
                    }
                }

                // Clean up old sensors not existing anymore
                foreach (string file in Directory.GetFiles(Application_Paths.program_data_sensors))
                {
                    Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Clean old sensors", "Sensor: " + file);

                    string file_name = Path.GetFileName(file);
                    string file_id = file_name.Replace(".json", "");

                    bool found = false;

                    foreach (var sensor in scan_sensorItems)
                    {
                        if (sensor.id == file_id)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Clean old sensors", "Delete sensor: " + file);
                        File.Delete(file);
                    }
                }

                // Now read & consume each sensor
                foreach (var sensor in Directory.GetFiles(Application_Paths.program_data_sensors))
                {
                    string sensor_json = File.ReadAllText(sensor);
                    Sensor sensor_item = JsonSerializer.Deserialize<Sensor>(sensor_json);

                    Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check sensor execution", "Sensor: " + sensor_item.name + " time_scheduler_type: " + sensor_item.time_scheduler_type);

                    // Check thresholds
                    // Check notification treshold
                    if (string.IsNullOrEmpty(sensor_item.notification_treshold_count.ToString()))
                    {
                        sensor_item.notification_treshold_count = 0;
                        string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                        File.WriteAllText(sensor, updated_sensor_json);
                    }

                    // Check action treshold
                    if (string.IsNullOrEmpty(sensor_item.action_treshold_count.ToString()))
                    {
                        sensor_item.action_treshold_count = 0;
                        string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                        File.WriteAllText(sensor, updated_sensor_json);
                    }

                    // Check enabled
                    /*if (!sensor_item.enabled)
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check sensor execution", "Sensor disabled");

                        continue;
                    }*/

                    bool execute = false;

                    if (sensor_item.time_scheduler_type == 0) // system boot
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "System boot", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()) + " Last boot: " + os_up_time.ToString());

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Parse(sensor_item.last_run) < os_up_time)
                            execute = true;
                    }
                    else if (sensor_item.time_scheduler_type == 1) // date & time
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date & time", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        DateTime scheduledDateTime = DateTime.ParseExact($"{sensor_item.time_scheduler_date.Split(' ')[0]} {sensor_item.time_scheduler_time}", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        // Check if last run is empty, if so, subsract 24 hours from scheduled time to trigger the execution
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = (scheduledDateTime - TimeSpan.FromHours(24)).ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        DateTime lastRunDateTime = DateTime.Parse(sensor_item.last_run);

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date & time", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " scheduledDateTime: " + scheduledDateTime.ToString() + " execute: " + execute.ToString());

                        if (DateTime.Now.Date >= scheduledDateTime.Date && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;
                    }
                    else if (sensor_item.time_scheduler_type == 2) // all x seconds
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 3) // all x minutes
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x minutes", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x minutes", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 4) // all x hours
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x hours", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "all x hours", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 5) // date, all x seconds
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Now.Date == DateTime.Parse(sensor_item.time_scheduler_date).Date && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 6) // date, all x minutes
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x minutes", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Now.Date == DateTime.Parse(sensor_item.time_scheduler_date).Date && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x minutes", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 7) // date, all x hours
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x hours", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Now.Date == DateTime.Parse(sensor_item.time_scheduler_date).Date && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "date, all x hours", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 8) // following days at X time
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days at X time", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        DateTime scheduledDateTime = DateTime.ParseExact($"{sensor_item.time_scheduler_date.Split(' ')[0]} {sensor_item.time_scheduler_time}", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        // Check if last run is empty, if so, subsract 24 hours from scheduled time to trigger the execution
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = (scheduledDateTime - TimeSpan.FromHours(24)).ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        DateTime lastRunDateTime = DateTime.Parse(sensor_item.last_run);

                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && sensor_item.time_scheduler_monday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && sensor_item.time_scheduler_tuesday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && sensor_item.time_scheduler_wednesday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && sensor_item.time_scheduler_thursday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && sensor_item.time_scheduler_friday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && sensor_item.time_scheduler_saturday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && sensor_item.time_scheduler_sunday && DateTime.Now.TimeOfDay >= scheduledDateTime.TimeOfDay && lastRunDateTime < scheduledDateTime)
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days at X time", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 9) // following days, x seconds
                    {
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days, x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run ?? DateTime.Now.ToString()));

                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = DateTime.Now.ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && sensor_item.time_scheduler_monday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && sensor_item.time_scheduler_tuesday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && sensor_item.time_scheduler_wednesday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && sensor_item.time_scheduler_thursday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && sensor_item.time_scheduler_friday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && sensor_item.time_scheduler_saturday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && sensor_item.time_scheduler_sunday && DateTime.Parse(sensor_item.last_run) <= DateTime.Now - TimeSpan.FromSeconds(sensor_item.time_scheduler_seconds))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days, x seconds", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 10) // following days, x minutes
                    {
                        // Check if last run is empty, if so set it to now
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                            sensor_item.last_run = DateTime.Now.ToString();

                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && sensor_item.time_scheduler_monday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && sensor_item.time_scheduler_tuesday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && sensor_item.time_scheduler_wednesday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && sensor_item.time_scheduler_thursday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && sensor_item.time_scheduler_friday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && sensor_item.time_scheduler_saturday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && sensor_item.time_scheduler_sunday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromMinutes(sensor_item.time_scheduler_minutes))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days, x minutes", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }
                    else if (sensor_item.time_scheduler_type == 11) // following days, x hours
                    {
                        DateTime scheduledDateTime = DateTime.ParseExact($"{sensor_item.time_scheduler_date.Split(' ')[0]} {sensor_item.time_scheduler_time}", "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                        // Check if last run is empty, if so, subsract 24 hours from scheduled time to trigger the execution
                        if (String.IsNullOrEmpty(sensor_item.last_run))
                        {
                            sensor_item.last_run = (scheduledDateTime - TimeSpan.FromHours(24)).ToString();
                            string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                            File.WriteAllText(sensor, updated_sensor_json);
                        }

                        DateTime lastRunDateTime = DateTime.Parse(sensor_item.last_run);

                        if (DateTime.Now.DayOfWeek.ToString() == "Monday" && sensor_item.time_scheduler_monday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Tuesday" && sensor_item.time_scheduler_tuesday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Wednesday" && sensor_item.time_scheduler_wednesday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Thursday" && sensor_item.time_scheduler_thursday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Friday" && sensor_item.time_scheduler_friday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Saturday" && sensor_item.time_scheduler_saturday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        if (DateTime.Now.DayOfWeek.ToString() == "Sunday" && sensor_item.time_scheduler_sunday && DateTime.Parse(sensor_item.last_run) < DateTime.Now - TimeSpan.FromHours(sensor_item.time_scheduler_hours))
                            execute = true;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "following days, x hours", "name: " + sensor_item.name + " id: " + sensor_item.id + " last_run: " + DateTime.Parse(sensor_item.last_run) + " execute: " + execute.ToString());
                    }

                    // Execute if needed
                    if (execute)
                    {
                        bool triggered = false;
                        
                        string action_result = String.Empty;

                        if (sensor_item.action_treshold_max != 1)
                            action_result = "[" + DateTime.Now.ToString() + "]";

                        string details_en_us = String.Empty;
                        string details_de_de = String.Empty;
                        string action_history = String.Empty;

                        int cpu_usage = 0;

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Execute sensor", "name: " + sensor_item.name + " id: " + sensor_item.id);

                        if (sensor_item.category == 0) // utilization 
                        {
                            if (sensor_item.sub_category == 0) // cpu
                            {
                                cpu_usage = Device_Information.Hardware.CPU_Utilization();

                                if (sensor_item.cpu_usage < cpu_usage) // Check if CPU utilization is higher than the treshold
                                {
                                    // if action treshold is reached, execute the action and reset the counter
                                    if (sensor_item.action_treshold_count >= sensor_item.action_treshold_max)
                                    {
                                        action_result += " " + Environment.NewLine + PowerShell.Execute_Script("Sensors.Time_Scheduler.Check_Execution (execute action) " + sensor_item.name, sensor_item.script_action);

                                        // Create action history if not exists
                                        if (String.IsNullOrEmpty(sensor_item.action_history))
                                        {
                                            List<string> action_history_list = new List<string>
                                            {
                                                action_result
                                            };

                                            sensor_item.action_history = JsonSerializer.Serialize(action_history_list);
                                        }
                                        else // if exists, add the result to the list
                                        {
                                            List<string> action_history_list = JsonSerializer.Deserialize<List<string>>(sensor_item.action_history);
                                            action_history_list.Add(action_result);
                                            sensor_item.action_history = JsonSerializer.Serialize(action_history_list);
                                        }

                                        // Create event
                                        if (Service.language == "en-US")
                                        {
                                            details_en_us =
                                                "Name: " + sensor_item.name + Environment.NewLine +
                                                "Description: " + sensor_item.description + Environment.NewLine +
                                                "Type: Processor" + Environment.NewLine +
                                                "Selected limit: " + sensor_item.cpu_usage + " (%)" + Environment.NewLine +
                                                "In usage: " + cpu_usage + " (%)" + Environment.NewLine +
                                                "Action result: " + Environment.NewLine + action_result + Environment.NewLine +
                                                "Action history";
                                        }
                                        else if (Service.language == "de-DE")
                                        {
                                            details_de_de =
                                               "Name: " + sensor_item.name + Environment.NewLine +
                                               "Beschreibung: " + sensor_item.description + Environment.NewLine +
                                               "Typ: Prozessor" + Environment.NewLine +
                                               "Festgelegtes Limit: " + sensor_item.cpu_usage + " (%)" + Environment.NewLine +
                                               "In Verwendung: " + cpu_usage + " (%)" + Environment.NewLine +
                                               "Ergebnis der Aktion: " + Environment.NewLine + action_result + Environment.NewLine +
                                               "Historie der Aktionen";
                                        }
                                           
                                        // Reset the counter
                                        sensor_item.action_treshold_count = 0;
                                        string action_treshold_updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                                        File.WriteAllText(sensor, action_treshold_updated_sensor_json);
                                    }
                                    else // if not, increment the counter
                                    {
                                        sensor_item.action_treshold_count++;
                                        string action_treshold_updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                                        File.WriteAllText(sensor, action_treshold_updated_sensor_json);
                                    }
                                        
                                    triggered = true;
                                }
                                else
                                    continue;
                            }
                        }

                        // Insert event
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Sensor executed", "name: " + sensor_item.name + " id: " + sensor_item.id);

                        if (triggered)
                        {
                            Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Triggered", true.ToString());

                            // if notification treshold is reached, insert event and reset the counter
                            if (sensor_item.notification_treshold_count >= sensor_item.notification_treshold_max)
                            {
                                // Create action history, if treshold is not 1
                                if (sensor_item.action_treshold_max != 1)
                                {
                                    List<string> action_history_list = JsonSerializer.Deserialize<List<string>>(sensor_item.action_history);

                                    foreach (var action_history_item in action_history_list)
                                        action_history += Environment.NewLine + action_history_item + Environment.NewLine;
                                }

                                // Inesert event
                                if (Service.language == "en-US")
                                    Events.Logger.Insert_Event(sensor_item.severity.ToString(), "Sensors", "Sensor  (CPU).", details_en_us + action_history, 2, 0);
                                else if (Service.language == "de-DE")
                                    Events.Logger.Insert_Event(sensor_item.severity.ToString(), "Sensors", "Sensor (CPU) angeschlagen.", details_de_de + action_history, 2, 1);

                                sensor_item.notification_treshold_count = 0;
                                string notification_treshold_updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                                File.WriteAllText(sensor, notification_treshold_updated_sensor_json);
                            }
                            else // if not, increment the counter
                            {
                                sensor_item.notification_treshold_count++;
                                string notification_treshold_updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                                File.WriteAllText(sensor, notification_treshold_updated_sensor_json);
                            }
                        }
                        else
                            Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Triggered", false.ToString());

                        // Update last run
                        sensor_item.last_run = DateTime.Now.ToString();
                        string updated_sensor_json = JsonSerializer.Serialize(sensor_item);
                        File.WriteAllText(sensor, updated_sensor_json);

                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Execution finished", "name: " + sensor_item.name + " id: " + sensor_item.id);
                    }
                    else
                        Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Sensor will not be executed", "name: " + sensor_item.name + " id: " + sensor_item.id);
                }

                Logging.Handler.Sensors("Sensors.Time_Scheduler.Check_Execution", "Check sensor execution", "Stop");
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Sensors.Time_Scheduler.Check_Execution", "General Error", ex.ToString());
            }
        }
    }
}
