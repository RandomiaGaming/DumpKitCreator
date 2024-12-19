using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RomHelper
{
    public static class Program
    {
        public const string ProgramFullName = "Dump Kit Creator - Version 1.0.0";
        public static bool InShell = true;
        public static string SwitchSDPath = GetDefaultSwitchSDPath();
        private static string GetDefaultSwitchSDPath()
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            foreach (DriveInfo drive in drives)
            {
                if (drive.IsReady)
                {
                    string nintendoFolder = Path.Combine(drive.RootDirectory.FullName, "Nintendo\\Contents\\registered");
                    if (Directory.Exists(nintendoFolder))
                    {
                        return drive.RootDirectory.FullName;
                    }
                }
            }
            return null;
        }
        public static void ConvertAllAudioFiles(string sourceFolderPath, string destinationFolderPath, string audioConverterPath, string commandTemplate)
        {
            List<string> files = new List<string>(Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories));

            for (int i = 0; i < files.Count; i++)
            {
                if (!files[i].ToLower().EndsWith(".bwav"))
                {
                    files.RemoveAt(i);
                    i--;
                }
            }

            List<Process> children = new List<Process>();

            for (int i = 0; i < files.Count; i++)
            {
                string source = files[i];

                string destination = files[i].Substring(sourceFolderPath.Length);
                destination = destinationFolderPath + destination;
                destination = destination.Replace(".bwav", ".wav");

                string folder = Path.GetDirectoryName(destination);
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                if (!File.Exists(destination))
                {
                    while (children.Count > 25)
                    {
                        for (int j = 0; j < children.Count; j++)
                        {
                            if (children[j].HasExited)
                            {
                                children.RemoveAt(j);
                                j--;
                            }
                        }
                    }

                    string command = commandTemplate.Replace("%source%", source).Replace("%destination%", destination);

                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = audioConverterPath;
                    processStartInfo.Arguments = command;
                    processStartInfo.UseShellExecute = true;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    processStartInfo.CreateNoWindow = true;

                    children.Add(Process.Start(processStartInfo));
                }

                Console.WriteLine($"{i} of {files.Count}");
            }
        }
        public sealed class ExtensionInfo
        {
            public string Extension => _extension;
            private string _extension = null;
            public int Count => _count;
            private int _count = 0;
            public ExtensionInfo(string extension)
            {
                _extension = extension;
                _count = 0;
            }
            public void Increment()
            {
                _count++;
            }
        }
        public static void FilesExts(string sourceFolderPath)
        {
            List<string> files = new List<string>(Directory.GetFiles(sourceFolderPath, "*", SearchOption.AllDirectories));
            List<ExtensionInfo> extensionInfo = new List<ExtensionInfo>();

            for (int i = 0; i < files.Count; i++)
            {
                string file = files[i];
                string ext = Path.GetExtension(file);

                bool foundExtEntry = false;

                for (int j = 0; j < extensionInfo.Count; j++)
                {
                    if (extensionInfo[j].Extension == ext)
                    {
                        extensionInfo[j].Increment();
                        foundExtEntry = true;
                        break;
                    }
                }

                if (!foundExtEntry)
                {
                    ExtensionInfo newExtensionInfo = new ExtensionInfo(ext);
                    newExtensionInfo.Increment();
                    extensionInfo.Add(newExtensionInfo);
                }
            }

            extensionInfo.Sort((ExtensionInfo a, ExtensionInfo b) => { return a.Count - b.Count; });

            for (int i = 0; i < extensionInfo.Count; i++)
            {
                Console.WriteLine($"{extensionInfo[i].Count} files with extension {extensionInfo[i].Extension}.");
            }
            Console.ReadLine();
        }
        public static void Main(string[] args)
        {
            FilesExts("C:\\Users\\RandomiaGaming\\AppData\\Roaming\\yuzu\\dump\\010015100B514000\\romfs");
            Console.ReadLine();
            return;
            try
            {
                Console.WriteLine(ProgramFullName);
                Console.WriteLine();

                if (!(args is null) && args.Length > 0)
                {
                    string command = "";
                    for (int i = 0; i < args.Length; i++)
                    {
                        command += args[i];
                        if (i != args.Length - 1)
                        {
                            command += " ";
                        }
                    }
                    InShell = false;
                    RunCommand(command);
                }

                while (InShell)
                {
                    Console.Write(">");
                    RunCommand(Console.ReadLine());
                }
            }
            catch (Exception ex)
            {
                WriteError($"Fatal error: {ex.Message}.");
            }
        }
        public static void RunCommand(string command)
        {
            try
            {
                string commandName = command.ToLower();
                string arguments = "";
                for (int i = 0; i < command.Length; i++)
                {
                    if (command[i] == ' ')
                    {
                        commandName = command.Substring(0, i).ToLower();
                        if (i != command.Length - 1)
                        {
                            arguments = command.Substring(i + 1);
                        }
                        break;
                    }
                }

                if (commandName is "exit")
                {
                    ExitCommand(arguments);
                }
                else if (commandName is "help")
                {
                    HelpCommand(arguments);
                }
                else if (commandName is "selectsd")
                {
                    SelectSDCommand(arguments);
                }
                else if (commandName is "getalbum")
                {
                    GetAlbumCommand(arguments);
                }
                else if (commandName is "wheresd")
                {
                    WhereSDCommand(arguments);
                }
                else
                {
                    throw new Exception($"Invalid command \"{commandName}\"");
                }
            }
            catch (Exception ex)
            {
                WriteError($"Error: {ex.Message}.");
            }
        }
        public static void WriteError(string error)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(error);
            Console.ForegroundColor = originalColor;
        }
        public static void WriteWarning(string warning)
        {
            ConsoleColor originalColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(warning);
            Console.ForegroundColor = originalColor;
        }

        public static void ExitCommand(string arguments)
        {
            if (arguments != "")
            {
                throw new Exception("Invalid arguments.");
            }
            InShell = false;
        }
        public static void HelpCommand(string arguments)
        {
            Console.WriteLine("No help for you. Get fukd.");
        }
        public static void SelectSDCommand(string arguments)
        {
            SwitchSDPath = arguments;
        }
        public static void GetAlbumCommand(string arguments)
        {
            string albumSourcePath = Path.Combine(SwitchSDPath, "Nintendo\\Album");
            string albumDestinationPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Nintedo Switch Album");

            if (!Directory.Exists(albumDestinationPath))
            {
                Directory.CreateDirectory(albumDestinationPath);
            }

            foreach (string file in Directory.GetFiles(albumSourcePath, "*", SearchOption.AllDirectories))
            {
                File.Copy(file, Path.Combine(albumDestinationPath, Path.GetFileName(file)));
            }
        }
        public static void WhereSDCommand(string arguments)
        {
            Console.WriteLine(SwitchSDPath);
        }
        public static void InstallDumpkitCommand(string arguments)
        {

        }
        public static void UpdateAll(string arguments)
        {

        }
        public static void InstallAll(string arguments)
        {

        }
        public static void AtmosphereCommand(string arguments)
        {

        }
        public static void HetakeCommand(string arguments)
        {

        }
    }
}
