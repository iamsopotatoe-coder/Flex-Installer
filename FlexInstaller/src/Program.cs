using System;
using System.Windows.Forms;
using System.IO;
using System.Security.Principal;
using System.Diagnostics;

namespace FlexInstaller {
static class ApplicationEntry
{
[STAThread]
static void Main(string[] args)
{
string appFile=Path.GetFileName(Application.ExecutablePath).ToLower();
bool isRemoving=appFile.Contains("uninstall") || (args.Length>0 && args[0]=="/uninstall");

if(isRemoving) {
if(AppConfig.requireAdmin && !AdminCheck())
{
try
{
ProcessStartInfo psi=new ProcessStartInfo();
psi.UseShellExecute=true;
psi.WorkingDirectory=Environment.CurrentDirectory;
psi.FileName=Application.ExecutablePath;
psi.Verb="runas";
if(args.Length>0) {
psi.Arguments=string.Join(" ",args);
}
Process.Start(psi);
Environment.Exit(0);
return;
} catch {
MessageBox.Show("This uninstaller requires administrator privileges.","Administrator Rights Required",MessageBoxButtons.OK,MessageBoxIcon.Warning);
Environment.Exit(1);
return;
}
}

Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new UninstallerForm());
} else
{
Application.EnableVisualStyles();
Application.SetCompatibleTextRenderingDefault(false);
Application.Run(new InstallerMainWindow());
}
}

private static bool AdminCheck() {
try {
WindowsIdentity userInfo=WindowsIdentity.GetCurrent();
WindowsPrincipal userPrincipal=new WindowsPrincipal(userInfo);
return userPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
} catch {
return false;
}
}
}
}
