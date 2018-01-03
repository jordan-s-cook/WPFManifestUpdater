# XBAP Manifest Updater

Signing a [WPF](https://docs.microsoft.com/en-us/visualstudio/designers/introduction-to-wpf)  application is an excellent idea. Not only is it **required** for using ClickOnce deployments, but signing helps ensure authenticity of the application after publishing. 

Unfortunately, one of the issues with signing a WPF application is that the files must remain unchanged as each file is uniquely hashed during the deployment. This quickly becomes troublesome as files, especially configuration files, are designed to be modified after deployment so there is no need for re-deployment.

That is where **XBAP Manifest Updater** can help. This desktop utility can re-hash the application manifest for a WPF application. 

Features:
- Automatically creates a configuration file to save previous application.
- Allows the version of the application to be updated.
- Supports password-protected certificates.
- Supports local + network paths.

> **Note:** XBAP Manifest Updater requires  [.NET Framework](https://www.microsoft.com/net/download/windows).