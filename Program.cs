using System;
using System.Threading;
using System.Diagnostics;
using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Exceptions;
using FlaUI.UIA3.Patterns;
using System.Collections.Generic;
using System.Linq;

namespace UIA_Chrome_Automation
{
    class Program
    {
        static void Main(string[] args)
        {
            // 启动Chrome浏览器
            var processStartInfo = new ProcessStartInfo
            {
                FileName = ChromeLocator.GetChromeExecutablePath(),
                Arguments = "--force-renderer-accessibility --new-window https://www.google.com"
            };

            try
            {
                FlaUI.Core.Application.Launch(processStartInfo);
                // 等待浏览器加载
                Thread.Sleep(3000);

                var document = FindBrowserDocument();
                // 遍历并打印UIA树
                PrintAutomationElementTree(document, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine("请确保已安装Chrome浏览器，并且可以正常启动。");
                Console.ReadKey();
            }
        }

        // 获取当前所有Chrome窗口的进程ID
        private static List<IntPtr> GetCurrentChromeWindows()
        {
            return Process.GetProcessesByName("chrome")
                .Where(p => p.MainWindowHandle != IntPtr.Zero)
                .Select(p => p.MainWindowHandle)
                .ToList();
        }

        public static AutomationElement FindBrowserDocument(int timeoutMilliseconds = 3000)
        {
            AutomationElement found = null;
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Use UIA3Automation for better support of modern apps like Chrome
            var automation = new UIA3Automation();
            while (stopwatch.ElapsedMilliseconds < timeoutMilliseconds)
            {
                var ChromeWindows = GetCurrentChromeWindows();
                for (int i = 0; i < ChromeWindows.Count; i++)
                {
                    var window = automation.FromHandle(ChromeWindows[i]);
                    // Skip windows that are likely not interactable
                    if (window.IsOffscreen) // Basic checks
                    {
                        continue;
                    }
                    try
                    {
                        var cfWindow = window.ConditionFactory; // Use condition factory from the window element

                        ConditionBase condition = new FlaUI.Core.Conditions.AndCondition(
                           cfWindow.ByControlType(ControlType.Document),
                           cfWindow.ByClassName("Chrome_RenderWidgetHostHWND")
                        );

                        var documentWindow = window.FindFirstChild(condition);
                        if (documentWindow != null)
                        {
                            found = documentWindow;
                            break; // Exit the for loop
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log other potential errors during UIA interaction
                        Console.WriteLine($"Error inspecting window '{window?.Name ?? "N/A"}': {ex.Message}");
                    }
                    finally
                    {
                    }
                }

                if (found != null)
                {
                    break; // Exit the while loop
                }
                Thread.Sleep(300); // Wait a bit before the next scan
            } // End while loop
            stopwatch.Stop();
            return found; // Return the found Window element or null
        }

        /// <summary>
        /// 递归打印自动化元素树
        /// </summary>
        /// <param name="element">自动化元素</param>
        /// <param name="level">缩进级别</param>
        private static void PrintAutomationElementTree(AutomationElement element, int level)
        {
            // 创建缩进
            string indent = new string(' ', level * 2);

            // 打印当前元素的信息
            string name = element.Name;
            //string automationId = element.AutomationId;

            string className = "";
            try
            {
                className = element.ClassName;
            }
            catch (Exception ex)
            {
                className = $"[Error: {ex.Message}]";
            }
            string controlType = element.ControlType.ToString();

            Console.WriteLine($"{indent}+ {controlType} '{name}'");

            // 递归处理所有子元素
            //foreach (var child in element.FindAllDescendants())
            foreach (var child in element.FindAllChildren())
            {
                PrintAutomationElementTree(child, level + 1);
            }
        }
    }
}