using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DumpKitCreator
{
    public static class GithubHelper
    {
        private static readonly WebClient reusableClient = CreateClient();
        private static Stopwatch downloadTimer = Stopwatch.StartNew();
        private static Random RNG = new Random();
        private static long nextValidDownloadTime = 0;
        private static WebClient CreateClient()
        {
            WebClient output = new WebClient();

            //Set a more reasonable user agent.
            output.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.1234.56 Safari/537.36");

            //Accept some file types
            //output.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");

            //Use en-us
            //output.Headers.Add("Accept-Language", "en-US,en;q=0.8");

            //Set our referrer to a reddit post
            output.Headers.Add("Referer", "https://www.google.com/search?q=Lockpick+rcm&sca_esv=575056345&sxsrf=AM9HkKmNIcXz6B9LvzotIQ1biQ1mhOE0YA%3A1697767548697&source=hp&ei=fOAxZefiH5yr0PEPr4eesAs&iflsig=AO6bgOgAAAAAZTHujGrWSSvNkNTdb52oIZiapAobzKIB&ved=0ahUKEwjnkJD9xIOCAxWcFTQIHa-DB7YQ4dUDCAw&uact=5&oq=Lockpick+rcm&gs_lp=Egdnd3Mtd2l6IgxMb2NrcGljayByY20yBRAAGIAEMgoQABiABBgUGIcCMgUQABiABDIKEAAYgAQYFBiHAjIFEAAYgAQyBRAAGIAEMgoQABiABBixAxgKMgUQABiABDIFEAAYgAQyBRAAGIAESK0bUP0FWL0ZcAF4AJABAJgBS6AB-wWqAQIxMrgBA8gBAPgBAagCCsICBxAjGOoCGCfCAgcQIxiKBRgnwgIEECMYJ8ICBxAAGIoFGEPCAg4QLhiABBixAxjHARjRA8ICERAuGIAEGLEDGIMBGMcBGNEDwgINEAAYgAQYFBiHAhixA8ICDhAAGIAEGLEDGIMBGMkDwgIIEAAYigUYkgPCAggQABiABBixA8ICCxAAGIAEGLEDGMkDwgIIEAAYgAQYkgPCAgsQABiABBixAxiDAcICDRAAGIoFGLEDGIMBGEPCAgoQABiKBRixAxhDwgILEC4YgAQYsQMYgwHCAgsQLhiDARixAxiABMICCxAAGIoFGLEDGIMBwgIIEAAYigUYsQPCAgkQABiKBRgKGEPCAgUQLhiABA&sclient=gws-wiz");

            return output;
        }
        private static void PreDownload()
        {
            //Wait until its okay to download something by sleeping for 100 miliseconds.
            while (downloadTimer.ElapsedTicks < nextValidDownloadTime)
            {
                Thread.Sleep(100);
            }
        }
        private static void PostDownload()
        {
            //Random delay between 0.5 seconds and 3 seconds.
            nextValidDownloadTime = downloadTimer.ElapsedTicks + RNG.Next(5000000, 30000000);
        }
        public static ReleaseMeta GetLatestReleaseMeta(string author, string repo)
        {
            string endpointURL = $"https://api.github.com/repos/{author}/{repo}/releases/latest";
            PreDownload();
            string json = reusableClient.DownloadString(endpointURL);
            PostDownload();
            return JsonConvert.DeserializeObject<ReleaseMeta>(json);
        }
        public static void DownloadFileFromRelease(ReleaseMeta releaseMeta, string targetFileName, string destinationFilePath)
        {
            string sourceFileURL = null;
            foreach (ReleaseMeta.ReleaseMetaAsset asset in releaseMeta.assets)
            {
                if (asset.name == targetFileName)
                {
                    sourceFileURL = asset.browser_download_url;
                    break;
                }
            }
            if (sourceFileURL == null)
            {
                throw new Exception($"Github release did not contain a file named \"{targetFileName}\"");
            }
            PreDownload();
            reusableClient.DownloadFile(sourceFileURL, destinationFilePath);
            PostDownload();
        }


        public class ReleaseMeta
        {
            public string url;
            public string assets_url;
            public string upload_url;
            public string html_url;
            public int id;
            public ReleaseMetaAuthor author;
            public string node_id;
            public string tag_name;
            public string target_commitish;
            public string name;
            public bool draft;
            public bool prerelease;
            public DateTime created_at;
            public DateTime published_at;
            public List<ReleaseMetaAsset> assets;
            public string tarball_url;
            public string zipball_url;
            public string body;
            public class ReleaseMetaAsset
            {
                public string url;
                public int id;
                public string node_id;
                public string name;
                public object label;
                public ReleaseMetaUploader uploader;
                public string content_type;
                public string state;
                public int size;
                public int download_count;
                public DateTime created_at;
                public DateTime updated_at;
                public string browser_download_url;
                public class ReleaseMetaUploader
                {
                    public string login;
                    public int id;
                    public string node_id;
                    public string avatar_url;
                    public string gravatar_id;
                    public string url;
                    public string html_url;
                    public string followers_url;
                    public string following_url;
                    public string gists_url;
                    public string starred_url;
                    public string subscriptions_url;
                    public string organizations_url;
                    public string repos_url;
                    public string events_url;
                    public string received_events_url;
                    public string type;
                    public bool site_admin;
                }
            }
            public class ReleaseMetaAuthor
            {
                public string login;
                public int id;
                public string node_id;
                public string avatar_url;
                public string gravatar_id;
                public string url;
                public string html_url;
                public string followers_url;
                public string following_url;
                public string gists_url;
                public string starred_url;
                public string subscriptions_url;
                public string organizations_url;
                public string repos_url;
                public string events_url;
                public string received_events_url;
                public string type;
                public bool site_admin;
            }
        }
    }
}
