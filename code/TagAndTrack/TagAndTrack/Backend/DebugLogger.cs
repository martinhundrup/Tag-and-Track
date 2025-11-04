using System;
using System.IO;

namespace TagAndTrack.Backend
{
    internal static class DebugLogger
    {
        private static readonly string logFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "TagAndTrack_DebugLog.txt");


        public static void Init()
        {
            try
            {
                // Clear the existing log file (or create if missing)
                File.WriteAllText(logFilePath, $"=== Log initialized at {DateTime.Now} ==={Environment.NewLine}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DebugLogger.Init] Failed to clear log: {ex.Message}");
            }
        }

        public static void Log(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
                File.AppendAllText(logFilePath, logEntry);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DebugLogger.Log] Failed to write log: {ex.Message}");
            }
        }

        public static string GetLogFilePath()
        {
            return logFilePath;
        }
    }
}
