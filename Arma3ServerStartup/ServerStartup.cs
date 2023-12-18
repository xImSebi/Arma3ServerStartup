using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arma3ServerStartup
{
    public class ServerStartup
    {
        private IConfiguration Configuration;

        private readonly string nameKey = "name";
        private readonly string serverPathKey = "serverPath";
        private readonly string serverConfigPathKey = "serverConfigPath";
        private readonly string battleyePathKey = "battleyePath";
        private readonly string profilesPathKey = "profilesPath";
        private readonly string modsPathKey = "modsPath";
        private readonly string modListPathKey = "modListPath";
        private readonly string allowedModListPathKey = "allowedModListPath";
        private readonly string additionalStartupParamsKey = "additionalStartupParameters";

        public ServerStartup()
        {
            try
            {
                Configuration = new ConfigurationBuilder()
                    .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                    .Build();
            }
            catch (Exception ex)
            {
                if (ex.InnerException.Message == "Could not parse the JSON file.")
                    File.Delete("config.json");
            }
        }

        public void Start()
        {
            CheckNeededFiles();

            var name = Configuration[nameKey];
            var serverPath = Configuration[serverPathKey];
            var serverConfigPath = Configuration[serverConfigPathKey];
            var battleyePath = Configuration[battleyePathKey];
            var profilesPath = Configuration[profilesPathKey];
            var modsPath = Configuration[modsPathKey];
            var modListPath = Configuration[modListPathKey];
            var allowedModListPath = Configuration[allowedModListPathKey];
            var additionalStartupParams = Configuration[additionalStartupParamsKey];

            Log.Information("Cleaning keys directory");
            Log.Information("Moving a3.bikey from keys directory to main directory");
            File.Move(serverPath + @"\keys\a3.bikey", serverPath + @"\a3.bikey");
            Log.Information("Deleting files in keys directory");
            foreach (var fileName in Directory.GetFiles(serverPath + @"\keys"))
            {
                File.Delete(fileName);
            }
            Log.Information("Moving a3.bikey from main directory to keys directory");
            File.Move(serverPath + @"\a3.bikey", serverPath + @"\keys\a3.bikey");

            var (useMods, modList) = LoadMods(serverPath, modsPath, modListPath);

            LoadAllowedMods(serverPath, modsPath, allowedModListPath);

            var startupCommand = @$"start ""{name}"" /wait ""{serverPath + @"\"}arma3server_x64.exe"" ""-config={serverConfigPath}"" ""-bePath={battleyePath}"" ""-profiles={profilesPath}""";
            if (useMods)
            {
                startupCommand += @" ""-mod=";
                foreach (var mod in modList)
                {
                    startupCommand += $"{modsPath}\\{mod};";
                }
                var lastSemicolonIndex = startupCommand.LastIndexOf(';');
                startupCommand.Remove(lastSemicolonIndex);
                startupCommand += @"""";
            }

            if (!string.IsNullOrWhiteSpace(additionalStartupParams))
            {
                startupCommand += " " + additionalStartupParams;
            }

            Log.Information("Starting " + name);
            Process cmd = new Process();
            cmd.StartInfo.FileName = "cmd.exe";
            cmd.StartInfo.RedirectStandardInput = true;
            cmd.StartInfo.RedirectStandardOutput = false;
            cmd.StartInfo.CreateNoWindow = true;
            cmd.StartInfo.UseShellExecute = false;
            cmd.Start();

            cmd.StandardInput.WriteLine(startupCommand);
            cmd.StandardInput.Flush();
            cmd.StandardInput.Close();
            cmd.Close();
        }

        private (bool, string[]) LoadMods(string serverPath, string modsPath, string modListPath)
        {
            string[] modList;
            Log.Information("Reading Mod List...");
            if (File.Exists(modListPath))
            {
                modList = File.ReadAllLines(modListPath);
                Log.Information("Mod List read successfully. Loaded Mods: {ModCount}", modList.Length);
            }
            else
            {
                Log.Warning("Mod List could not be found... Missing file: {ModListPath}", modListPath);
                Log.Warning("Proceeding without mods...");
                return (false, null);
            }

            Log.Information("Copying BI-Keys from mods to keys directory");
            foreach (var mod in modList)
            {
                if (Directory.Exists(modsPath + @"\" + mod + @"\keys"))
                {
                    foreach (var keysFilePath in Directory.GetFileSystemEntries(modsPath + @"\" + mod + @"\keys"))
                    {
                        var keysFile = keysFilePath.Split("\\");
                        try
                        {
                            if (keysFile.LastOrDefault().EndsWith(".bikey"))
                                File.Copy(modsPath + @"\" + mod + @"\keys\" + keysFile.LastOrDefault(), serverPath + @"\keys\" + keysFile.LastOrDefault());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }
                }
            }
            Log.Information("Successfully copied key files from mods to keys directory");
            return (true, modList);
        }

        private bool LoadAllowedMods(string serverPath, string modsPath, string allowedModListPath)
        {
            string[] modList;
            Log.Information("Reading Allowed Mod List...");
            if (File.Exists(allowedModListPath))
            {
                modList = File.ReadAllLines(allowedModListPath);
                Log.Information("Allowed Mod List read successfully. Loaded Additional Allowed Mods: {ModCount}", modList.Length);
            }
            else
            {
                Log.Warning("Mod List could not be found... Missing file: {ModListPath}", allowedModListPath);
                Log.Warning("Proceeding without additional allowed mods...");
                return false;
            }

            Log.Information("Copying BI-Keys from mods to keys directory");
            foreach (var mod in modList)
            {
                if (Directory.Exists(modsPath + @"\" + mod + @"\keys"))
                {
                    foreach (var keysFilePath in Directory.GetFileSystemEntries(modsPath + @"\" + mod + @"\keys"))
                    {
                        var keysFile = keysFilePath.Split("\\");
                        try
                        {
                            if (keysFile.LastOrDefault().EndsWith(".bikey"))
                                File.Copy(modsPath + @"\" + mod + @"\keys\" + keysFile.LastOrDefault(), serverPath + @"\keys\" + keysFile.LastOrDefault());
                        }
                        catch (Exception ex)
                        {
                            Log.Error(ex.Message);
                        }
                    }
                }
            }
            Log.Information("Successfully copied key files from mods to keys directory");
            return true;
        }

        private void CheckNeededFiles()
        {
            Log.Information("Checking essential files...");
            if (!ConfigFileExist())
                CreateConfigFile();

            ConfigEntriesExist();
        }

        private bool ConfigFileExist()
        {
            Log.Information("Checking for config.json");
            if (!File.Exists("config.json"))
            {
                Log.Warning("config.json could not be found");
                return false;
            }
            Log.Information("config.json found");
            return true;
        }

        private void CreateConfigFile()
        {
            Log.Information("Creating default config.json");
            File.WriteAllText("config.json", $@"{"{"}
  ""{nameKey}"": ""Arma 3 Server"",
  ""{serverPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server"",
  ""{serverConfigPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\server.cfg"",
  ""{battleyePathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\battleye"",
  ""{profilesPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\profiles"",
  ""{modsPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3\\!Workshop"",
  ""{modListPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\modlist.txt"",
  ""{allowedModListPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\allowedmodlist.txt"",
  ""{additionalStartupParamsKey}"": ""-netlog -autoinit""
{"}"}");

            Configuration = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true, reloadOnChange: true)
                .Build();

            if (!ConfigFileExist())
            {
                Log.Error("Error while creating config.json");
                return;
            }
        }

        private bool ConfigEntriesExist()
        {
            Log.Information("Checking for config.json entries... ");

            if (Configuration[nameKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", nameKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{nameKey}"": ""Arma 3 Server""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[serverPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", serverPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{serverPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[serverConfigPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", serverConfigPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{serverConfigPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\server.cfg""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[battleyePathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", battleyePathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{battleyePathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\battleye""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[profilesPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", profilesPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{profilesPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\profiles""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[modsPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", modsPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{modsPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3\\!Workshop""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[modListPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", modListPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{modListPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\modlist.txt""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[allowedModListPathKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", allowedModListPathKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{allowedModListPathKey}"": ""D:\\SteamLibrary\\steamapps\\common\\Arma 3 Server\\allowedmodlist.txt""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            if (Configuration[additionalStartupParamsKey] == null)
            {
                Log.Warning("{Key} value could not be found, creating...", additionalStartupParamsKey);
                var fileContent = File.ReadAllText("config.json");
                int lastBraceIndex = fileContent.LastIndexOf('}');
                if (lastBraceIndex > 0)
                {
                    fileContent = fileContent.Insert(lastBraceIndex - 2, @$",
  ""{additionalStartupParamsKey}"": ""-netlog -autoinit""");
                }
                File.WriteAllText("config.json", fileContent);
            }

            Log.Information("    Success");

            return true;
        }
    }
}
