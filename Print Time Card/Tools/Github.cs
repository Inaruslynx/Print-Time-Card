using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json.Linq;

namespace Print_Time_Card.Tools
{
    internal class Github
    {
        private static readonly string currentVersion = "1.6.0.0";
        private static readonly string repoOwner = "Inaruslynx";
        private static readonly string repoName = "Print-Time-Card";
        private static readonly string githubToken = ""; // Secret 

        public static async Task CheckVersion()
        {
            var (latestVersion, releaseUrl) = await GetLatestVersionFromGitHub();
            if (IsNewVersionAvailable(currentVersion,latestVersion))
            {
                ShowUpdateNotification(latestVersion, releaseUrl);
            }
        }

        private static string GetCurrentVersion()
        {
            return Application.ProductVersion;
        }

        private static async Task<(string, string)> GetLatestVersionFromGitHub()
        {
            using (var httpClient = new HttpClient())
            {
                if (!string.IsNullOrEmpty(githubToken))
                {
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Token", githubToken);
                }
                else
                {
                    throw new Exception("Error while trying to check if updates to App are available.");
                }
                var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";
                httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("AppName", "1.0"));
                var response = await httpClient.GetStringAsync(url);
                var json = JObject.Parse(response);

                var latestVersion = json["tag_name"].ToString().TrimStart('v');
                var releaseUrl = json["html_url"].ToString();
                return (latestVersion, releaseUrl);
            }
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion)) return false;
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);

            return latest.CompareTo(current) > 0;
        }

        private static void ShowUpdateNotification(string latestVersion, string releaseUrl)
        {
            var message = $"A new version ({latestVersion}) is available. Click OK to open the release page.";
            var result = MessageBox.Show(message, "Update Available", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            if (result == DialogResult.OK)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = releaseUrl,
                    UseShellExecute = true
                });
            }
        }
    }
}
