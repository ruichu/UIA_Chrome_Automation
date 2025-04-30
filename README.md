# Chrome Browser UIA Tree Automation

这是一个使用C#开发的自动化工具，可以获取Chrome浏览器的UI Automation (UIA) 树结构。该工具使用FlaUI库来实现Windows UI Automation功能。

## 功能特点

- 自动启动Chrome浏览器
- 获取完整的UIA树结构
- 递归显示所有UI元素的详细信息，包括：
  - 控件类型
  - 元素名称
  - AutomationId
  - 类名
- 使用缩进方式清晰展示层级结构
- 包含完整的错误处理和资源清理机制

## 系统要求

- Windows操作系统
- .NET 9.0 或更高版本
- Google Chrome浏览器
- Visual Studio 2022或其他.NET IDE（可选）

## 依赖项

- FlaUI.Core (5.0.0)
- FlaUI.UIA3 (5.0.0)

## 安装步骤

1. 克隆仓库：
```bash
git clone https://github.com/ruichu/UIA_Chrome_Automation.git
```

2. 进入项目目录：
```bash
cd UIA_Chrome_Automation
```

3. 还原NuGet包：
```bash
dotnet restore
```

## 使用方法

1. 编译并运行程序：
```bash
dotnet run
```

2. 程序会自动：
   - 启动Chrome浏览器
   - 导航到预设的URL（默认为Google主页）
   - 获取并显示UIA树结构
   - 等待用户按任意键后退出

## 自定义设置

如果需要修改目标URL，可以在`Program.cs`文件中修改以下行：
```csharp
Arguments = "--new-window https://www.google.com" // 将URL替换为你想要的地址
```

## 输出示例

程序会以树形结构显示UI元素信息，例如：
```
+ Window 'Google Chrome' (AutomationId: ChromeWindow, Class: Chrome_WidgetWin_1)
  + Custom 'Google' (AutomationId: page, Class: Chrome_RenderWidgetHostHWND)
    + Document 'Google' (AutomationId: , Class: )
      + Group '' (AutomationId: , Class: )
        + Button 'Search' (AutomationId: search, Class: gLFyf)
```

## 注意事项

- 确保系统已安装Chrome浏览器
- 程序需要足够的权限来启动和控制Chrome浏览器
- 某些网页可能需要较长的加载时间，可以根据需要调整代码中的等待时间

## 许可证

MIT License

## 贡献

欢迎提交Issue和Pull Request来改进这个项目。

## 作者

[ruichu](https://github.com/ruichu)