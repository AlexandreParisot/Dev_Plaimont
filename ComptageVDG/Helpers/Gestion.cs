
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ComptageVDG.Helpers
{
    public static class Gestion
    {
        private static ILogger? Logger { get; set; } = null;
        public static string Version = string.Empty;
        public static string NameApp = string.Empty;
        public static string Current_Dir = System.AppDomain.CurrentDomain.BaseDirectory;

        public static void SetLoggerProvider(ILogger logger)
        {
            Logger = (logger == null)? Logger = new Log4NetProvider(Path.Combine(Current_Dir, "log4net.config")).CreateLogger():Logger = logger;
        }

        public static bool Info( string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            if(Logger == null)  
                return false;

            message = $"[{$"{System.IO.Path.GetFileNameWithoutExtension(fileName)}.{memberName}".PadRight(30)}] | {message.Trim().Replace("|", "/").Replace(Constants.vbCrLf, "Chr(13)")}";
            Logger.LogTrace(message);

            return true;
        }

        public static bool Obligatoire(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            if (Logger == null)
                return false;

            message = $"[{$"{System.IO.Path.GetFileNameWithoutExtension(fileName)}.{memberName}".PadRight(30)}] | {message.Trim().Replace("|", "/").Replace(Constants.vbCrLf, "Chr(13)")}";
            Logger.LogInformation(message);

            return true;
        }

        public static bool Erreur(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            if (Logger == null)
                return false;

            message = $"[{$"{System.IO.Path.GetFileNameWithoutExtension(fileName)}.{memberName}".PadRight(30)}] | {message.Trim().Replace("|", "/").Replace(Constants.vbCrLf, "Chr(13)")}";
            Logger.LogError(message);

            return true;
        }

        public static bool Warning(string message, [System.Runtime.CompilerServices.CallerMemberName] string memberName = "", [System.Runtime.CompilerServices.CallerFilePath] string fileName = "")
        {
            if (Logger == null)
                return false;

            message = $"[{$"{System.IO.Path.GetFileNameWithoutExtension(fileName)}.{memberName}".PadRight(30)}] | {message.Trim().Replace("|", "/").Replace(Constants.vbCrLf, "Chr(13)")}";
            Logger.LogWarning(message);

            return true;
        }

        public static void CreateService(string nameService)
        {
            Process p = new Process();


            var exePath = System.IO.Path.Combine(Gestion.Current_Dir, Assembly.GetEntryAssembly()!.GetName().Name + ".exe");
            string cmd = $"sc.exe  ";
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = $@"create {nameService} DisplayName= ""{nameService}"" start= auto binPath= ""\""{exePath}\"" ";
            p.Start();
            p.WaitForExit();

            if (p.HasExited && p.ExitCode == 0)
            {
                Console.WriteLine("Creation du service Ok!");
            }
            else
                Console.WriteLine($"Creation du service {nameService} Echec!{Constants.vbCrLf} Code erreur : {p.ExitCode}");
        }

        public static void RemoveService(string nameService)
        {
            Process p = new Process();


            var exePath = System.IO.Path.Combine(Gestion.Current_Dir, Assembly.GetEntryAssembly()!.GetName().Name + ".exe");
            string cmd = $"sc.exe  ";
            p.StartInfo.FileName = cmd;
            p.StartInfo.Arguments = $@"delete {nameService}";
            p.Start();
            p.WaitForExit();

            if (p.HasExited && p.ExitCode == 0)
            {
                Console.WriteLine("Suppression du service Ok!");
            }
            else
                Console.WriteLine($"Suppression du service {nameService} Echec!{Constants.vbCrLf} Code erreur : {p.ExitCode}");
        }
    }
}


