using System;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using ByteSizeLib;
using Microsoft.WindowsAPICodePack.Taskbar;
using StellaLauncher.Forms;
using StellaLauncher.Properties;

namespace StellaLauncher.Scripts.Download
{
    internal static class ReShadeIni
    {
        public static async Task<int> CheckForUpdates()
        {
            string gamePath = await Utils.GetGame("giGameDir");
            if (!Directory.Exists(gamePath))
            {
                Default.UpdateIsAvailable = false;

                Default._status_Label.Text += $"[x] {Resources.ReShadeIniUpdate_GamePathWasNotFoundOnYourPC}\n";

                Log.SaveErrorLog(new Exception(Resources.ReShadeIniUpdate_GamePathWasNotFoundOnYourPC));
                return -1;
            }

            string reShadePath = Path.Combine(gamePath, "ReShade.ini");
            if (!File.Exists(reShadePath))
            {
                Default.UpdateIsAvailable = false;

                Default._updates_LinkLabel.LinkColor = Color.OrangeRed;
                Default._updates_LinkLabel.Text = Resources.ReShadeIniUpdate_DownloadTheRequiredFile;
                Default._status_Label.Text += $"[x] {Resources.ReShadeIniUpdate_FileReShadeIniWasNotFoundInYourGameDir}\n";
                Default._updateIco_PictureBox.Image = Resources.icons8_download_from_the_cloud;

                Log.Output(string.Format(Resources.ReShadeIniUpdate_ReShadeIniWasNotFoundIn_, reShadePath));
                return -2;
            }

            WebClient webClient = new WebClient();
            webClient.Headers.Add("user-agent", Program.UserAgent);
            string content = await webClient.DownloadStringTaskAsync("https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.ini");
            NameValueCollection iniData = new NameValueCollection();
            using (StringReader reader = new StringReader(content))
            {
                string line;
                while ((line = await reader.ReadLineAsync()) != null)
                    if (line.Contains("="))
                    {
                        int separatorIndex = line.IndexOf("=", StringComparison.Ordinal);
                        string key = line.Substring(0, separatorIndex).Trim();
                        string value = line.Substring(separatorIndex + 1).Trim();
                        iniData.Add(key, value);
                    }
            }

            IniFile reShadeIni = new IniFile(reShadePath);
            string localIniVersion = reShadeIni.ReadString("STELLA", "ConfigVersion", null);
            if (string.IsNullOrEmpty(localIniVersion))
            {
                Default.UpdateIsAvailable = false;

                Default._updates_LinkLabel.LinkColor = Color.Cyan;
                Default._updates_LinkLabel.Text = Resources.ReShadeIniUpdate_DownloadTheRequiredFile;
                Default._status_Label.Text += $"[x] {Resources.ReShadeIniUpdate_TheVersionOfReShadeCfgWasNotFound}\n";
                Default._updateIco_PictureBox.Image = Resources.icons8_download_from_the_cloud;

                Log.Output(string.Format(Resources.ReShadeIniUpdate_StellaConfigVersionIsNullInReShadeIni));
                TaskbarManager.Instance.SetProgressValue(100, 100);
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Error);
                return -2;
            }

            string remoteIniVersion = iniData["ConfigVersion"];
            if (localIniVersion == remoteIniVersion) return 0;

            Default.UpdateIsAvailable = true;

            Default._updates_LinkLabel.LinkColor = Color.DodgerBlue;
            Default._updates_LinkLabel.Text = Resources.ReShadeIniUpdate_UpdateReShadeCfg;
            Default._updateIco_PictureBox.Image = Resources.icons8_download_from_the_cloud;
            Default._version_LinkLabel.Text = $@"v{localIniVersion} → v{remoteIniVersion}";
            Default._status_Label.Text += $"[i] {Resources.ReShadeIniUpdate_NewReShadeConfigVersionIsAvailable}\n";
            Log.Output(string.Format(Resources.ReShadeIniUpdate_NewReShadeCfgIsAvailable_v_, localIniVersion, remoteIniVersion));

            using (WebClient wc = new WebClient())
            {
                wc.Headers.Add("user-agent", Program.UserAgent);
                await wc.OpenReadTaskAsync("https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.ini");
                string updateSize = ByteSize.FromBytes(Convert.ToInt64(wc.ResponseHeaders["Content-Length"])).KiloBytes.ToString("0.00");
                Default._status_Label.Text += $"{string.Format(Resources.ReShadeIniUpdate_UpdateSize_KB, updateSize)}\n";

                Log.Output(string.Format(Resources.ReShadeIniUpdate_FileSize_KB, updateSize));
                TaskbarManager.Instance.SetProgressValue(100, 100);
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Paused);
                return 1;
            }
        }

        public static async Task<int> Download(int resultInt, string resourcesPath, DialogResult msgBoxResult)
        {
            string gameDir = await Utils.GetGame("giGameDir");
            string reShadePath = Path.Combine(gameDir, "ReShade.ini");

            switch (msgBoxResult)
            {
                case DialogResult.Yes:
                    try
                    {
                        Default._updates_LinkLabel.LinkColor = Color.DodgerBlue;
                        Default._updates_LinkLabel.Text = Resources.Default_Downloading;
                        TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);

                        WebClient client2 = new WebClient();
                        client2.Headers.Add("user-agent", Program.UserAgent);
                        await client2.DownloadFileTaskAsync("https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.ini", reShadePath);

                        if (File.Exists(reShadePath))
                        {
                            string reShadeIniContent = File.ReadAllText(reShadePath);
                            string newData = reShadeIniContent?
                                .Replace("{addon.path}", Path.Combine(resourcesPath, "ReShade", "Addons"))
                                .Replace("{general.effects}", Path.Combine(resourcesPath, "ReShade", "Shaders", "Effects"))
                                .Replace("{general.cache}", Path.Combine(resourcesPath, "ReShade", "Cache"))
                                .Replace("{general.preset}", Path.Combine(resourcesPath, "ReShade", "Presets", "3. Preset by Sefinek - Medium settings [Default].ini"))
                                .Replace("{general.textures}", Path.Combine(resourcesPath, "ReShade", "Shaders", "Textures"))
                                .Replace("{screenshot.path}", Path.Combine(resourcesPath, "Screenshots"))
                                .Replace("{screenshot.sound}", Path.Combine(Program.AppPath, "data", "sounds", "screenshot.wav"));

                            File.WriteAllText(reShadePath, newData);

                            Default._status_Label.Text += $"[✓] {Resources.Default_SuccessfullyDownloadedReShadeIni}\n";
                            Log.Output(string.Format(Resources.Default_SuccessfullyDownloadedReShadeIniAndSavedIn, reShadePath));

                            await Default.CheckForUpdates();
                            return 0;
                        }

                        Default._status_Label.Text += $"[x] {Resources.Default_FileWasNotFound}\n";
                        Log.SaveErrorLog(new Exception(string.Format(Resources.Default_DownloadedReShadeIniWasNotFoundIn_, reShadePath)));

                        Utils.HideProgressBar(true);
                    }
                    catch (Exception ex)
                    {
                        Default._status_Label.Text += $"[x] {Resources.Default_Meeow_FailedToDownloadReShadeIni_TryAgain}\n";
                        Default._updates_LinkLabel.LinkColor = Color.Red;
                        Default._updates_LinkLabel.Text = Resources.Default_FailedToDownload;

                        Log.SaveErrorLog(ex);
                        if (!File.Exists(reShadePath)) Log.Output(Resources.Default_TheReShadeIniFileStillDoesNotExist);

                        Utils.HideProgressBar(true);
                    }

                    break;
                case DialogResult.No:
                {
                    Default._status_Label.Text += $"[i] {Resources.Default_CanceledByTheUser_AreYouSureOfWhatYoureDoing}\n";
                    Log.Output(Resources.Default_FileDownloadHasBeenCanceledByTheUser);

                    if (!File.Exists(reShadePath)) Log.Output(Resources.Default_TheReShadeIniFileStillDoesNotExist);

                    Utils.HideProgressBar(true);
                    break;
                }
            }

            return resultInt;
        }
    }
}