using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Taskbar;
using Newtonsoft.Json;
using StellaLauncher.Forms;
using StellaLauncher.Models;
using StellaLauncher.Properties;
using StellaLauncher.Scripts.Download;
using StellaLauncher.Scripts.Patrons;

namespace StellaLauncher.Scripts.Forms.MainForm
{
    internal static class CheckForUpdates
    {
        public static async Task<int> Analyze()
        {
            Default._updates_LinkLabel.LinkColor = Color.White;
            Default._updates_LinkLabel.Text = Resources.Default_CheckingForUpdates;

            TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.Indeterminate);
            Log.Output("Checking for new updates...");

            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", Program.UserAgent);
                string json = await client.DownloadStringTaskAsync("https://api.sefinek.net/api/v4/genshin-stella-mod/version/app/launcher");
                StellaApiVersion res = JsonConvert.DeserializeObject<StellaApiVersion>(json);

                string remoteVersion = res.Launcher.Version;
                DateTime remoteVerDate = DateTime.Parse(res.Launcher.ReleaseDate, null, DateTimeStyles.RoundtripKind).ToUniversalTime().ToLocalTime();


                Default._progressBar1.Value = 62;
                // == Major release ==
                if (Program.AppVersion[0] != remoteVersion[0])
                {
                    Default.UpdateIsAvailable = true;

                    MajorRelease.Run(remoteVersion, remoteVerDate);
                    return 1;
                }


                Default._progressBar1.Value = 68;
                // == Normal release ==
                if (Program.AppVersion != remoteVersion)
                {
                    Default.UpdateIsAvailable = true;

                    NormalRelease.Run(remoteVersion, remoteVerDate);
                    return 1;
                }


                Default._progressBar1.Value = 74;
                // == Check new updates of resources ==
                if (!Secret.IsMyPatron)
                {
                    string jsonFile = Path.Combine(Default.ResourcesPath, "data.json");
                    if (!File.Exists(jsonFile))
                    {
                        Default._status_Label.Text += $"{string.Format(Resources.Default_File_WasNotFound, jsonFile)}\n";
                        Log.SaveError($"File {jsonFile} was not found.");

                        Utils.HideProgressBar(true);
                        return -1;
                    }

                    string jsonContent = File.ReadAllText(jsonFile);
                    LocalResources data = JsonConvert.DeserializeObject<LocalResources>(jsonContent);

                    WebClient resClient = new WebClient();
                    resClient.Headers.Add("user-agent", Program.UserAgent);
                    string resJson = await resClient.DownloadStringTaskAsync("https://api.sefinek.net/api/v4/genshin-stella-mod/version/app/launcher/resources");
                    StellaResources resourcesRes = JsonConvert.DeserializeObject<StellaResources>(resJson);

                    if (data.Version != resourcesRes.Message)
                    {
                        Default.UpdateIsAvailable = true;

                        DownloadResources.Run(data.Version, resourcesRes.Message, resourcesRes.Date);
                        return 1;
                    }
                }


                Default._progressBar1.Value = 79;
                // == Check new updates for ReShade.ini file ==
                int resultInt = await ReShadeIni.CheckForUpdates();
                switch (resultInt)
                {
                    case -2:
                    {
                        DialogResult msgBoxResult = MessageBox.Show(Resources.Default_TheReShadeIniFileCouldNotBeLocatedInYourGameFiles, Program.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        int number = await ReShadeIni.Download(resultInt, Default.ResourcesPath, msgBoxResult);
                        return number;
                    }

                    case 1:
                    {
                        DialogResult msgReply = MessageBox.Show(Resources.Default_AreYouSureWantToUpdateReShadeConfiguration, Program.AppName, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                        if (msgReply == DialogResult.No || msgReply == DialogResult.Cancel)
                        {
                            Log.Output("The update of ReShade.ini has been cancelled by the user.");
                            MessageBox.Show(Resources.Default_ForSomeReasonYouDidNotGiveConsentForTheAutomaticUpdateOfTheReShadeFile, Program.AppName, MessageBoxButtons.OK, MessageBoxIcon.Stop);

                            Utils.HideProgressBar(true);
                            return 1;
                        }

                        int number = await ReShadeIni.Download(resultInt, Default.ResourcesPath, DialogResult.Yes);
                        return number;
                    }
                }


                // == For patrons ==
                if (Secret.IsMyPatron)
                {
                    int found = await CheckForUpdatesOfBenefits.Analyze();
                    if (found == 1)
                    {
                        Labels.HideStartGameBtns();

                        Default._updates_LinkLabel.LinkColor = Color.Cyan;
                        Default._updates_LinkLabel.Text = Resources.Default_UpdatingBenefits;
                        Default._updateIco_PictureBox.Image = Resources.icons8_download_from_the_cloud;
                        Utils.RemoveClickEvent(Default._updates_LinkLabel);
                        return found;
                    }
                }


                // == Not found any new updates ==
                Default._updates_LinkLabel.Text = Resources.Default_CheckForUpdates;
                Default._updateIco_PictureBox.Image = Resources.icons8_available_updates;

                Default._version_LinkLabel.Text = $@"v{Program.AppVersion}";

                Utils.RemoveClickEvent(Default._updates_LinkLabel);
                Default._updates_LinkLabel.Click += CheckUpdates_Click;

                Default.UpdateIsAvailable = false;
                Log.Output($"Not found any new updates. Your installed version: v{Program.AppVersion}");

                Default._progressBar1.Value = 84;
                TaskbarManager.Instance.SetProgressState(TaskbarProgressBarState.NoProgress);
                return 0;
            }
            catch (Exception e)
            {
                Default.UpdateIsAvailable = false;

                Default._updates_LinkLabel.LinkColor = Color.Red;
                Default._updates_LinkLabel.Text = Resources.Default_OhhSomethingWentWrong;
                Default._status_Label.Text += $"[x] {e.Message}\n";

                Log.SaveError(string.Format(Resources.Default_SomethingWentWrongWhileCheckingForNewUpdates, e));
                Utils.HideProgressBar(true);
                return -1;
            }
        }

        public static async void CheckUpdates_Click(object sender, EventArgs e)
        {
            Music.PlaySound("winxp", "hardware_insert");
            int update = await Analyze();

            if (update == -1)
            {
                Music.PlaySound("winxp", "hardware_fail");
                return;
            }

            if (update != 0) return;

            Music.PlaySound("winxp", "hardware_remove");

            Default._updates_LinkLabel.LinkColor = Color.LawnGreen;
            Default._updates_LinkLabel.Text = Resources.Default_YouHaveTheLatestVersion;
            Default._updateIco_PictureBox.Image = Resources.icons8_available_updates;
        }
    }
}
