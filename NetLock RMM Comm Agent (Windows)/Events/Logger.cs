using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Data;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;

namespace NetLock_RMM_Comm_Agent_Windows.Events
{
    internal class Logger
    {
        public static void Insert_Event(string severity, string reported_by, string _event, string description, int type, int lang)
        {
            while (Service.events_processing == true)
                Thread.Sleep(1000);

            try
            {
                Service.events_data_table.Rows.Add(severity, reported_by, _event, description, type, lang);
                Logging.Handler.Debug("Events.Logger.Insert_Event", "", "Done");
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Events.Logger.Insert_Event", "Failed", ex.Message);
            }
        }

        public static void Consume_Events()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                Logging.Handler.Debug("Events.Logger.Consume_Events", "", "Start");

                using (SQLiteConnection db_conn = new SQLiteConnection(Application_Settings.NetLock_Events_Database_String))
                {
                    db_conn.Open();

                    foreach (DataRow row in Service.events_data_table.Rows)
                    {
                        SQLiteCommand command = new SQLiteCommand("INSERT INTO events ('severity', 'reported_by', 'event', 'description', 'type', 'lang', 'mmc') VALUES (" +
                            "'" + Encryption.String_Encryption.Encrypt(row["severity"].ToString(), Application_Settings.NetLock_Local_Encryption_Key) + "', " + //_event
                            "'" + Encryption.String_Encryption.Encrypt(row["reported_by"].ToString(), Application_Settings.NetLock_Local_Encryption_Key) + "', " + //_event
                            "'" + Encryption.String_Encryption.Encrypt(row["event"].ToString(), Application_Settings.NetLock_Local_Encryption_Key) + "', " + //_event
                            "'" + Encryption.String_Encryption.Encrypt(row["description"].ToString(), Application_Settings.NetLock_Local_Encryption_Key) + "', " + //description
                            "'" + row["type"] + "', " + //type
                            "'" + row["lang"] + "', " + //lang
                            "'0'" + //mmc
                            ")"
                            , db_conn);

                        command.ExecuteNonQuery();
                    }

                    db_conn.Close();
                    db_conn.Dispose();
                }

                Service.events_data_table.Rows.Clear();

                Logging.Handler.Debug("Events.Logger.Consume_Events", "", "Stop");
            }
            catch (Exception ex)
            {
                Logging.Handler.Error("Events.Logger.Consume_Events", "Failed", ex.Message);
            }
        }

        public static async Task Process_Events()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            try
            {
                Logging.Handler.Debug("Events.Logger.Process_Events", "", "Started");

                int count = 0;
                bool connection_status = false;

                using (SQLiteConnection db_conn = new SQLiteConnection(Application_Settings.NetLock_Events_Database_String))
                {
                    db_conn.Open();

                    //Count Reader
                    SQLiteCommand count_scdCommand = new SQLiteCommand("SELECT * FROM events;", db_conn);
                    SQLiteDataReader count_reader = count_scdCommand.ExecuteReader();

                    while (count_reader.Read())
                        count++;

                    //If events existing to report, do connection check
                    if (count > 0)
                        connection_status = await Initialization.Check_Connection.Communication_Server();
                    else //No events
                    {
                        Logging.Handler.Debug("Events.Logger.Process_Events", "Get Events", "No events. Canceling event processing...");
                        return;
                    }

                    //Connection result
                    if (connection_status == false)
                        Logging.Handler.Debug("Events.Logger.Process_Events", "Connection Check", "Not online.");
                    else
                        Logging.Handler.Debug("Events.Logger.Process_Events", "Connection Check", "Online.");

                    //Start processing
                    SQLiteCommand scdCommand = new SQLiteCommand("SELECT * FROM events;", db_conn);
                    SQLiteDataReader reader = scdCommand.ExecuteReader();

                    while (reader.Read())
                    {
                        Logging.Handler.Debug("Events.Logger.Process_Events", "Process Event", Environment.NewLine + "Reported by: " + reader["reported_by"].ToString() + Environment.NewLine + "Event: " + reader["event"].ToString() + Environment.NewLine + "Description: " + reader["description"].ToString() + Environment.NewLine + "Type: " + reader["type"].ToString() + Environment.NewLine + "Lang: " + reader["lang"].ToString());

                        //Send mmc if not send already 
                        bool mmc_send = false;

                        if (connection_status && reader["mmc"].ToString() == "0")
                            mmc_send = await Events.Sender.Send_Event(reader["severity"].ToString(), reader["reported_by"].ToString(), reader["event"].ToString(), reader["description"].ToString(), reader["type"].ToString(), reader["lang"].ToString());

                        //If mmc send success, update its status in db
                        if (mmc_send)
                        {
                            SQLiteCommand command = new SQLiteCommand("UPDATE events SET mmc = '1' WHERE id = '" + reader["id"].ToString() + "';", db_conn);
                            command.ExecuteNonQuery();
                        }

                        Logging.Handler.Debug("Events.Logger.Process_Events", "Process Event", "Send mmc: " + mmc_send);

                        //Delete event if everything done
                        bool log_delete = false;

                        if (reader["mmc"].ToString() == "1")
                            log_delete = true;

                        Logging.Handler.Debug("Events.Logger.Process_Events", "Process Event", "log_delete: " + log_delete);

                        //Delete processed event
                        if (log_delete)
                        {
                            SQLiteCommand command = new SQLiteCommand("DELETE FROM events WHERE id = '" + reader["id"].ToString() + "';", db_conn);
                            command.ExecuteNonQuery();
                        }
                    }

                    reader.Close();
                    db_conn.Close();
                    db_conn.Dispose();

                    Logging.Handler.Debug("Events.Logger.Process_Events", "Stopped", "");
                }
            }
            catch (Exception ex)
            {
                Logging.Handler.Debug("Events.Logger.Process_Events", "Failed", ex.Message);
            }
        }
    }
}
