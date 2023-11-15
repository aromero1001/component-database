using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace YourNamespace
{
    public static class Logger
    {
        private static readonly object LogLock = new object();
        private static readonly string LogFolderPath;

        static Logger()
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            LogFolderPath = configuration["Logs:LogFolderPath"];
        }

        public static void Log(string message)
        {
            try
            {
                lock (LogLock)
                {
                    string logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LogFolderPath);
                    string logFileName = $"log-{DateTime.Now:yyyy-MM-dd}.txt";
                    string logFilePath = Path.Combine(logDirectory, logFileName);

                    if (!Directory.Exists(logDirectory))
                    {
                        Directory.CreateDirectory(logDirectory);
                    }

                    bool isNewFile = !File.Exists(logFilePath);

                    using (StreamWriter sw = new StreamWriter(logFilePath, true))
                    {
                        if (isNewFile)
                        {
                            sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Log file created.");
                        }
                        sw.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: {message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar en el archivo de log: {ex.Message}");
            }
        }
    }
}
