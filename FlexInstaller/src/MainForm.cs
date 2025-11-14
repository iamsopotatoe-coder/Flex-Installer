using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Diagnostics;

namespace FlexInstaller
{
    public partial class InstallerMainWindow : Form
    {
        private Panel currentPanel;
        private Button nextButton;
        private Button backButton;
        private Button cancelButton;
        private int currentStep;
        private bool licenseAccepted;
        private FileTransferManager dlMgr;
        private SetupManager instMgr;

        public InstallerMainWindow()
        {
            InitializeComponent();
            CheckAdministratorRights();
            instMgr = new SetupManager();
            currentStep = 0;
            ShowWelcomeScreen();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(500, 380);
            this.Text = AppConfig.appName + " Setup";
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

            nextButton = new Button();
            nextButton.Text = "&Next >";
            nextButton.Size = new Size(75, 23);
            nextButton.Location = new Point(330, 320);
            nextButton.Click += NextButton_Click;
            nextButton.UseVisualStyleBackColor = true;

            backButton = new Button();
            backButton.Text = "< &Back";
            backButton.Size = new Size(75, 23);
            backButton.Location = new Point(249, 320);
            backButton.Click += BackButton_Click;
            backButton.Enabled = false;
            backButton.UseVisualStyleBackColor = true;

            cancelButton = new Button();
            cancelButton.Text = "Cancel";
            cancelButton.Size = new Size(75, 23);
            cancelButton.Location = new Point(411, 320);
            cancelButton.Click += CancelButton_Click;
            cancelButton.UseVisualStyleBackColor = true;

            this.Controls.Add(nextButton);
            this.Controls.Add(backButton);
            this.Controls.Add(cancelButton);
        }

        private void CheckAdministratorRights()
        {
            if (AppConfig.requireAdmin)
            {
                bool isAdmin = false;
                try
                {
                    WindowsIdentity identity = WindowsIdentity.GetCurrent();
                    WindowsPrincipal principal = new WindowsPrincipal(identity);
                    isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch
                {
                    isAdmin = false;
                }

                if (!isAdmin)
                {
                    try
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = Environment.CurrentDirectory;
                        startInfo.FileName = Application.ExecutablePath;
                        startInfo.Verb = "runas";
                        Process.Start(startInfo);
                        Environment.Exit(0);
                        return;
                    }
                    catch
                    {
                        MessageBox.Show("This installer requires administrator privileges.", "Administrator Rights Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        Environment.Exit(1);
                    }
                }
            }
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
            iconBox.Image = SystemIcons.Information.ToBitmap();
            iconBox.SizeMode = PictureBoxSizeMode.StretchImage;

            Label welcomeTitle = new Label();
            welcomeTitle.Text = "Welcome to the " + AppConfig.appName + " Setup Wizard";
            welcomeTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            welcomeTitle.Size = new Size(380, 60);
            welcomeTitle.Location = new Point(70, 20);

            Label welcomeText = new Label();
            welcomeText.Text = "This wizard will install " + AppConfig.appName + " on your computer.\n\n" + AppConfig.welcomeMsg;
            welcomeText.Font = new Font("Microsoft Sans Serif", 8.25f);
            welcomeText.Size = new Size(420, 80);
            welcomeText.Location = new Point(20, 90);

            GroupBox infoGroup = new GroupBox();
            infoGroup.Text = "Setup Information";
            infoGroup.Size = new Size(420, 100);
            infoGroup.Location = new Point(20, 180);

            Label versionInfo = new Label();
            versionInfo.Text = string.Format("Version: {0}\nPublisher: {1}\nInstall Location: {2}", AppConfig.appVer, AppConfig.pubName, AppConfig.instPath);
            versionInfo.Font = new Font("Microsoft Sans Serif", 8.25f);
            versionInfo.Size = new Size(400, 70);
            versionInfo.Location = new Point(15, 20);

            infoGroup.Controls.Add(versionInfo);
            currentPanel.Controls.Add(iconBox);
            currentPanel.Controls.Add(welcomeTitle);
            currentPanel.Controls.Add(welcomeText);
            currentPanel.Controls.Add(infoGroup);

            this.Controls.Add(currentPanel);
        }

        private void ShowLicenseScreen()
        {
            ClearCurrentPanel();

            currentPanel = new Panel();
            currentPanel.Size = new Size(460, 290);
            currentPanel.Location = new Point(12, 12);
            currentPanel.BackColor = SystemColors.Control;

            Label licenseTitle = new Label();
            licenseTitle.Text = "License Agreement";
            licenseTitle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
            licenseTitle.Size = new Size(440, 20);
            licenseTitle.Location = new Point(20, 10);

            Label instructionLabel = new Label();
            instructionLabel.Text = "Please read the following license agreement. You must accept the agreement to continue.";
            instructionLabel.Font = new Font("Microsoft Sans Serif", 8.25f);
            instructionLabel.Size = new Size(440, 30);
            instructionLabel.Location = new Point(20, 35);

            TextBox licenseTextBox = new TextBox();
            licenseTextBox.Text = AppConfig.licenseText;
            licenseTextBox.Multiline = true;
            licenseTextBox.ScrollBars = ScrollBars.Vertical;
            licenseTextBox.ReadOnly = true;
            licenseTextBox.Size = new Size(420, 180);
            licenseTextBox.Location = new Point(20, 70);
            licenseTextBox.Font = new Font("Courier New", 8.25f);
            licenseTextBox.BackColor = SystemColors.Window;

            CheckBox acceptCheckBox = new CheckBox();
            acceptCheckBox.Text = "I &accept the terms in the License Agreement";
            acceptCheckBox.Size = new Size(300, 20);
            acceptCheckBox.Location = new Point(20, 260);
            acceptCheckBox.Font = new Font("Microsoft Sans Serif", 8.25f);
            acceptCheckBox.CheckedChanged += (s, e) => {
                licenseAccepted = acceptCheckBox.Checked;
                nextButton.Enabled = licenseAccepted;
            };

            currentPanel.Controls.Add(licenseTitle);
            currentPanel.Controls.Add(instructionLabel);
            currentPanel.Controls.Add(licenseTextBox);
            currentPanel.Controls.Add(acceptCheckBox);

            this.Controls.Add(currentPanel);
            nextButton.Enabled = false;
            backButton.Enabled = true;
        }

        private void ShowInstallationScreen()
        {
            ClearCurrentPanel();

            currentPanel = new Panel();
            currentPanel.Size = new Size(460, 290);
            currentPanel.Location = new Point(12, 12);
            currentPanel.BackColor = SystemColors.Control;

            Label installTitle = new Label();
            installTitle.Text = "Installing " + AppConfig.appName;
            installTitle.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Bold);
            installTitle.Size = new Size(440, 20);
            installTitle.Location = new Point(20, 20);

            Label statusAction = new Label();
            statusAction.Text = "Please wait while Setup installs " + AppConfig.appName + " on your computer.";
            statusAction.Font = new Font("Microsoft Sans Serif", 8.25f);
            statusAction.Size = new Size(420, 40);
            statusAction.Location = new Point(20, 50);

            GroupBox progressGroup = new GroupBox();
            progressGroup.Text = "Progress";
            progressGroup.Size = new Size(420, 120);
            progressGroup.Location = new Point(20, 100);

            ProgressBar progressBar = new ProgressBar();
            progressBar.Size = new Size(390, 20);
            progressBar.Location = new Point(15, 30);
            progressBar.Style = ProgressBarStyle.Continuous;

            Label statusLabel = new Label();
            statusLabel.Text = "Preparing installation...";
            statusLabel.Size = new Size(390, 20);
            statusLabel.Location = new Point(15, 60);
            statusLabel.Font = new Font("Microsoft Sans Serif", 8.25f);

            Label detailsLabel = new Label();
            detailsLabel.Text = "Destination: " + AppConfig.instPath;
            detailsLabel.Font = new Font("Microsoft Sans Serif", 8.25f);
            detailsLabel.Size = new Size(390, 40);
            detailsLabel.Location = new Point(15, 80);

            progressGroup.Controls.Add(progressBar);
            progressGroup.Controls.Add(statusLabel);
            progressGroup.Controls.Add(detailsLabel);

            Label warningLabel = new Label();
            warningLabel.Text = "Warning: Do not turn off your computer during installation.";
            warningLabel.Font = new Font("Microsoft Sans Serif", 8.25f);
            warningLabel.Size = new Size(420, 20);
            warningLabel.Location = new Point(20, 235);
            warningLabel.ForeColor = Color.Red;

            currentPanel.Controls.Add(installTitle);
            currentPanel.Controls.Add(statusAction);
            currentPanel.Controls.Add(progressGroup);
            currentPanel.Controls.Add(warningLabel);

            this.Controls.Add(currentPanel);
            nextButton.Enabled = false;
            backButton.Enabled = false;
            cancelButton.Enabled = false;

            dlMgr = new FileTransferManager(progressBar, statusLabel);
            StartInstallationProcess();
        }

        private async void StartInstallationProcess()
        {
            try
            {
                if (!instMgr.CreateDirectoryStructure())
                {
                    return;
                }

                string tempFile = instMgr.GetTemporaryPath();
                bool downloadSuccess = await dlMgr.RetrieveFileFromCloud(AppConfig.dlUrl, tempFile);

                if (!downloadSuccess)
                {
                    MessageBox.Show("Failed to download the application files.", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool installSuccess = instMgr.InstallApplication(tempFile);

                if (installSuccess)
                {
                    nextButton.Enabled = true;
                    cancelButton.Enabled = true;
                    ShowCompletionScreen();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Installation failed: {0}", ex.Message), "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            completeTitle.Text = "Completing the " + AppConfig.appName + " Setup Wizard";
            completeTitle.Font = new Font("Microsoft Sans Serif", 12, FontStyle.Bold);
            completeTitle.Size = new Size(380, 60);
            completeTitle.Location = new Point(70, 20);

            Label successMessage = new Label();
            successMessage.Text = "Setup has finished installing " + AppConfig.appName + " on your computer.\n\n" + AppConfig.completeMsg;
            successMessage.Font = new Font("Microsoft Sans Serif", 8.25f);
            successMessage.Size = new Size(420, 80);
            successMessage.Location = new Point(20, 90);

            GroupBox optionsGroup = new GroupBox();
            optionsGroup.Text = "What would you like to do now?";
            optionsGroup.Size = new Size(420, 100);
            optionsGroup.Location = new Point(20, 180);

            CheckBox launchCheckBox = new CheckBox();
            launchCheckBox.Text = "&Launch " + AppConfig.appName;
            launchCheckBox.Checked = AppConfig.runAfter;
            launchCheckBox.Size = new Size(380, 20);
            launchCheckBox.Location = new Point(15, 25);
            launchCheckBox.Font = new Font("Microsoft Sans Serif", 8.25f);

            CheckBox desktopCheckBox = new CheckBox();
            desktopCheckBox.Text = "Create &Desktop shortcut";
            desktopCheckBox.Checked = AppConfig.createDesktop;
            desktopCheckBox.Size = new Size(380, 20);
            desktopCheckBox.Location = new Point(15, 45);
            desktopCheckBox.Font = new Font("Microsoft Sans Serif", 8.25f);

            Label thanksLabel = new Label();
            thanksLabel.Text = "Click Finish to exit Setup.";
            thanksLabel.Font = new Font("Microsoft Sans Serif", 8.25f);
            thanksLabel.Size = new Size(380, 20);
            thanksLabel.Location = new Point(15, 70);

            optionsGroup.Controls.Add(launchCheckBox);
            optionsGroup.Controls.Add(desktopCheckBox);
            optionsGroup.Controls.Add(thanksLabel);

            currentPanel.Controls.Add(successIcon);
            currentPanel.Controls.Add(completeTitle);
            currentPanel.Controls.Add(successMessage);
            currentPanel.Controls.Add(optionsGroup);

            this.Controls.Add(currentPanel);

            nextButton.Text = "&Finish";
            nextButton.Enabled = true;
            nextButton.Tag = new CheckBox[] { launchCheckBox, desktopCheckBox };
            cancelButton.Enabled = false;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            if (nextButton.Text == "&Finish")
            {
                CheckBox[] checkboxes = nextButton.Tag as CheckBox[];
                if (checkboxes != null)
                {
                    CheckBox launchBox = checkboxes[0];
                    CheckBox desktopBox = checkboxes[1];
                    
                    if (desktopBox.Checked)
                    {
                        instMgr.CreateDesktopShortcutManual();
                    }
                    
                    if (launchBox.Checked)
                    {
                        instMgr.LaunchInstalledApplication();
                    }
                }
                this.Close();
                return;
            }

            currentStep++;

            switch (currentStep)
            {
                case 1:
                    if (AppConfig.showLicense)
                    {
                        ShowLicenseScreen();
                    }
                    else
                    {
                        currentStep++;
                        ShowInstallationScreen();
                    }
                    break;
                case 2:
                    ShowInstallationScreen();
                    break;
            }
        }

        private void BackButton_Click(object sender, EventArgs e)
        {
            currentStep--;

            switch (currentStep)
            {
                case 0:
                    ShowWelcomeScreen();
                    break;
                case 1:
                    if (AppConfig.showLicense)
                    {
                        ShowLicenseScreen();
                    }
                    else
                    {
                        ShowWelcomeScreen();
                    }
                    break;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to cancel the installation?", "Cancel Installation", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            FileTransferManager.Dispose();
            base.OnFormClosed(e);
        }
    }
}
