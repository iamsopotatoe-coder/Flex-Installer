using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Diagnostics;

namespace FlexInstaller
{
    public partial class UninstallerForm : Form
    {
        private Panel currentPanel;
        private Button uninstallButton;
        private Button cancelButton;
        private ProgressBar progressBar;
        private Label statusLabel;
        private bool uninstallCompleted = false;

        public UninstallerForm()
        {
            InitializeComponent();
            ShowWelcomeScreen();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 380);
            this.Text = "Uninstall " + AppConfig.appName;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            try
            {
                this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            }
            catch
            {
                this.Icon = SystemIcons.Application;
            }
            
            this.BackColor = SystemColors.Control;

            uninstallButton = new Button();
            uninstallButton.Text = "&Uninstall";
            uninstallButton.Size = new Size(75, 23);
            uninstallButton.Location = new Point(330, 320);
            uninstallButton.Click += UninstallButton_Click;
            uninstallButton.UseVisualStyleBackColor = true;

            cancelButton = new Button();
            cancelButton.Text = "&Cancel";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Location = new Point(411, 320);
            cancelButton.Click += CancelButton_Click;
            cancelButton.UseVisualStyleBackColor = true;

            this.Controls.Add(uninstallButton);
            this.Controls.Add(cancelButton);
        }

        private void ShowWelcomeScreen()
        {
            ClearCurrentPanel();

            currentPanel = new Panel();
            currentPanel.Size = new Size(460, 290);
            currentPanel.Location = new Point(12, 12);
            currentPanel.BackColor = SystemColors.Control;

            PictureBox iconBox = new PictureBox();
            iconBox.Size = new Size(32, 32);
            iconBox.Location = new Point(20, 20);
            iconBox.Image = SystemIcons.Warning.ToBitmap();
            iconBox.SizeMode = PictureBoxSizeMode.StretchImage;

            Label uninstallTitle = new Label();
            uninstallTitle.Text = "Uninstall " + AppConfig.appName;
            uninstallTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            uninstallTitle.Size = new Size(380, 30);
            uninstallTitle.Location = new Point(70, 20);

            Label confirmText = new Label();
            confirmText.Text = "This will completely remove " + AppConfig.appName + " from your computer.\n\nThe following will be removed:";
            confirmText.Font = new Font("Microsoft Sans Serif", 8.25f);
            confirmText.Size = new Size(420, 40);
            confirmText.Location = new Point(20, 70);

            ListBox itemsList = new ListBox();
            itemsList.Size = new Size(420, 100);
            itemsList.Location = new Point(20, 120);
            itemsList.Font = new Font("Microsoft Sans Serif", 8.25f);
            itemsList.SelectionMode = SelectionMode.None;
            
            itemsList.Items.Add("• Application files from: " + AppConfig.instPath);
            itemsList.Items.Add("• Desktop shortcuts");
            itemsList.Items.Add("• Start menu shortcuts"); 
            itemsList.Items.Add("• Registry entries");
            itemsList.Items.Add("• Installation directory");

            Label warningText = new Label();
            warningText.Text = "Warning: This action cannot be undone!";
            warningText.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
            warningText.ForeColor = Color.Red;
            warningText.Size = new Size(420, 20);
            warningText.Location = new Point(20, 240);

            Label questionText = new Label();
            questionText.Text = "Do you want to continue with the uninstallation?";
            questionText.Font = new Font("Microsoft Sans Serif", 8.25f);
            questionText.Size = new Size(420, 20);
            questionText.Location = new Point(20, 265);

            currentPanel.Controls.Add(iconBox);
            currentPanel.Controls.Add(uninstallTitle);
            currentPanel.Controls.Add(confirmText);
            currentPanel.Controls.Add(itemsList);
            currentPanel.Controls.Add(warningText);
            currentPanel.Controls.Add(questionText);

            this.Controls.Add(currentPanel);
        }

        private void ShowUninstallProgress()
        {
            ClearCurrentPanel();

            currentPanel = new Panel();
            currentPanel.Size = new Size(460, 290);
            currentPanel.Location = new Point(12, 12);
            currentPanel.BackColor = SystemColors.Control;

            Label progressTitle = new Label();
            progressTitle.Text = "Uninstalling " + AppConfig.appName;
            progressTitle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
            progressTitle.Size = new Size(440, 20);
            progressTitle.Location = new Point(20, 20);

            Label progressText = new Label();
            progressText.Text = "Please wait while " + AppConfig.appName + " is being removed from your computer.";
            progressText.Font = new Font("Microsoft Sans Serif", 8.25f);
            progressText.Size = new Size(420, 40);
            progressText.Location = new Point(20, 50);

            GroupBox progressGroup = new GroupBox();
            progressGroup.Text = "Uninstall Progress";
            progressGroup.Size = new Size(420, 120);
            progressGroup.Location = new Point(20, 100);

            progressBar = new ProgressBar();
            progressBar.Size = new Size(390, 20);
            progressBar.Location = new Point(15, 30);
            progressBar.Style = ProgressBarStyle.Continuous;
            progressBar.Value = 0;

            statusLabel = new Label();
            statusLabel.Text = "Preparing uninstallation...";
            statusLabel.Size = new Size(390, 20);
            statusLabel.Location = new Point(15, 60);
            statusLabel.Font = new Font("Microsoft Sans Serif", 8.25f);

            progressGroup.Controls.Add(progressBar);
            progressGroup.Controls.Add(statusLabel);

            Label warningLabel = new Label();
            warningLabel.Text = "Please wait until the uninstallation is complete.";
            warningLabel.Font = new Font("Microsoft Sans Serif", 8.25f);
            warningLabel.Size = new Size(420, 20);
            warningLabel.Location = new Point(20, 235);
            warningLabel.ForeColor = Color.Blue;

            currentPanel.Controls.Add(progressTitle);
            currentPanel.Controls.Add(progressText);
            currentPanel.Controls.Add(progressGroup);
            currentPanel.Controls.Add(warningLabel);

            this.Controls.Add(currentPanel);
            
            uninstallButton.Enabled = false;
            cancelButton.Enabled = false;
        }

        private void ShowCompletionScreen()
        {
            ClearCurrentPanel();

            currentPanel = new Panel();
            currentPanel.Size = new Size(460, 290);
            currentPanel.Location = new Point(12, 12);
            currentPanel.BackColor = SystemColors.Control;

            PictureBox successIcon = new PictureBox();
            successIcon.Size = new Size(32, 32);
            successIcon.Location = new Point(20, 20);
            successIcon.Image = SystemIcons.Information.ToBitmap();
            successIcon.SizeMode = PictureBoxSizeMode.StretchImage;

            Label completeTitle = new Label();
            completeTitle.Text = "Uninstall Complete";
            completeTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            completeTitle.Size = new Size(380, 30);
            completeTitle.Location = new Point(70, 20);

            Label successMessage = new Label();
            successMessage.Text = AppConfig.appName + " has been successfully removed from your computer.\n\nAll files, shortcuts, and registry entries have been deleted.\nThe installation folder will be removed when you close this window.";
            successMessage.Font = new Font("Microsoft Sans Serif", 8.25f);
            successMessage.Size = new Size(420, 80);
            successMessage.Location = new Point(20, 70);

            GroupBox summaryGroup = new GroupBox();
            summaryGroup.Text = "Uninstall Summary";
            summaryGroup.Size = new Size(420, 120);
            summaryGroup.Location = new Point(20, 160);

            ListBox summaryList = new ListBox();
            summaryList.Size = new Size(390, 90);
            summaryList.Location = new Point(15, 20);
            summaryList.Font = new Font("Microsoft Sans Serif", 8.25f);
            summaryList.SelectionMode = SelectionMode.None;
            
            summaryList.Items.Add("✓ Application files removed");
            summaryList.Items.Add("✓ Desktop shortcuts removed");
            summaryList.Items.Add("✓ Registry entries removed");
            summaryList.Items.Add("✓ Installation directory removed");

            summaryGroup.Controls.Add(summaryList);

            Label finishText = new Label();
            finishText.Text = "Click Close to complete the uninstallation and remove the installation folder.";
            finishText.Font = new Font("Microsoft Sans Serif", 8.25f);
            finishText.Size = new Size(420, 20);
            finishText.Location = new Point(20, 290);

            currentPanel.Controls.Add(successIcon);
            currentPanel.Controls.Add(completeTitle);
            currentPanel.Controls.Add(successMessage);
            currentPanel.Controls.Add(summaryGroup);
            currentPanel.Controls.Add(finishText);

            this.Controls.Add(currentPanel);

            uninstallButton.Text = "&Close";
            uninstallButton.Enabled = true;
            cancelButton.Enabled = false;
            uninstallCompleted = true;
        }

        private async void UninstallButton_Click(object sender, EventArgs e)
        {
            if (uninstallCompleted)
            {
                Environment.Exit(0);
                return;
            }

            DialogResult result = MessageBox.Show(
                "Are you sure you want to uninstall " + AppConfig.appName + "?\n\nThis will permanently remove the application from your computer.",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                ShowUninstallProgress();
                await PerformUninstallation();
            }
        }

        private async System.Threading.Tasks.Task PerformUninstallation()
        {
            try
            {
                statusLabel.Text = "Removing registry entries...";
                progressBar.Value = 10;
                await System.Threading.Tasks.Task.Delay(500);
                RemoveRegistryEntries();

                statusLabel.Text = "Removing desktop shortcuts...";
                progressBar.Value = 25;
                await System.Threading.Tasks.Task.Delay(500);
                RemoveDesktopShortcuts();

                statusLabel.Text = "Removing start menu shortcuts...";
                progressBar.Value = 40;
                await System.Threading.Tasks.Task.Delay(500);
                RemoveStartMenuShortcuts();

                statusLabel.Text = "Closing running instances...";
                progressBar.Value = 60;
                await System.Threading.Tasks.Task.Delay(500);
                CloseRunningInstances();

                statusLabel.Text = "Removing application files...";
                progressBar.Value = 80;
                await System.Threading.Tasks.Task.Delay(1000);
                RemoveApplicationFiles();

                statusLabel.Text = "Cleaning up installation directory...";
                progressBar.Value = 95;
                await System.Threading.Tasks.Task.Delay(500);
                RemoveInstallationDirectory();

                statusLabel.Text = "Uninstallation completed successfully!";
                progressBar.Value = 100;
                await System.Threading.Tasks.Task.Delay(1000);

                ShowCompletionScreen();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error during uninstallation: " + ex.Message, "Uninstall Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }

        private void RemoveRegistryEntries()
        {
            try
            {
                string uninstallKey = string.Format(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{0}", AppConfig.appName);
                Registry.LocalMachine.DeleteSubKey(uninstallKey, false);
            }
            catch { }
        }

        private void RemoveDesktopShortcuts()
        {
            try
            {
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                string shortcutPath = Path.Combine(desktopPath, AppConfig.appName + ".lnk");
                if (File.Exists(shortcutPath))
                {
                    File.Delete(shortcutPath);
                }
            }
            catch { }
        }

        private void RemoveStartMenuShortcuts()
        {
            try
            {
                string startMenuPath = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
                string appStartMenuPath = Path.Combine(startMenuPath, "Programs", AppConfig.appName + ".lnk");
                if (File.Exists(appStartMenuPath))
                {
                    File.Delete(appStartMenuPath);
                }
            }
            catch { }
        }

        private void CloseRunningInstances()
        {
            try
            {
                string processName = Path.GetFileNameWithoutExtension(AppConfig.exeName);
                Process[] processes = Process.GetProcessesByName(processName);
                foreach (Process process in processes)
                {
                    try
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                        process.Dispose();
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void RemoveApplicationFiles()
        {
            try
            {
                string appPath = Path.Combine(AppConfig.instPath, AppConfig.exeName);
                if (File.Exists(appPath))
                {
                    File.Delete(appPath);
                }

                if (Directory.Exists(AppConfig.instPath))
                {
                    string[] files = Directory.GetFiles(AppConfig.instPath);
                    foreach (string file in files)
                    {
                        if (!file.EndsWith("uninstall.exe", StringComparison.OrdinalIgnoreCase))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private void RemoveInstallationDirectory()
        {
            try
            {
                if (Directory.Exists(AppConfig.instPath))
                {
                    string batchFile = Path.Combine(Path.GetTempPath(), "cleanup_" + DateTime.Now.Ticks.ToString() + ".bat");
                    string uninstallerPath = Application.ExecutablePath;
                    string installPath = AppConfig.instPath;
                    
                    using (StreamWriter writer = new StreamWriter(batchFile))
                    {
                        writer.WriteLine("@echo off");
                        writer.WriteLine("echo Cleaning up installation...");
                        writer.WriteLine("timeout /t 3 /nobreak > nul");
                        writer.WriteLine("");
                        writer.WriteLine("REM Kill any remaining processes");
                        writer.WriteLine("taskkill /f /im \"" + Path.GetFileName(uninstallerPath) + "\" 2>nul");
                        writer.WriteLine("timeout /t 1 /nobreak > nul");
                        writer.WriteLine("");
                        writer.WriteLine("REM Delete the uninstaller");
                        writer.WriteLine(":DELETE_UNINSTALLER");
                        writer.WriteLine("del /f /q \"" + uninstallerPath + "\" 2>nul");
                        writer.WriteLine("if exist \"" + uninstallerPath + "\" (");
                        writer.WriteLine("    timeout /t 1 /nobreak > nul");
                        writer.WriteLine("    goto DELETE_UNINSTALLER");
                        writer.WriteLine(")");
                        writer.WriteLine("");
                        writer.WriteLine("REM Remove installation directory");
                        writer.WriteLine(":DELETE_FOLDER");
                        writer.WriteLine("rmdir /s /q \"" + installPath + "\" 2>nul");
                        writer.WriteLine("if exist \"" + installPath + "\" (");
                        writer.WriteLine("    timeout /t 1 /nobreak > nul");
                        writer.WriteLine("    goto DELETE_FOLDER");
                        writer.WriteLine(")");
                        writer.WriteLine("");
                        writer.WriteLine("REM Delete this batch file");
                        writer.WriteLine("del \"%~f0\"");
                    }

                    ProcessStartInfo psi = new ProcessStartInfo();
                    psi.FileName = "cmd.exe";
                    psi.Arguments = "/c \"" + batchFile + "\"";
                    psi.WindowStyle = ProcessWindowStyle.Hidden;
                    psi.CreateNoWindow = true;
                    psi.UseShellExecute = false;
                    Process.Start(psi);
                    
                    try
                    {
                        string[] files = Directory.GetFiles(AppConfig.instPath);
                        foreach (string file in files)
                        {
                            if (!file.Equals(uninstallerPath, StringComparison.OrdinalIgnoreCase))
                            {
                                try
                                {
                                    File.Delete(file);
                                }
                                catch { }
                            }
                        }
                    }
                    catch { }
                }
            }
            catch { }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel the uninstallation?", "Cancel Uninstall", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                this.Close();
            }
        }

        private void ClearCurrentPanel()
        {
            if (currentPanel != null)
            {
                this.Controls.Remove(currentPanel);
                currentPanel.Dispose();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (!uninstallCompleted && uninstallButton.Enabled == false)
            {
                MessageBox.Show("Please wait for the uninstallation to complete.", "Uninstall In Progress", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
    }
}
