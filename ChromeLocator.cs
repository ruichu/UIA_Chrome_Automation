using System;
using System.IO;
using Microsoft.Win32;

public static class ChromeLocator
{
    public static string GetChromeExecutablePath()
    {
        string chromePath = null;

        // 1. 尝试 HKEY_LOCAL_MACHINE (适用于为所有用户安装)
        try
        {
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"))
            {
                if (key != null)
                {
                    // (Default) 值通常包含完整路径
                    object value = key.GetValue(null);
                    if (value != null)
                    {
                        chromePath = value.ToString();
                    }
                }
            }
        }
        catch (System.Security.SecurityException) { /* 忽略权限错误 */ }
        catch (Exception ex) { Console.WriteLine($"Error reading HKLM App Paths: {ex.Message}"); }

        // 2. 如果 HKLM 中未找到，尝试 HKEY_CURRENT_USER (适用于为当前用户安装)
        if (string.IsNullOrEmpty(chromePath) || !File.Exists(chromePath))
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\App Paths\chrome.exe"))
                {
                    if (key != null)
                    {
                        object value = key.GetValue(null);
                        if (value != null)
                        {
                            chromePath = value.ToString();
                        }
                    }
                }
            }
            catch (System.Security.SecurityException) { /* 忽略权限错误 */ }
            catch (Exception ex) { Console.WriteLine($"Error reading HKCU App Paths: {ex.Message}"); }
        }

        // 3. 验证找到的路径是否存在
        if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
        {
            return chromePath;
        }

        // 4. 如果 App Paths 中没有，可以尝试作为后备手段查询 Uninstall 项
        chromePath = GetChromePathFromUninstallKeys();
        if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
        {
            return chromePath;
        }

        // 5. 如果注册表中都找不到，尝试检查默认安装位置
        chromePath = CheckDefaultInstallLocations();
        if (!string.IsNullOrEmpty(chromePath) && File.Exists(chromePath))
        {
            return chromePath;
        }

        return null; // 未找到
    }

    private static string GetChromePathFromUninstallKeys()
    {
        string installPath = null;

        // 检查 Local Machine (64-bit view and 32-bit view)
        installPath = SearchUninstallRegistry(RegistryHive.LocalMachine, RegistryView.Registry64, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (!string.IsNullOrEmpty(installPath)) return FindChromeExe(installPath);

        installPath = SearchUninstallRegistry(RegistryHive.LocalMachine, RegistryView.Registry32, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (!string.IsNullOrEmpty(installPath)) return FindChromeExe(installPath);

        // 检查 Current User
        installPath = SearchUninstallRegistry(RegistryHive.CurrentUser, RegistryView.Default, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
        if (!string.IsNullOrEmpty(installPath)) return FindChromeExe(installPath);

        return null;
    }

    private static string SearchUninstallRegistry(RegistryHive hive, RegistryView view, string uninstallKeyPath)
    {
        try
        {
            using (RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, view))
            {
                if (baseKey == null) return null;
                using (RegistryKey uninstallKey = baseKey.OpenSubKey(uninstallKeyPath))
                {
                    if (uninstallKey == null) return null;

                    foreach (string subKeyName in uninstallKey.GetSubKeyNames())
                    {
                        using (RegistryKey appKey = uninstallKey.OpenSubKey(subKeyName))
                        {
                            if (appKey != null)
                            {
                                object displayName = appKey.GetValue("DisplayName");
                                if (displayName != null && displayName.ToString().IndexOf("Google Chrome", StringComparison.OrdinalIgnoreCase) >= 0)
                                {
                                    // 优先尝试 DisplayIcon，它通常包含 exe 路径
                                    object displayIcon = appKey.GetValue("DisplayIcon");
                                    if (displayIcon != null)
                                    {
                                        string iconPath = displayIcon.ToString().Split(',')[0].Trim('"');
                                        if (File.Exists(iconPath) && Path.GetFileName(iconPath).Equals("chrome.exe", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return iconPath; // 直接返回 exe 路径
                                        }
                                    }

                                    // 其次尝试 InstallLocation
                                    object installLocation = appKey.GetValue("InstallLocation");
                                    if (installLocation != null && !string.IsNullOrEmpty(installLocation.ToString()))
                                    {
                                        return installLocation.ToString(); // 返回安装目录
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        catch (System.Security.SecurityException) { /* Ignore */ }
        catch (Exception ex) { Console.WriteLine($"Error searching Uninstall registry ({hive}, {view}): {ex.Message}"); }
        return null;
    }

    private static string FindChromeExe(string installDir)
    {
        if (string.IsNullOrEmpty(installDir)) return null;

        // Chrome 通常在安装目录下的 Application 子目录中
        string potentialPath = Path.Combine(installDir, "Application", "chrome.exe");
        if (File.Exists(potentialPath))
        {
            return potentialPath;
        }

        // 有时 installDir 本身就是 Application 目录
        potentialPath = Path.Combine(installDir, "chrome.exe");
        if (File.Exists(potentialPath))
        {
            return potentialPath;
        }

        return null;
    }

    private static string CheckDefaultInstallLocations()
    {
        string[] potentialPaths = {
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe"),
             Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Google", "Chrome", "Application", "chrome.exe") // User install
         };

        foreach (string path in potentialPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        return null;
    }
}