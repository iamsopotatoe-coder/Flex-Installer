using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;
using System.Text;

namespace FlexInstaller {
public class FileTransferManager
{
private ProgressBar barProgress;
private Label txtStatus;
private WebClient client;
private bool finished;
private bool worked;

public FileTransferManager(ProgressBar progress,Label status) {
barProgress=progress;
txtStatus=status;
}

public async Task<bool> RetrieveFileFromCloud(string webUrl,string localFile)
{
try {
if(string.IsNullOrEmpty(webUrl)||!Uri.IsWellFormedUriString(webUrl,UriKind.Absolute)) {
txtStatus.Text="Invalid download URL";
return false;
}

string processedUrl = ProcessDownloadVariation(webUrl);
if(processedUrl.Contains("api.github.com")) {
return await HandleGitHubApiDownload(processedUrl, localFile);
}

txtStatus.Text="Connecting to download server...";
Application.DoEvents();

ServicePointManager.SecurityProtocol=(SecurityProtocolType)3072|(SecurityProtocolType)768|(SecurityProtocolType)192;
ServicePointManager.ServerCertificateValidationCallback=delegate{return true;};

client=new WebClient();
client.Headers.Add("User-Agent",AppConfig.appName+" Installer v"+AppConfig.appVer);

finished=false;
worked=false;

client.DownloadProgressChanged+=(sender,e)=> {
barProgress.Value=e.ProgressPercentage;
txtStatus.Text=string.Format("Downloading... {0}% ({1:F1} MB of {2:F1} MB)",
e.ProgressPercentage,
e.BytesReceived/1024.0/1024.0,
e.TotalBytesToReceive/1024.0/1024.0);
Application.DoEvents();
};

client.DownloadFileCompleted+=(sender,e)=> {
finished=true;
if(e.Error!=null) {
txtStatus.Text=string.Format("Download failed: {0}",e.Error.Message);
worked=false;
} else if(e.Cancelled) {
txtStatus.Text="Download was cancelled";
worked=false;
} else {
txtStatus.Text="Download completed successfully";
worked=true;
}
};

client.DownloadFileAsync(new Uri(processedUrl),localFile);

while(!finished) {
await Task.Delay(100);
Application.DoEvents();
}

client.Dispose();
return worked;
} catch(Exception ex) {
txtStatus.Text=string.Format("Download error: {0}",ex.Message);
if(client!=null) {
client.Dispose();
}
return false;
}
}

private async Task<bool> HandleGitHubApiDownload(string apiUrl, string localFile)
{
try {
txtStatus.Text="Getting GitHub release info...";
Application.DoEvents();

ServicePointManager.SecurityProtocol=(SecurityProtocolType)3072|(SecurityProtocolType)768|(SecurityProtocolType)192;
ServicePointManager.ServerCertificateValidationCallback=delegate{return true;};

using(WebClient apiClient = new WebClient()) {
apiClient.Headers.Add("User-Agent",AppConfig.appName+" Installer v"+AppConfig.appVer);
apiClient.Headers.Add("Accept","application/vnd.github.v3+json");
string response = await apiClient.DownloadStringTaskAsync(apiUrl);

string downloadUrl = ExtractAssetUrl(response);
if(string.IsNullOrEmpty(downloadUrl)) {
txtStatus.Text="Could not find download asset";
return false;
}

return await DownloadFromUrl(downloadUrl, localFile);
}
} catch(Exception ex) {
txtStatus.Text=string.Format("GitHub API error: {0}",ex.Message);
return false;
}
}

private async Task<bool> DownloadFromUrl(string url, string localFile)
{
txtStatus.Text="Connecting to download server...";
Application.DoEvents();

client=new WebClient();
client.Headers.Add("User-Agent",AppConfig.appName+" Installer v"+AppConfig.appVer);

finished=false;
worked=false;

client.DownloadProgressChanged+=(sender,e)=> {
barProgress.Value=e.ProgressPercentage;
txtStatus.Text=string.Format("Downloading... {0}% ({1:F1} MB of {2:F1} MB)",
e.ProgressPercentage,
e.BytesReceived/1024.0/1024.0,
e.TotalBytesToReceive/1024.0/1024.0);
Application.DoEvents();
};

client.DownloadFileCompleted+=(sender,e)=> {
finished=true;
if(e.Error!=null) {
txtStatus.Text=string.Format("Download failed: {0}",e.Error.Message);
worked=false;
} else if(e.Cancelled) {
txtStatus.Text="Download was cancelled";
worked=false;
} else {
txtStatus.Text="Download completed successfully";
worked=true;
}
};

client.DownloadFileAsync(new Uri(url),localFile);

while(!finished) {
await Task.Delay(100);
Application.DoEvents();
}

client.Dispose();
return worked;
}

private string ExtractAssetUrl(string jsonResponse)
{
try {
int assetsStart = jsonResponse.IndexOf("\"assets\":");
if(assetsStart == -1) return null;

string assetsSection = jsonResponse.Substring(assetsStart);
int nameIndex = assetsSection.IndexOf("\"name\":\"" + AppConfig.exeName + "\"");
if(nameIndex == -1) {
nameIndex = assetsSection.IndexOf("\"name\":");
if(nameIndex == -1) return null;
}

int urlStart = assetsSection.IndexOf("\"browser_download_url\":\"", nameIndex);
if(urlStart == -1) return null;

urlStart += 25;
int urlEnd = assetsSection.IndexOf("\"", urlStart);
if(urlEnd == -1) return null;

return assetsSection.Substring(urlStart, urlEnd - urlStart);
} catch {
return null;
}
}

private string ProcessDownloadVariation(string originalUrl)
{
try {
if(originalUrl.Contains("github.com") && originalUrl.Contains("/releases/")) {
if(originalUrl.Contains("/download/")) {
return originalUrl;
} else if(originalUrl.Contains("/tag/")) {
string tagUrl = originalUrl;
string[] parts = tagUrl.Split('/');
if(parts.Length >= 7) {
string owner = parts[3];
string repo = parts[4];
string tag = parts[6];
return string.Format("https://api.github.com/repos/{0}/{1}/releases/tags/{2}",owner,repo,tag);
}
}
} else if(originalUrl.Contains("dropbox.com") && originalUrl.Contains("?dl=0")) {
return originalUrl.Replace("?dl=0","?dl=1");
} else if(originalUrl.Contains("dropbox.com") && !originalUrl.Contains("?dl=")) {
return originalUrl + "?dl=1";
}
return originalUrl;
} catch {
return originalUrl;
}
}

public static void Dispose() {
}
}
}
