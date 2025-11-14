using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Win32;

namespace FlexInstaller
{
    public class SetupManager
    {
        private string temporaryFilePath;
        private string finalInstallLocation;

        public SetupManager()
        {
            temporaryFilePath = Path.Combine(Path.GetTempPath(), AppConfig.exeName);
            finalInstallLocation = Path.Combine(AppConfig.instPath, AppConfig.exeName);
        }

        public bool CreateDirectoryStructure()
        {
            try
            {
                if (!Directory.Exists(AppConfig.instPath))
                {
                    Directory.CreateDirectory(AppConfig.instPath);
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Failed to create installation directory: {0}", ex.Message), "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool InstallApplication(string downloadedFile)
        {
            try
            {
                if (!File.Exists(downloadedFile))
                {
                    MessageBox.Show("Downloaded file not found.", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                FileInfo fileInfo = new FileInfo(downloadedFile);
                if (fileInfo.Length < 1024)
                {
                    MessageBox.Show("Downloaded file appears to be invalid or corrupted.", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                if (File.Exists(finalInstallLocation))
                {
                    try
                    {
                        File.Delete(finalInstallLocation);
                    }
                    catch
                    {
                        MessageBox.Show("Cannot overwrite existing installation. Please close " + AppConfig.appName + " and try again.", "File In Use", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                File.Move(downloadedFile, finalInstallLocation);

                CreateUninstaller();

                if (AppConfig.createStartMenu)
                {
                    CreateStartMenuLink();
                }

                RegisterInControlPanel();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Installation failed: {0}", ex.Message), "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void CreateDesktopShortcutManual()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktopPath, AppConfig.appName + ".lnk");
                
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = "powershell.exe";
                psi.Arguments = string.Format("-Command \"$WshShell = New-Object -comObject WScript.Shell; $Shortcut = $WshShell.CreateShortcut('{0}'); $Shortcut.TargetPath = '{1}'; $Shortcut.WorkingDirectory = '{2}'; $Shortcut.Save()\"", 
                    shortcutPath, finalInstallLocation, AppConfig.instPath);
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                Process.Start(psi).WaitForExit();
            }
            catch
            {
            }
        }

        private void CreateDesktopLink()
        {
        }

        private void CreateStartMenuLink()
        {
        }

        private void CreateUninstaller()
        {
            try
            {
                string uninstallerPath = Path.Combine(AppConfig.instPath, "uninstall.exe");
                
                File.Copy(Application.ExecutablePath, uninstallerPath, true);
            }
            catch
            {
            }
        }

        private void RegisterInControlPanel()
        {
            try
            {
                string uninstallKey = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0}", AppConfig.appName);
                string uninstallerPath = Path.Combine(AppConfig.instPath, "uninstall.exe");
                
                using (RegistryKey key = Registry.LocalMachine.CreateSubKey(uninstallKey))
                {
                    key.SetValue("DisplayName", AppConfig.appName);
                    key.SetValue("DisplayVersion", AppConfig.appVer);
                    key.SetValue("Publisher", AppConfig.pubName);
                    key.SetValue("InstallLocation", AppConfig.instPath);
                    key.SetValue("UninstallString", string.Format("\"{0}\"", uninstallerPath));
                    key.SetValue("DisplayIcon", finalInstallLocation);
                    key.SetValue("HelpLink", AppConfig.supportUrl);
                    key.SetValue("URLInfoAbout", AppConfig.website);
                    key.SetValue("NoModify", 1);
                    key.SetValue("NoRepair", 1);
                }
            }
            catch
            {
            }
        }

        public void LaunchInstalledApplication()
        {
            try
            {
                if (File.Exists(finalInstallLocation))
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.FileName = finalInstallLocation;
                    startInfo.WorkingDirectory = AppConfig.instPath;
                    startInfo.UseShellExecute = true;
                    Process.Start(startInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Could not launch application: {0}", ex.Message), "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public string GetTemporaryPath()
        {
            return temporaryFilePath;
        }
    }
}
