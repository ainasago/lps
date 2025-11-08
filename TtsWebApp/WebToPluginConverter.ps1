<#
.SYNOPSIS
    将普通 Web 程序转换为插件格式

.DESCRIPTION
    此脚本可以将一个标准的 ASP.NET Core Web 应用程序转换为符合插件系统规范的插件项目。
    
.PARAMETER SourcePath
    源 Web 项目的路径
    
.PARAMETER PluginName
    插件名称（如 MyPlugin）
    
.PARAMETER OutputPath
    输出目录（默认为当前目录的 output 文件夹）
    
.PARAMETER DisplayName
    插件显示名称（默认与 PluginName 相同）
    
.PARAMETER Version
    插件版本（默认 1.0.0）
    
.PARAMETER Author
    作者名称
    
.PARAMETER Description
    插件描述
    
.EXAMPLE
    .\WebToPluginConverter.ps1 -SourcePath "C:\MyWebApp" -PluginName "MyPlugin"
    
.EXAMPLE
    .\WebToPluginConverter.ps1 -SourcePath "C:\MyWebApp" -PluginName "MyPlugin" -OutputPath "D:\Plugins" -DisplayName "我的插件"
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$SourcePath,
    
    [Parameter(Mandatory=$true)]
    [string]$PluginName,
    
    [Parameter(Mandatory=$false)]
    [string]$OutputPath = ".\output",
    
    [Parameter(Mandatory=$false)]
    [string]$DisplayName = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Version = "1.0.0",
    
    [Parameter(Mandatory=$false)]
    [string]$Author = "",
    
    [Parameter(Mandatory=$false)]
    [string]$Description = ""
)

# 设置错误处理
$ErrorActionPreference = "Stop"

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Success {
    param([string]$Message)
    Write-ColorOutput "✅ $Message" "Green"
}

function Write-Info {
    param([string]$Message)
    Write-ColorOutput "ℹ️  $Message" "Cyan"
}

function Write-Warning {
    param([string]$Message)
    Write-ColorOutput "⚠️  $Message" "Yellow"
}

function Write-Error {
    param([string]$Message)
    Write-ColorOutput "❌ $Message" "Red"
}

function Write-Step {
    param([string]$Message)
    Write-ColorOutput "`n📌 $Message" "Magenta"
}

# 开始转换
Write-ColorOutput "`n╔════════════════════════════════════════════════════════════╗" "Cyan"
Write-ColorOutput "║          Web 程序转插件转换工具 v1.0.0                    ║" "Cyan"
Write-ColorOutput "╚════════════════════════════════════════════════════════════╝`n" "Cyan"

# 验证源路径
Write-Step "验证源路径"
if (-not (Test-Path $SourcePath)) {
    Write-Error "源路径不存在: $SourcePath"
    exit 1
}
Write-Success "源路径有效: $SourcePath"

# 查找项目文件
$projectFiles = Get-ChildItem -Path $SourcePath -Filter "*.csproj" -Recurse
if ($projectFiles.Count -eq 0) {
    Write-Error "未找到 .csproj 项目文件"
    exit 1
}

$sourceProjectFile = $projectFiles[0].FullName
Write-Success "找到项目文件: $($projectFiles[0].Name)"

# 设置默认值
if ([string]::IsNullOrEmpty($DisplayName)) {
    $DisplayName = $PluginName
}

# 创建输出目录
Write-Step "创建输出目录"
$pluginOutputPath = Join-Path $OutputPath $PluginName
if (Test-Path $pluginOutputPath) {
    Write-Warning "输出目录已存在，将被清空"
    Remove-Item -Path $pluginOutputPath -Recurse -Force
}
New-Item -Path $pluginOutputPath -ItemType Directory -Force | Out-Null
Write-Success "输出目录已创建: $pluginOutputPath"

# 复制源文件
Write-Step "复制源文件"

# 排除的目录（更全面）
$excludeDirs = @(
    "bin", "obj", ".vs", ".git", ".svn", 
    "node_modules", "packages", ".vscode", ".idea",
    "TestResults", "Debug", "Release",
    "output", "dist", "build"
)

# 排除的文件模式（更全面）
$excludeFiles = @(
    "*.user", "*.suo", "*.cache", "*.log",
    "*.tmp", "*.temp", "*.bak", "*.old",
    "*.dll", "*.pdb", "*.exe",
    "*.ps1", "*.bat", "*.sh"  # 排除脚本文件
)

Write-Info "正在复制文件（排除 bin、obj 等目录）..."

# 使用 robocopy 或优化的复制逻辑
$fileCount = 0
$skippedCount = 0

Get-ChildItem -Path $SourcePath -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $relativePath = $_.FullName.Substring($SourcePath.Length)
    
    # 检查是否在排除目录中（优化：提前检查目录）
    $shouldExclude = $false
    
    # 如果是目录，检查是否应该排除
    if ($_.PSIsContainer) {
        foreach ($excludeDir in $excludeDirs) {
            if ($_.Name -eq $excludeDir -or $relativePath -like "*\$excludeDir\*" -or $relativePath -like "*/$excludeDir/*") {
                $shouldExclude = $true
                $skippedCount++
                break
            }
        }
    } else {
        # 如果是文件，检查父目录是否在排除列表中
        foreach ($excludeDir in $excludeDirs) {
            if ($relativePath -like "*\$excludeDir\*" -or $relativePath -like "*/$excludeDir/*") {
                $shouldExclude = $true
                $skippedCount++
                break
            }
        }
        
        # 检查文件模式
        if (-not $shouldExclude) {
            foreach ($excludeFile in $excludeFiles) {
                if ($_.Name -like $excludeFile) {
                    $shouldExclude = $true
                    $skippedCount++
                    break
                }
            }
        }
    }
    
    if (-not $shouldExclude) {
        $targetPath = Join-Path $pluginOutputPath $relativePath
        
        if ($_.PSIsContainer) {
            if (-not (Test-Path $targetPath)) {
                New-Item -Path $targetPath -ItemType Directory -Force | Out-Null
            }
        } else {
            $targetDir = Split-Path $targetPath -Parent
            if (-not (Test-Path $targetDir)) {
                New-Item -Path $targetDir -ItemType Directory -Force | Out-Null
            }
            Copy-Item -Path $_.FullName -Destination $targetPath -Force
            $fileCount++
            
            # 每复制 50 个文件显示一次进度
            if ($fileCount % 50 -eq 0) {
                Write-Host "." -NoNewline
            }
        }
    }
}

Write-Host ""
Write-Success "源文件已复制 (复制: $fileCount 个文件, 跳过: $skippedCount 项)"

# 创建插件配置文件
Write-Step "创建插件配置文件"

# 1. plugin.json
$pluginJson = @{
    name = $PluginName
    uniqueKey = $PluginName
    displayName = $DisplayName
    version = $Version
    author = $Author
    description = $Description
} | ConvertTo-Json -Depth 10

$pluginJsonPath = Join-Path $pluginOutputPath "plugin.json"
$pluginJson | Out-File -FilePath $pluginJsonPath -Encoding UTF8
Write-Success "已创建 plugin.json"

# 2. ModuleDefiniation.cs
$moduleDefiniationContent = @"
namespace $PluginName
{
    public class ModuleDefiniation : CoolCat.Core.Models.ModuleDefiniation
    {
        public const string MODULE_NAME = "$PluginName";

        public ModuleDefiniation() : base(MODULE_NAME)
        {

        }
    }
}
"@

$moduleDefiniationPath = Join-Path $pluginOutputPath "ModuleDefiniation.cs"
$moduleDefiniationContent | Out-File -FilePath $moduleDefiniationPath -Encoding UTF8
Write-Success "已创建 ModuleDefiniation.cs"

# 3. PluginStartup.cs
$pluginStartupContent = @"
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using CoolCat.Core.Mvc;
using System;

namespace $PluginName
{
    /// <summary>
    /// 插件启动类
    /// </summary>
    public class PluginStartup : IPluginStartup
    {
        /// <summary>
        /// 配置服务
        /// </summary>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("🔧 [$PluginName] ConfigureServices 被调用");
            
            // TODO: 在这里注册插件的服务
            // 例如：
            // services.AddScoped<IMyService, MyService>();
            // services.Configure<MySettings>(configuration.GetSection("$PluginName"));
            
            Console.WriteLine("✅ [$PluginName] 服务注册完成");
        }

        /// <summary>
        /// 配置中间件
        /// </summary>
        public void Configure(IApplicationBuilder app, IConfiguration configuration)
        {
            Console.WriteLine("🔧 [$PluginName] Configure 被调用");
            
            // TODO: 在这里配置插件的中间件
            // 例如：
            // app.Use(async (context, next) =>
            // {
            //     if (context.Request.Path.StartsWithSegments("/$PluginName"))
            //     {
            //         context.Response.Headers["X-Plugin"] = "$PluginName";
            //     }
            //     await next();
            // });
            
            Console.WriteLine("✅ [$PluginName] 中间件配置完成");
        }

        /// <summary>
        /// 插件启动优先级
        /// 数字越小越先执行，默认为 100
        /// </summary>
        public int Order => 100;
    }
}
"@

$pluginStartupPath = Join-Path $pluginOutputPath "PluginStartup.cs"
$pluginStartupContent | Out-File -FilePath $pluginStartupPath -Encoding UTF8
Write-Success "已创建 PluginStartup.cs"

# 4. 修改项目文件
Write-Step "修改项目文件"

$newProjectContent = @"
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <OutputType>Library</OutputType>
    <CopyRefAssembliesToPublishDirectory>true</CopyRefAssembliesToPublishDirectory>
    <PreserveCompilationReferences>true</PreserveCompilationReferences>
    <PreserveCompilationContext>true</PreserveCompilationContext>
    <AspNetCoreHostingModel>InProcess</AspNetCoreHostingModel>
    <MvcRazorCompileOnBuild>false</MvcRazorCompileOnBuild>
    <MvcRazorCompileOnPublish>false</MvcRazorCompileOnPublish>
    <RazorCompileOnBuild>false</RazorCompileOnBuild>
    <RazorCompileOnPublish>false</RazorCompileOnPublish>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <PropertyGroup Condition="'`$(Configuration)|`$(Platform)'=='Debug|AnyCPU'">
    <OutputPath></OutputPath>
  </PropertyGroup>

  <ItemGroup>
    <!-- 插件必需的项目引用 -->
    <!-- 请根据实际路径调整 -->
    <!-- <ProjectReference Include="..\..\CoolCat.Core.Mvc\CoolCat.Core.Mvc.csproj" /> -->
    <!-- <ProjectReference Include="..\..\CoolCat.Core\CoolCat.Core.csproj" /> -->
  </ItemGroup>

</Project>
"@

$projectFilePath = Join-Path $pluginOutputPath "$PluginName.csproj"
# 如果存在旧的项目文件，先删除
Get-ChildItem -Path $pluginOutputPath -Filter "*.csproj" | Remove-Item -Force
$newProjectContent | Out-File -FilePath $projectFilePath -Encoding UTF8
Write-Success "已创建项目文件: $PluginName.csproj"

# 5. 处理 Controllers
Write-Step "处理 Controllers"
$controllersPath = Join-Path $pluginOutputPath "Controllers"
if (Test-Path $controllersPath) {
    Get-ChildItem -Path $controllersPath -Filter "*.cs" | ForEach-Object {
        $content = Get-Content $_.FullName -Raw -Encoding UTF8
        
        # 添加 Area 属性
        if ($content -notmatch '\[Area\(') {
            $content = $content -replace '(public\s+class\s+\w+Controller\s*:\s*Controller)', "[Area(ModuleDefiniation.MODULE_NAME)]`r`n    `$1"
        }
        
        # 更新命名空间
        $content = $content -replace 'namespace\s+[\w\.]+', "namespace $PluginName.Controllers"
        
        $content | Out-File -FilePath $_.FullName -Encoding UTF8 -NoNewline
    }
    Write-Success "已处理 Controllers"
} else {
    Write-Warning "未找到 Controllers 目录"
}

# 6. 处理 Views
Write-Step "处理 Views"
$viewsPath = Join-Path $pluginOutputPath "Views"
if (Test-Path $viewsPath) {
    # 在 Views 目录下创建 _ViewImports.cshtml
    $viewImportsContent = @"
@using $PluginName
@using $PluginName.Models
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
"@
    $viewImportsPath = Join-Path $viewsPath "_ViewImports.cshtml"
    $viewImportsContent | Out-File -FilePath $viewImportsPath -Encoding UTF8
    Write-Success "已创建 _ViewImports.cshtml"
} else {
    Write-Warning "未找到 Views 目录"
}

# 7. 创建 README.md
Write-Step "创建文档"
$readmeContent = @"
# $DisplayName

## 📋 插件信息

- **插件名称**: $DisplayName
- **唯一标识**: $PluginName
- **版本**: $Version
- **作者**: $Author
- **描述**: $Description

## 🚀 快速开始

### 1. 配置项目引用

编辑 ``$PluginName.csproj``，添加正确的项目引用：

``````xml
<ItemGroup>
  <ProjectReference Include="..\..\CoolCat.Core.Mvc\CoolCat.Core.Mvc.csproj" />
  <ProjectReference Include="..\..\CoolCat.Core\CoolCat.Core.csproj" />
</ItemGroup>
``````

### 2. 实现 PluginStartup

在 ``PluginStartup.cs`` 中：

- **ConfigureServices**: 注册服务、配置选项
- **Configure**: 配置中间件、路由

### 3. 编译插件

``````bash
dotnet build
``````

### 4. 部署插件

将编译输出复制到主应用的 ``Modules/$PluginName`` 目录。

## 📁 目录结构

``````
$PluginName/
├── Controllers/          # 控制器
├── Views/               # 视图
├── Models/              # 模型
├── Services/            # 服务
├── Data/                # 数据访问
├── wwwroot/             # 静态资源
├── plugin.json          # 插件配置
├── ModuleDefiniation.cs # 模块定义
├── PluginStartup.cs     # 启动类
└── $PluginName.csproj   # 项目文件
``````

## ⚙️ 配置说明

### plugin.json

``````json
{
  "name": "$PluginName",
  "uniqueKey": "$PluginName",
  "displayName": "$DisplayName",
  "version": "$Version"
}
``````

### appsettings.plugin.json (可选)

``````json
{
  "$PluginName": {
    "Setting1": "Value1",
    "Setting2": "Value2"
  }
}
``````

## 🔧 开发指南

### Controllers

所有控制器必须添加 ``[Area]`` 属性：

``````csharp
[Area(ModuleDefiniation.MODULE_NAME)]
public class MyController : Controller
{
    // ...
}
``````

### 路由

使用 Area 路由：

``````
/$PluginName/ControllerName/ActionName
``````

### 依赖注入

在 ``PluginStartup.ConfigureServices`` 中注册服务：

``````csharp
services.AddScoped<IMyService, MyService>();
``````

## 📝 注意事项

1. ✅ 确保所有 Controller 都有 ``[Area]`` 属性
2. ✅ 命名空间使用 ``$PluginName.*``
3. ✅ 项目引用路径正确
4. ✅ 输出类型为 ``Library``
5. ✅ Razor 编译选项正确设置

## 🐛 故障排除

### 问题 1: 插件无法加载

**解决方案:**
- 检查 ``plugin.json`` 格式
- 确认项目引用正确
- 查看编译输出是否有错误

### 问题 2: 路由不工作

**解决方案:**
- 确认 ``[Area]`` 属性已添加
- 检查路由格式
- 查看 ``PluginStartup.Configure``

### 问题 3: 视图找不到

**解决方案:**
- 确认 ``_ViewImports.cshtml`` 存在
- 检查视图路径
- 确认 Razor 编译选项

## 📚 相关文档

- [插件开发指南](../docs/plugin-development.md)
- [API 参考](../docs/api-reference.md)

---

**转换时间**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**转换工具**: WebToPluginConverter v1.0.0
"@

$readmePath = Join-Path $pluginOutputPath "README.md"
$readmeContent | Out-File -FilePath $readmePath -Encoding UTF8
Write-Success "已创建 README.md"

# 8. 创建转换报告
Write-Step "生成转换报告"

$reportContent = @"
# 转换报告

## 基本信息

- **源路径**: $SourcePath
- **插件名称**: $PluginName
- **显示名称**: $DisplayName
- **版本**: $Version
- **输出路径**: $pluginOutputPath
- **转换时间**: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## 文件统计

"@

# 统计文件
$totalFiles = (Get-ChildItem -Path $pluginOutputPath -Recurse -File).Count
$csFiles = (Get-ChildItem -Path $pluginOutputPath -Recurse -Filter "*.cs").Count
$cshtmlFiles = (Get-ChildItem -Path $pluginOutputPath -Recurse -Filter "*.cshtml").Count
$jsonFiles = (Get-ChildItem -Path $pluginOutputPath -Recurse -Filter "*.json").Count

$reportContent += @"
- **总文件数**: $totalFiles
- **C# 文件**: $csFiles
- **Razor 视图**: $cshtmlFiles
- **JSON 文件**: $jsonFiles

## 创建的文件

- ✅ plugin.json
- ✅ ModuleDefiniation.cs
- ✅ PluginStartup.cs
- ✅ $PluginName.csproj
- ✅ README.md
"@

if (Test-Path (Join-Path $viewsPath "_ViewImports.cshtml")) {
    $reportContent += "`n- ✅ Views/_ViewImports.cshtml"
}

$reportContent += @"

## 需要手动处理的项目

### 1. 项目引用
编辑 ``$PluginName.csproj``，添加正确的项目引用路径。

### 2. 命名空间
检查并更新所有文件的命名空间为 ``$PluginName.*``。

### 3. Controllers
确认所有 Controller 都添加了 ``[Area(ModuleDefiniation.MODULE_NAME)]`` 属性。

### 4. 依赖注入
在 ``PluginStartup.ConfigureServices`` 中注册所需的服务。

### 5. 配置文件
如果需要配置，创建 ``appsettings.plugin.json``。

### 6. 数据库
如果使用数据库，在 ``PluginStartup`` 中配置数据库连接。

## 下一步

1. 打开 ``$pluginOutputPath``
2. 编辑 ``$PluginName.csproj`` 配置项目引用
3. 实现 ``PluginStartup.cs`` 中的服务注册和中间件配置
4. 测试编译: ``dotnet build``
5. 部署到主应用的 Modules 目录

## 注意事项

⚠️ 此转换工具执行基本的结构转换，某些高级功能可能需要手动调整：

- 复杂的依赖注入
- 自定义中间件
- 数据库迁移
- 静态资源处理
- 第三方库集成

---

**转换成功！** 🎉
"@

$reportPath = Join-Path $pluginOutputPath "CONVERSION_REPORT.md"
$reportContent | Out-File -FilePath $reportPath -Encoding UTF8
Write-Success "已生成转换报告: CONVERSION_REPORT.md"

# 完成
Write-ColorOutput "`n╔════════════════════════════════════════════════════════════╗" "Green"
Write-ColorOutput "║                  转换完成！                                ║" "Green"
Write-ColorOutput "╚════════════════════════════════════════════════════════════╝`n" "Green"

Write-Info "输出目录: $pluginOutputPath"
Write-Info "请查看 CONVERSION_REPORT.md 了解详细信息"
Write-Info "请查看 README.md 了解使用说明"

Write-ColorOutput "`n📋 下一步操作:" "Yellow"
Write-ColorOutput "  1. cd $pluginOutputPath" "White"
Write-ColorOutput "  2. 编辑 $PluginName.csproj 配置项目引用" "White"
Write-ColorOutput "  3. 实现 PluginStartup.cs" "White"
Write-ColorOutput "  4. dotnet build" "White"
Write-ColorOutput "  5. 部署到 Modules/$PluginName 目录`n" "White"

# 返回统计信息
return @{
    Success = $true
    PluginName = $PluginName
    OutputPath = $pluginOutputPath
    TotalFiles = $totalFiles
    CSharpFiles = $csFiles
    RazorFiles = $cshtmlFiles
}
