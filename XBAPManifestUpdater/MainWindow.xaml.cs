using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml;

namespace XBAPManifestUpdater
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			DataContext = this;
			// Check for Config file
			String currentDirectory = Directory.GetCurrentDirectory();
			configFilePath = String.Format(@"{0}\XBAPManifestUpdater.config", currentDirectory);
			configFileExists = File.Exists(configFilePath);

			pbUpdate.Visibility = Visibility.Hidden;
			lblCertificateRequiresPassword.Visibility = Visibility.Hidden;
			pwbCertPassword.Visibility = Visibility.Hidden;

			if (configFileExists)
			{
				XmlDocument configFile= new XmlDocument();
				configFile.Load(configFilePath);

				ManifestFilePath = tbAppFilePath.Text = configFile.GetElementsByTagName("manifest").Count > 0 ? configFile.GetElementsByTagName("manifest")[0].InnerText : "";
				CertificateFilePath = tbCertFilePath.Text = configFile.GetElementsByTagName("certificate").Count > 0 ? configFile.GetElementsByTagName("certificate")[0].InnerText : "";
				CertificatePassword = pwbCertPassword.Password = configFile.GetElementsByTagName("password").Count > 0 ? configFile.GetElementsByTagName("password")[0].InnerText : "";
				PopulateAndShowVersion();
				CheckCertificate();

				if (CertificatePassword != null)
				{
					btnCheckCertPassword_Click(null, null);
				}
			}
		}

		private bool configFileExists = false;
		private string configFilePath = "";

		public string ManifestFilePath { get; set; }

		public string CertificateFilePath { get; set;}

		public string CertificatePassword { get; set; }

		public string Version { get; set; }

		public string ApplicationName { get; set; }

		private void btnBrowse_Click(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog 
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension 
			dlg.DefaultExt = ".xbap";
			dlg.Filter = "WPF Manifest (*.xbap)|*.xbap";


			// Display OpenFileDialog by calling ShowDialog method 
			Nullable<bool> result = dlg.ShowDialog();


			// Get the selected file name and display in a TextBox 
			if (result == true)
			{
				ManifestFilePath = tbAppFilePath.Text = dlg.FileName;
			}

			PopulateAndShowVersion();

			if (!configFileExists)
			{
				using (XmlWriter writer = XmlWriter.Create("XBAPManifestUpdater.config"))
				{
					writer.WriteStartElement("AppConfig");
					writer.WriteElementString("manifest", ManifestFilePath);
					writer.WriteEndElement();
					writer.Flush();
				}
				configFileExists = true;
			}
			else
			{
				XmlDocument configFile = new XmlDocument();
				configFile.Load(configFilePath);

				XmlNode manifestNode = configFile.GetElementsByTagName("manifest").Count > 0 ? configFile.GetElementsByTagName("manifest")[0] : configFile.CreateNode(XmlNodeType.Element, "manifest","");
				manifestNode.InnerText = ManifestFilePath;

				XmlNode AppConfigNode = configFile.GetElementsByTagName("AppConfig")[0];
				AppConfigNode.AppendChild(manifestNode);
				configFile.Save(configFilePath);
			}
		}

		private void CheckCertificate()
		{
			try
			{
				X509Certificate.CreateFromCertFile(CertificateFilePath);
				lblCertificateRequiresPassword.Visibility = Visibility.Hidden;
				pwbCertPassword.Visibility = Visibility.Hidden;
			}
			catch
			{
				lblCertificateRequiresPassword.Visibility = Visibility.Visible;
				pwbCertPassword.Visibility = Visibility.Visible;
			}
		}

		private void PopulateAndShowVersion()
		{
			// Open XBAP as XML Doc
			XmlDocument xbap = new XmlDocument();
			xbap.Load(ManifestFilePath);

			XmlNode dependentAssembly = xbap.GetElementsByTagName("dependentAssembly")[0];
			XmlNode assemblyIdentity = xbap.GetElementsByTagName("assemblyIdentity")[0];

			lblApplicationName.Content = ApplicationName = assemblyIdentity.Attributes["name"].Value.Substring(0, assemblyIdentity.Attributes["name"].Value.Length - 5);

			tbVersion.Text = Version = assemblyIdentity.Attributes["version"].Value;

			lblApplicationName.Visibility = Visibility.Visible;
			lblVersion.Visibility = Visibility.Visible;
			tbVersion.Visibility = Visibility.Visible;
		}

		private void tbAppFilePath_TextChanged(object sender, TextChangedEventArgs e)
		{
			btnUpdate.IsEnabled = tbAppFilePath.Text.Trim().Length > 0 && tbCertFilePath.Text.Trim().Length > 0 && (lblCertificateRequiresPassword.Visibility == Visibility.Hidden || pwbCertPassword.Password.Length > 0) ? true : false;
		}

		private void btnBrowseCert_Click(object sender, RoutedEventArgs e)
		{
			// Create OpenFileDialog 
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

			// Set filter for file extension and default file extension 
			dlg.DefaultExt = ".pfx";
			dlg.Filter = "Certificate (*.pfx)|*.pfx";


			// Display OpenFileDialog by calling ShowDialog method 
			Nullable<bool> result = dlg.ShowDialog();


			// Get the selected file name and display in a TextBox 
			if (result == true)
			{
				CertificateFilePath = tbCertFilePath.Text = dlg.FileName;
			}

			CheckCertificate();

			if (!configFileExists)
			{
				using (XmlWriter writer = XmlWriter.Create("XBAPManifestUpdater.config"))
				{
					writer.WriteStartElement("AppConfig");
					writer.WriteElementString("certificate", CertificateFilePath);
					writer.WriteEndElement();
					writer.Flush();
				}
				configFileExists = true;
			}
			else
			{
				XmlDocument configFile = new XmlDocument();
				configFile.Load(configFilePath);

				XmlNode certificateNode = configFile.GetElementsByTagName("certificate").Count > 0 ? configFile.GetElementsByTagName("certificate")[0] : configFile.CreateNode(XmlNodeType.Element, "certificate", "");
				certificateNode.InnerText = CertificateFilePath;

				XmlNode AppConfigNode = configFile.GetElementsByTagName("AppConfig")[0];
				AppConfigNode.AppendChild(certificateNode);
				configFile.Save(configFilePath);
			}
		}

		private void tbCertFilePath_TextChanged(object sender, TextChangedEventArgs e)
		{
			CheckCertificate();
			btnUpdate.IsEnabled = tbAppFilePath.Text.Trim().Length > 0 && tbCertFilePath.Text.Trim().Length > 0 && (lblCertificateRequiresPassword.Visibility == Visibility.Hidden || pwbCertPassword.Password.Length > 0) ? true : false;
		}

		// extracts [resource] into the the file specified by [path]
		private void ExtractResource( string resource, string path )
		{
			Stream stream = GetType().Assembly.GetManifestResourceStream( resource );
			byte[] bytes = new byte[(int)stream.Length];
			stream.Read( bytes, 0, bytes.Length );
			File.WriteAllBytes( path, bytes );
		}

		private void UpdateStatusMessage(string Message)
		{
			App.Current.Dispatcher.Invoke((Action)delegate
			{
				lblStatusMessage.Content = Message;
			});
		}

		private void btnUpdate_Click(object sender, RoutedEventArgs e)
		{
			Version = tbVersion.Text;
			pbUpdate.Visibility = Visibility.Visible;
			BackgroundWorker backgroundThread = new BackgroundWorker();
			backgroundThread.WorkerReportsProgress = true;
			backgroundThread.ProgressChanged += UpdateProgressBar;
			backgroundThread.DoWork += UpdateManifest;
			backgroundThread.RunWorkerCompleted += UpdateManifestCompleted;
			backgroundThread.RunWorkerAsync();
		}

		private void UpdateManifestCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			pbUpdate.Visibility = Visibility.Hidden;
		}

		private void UpdateManifest(object sender, DoWorkEventArgs e)
		{
			// Show progress bar and start update
			UpdateStatusMessage("Starting Update...");
			(sender as BackgroundWorker).ReportProgress(5);

			// Rename .deploy files
			UpdateStatusMessage("Removing .deploy file extensions...");
			(sender as BackgroundWorker).ReportProgress(10);
			DirectoryInfo parentApplicationDirectory = Directory.GetParent(ManifestFilePath);
			DirectoryInfo dirToSearch = new DirectoryInfo(String.Format("{0}\\Application Files\\{1}_{2}", parentApplicationDirectory.FullName, ApplicationName, Version.Replace('.','_')));
			string[] deployFiles = Directory.GetFiles(dirToSearch.FullName, "*.deploy", SearchOption.AllDirectories);
			foreach (string file in deployFiles)
			{
				UpdateStatusMessage("Moving "+ file +" ...");
				File.Move(file, file.Substring(0, file.Length - 7));
			}

			// Extract the mage.exe from resources and save it to the temp directory
			UpdateStatusMessage("Preparing for update...");
			(sender as BackgroundWorker).ReportProgress(15);
			string mageExePath = "C:\\temp\\mage.exe";
			ExtractResource("XBAPManifestUpdater.mage.exe", mageExePath);

			// Load the .xbap file as XML document
			XmlDocument xmlManifest = new XmlDocument();
			xmlManifest.Load(ManifestFilePath);

			// Use the xbap to find the correct version folder in Application Files
			XmlNode assembly = xmlManifest.GetElementsByTagName("dependentAssembly")[0];
			string ApplicationManifestPath = ManifestFilePath.Substring(0, ManifestFilePath.Length - System.IO.Path.GetFileName(ManifestFilePath).Length) + assembly.Attributes["codebase"].Value;

			// Arguments for running mage.exe first time
			string appParameters = String.Format("-update \"{0}\" -CertFile \"{1}\" -Password [password] -Version \"{2}\"", ApplicationManifestPath, CertificateFilePath, Version);

			// Set up mage.exe to run headless without a terminal window
			Process mageProcess = new Process();
			mageProcess.StartInfo.UseShellExecute = false;
			mageProcess.StartInfo.CreateNoWindow = true;
			mageProcess.StartInfo.FileName = mageExePath;
			mageProcess.StartInfo.Arguments = appParameters;

			// Update Application Manifest
			UpdateStatusMessage("Updating the application manifest...");
			(sender as BackgroundWorker).ReportProgress(25);
			mageProcess.Start();
			mageProcess.WaitForExit();

			// Arguments for running mage.exe second time
			string deployParameters = String.Format("-update \"{0}\" -appmanifest \"{1}\" -CertFile \"{2}\" -Password [password] -Version \"{3}\"", ManifestFilePath, ApplicationManifestPath, CertificateFilePath, Version);
			mageProcess.StartInfo.Arguments = deployParameters;

			// Update Deployment Manifest
			UpdateStatusMessage("Updating the deployment manifest...");
			(sender as BackgroundWorker).ReportProgress(65);
			mageProcess.Start();
			mageProcess.WaitForExit();

			// Renaming files back to .deploy (excluding xbap and manifest files)
			UpdateStatusMessage("Adding .deploy extensions...");
			(sender as BackgroundWorker).ReportProgress(95);
			foreach (string file in deployFiles)
			{
				if (!file.Contains(".xbap") || !file.Contains(".manifest"))
				{
					File.Move(file.Substring(0, file.Length - 7), file);
				}
			}
			// Copy the updated xbap file to the Application Files folder
			File.Copy(ManifestFilePath, ApplicationManifestPath.Substring(0, ApplicationManifestPath.Length - 13) + ".xbap", true);

			// Delete the mage.exe from the temp directory
			File.Delete(mageExePath);

			UpdateStatusMessage("Update complete!");
			(sender as BackgroundWorker).ReportProgress(100);
		}

		private void pwbCertPassword_PasswordChanged(object sender, RoutedEventArgs e)
		{
			lblCertificateRequiresPassword.Foreground = Brushes.Red;
			btnUpdate.IsEnabled = false;
		}

		private void btnCheckCertPassword_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				X509Certificate2 cert = new X509Certificate2(CertificateFilePath, pwbCertPassword.Password);
				lblCertificateRequiresPassword.Foreground = Brushes.Green;
				btnUpdate.IsEnabled = tbAppFilePath.Text.Trim().Length > 0 && tbCertFilePath.Text.Trim().Length > 0 ? true : false;

				if (!configFileExists)
				{
					using (XmlWriter writer = XmlWriter.Create("XBAPManifestUpdater.config"))
					{
						writer.WriteStartElement("AppConfig");
						writer.WriteElementString("password", pwbCertPassword.Password);
						writer.WriteEndElement();
						writer.Flush();
					}
					configFileExists = true;
				}
				else
				{
					XmlDocument configFile = new XmlDocument();
					configFile.Load(configFilePath);

					XmlNode certPasswordNode = configFile.GetElementsByTagName("password").Count > 0 ? configFile.GetElementsByTagName("password")[0] : configFile.CreateNode(XmlNodeType.Element, "password", "");
					certPasswordNode.InnerText = pwbCertPassword.Password;

					XmlNode AppConfigNode = configFile.GetElementsByTagName("AppConfig")[0];
					AppConfigNode.AppendChild(certPasswordNode);
					configFile.Save(configFilePath);
				}
		
			}
			catch
			{
				lblCertificateRequiresPassword.Foreground = Brushes.Red;
			}

		}

        private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
        {
			pbUpdate.Value = e.ProgressPercentage;
        }
	}
}
