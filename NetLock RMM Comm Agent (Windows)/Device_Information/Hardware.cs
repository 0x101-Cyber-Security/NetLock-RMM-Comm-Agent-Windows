﻿using Newtonsoft.Json;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static NetLock_RMM_Comm_Agent_Windows.Online_Mode.Handler;

namespace NetLock_RMM_Comm_Agent_Windows.Device_Information
{
    internal class Hardware
    {
        public static string CPU_Information()
        {
            string cpu_information_json = string.Empty;

            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        HashSet<string> uniqueSockets = new HashSet<string>();

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
                            l1_cache = Math.Round(Convert.ToDouble(obj["L2CacheSize"]) / 1024).ToString(), //l1cache is not available in WMI
                            l2_cache = Math.Round(Convert.ToDouble(obj["L2CacheSize"]) / 1024).ToString(),
                            l3_cache = Math.Round(Convert.ToDouble(obj["L3CacheSize"]) / 1024).ToString()
                        };

                        // Serialize the process object into a JSON string and add it to the list
                        cpu_information_json = JsonConvert.SerializeObject(cpuInfo, Formatting.Indented);
                        Logging.Handler.Device_Information("Device_Information.Hardware.CPU_Information", "cpu_information_json", cpu_information_json);
                    }
                }

                return cpu_information_json;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Device_Information.Hardware.CPU_Information", "CPU_Information", "An error occurred while querying for WMI data: " + ex.ToString());
                return "{}";
            }
        }

        // needs rework some day to support gathering of multiple RAM sticks
        public static string RAM_Information()
        {
            string ram_information_json = string.Empty;

            try
            {
                string name = string.Empty;
                string available = string.Empty;
                string assured = $"{((double)Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024 * 1024)):N1} / {((double)new PerformanceCounter("Memory", "Available MBytes").NextValue() / 1024):N1}"; //data is not correct. Either my knowledge is wrong here, or the calculations. Doesnt represent the numbers in task manager
                string cache = string.Empty;
                string outsourced_pool = string.Empty;
                string not_outsourced_pool = string.Empty;
                string speed = string.Empty;
                string slots = new ManagementObjectSearcher("SELECT MemoryDevices FROM Win32_PhysicalMemoryArray").Get().Cast<ManagementBaseObject>().Select(obj => obj["MemoryDevices"].ToString()).FirstOrDefault();
                int slots_used = 0;
                string form_factor = string.Empty;
                string hardware_reserved = string.Empty;

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PerfRawData_PerfOS_Memory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        cache = Math.Floor(Convert.ToDouble(obj["AvailableBytes"]) / (1024 * 1024)).ToString(); // currently no way known to get the cache size. This is a placeholder
                        outsourced_pool = Math.Floor(Convert.ToDouble(obj["PoolPagedBytes"]) / (1024 * 1024)).ToString();
                        not_outsourced_pool = Math.Floor(Convert.ToDouble(obj["PoolNonpagedBytes"]) / (1024 * 1024)).ToString();
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT TotalVisibleMemorySize, FreePhysicalMemory FROM Win32_OperatingSystem"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        ulong totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]); // Gesamtspeicher in KB
                        ulong freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]); // Freier Speicher in KB
                        ulong hardwareReservedMemory = totalMemory - freeMemory; // Hardware-reservierter Speicherplatz in KB

                        hardware_reserved = (hardwareReservedMemory / 1024d).ToString();
                    }
                }

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        slots_used++;

                        name = obj["Name"].ToString();
                        available = available = (Convert.ToDouble(obj["Capacity"]) / (1024 * 1024)).ToString();
                        speed = obj["Speed"].ToString();
                        form_factor = obj["DeviceLocator"].ToString();
                    }
                }

                // Create JSON
                RAM_Information ramInfo = new RAM_Information
                {
                    name = name,
                    available = available,
                    assured = assured,
                    cache = cache, 
                    outsourced_pool = outsourced_pool, 
                    not_outsourced_pool = not_outsourced_pool, 
                    speed = speed, 
                    slots = slots,
                    slots_used = slots_used.ToString(),
                    form_factor = form_factor, 
                    hardware_reserved = hardware_reserved, 
                };

                ram_information_json = JsonConvert.SerializeObject(ramInfo, Formatting.Indented);

                // Create and log JSON array
                Logging.Handler.Device_Information("Device_Information.Hardware.RAM_Information", "Collected the following process information.", ram_information_json);

                return ram_information_json;
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Device_Information.Hardware.RAM_Information", "RAM_Information", "An error occurred while querying for WMI data: " + ex.ToString());
                return "{}";
            }
        }

        public static string Disks()
        {
            try
            {
                // Create a list of JSON strings for each process
                List<string> disksJsonList = new List<string>();
                List<string> collectedLettersList = new List<string>();

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_DiskDrive"))
                {
                    // Parallel execution of the loop
                    Parallel.ForEach(searcher.Get().OfType<ManagementObject>(), obj =>
                    {
                        try
                        {
                            string letter = string.Empty;
                            string volume_label = string.Empty;
                            string DeviceID = obj["DeviceID"].ToString(); // wmi SELECT * FROM Win32_DiskDrive -> DeviceID
                            string PNPDeviceID = obj["PNPDeviceID"].ToString(); // wmi SELECT * FROM Win32_DiskDrive -> PNPDeviceID
                            
                            using (ManagementObjectSearcher searcher1 = new ManagementObjectSearcher("root\\CIMV2", "ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + obj["DeviceID"].ToString() + "'} WHERE AssocClass = Win32_DiskDriveToDiskPartition"))
                            {
                                foreach (ManagementObject obj1 in searcher1.Get())
                                {
                                    foreach (ManagementObject obj2 in new ManagementObjectSearcher("root\\CIMV2", "ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + obj1["DeviceID"].ToString() + "'} WHERE AssocClass = Win32_LogicalDiskToPartition").Get())
                                    {   
                                        letter = obj2["Name"].ToString();
                                        volume_label = new DriveInfo(letter)?.VolumeLabel ?? "N/A";
                                    }
                                }
                            }

                            // Get disks capacity by letter
                            DriveInfo driveInfo = new DriveInfo(letter);
                            string totalCapacityGBString = ((double)driveInfo.TotalSize / (1024 * 1024 * 1024)).ToString("0.00");
                            string usedCapacityPercentageString = $"{((double)(driveInfo.TotalSize - driveInfo.AvailableFreeSpace) / driveInfo.TotalSize) * 100:F2}";

                            //Add the letter to the list of collected letters
                            collectedLettersList.Add(letter);
                            Logging.Handler.Device_Information("Device_Information.Hardware.Disks", "Collected the following disk information.", $"Letter: {letter}, Volume Label: {volume_label}, DeviceID: {DeviceID}, PNPDeviceID: {PNPDeviceID}");

                            // Create disk object
                            Disks diskInfo = new Disks
                            {
                                letter = letter, // wmi SELECT * FROM Win32_LogicalDisk -> Name
                                label = string.IsNullOrEmpty(volume_label) ? "N/A" : volume_label, //wmi SELECT * FROM Win32_LogicalDisk -> Name
                                model = obj["Model"].ToString(), //wmi SELECT * FROM Win32_DiskDrive
                                firmware_revision = obj["FirmwareRevision"].ToString(), //wmi SELECT * FROM Win32_DiskDrive
                                serial_number = obj["SerialNumber"].ToString(), //wmi SELECT * FROM Win32_DiskDrive
                                interface_type = obj["InterfaceType"].ToString(), // wmi SELECT * FROM Win32_DiskDrive
                                drive_type = driveInfo.DriveType.ToString() == "Removable" ? "0" : driveInfo.DriveType.ToString() == "Fixed" ? "1" : driveInfo.DriveType.ToString() == "Network" ? "2" : driveInfo.DriveType.ToString() == "CDRom" ? "3" : driveInfo.DriveType.ToString() == "Ram" ? "4" : "5", // Removable = 0, Fixed = 1, Network = 2, CDRom = 3, Ram = 4, Unknown = 5
                                drive_format = driveInfo.DriveFormat,
                                drive_ready = driveInfo.IsReady.ToString(),
                                capacity = totalCapacityGBString,
                                usage = usedCapacityPercentageString,
                                status = obj["Status"].ToString(), //wmi SELECT * FROM Win32_DiskDrive
                            };

                            // Serialize the disk object into a JSON string and add it to the list
                            string disksJson = JsonConvert.SerializeObject(diskInfo, Formatting.Indented);
                            disksJsonList.Add(disksJson);
                        }
                        catch (Exception ex)
                        {
                            Logging.Handler.Device_Information("Device_Information.Hardware.Disks", "Failed.", ex.ToString());
                        }
                    });

                    //Try to gather basic disk information for each letter that failed
                    try
                    {
                        foreach (DriveInfo drive in DriveInfo.GetDrives())
                        {
                            string letter_short = drive.Name.Replace("\\", "");

                            try
                            {
                                if (!collectedLettersList.Contains(letter_short)) // Check if the letter is already in the list
                                {
                                    Disks diskInfo = new Disks
                                    {
                                        letter = drive.Name.Replace("\\", ""),
                                        label = string.IsNullOrEmpty(drive.VolumeLabel) ? "N/A" : drive.VolumeLabel,
                                        model = "N/A",
                                        firmware_revision = "N/A",
                                        serial_number = "N/A",
                                        interface_type = "N/A",
                                        drive_type = drive.DriveType.ToString() == "Removable" ? "0" : drive.DriveType.ToString() == "Fixed" ? "1" : drive.DriveType.ToString() == "Network" ? "2" : drive.DriveType.ToString() == "CDRom" ? "3" : drive.DriveType.ToString() == "Ram" ? "4" : "5", // Removable = 0, Fixed = 1, Network = 2, CDRom = 3, Ram = 4, Unknown = 5
                                        drive_format = drive.DriveFormat,
                                        drive_ready = drive.IsReady.ToString(),
                                        capacity = ((double)drive.TotalSize / (1024 * 1024 * 1024)).ToString("0.00"),
                                        usage = $"{((double)(drive.TotalSize - drive.AvailableFreeSpace) / drive.TotalSize) * 100:F2}",
                                        status = "N/A",
                                    };

                                    // Serialize the disk object into a JSON string and add it to the list
                                    string disksJson = JsonConvert.SerializeObject(diskInfo, Formatting.Indented);
                                    disksJsonList.Add(disksJson);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logging.Handler.Device_Information("Device_Information.Hardware.Disks", "Failed to gather basic disk information for each letter that failed.", ex.ToString());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logging.Handler.Device_Information("Device_Information.Hardware.Disks", "Failed to gather basic disk information for each letter that failed. (general error)", ex.ToString());
                    }

                    // Create and log JSON array
                    string disks_json = "[" + string.Join("," + Environment.NewLine, disksJsonList) + "]";

                    Logging.Handler.Device_Information("Device_Information.Hardware.Disks", "Collected the following disk information.", disks_json);
                    return disks_json;
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Device_Information.Hardware.Disks", "General error.", ex.ToString());
                return "[]";
            }
        }
    }
}