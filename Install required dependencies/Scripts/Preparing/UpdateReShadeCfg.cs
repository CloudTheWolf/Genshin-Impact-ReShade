using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace PrepareStella.Scripts.Preparing
{
    /// <summary>
    ///     Download the ReShade configuration file (ReShade.ini).
    /// </summary>
    internal static class UpdateReShadeCfg
    {
        public static async Task Run()
        {
            Console.WriteLine(@"Updating ReShade config...");

            if (Directory.Exists(Program.GameDirGlobal))
            {
                File.Delete(Program.ReShadeConfig);
                File.Delete(Program.ReShadeLogFile);

                WebClient wbClient1 = new WebClient();
                wbClient1.Headers.Add("user-agent", Program.UserAgent);
                await wbClient1.DownloadFileTaskAsync("https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.ini", Program.ReShadeConfig);

                WebClient wbClient2 = new WebClient();
                wbClient2.Headers.Add("user-agent", Program.UserAgent);
                await wbClient2.DownloadFileTaskAsync("https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.log", Program.ReShadeLogFile);

                if (File.Exists(Program.ReShadeConfig) && File.Exists(Program.ReShadeLogFile))
                    Log.Output(
                        "ReShade.ini and ReShade.log was successfully downloaded.\n" +
                        "» Source 1: https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.ini\n" +
                        "» Source 2: https://cdn.sefinek.net/resources/v3/genshin-stella-mod/reshade/ReShade.log"
                    );
                else
                    Log.ThrowError(new Exception($"Something went wrong. Config or log file for ReShade was not found in: {Program.ReShadeConfig}"), true);
            }
            else
            {
                Console.WriteLine(@"You must configure some settings manually.");
            }
        }
    }
}