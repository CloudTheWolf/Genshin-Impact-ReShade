using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using IWshRuntimeLibrary;
using File = System.IO.File;

namespace Genshin_Stella_Mod.Scripts
{
    internal static class Utils
    {
        private static readonly string FileWithGamePath = Path.Combine(Program.AppData, "game-path.sfn");

        public static string GetGame(string type)
        {
            if (!File.Exists(FileWithGamePath))
            {
                MessageBox.Show($"File with game path was not found in:\n{FileWithGamePath}", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log.Output($"File with game path was not found in: {FileWithGamePath}");
                return null;
            }

            string gameFilePath = File.ReadAllLines(FileWithGamePath).First();
            string gamePath = Path.Combine(gameFilePath);
            if (!Directory.Exists(gamePath))
            {
                MessageBox.Show($"Folder does not exists.\n{gamePath}", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Log.Output($"Directory {gamePath} does not exists.");
                return null;
            }


            switch (type)
            {
                case "giDir":
                {
                    Log.Output($"Found main Genshin Impact dir: {gamePath} [giDir]");
                    return gamePath;
                }

                case "giGameDir":
                {
                    string genshinImpactGame = Path.Combine(gamePath, "Genshin Impact game");
                    if (!Directory.Exists(genshinImpactGame))
                    {
                        Log.Output($"Genshin Impact game was not found in: {genshinImpactGame} [giGameDir]");
                        return null;
                    }

                    Log.Output($"Found Genshin Impact Game dir: {genshinImpactGame} [giGameDir]");
                    return genshinImpactGame;
                }

                case "giExe":
                {
                    string genshinImpactExeMain = Path.Combine(gamePath, "Genshin Impact game", "GenshinImpact.exe");
                    if (File.Exists(genshinImpactExeMain))
                    {
                        Log.Output($"Found GenshinImpact.exe in: {genshinImpactExeMain} [giExe]");
                        return genshinImpactExeMain;
                    }

                    MessageBox.Show($"File does not exists.\n{genshinImpactExeMain}", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Log.Output($"File does not exists in: {genshinImpactExeMain} [giExe]");

                    string genshinImpactExeYuanShen = Path.Combine(gamePath, "Genshin Impact game", "YuanShen.exe");
                    if (File.Exists(genshinImpactExeYuanShen))
                    {
                        Log.Output($"Found GenshinImpact.exe in: {genshinImpactExeMain} [giExe]");
                        return genshinImpactExeYuanShen;
                    }

                    MessageBox.Show($"File {genshinImpactExeYuanShen} does not exists.", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Log.Output($"File {genshinImpactExeYuanShen} does not exists. [giExe]");

                    return null;
                }

                case "giLauncher":
                {
                    string genshinImpactExe = Path.Combine(gamePath, "launcher.exe");
                    if (!File.Exists(genshinImpactExe))
                    {
                        MessageBox.Show($"Launcher file does not exists.\n{genshinImpactExe}", Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Log.Output($"Launcher file does not exists in: {genshinImpactExe} [giLauncher]");
                        return null;
                    }

                    Log.Output($"Found Genshin Impact Launcher in: {genshinImpactExe} [giLauncher]");
                    return genshinImpactExe;
                }

                default:
                {
                    Log.ThrowError(new Exception("Wrong parameter."));
                    return null;
                }
            }
        }

        public static void OpenUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                Log.ThrowError(new Exception("URL is null or empty."));
                return;
            }

            try
            {
                Process.Start(url);
                Log.Output($"Opened '{url}' in default browser.");
            }
            catch (Exception ex)
            {
                Log.ThrowError(new Exception($"Failed to open '{url}' in default browser.\n{ex}"));
            }
        }

        public static void RemoveClickEvent(Label button)
        {
            FieldInfo eventClickField = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
            object eventHandler = eventClickField?.GetValue(button);
            if (eventHandler == null) return;

            PropertyInfo eventsProperty = button.GetType().GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList eventHandlerList = (EventHandlerList)eventsProperty?.GetValue(button, null);

            eventHandlerList?.RemoveHandler(eventHandler, eventHandlerList[eventHandler]);
        }

        public static bool CheckFileExists(string fileName)
        {
            string filePath = Path.Combine(fileName);
            bool fileExists = File.Exists(filePath);

            Log.Output(fileExists
                ? $"File '{fileName}' was found at '{filePath}'."
                : $"File '{fileName}' was not found at '{filePath}'.");

            return fileExists;
        }

        public static bool CreateShortcut()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonDesktopDirectory);
                string shortcutPath = Path.Combine(desktopPath, "Stella Mod Launcher.lnk");

                WshShell shell = new WshShell();
                IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutPath);
                shortcut.Description = "Run official launcher for Genshin Impact Mod made by Sefinek.";
                shortcut.IconLocation = Path.Combine(Environment.CurrentDirectory, "icons", "52x52.ico");
                shortcut.WorkingDirectory = Environment.CurrentDirectory;
                shortcut.TargetPath = Path.Combine(Environment.CurrentDirectory, $"{Program.AppName}.exe");
                shortcut.Save();

                Log.Output("Desktop shortcut has been created.");
                return true;
            }
            catch (Exception e)
            {
                Log.ThrowError(new Exception($"An error occurred while creating the shortcut.\n\n{e}"));
                return false;
            }
        }
    }
}