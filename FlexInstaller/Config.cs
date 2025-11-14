using System;

namespace FlexInstaller
{
public static class AppConfig
{
public static string appName="Sample App";
public static string appVer="1.0.0";
public static string pubName="Your Company Name";
public static string dlUrl="https://github.com/iamsopotatoe-coder/test1/releases/download/test/XeninesBrowser.exe";
public static string instPath=@"C:\Program Files\Sample Application";
public static string exeName="XeninesBrowser.exe";
public static bool showLicense=true;
public static bool createDesktop=true;
public static bool createStartMenu=true;
public static bool runAfter=true;
public static string welcomeMsg="Welcome to the installation wizard!";
public static string licenseText=@"
END USER LICENSE AGREEMENT

This software is provided 'as is' without warranty of any kind.
By installing this software, you agree to these terms and conditions.

1. You may install and use this software on your computer.
2. You may not redistribute this software without permission.
3. The publisher is not liable for any damages caused by this software.

Do you accept these terms?";
public static string completeMsg="Installation completed successfully!";
public static bool requireAdmin=true;
public static string supportUrl="https://support.example.com";
public static string website="https://example.com";
}
}