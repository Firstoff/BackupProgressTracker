using System;
using System.Data.SqlClient;
using System.Threading;

public class BackupProgressTracker
{
    private string serverName;
    private string databaseName;

    public BackupProgressTracker(string serverName, string databaseName)
    {
        this.serverName = serverName;
        this.databaseName = databaseName;
    }

    public void TrackBackupProgress()
    {
        string connectionString = $"Data Source={serverName};Initial Catalog={databaseName};Integrated Security=True";
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            connection.Open();
            string query = $"SELECT percent_complete, estimated_completion_time FROM sys.dm_exec_requests WHERE command LIKE 'BACKUP%' AND database_id = DB_ID('{databaseName}')";
            //using (SqlCommand command = new SqlCommand(query, connection))
            //{
            //    using (SqlDataReader reader = command.ExecuteReader())
            //    {
            //        while (reader.Read())
            //        {
            //            for (int i = 0; i < reader.FieldCount; i++)
            //            {
            //                Console.Write(reader[i] + "\t");

            //                float percentcomplete = reader.GetFloat(0);
            //                Console.WriteLine("Percent complete: " + percentcomplete);//good

            //                long estimated_completion_time = reader.GetInt64(1);
            //                Console.WriteLine("estimated_completion_time: " + estimated_completion_time);//good

            //            }
            //            Console.WriteLine();
            //        }
            //        Console.ReadKey();
            //    }
            //}
            using (SqlCommand command = new SqlCommand(query, connection))
            {
                while (true)
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            float percentComplete = reader.GetFloat(0);
                            long estimatedCompletionTime = reader.GetInt64(1);

                            Console.WriteLine($"Percent complete: {percentComplete}%, Estimated Completion Time: {estimatedCompletionTime} seconds");

                            // Проверяем, завершено ли выполнение бекапа
                            if (percentComplete == 100)
                            {
                                Console.WriteLine("Backup completed.");
                                return;
                            }
                        }
                    }

                    // Задержка перед следующим обновлением прогресса
                    Thread.Sleep(10);
                }
            }
        }
    }
}

public class Program
{
    public static void Main()
    {
        string serverName = "zxhome\\mssql";
        string databaseName = "northwind33";

        BackupProgressTracker tracker = new BackupProgressTracker(serverName, databaseName);
        tracker.TrackBackupProgress();
    }
}
