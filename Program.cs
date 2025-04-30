using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.UIA3;
using System.Diagnostics;

namespace UIA_Chrome_Automation
{
    class Program
    {
        static void Main(string[] args)
        {
            // 启动Chrome浏览器
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "chrome.exe",
                Arguments = "--new-window https://www.google.com" // 可以修改为其他URL
            };

            try
            {
                using (var process = Process.Start(processStartInfo))
                {
                    Console.WriteLine("正在启动Chrome浏览器...");
                    // 等待浏览器加载
                    Thread.Sleep(3000);

                    // 初始化UI自动化
                    using (var automation = new UIA3Automation())
                    {
                        // 获取浏览器窗口
                        var app = Application.Attach(process);
                        var window = app.GetMainWindow(automation);

                        if (window != null)
                        {
                            Console.WriteLine("成功获取Chrome窗口！");
                            Console.WriteLine("\n开始遍历UIA树结构：");
                            Console.WriteLine("===================");

                            // 遍历并打印UIA树
                            PrintAutomationElementTree(window, 0);
                        }
                        else
                        {
                            Console.WriteLine("无法获取Chrome窗口。");
                        }
                    }

                    Console.WriteLine("\n按任意键退出...");
                    Console.ReadKey();

                    // 关闭浏览器
                    try
                    {
                        process.CloseMainWindow();
                        if (!process.WaitForExit(5000))
                        {
                            process.Kill();
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"关闭浏览器时出错: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发生错误: {ex.Message}");
                Console.WriteLine("请确保已安装Chrome浏览器，并且可以正常启动。");
                Console.ReadKey();
            }
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
            string automationId = element.AutomationId;
            string className = element.ClassName;
            string controlType = element.ControlType.ToString();

            Console.WriteLine($"{indent}+ {controlType} '{name}' (AutomationId: {automationId}, Class: {className})");

            // 递归处理所有子元素
            foreach (var child in element.FindAllChildren())
            {
                PrintAutomationElementTree(child, level + 1);
            }
        }
    }
}