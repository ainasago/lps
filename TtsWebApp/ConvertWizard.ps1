<#
.SYNOPSIS
    Web 转插件转换工具 - 交互式向导

.DESCRIPTION
    通过友好的交互式向导引导用户完成 Web 应用到插件的转换
    
.EXAMPLE
    .\ConvertWizard.ps1
#>

# 设置控制台编码
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$OutputEncoding = [System.Text.Encoding]::UTF8

# 颜色输出函数
function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Title {
    param([string]$Message)
    Write-Host ""
    Write-ColorOutput "═══════════════════════════════════════════════════════════" "Cyan"
    Write-ColorOutput "  $Message" "Cyan"
    Write-ColorOutput "═══════════════════════════════════════════════════════════" "Cyan"
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host ""
    Write-ColorOutput "📌 $Message" "Yellow"
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

# 读取用户输入（带默认值）
function Read-UserInput {
    param(
        [string]$Prompt,
        [string]$DefaultValue = "",
        [bool]$Required = $false,
        [string]$ValidationPattern = ""
    )
    
    while ($true) {
        if ($DefaultValue) {
            Write-Host "$Prompt " -NoNewline -ForegroundColor White
            Write-Host "[默认: $DefaultValue]" -NoNewline -ForegroundColor DarkGray
            Write-Host ": " -NoNewline -ForegroundColor White
        } else {
            Write-Host "${Prompt}: " -NoNewline -ForegroundColor White
        }
        
        $userInput = Read-Host
        
        # 如果用户直接回车，使用默认值
        if ([string]::IsNullOrWhiteSpace($userInput)) {
            if ($DefaultValue) {
                return $DefaultValue
            } elseif ($Required) {
                Write-Warning "此项为必填项，请输入值"
                continue
            } else {
                return ""
            }
        }
        
        # 验证输入
        if ($ValidationPattern -and $userInput -notmatch $ValidationPattern) {
            Write-Warning "输入格式不正确，请重新输入"
            continue
        }
        
        return $userInput
    }
}

# 选择路径
function Select-Path {
    param(
        [string]$Prompt,
        [string]$DefaultValue = "",
        [bool]$MustExist = $true
    )
    
    while ($true) {
        # 如果有默认值，则不要求必填
        $required = [string]::IsNullOrEmpty($DefaultValue)
        $path = Read-UserInput -Prompt $Prompt -DefaultValue $DefaultValue -Required $required
        
        if ($MustExist -and -not (Test-Path $path)) {
            Write-Warning "路径不存在: $path"
            $retry = Read-Host "是否重新输入？(Y/n)"
            if ($retry -eq "n" -or $retry -eq "N") {
                return $path
            }
            continue
        }
        
        return $path
    }
}

# 确认操作
function Confirm-Action {
    param([string]$Message)
    
    Write-Host ""
    Write-Host "$Message " -NoNewline -ForegroundColor Yellow
    Write-Host "(Y/n): " -NoNewline -ForegroundColor White
    $response = Read-Host
    
    return ($response -eq "" -or $response -eq "Y" -or $response -eq "y")
}

# 显示欢迎界面
function Show-Welcome {
    Clear-Host
    Write-ColorOutput @"

    ╔═══════════════════════════════════════════════════════════════╗
    ║                                                               ║
    ║          🚀 Web 程序转插件转换工具 - 交互式向导              ║
    ║                                                               ║
    ║                      版本: v1.0.0                             ║
    ║                                                               ║
    ╚═══════════════════════════════════════════════════════════════╝

"@ "Cyan"

    Write-Info "本向导将引导您完成 Web 应用到插件的转换过程"
    Write-Info "您可以使用默认值（直接按回车）或输入自定义值"
    Write-Host ""
    
    if (-not (Confirm-Action "准备好开始了吗？")) {
        Write-Info "已取消转换"
        exit 0
    }
}

# 收集基本信息
function Get-BasicInfo {
    Write-Title "步骤 1/6: 基本信息"
    
    Write-Info "请提供源 Web 项目的基本信息"
    Write-Host ""
    
    # 源路径
    Write-Step "源项目路径"
    Write-Info "请输入要转换的 Web 项目的完整路径"
    Write-Info "示例: C:\Projects\MyWebApp 或 ..\MyWebApp"
    Write-Info "留空则使用当前目录"
    
    $currentDir = Get-Location
    $sourcePath = Select-Path -Prompt "源项目路径" -DefaultValue $currentDir -MustExist $true
    
    # 验证是否包含项目文件
    $projectFiles = Get-ChildItem -Path $sourcePath -Filter "*.csproj" -ErrorAction SilentlyContinue
    $projectName = "MyPlugin"
    
    if ($projectFiles.Count -eq 0) {
        Write-Warning "未在该目录找到 .csproj 项目文件"
        if (-not (Confirm-Action "是否继续？")) {
            exit 1
        }
    } else {
        Write-Success "找到项目文件: $($projectFiles[0].Name)"
        # 从项目文件名提取项目名称（去掉 .csproj 扩展名和点号）
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($projectFiles[0].Name)
        # 去掉点号，例如 Project.Web -> ProjectWeb
        $projectName = $projectName -replace '\.', ''
    }
    
    return @{
        SourcePath = $sourcePath
        ProjectFile = if ($projectFiles.Count -gt 0) { $projectFiles[0].Name } else { "" }
        ProjectName = $projectName
    }
}

# 收集插件信息
function Get-PluginInfo {
    param($basicInfo)
    
    Write-Title "步骤 2/6: 插件信息"
    
    Write-Info "请提供插件的基本信息"
    Write-Host ""
    
    # 插件名称（英文）
    Write-Step "插件名称（英文标识）"
    Write-Info "用于代码命名空间和文件夹名称"
    Write-Info "只能包含字母、数字和下划线，不能以数字开头"
    Write-Info "示例: MyPlugin, UserManagement, OrderSystem"
    
    # 使用项目名称作为默认值
    $defaultPluginName = if ($basicInfo.ProjectName) { $basicInfo.ProjectName } else { "MyPlugin" }
    
    $pluginName = Read-UserInput `
        -Prompt "插件名称" `
        -DefaultValue $defaultPluginName `
        -Required $true `
        -ValidationPattern "^[a-zA-Z_][a-zA-Z0-9_]*$"
    
    # 显示名称（中文）
    Write-Step "插件显示名称"
    Write-Info "在界面上显示的名称，可以使用中文"
    Write-Info "示例: 我的插件, 用户管理, 订单系统"
    
    $displayName = Read-UserInput `
        -Prompt "显示名称" `
        -DefaultValue $pluginName
    
    # 版本号
    Write-Step "插件版本"
    Write-Info "遵循语义化版本规范 (主版本.次版本.修订号)"
    Write-Info "示例: 1.0.0, 2.1.3, 0.9.0"
    
    $version = Read-UserInput `
        -Prompt "版本号" `
        -DefaultValue "1.0.0" `
        -ValidationPattern "^\d+\.\d+\.\d+$"
    
    return @{
        PluginName = $pluginName
        DisplayName = $displayName
        Version = $version
    }
}

# 收集作者信息
function Get-AuthorInfo {
    Write-Title "步骤 3/6: 作者信息"
    
    Write-Info "请提供作者和描述信息（可选）"
    Write-Host ""
    
    # 作者
    Write-Step "作者名称"
    $author = Read-UserInput -Prompt "作者" -DefaultValue $env:USERNAME
    
    # 描述
    Write-Step "插件描述"
    Write-Info "简要描述插件的功能和用途"
    $description = Read-UserInput -Prompt "描述" -DefaultValue "一个基于 Web 应用转换的插件"
    
    # 邮箱（可选）
    Write-Step "联系邮箱（可选）"
    $email = Read-UserInput -Prompt "邮箱" -DefaultValue ""
    
    return @{
        Author = $author
        Description = $description
        Email = $email
    }
}

# 配置输出选项
function Get-OutputOptions {
    Write-Title "步骤 4/6: 输出配置"
    
    Write-Info "配置转换后的输出选项"
    Write-Host ""
    
    # 输出路径
    Write-Step "输出目录"
    Write-Info "转换后的插件将保存到此目录"
    
    $currentDir = Get-Location
    $defaultOutput = Join-Path $currentDir "output"
    
    $outputPath = Read-UserInput `
        -Prompt "输出路径" `
        -DefaultValue $defaultOutput
    
    # 是否覆盖
    $overwrite = $false
    if (Test-Path $outputPath) {
        Write-Warning "输出目录已存在"
        $overwrite = Confirm-Action "是否覆盖现有内容？"
    }
    
    return @{
        OutputPath = $outputPath
        Overwrite = $overwrite
    }
}

# 高级选项
function Get-AdvancedOptions {
    Write-Title "步骤 5/6: 高级选项"
    
    Write-Info "配置高级转换选项（可选）"
    Write-Host ""
    
    # 是否配置高级选项
    $configAdvanced = Confirm-Action "是否配置高级选项？（不配置将使用默认值）"
    
    if (-not $configAdvanced) {
        return @{
            CreateSampleData = $false
            CreateTests = $false
            CreateDocumentation = $true
            AddFreeSql = $false
        }
    }
    
    Write-Host ""
    
    # 创建示例数据
    Write-Step "示例数据"
    $createSampleData = Confirm-Action "是否创建示例数据和测试控制器？"
    
    # 创建单元测试
    Write-Step "单元测试"
    $createTests = Confirm-Action "是否创建单元测试项目？"
    
    # 创建文档
    Write-Step "文档"
    $createDocumentation = Confirm-Action "是否创建详细文档？"
    
    # 添加 FreeSql
    Write-Step "数据库支持"
    $addFreeSql = Confirm-Action "是否添加 FreeSql 数据库支持？"
    
    return @{
        CreateSampleData = $createSampleData
        CreateTests = $createTests
        CreateDocumentation = $createDocumentation
        AddFreeSql = $addFreeSql
    }
}

# 显示配置摘要
function Show-Summary {
    param($config)
    
    Write-Title "步骤 6/6: 确认配置"
    
    Write-Info "请确认以下配置信息："
    Write-Host ""
    
    Write-ColorOutput "【基本信息】" "Yellow"
    Write-Host "  源路径      : " -NoNewline; Write-ColorOutput $config.SourcePath "White"
    Write-Host "  插件名称    : " -NoNewline; Write-ColorOutput $config.PluginName "White"
    Write-Host "  显示名称    : " -NoNewline; Write-ColorOutput $config.DisplayName "White"
    Write-Host "  版本        : " -NoNewline; Write-ColorOutput $config.Version "White"
    Write-Host ""
    
    Write-ColorOutput "【作者信息】" "Yellow"
    Write-Host "  作者        : " -NoNewline; Write-ColorOutput $config.Author "White"
    Write-Host "  描述        : " -NoNewline; Write-ColorOutput $config.Description "White"
    if ($config.Email) {
        Write-Host "  邮箱        : " -NoNewline; Write-ColorOutput $config.Email "White"
    }
    Write-Host ""
    
    Write-ColorOutput "【输出配置】" "Yellow"
    Write-Host "  输出路径    : " -NoNewline; Write-ColorOutput $config.OutputPath "White"
    Write-Host "  完整路径    : " -NoNewline; Write-ColorOutput (Join-Path $config.OutputPath $config.PluginName) "DarkGray"
    Write-Host ""
    
    Write-ColorOutput "【高级选项】" "Yellow"
    Write-Host "  示例数据    : " -NoNewline; Write-ColorOutput $(if($config.CreateSampleData){"是"}else{"否"}) $(if($config.CreateSampleData){"Green"}else{"DarkGray"})
    Write-Host "  单元测试    : " -NoNewline; Write-ColorOutput $(if($config.CreateTests){"是"}else{"否"}) $(if($config.CreateTests){"Green"}else{"DarkGray"})
    Write-Host "  详细文档    : " -NoNewline; Write-ColorOutput $(if($config.CreateDocumentation){"是"}else{"否"}) $(if($config.CreateDocumentation){"Green"}else{"DarkGray"})
    Write-Host "  FreeSql     : " -NoNewline; Write-ColorOutput $(if($config.AddFreeSql){"是"}else{"否"}) $(if($config.AddFreeSql){"Green"}else{"DarkGray"})
    Write-Host ""
}

# 执行转换
function Start-Conversion {
    param($config)
    
    Write-Title "开始转换"
    
    Write-Info "正在执行转换，请稍候..."
    Write-Host ""
    
    # 构建参数
    $params = @{
        SourcePath = $config.SourcePath
        PluginName = $config.PluginName
        OutputPath = $config.OutputPath
        DisplayName = $config.DisplayName
        Version = $config.Version
        Author = $config.Author
        Description = $config.Description
    }
    
    # 调用主转换脚本
    $scriptPath = Join-Path $PSScriptRoot "WebToPluginConverter.ps1"
    
    if (-not (Test-Path $scriptPath)) {
        Write-Error "未找到转换脚本: $scriptPath"
        return $false
    }
    
    try {
        $result = & $scriptPath @params
        
        if ($result.Success) {
            Write-Host ""
            Write-Success "转换成功完成！"
            Write-Host ""
            
            # 显示统计信息
            Write-ColorOutput "【转换统计】" "Yellow"
            Write-Host "  总文件数    : " -NoNewline; Write-ColorOutput $result.TotalFiles "White"
            Write-Host "  C# 文件     : " -NoNewline; Write-ColorOutput $result.CSharpFiles "White"
            Write-Host "  Razor 文件  : " -NoNewline; Write-ColorOutput $result.RazorFiles "White"
            Write-Host ""
            
            # 后处理：创建额外内容
            if ($config.CreateSampleData) {
                Write-Info "正在创建示例数据..."
                New-SampleData -OutputPath $result.OutputPath -PluginName $config.PluginName
            }
            
            if ($config.AddFreeSql) {
                Write-Info "正在添加 FreeSql 支持..."
                Add-FreeSqlSupport -OutputPath $result.OutputPath -PluginName $config.PluginName
            }
            
            return $true
        } else {
            Write-Error "转换失败"
            return $false
        }
    }
    catch {
        Write-Error "转换过程中发生错误: $($_.Exception.Message)"
        return $false
    }
}

# 创建示例数据
function New-SampleData {
    param($OutputPath, $PluginName)
    
    $testControllerPath = Join-Path $OutputPath "Controllers\TestController.cs"
    
    $testControllerContent = @"
using Microsoft.AspNetCore.Mvc;

namespace $PluginName.Controllers
{
    [Area(ModuleDefiniation.MODULE_NAME)]
    [Route("$PluginName/[controller]/[action]")]
    public class TestController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            ViewBag.Message = "这是一个测试页面";
            ViewBag.PluginName = "$PluginName";
            ViewBag.Version = "1.0.0";
            return View();
        }

        [HttpGet]
        public IActionResult ApiTest()
        {
            return Json(new
            {
                success = true,
                message = "API 测试成功",
                plugin = "$PluginName",
                timestamp = System.DateTime.Now
            });
        }
    }
}
"@
    
    $testControllerContent | Out-File -FilePath $testControllerPath -Encoding UTF8 -Force
    Write-Success "已创建测试控制器"
}

# 添加 FreeSql 支持
function Add-FreeSqlSupport {
    param($OutputPath, $PluginName)
    
    # 创建 Data 目录和示例实体
    $dataPath = Join-Path $OutputPath "Data"
    if (-not (Test-Path $dataPath)) {
        New-Item -Path $dataPath -ItemType Directory -Force | Out-Null
    }
    
    $entityPath = Join-Path $dataPath "SampleEntity.cs"
    $entityContent = @"
using FreeSql.DataAnnotations;
using System;

namespace $PluginName.Data
{
    [Table(Name = "${PluginName}_SampleEntities")]
    public class SampleEntity
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }

        [Column(StringLength = 200)]
        public string Name { get; set; }

        [Column(StringLength = 1000)]
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
"@
    
    $entityContent | Out-File -FilePath $entityPath -Encoding UTF8 -Force
    Write-Success "已添加 FreeSql 示例实体"
}

# 显示下一步操作
function Show-NextSteps {
    param($config)
    
    Write-Title "下一步操作"
    
    $pluginPath = Join-Path $config.OutputPath $config.PluginName
    
    Write-Info "转换已完成！请按照以下步骤继续："
    Write-Host ""
    
    Write-ColorOutput "1️⃣  打开项目目录" "Yellow"
    Write-Host "   cd " -NoNewline
    Write-ColorOutput $pluginPath "Cyan"
    Write-Host ""
    
    Write-ColorOutput "2️⃣  配置项目引用" "Yellow"
    Write-Host "   编辑 $($config.PluginName).csproj"
    Write-Host "   添加 CoolCat.Core.Mvc 和 CoolCat.Core 的项目引用"
    Write-Host ""
    
    Write-ColorOutput "3️⃣  实现插件启动类" "Yellow"
    Write-Host "   编辑 PluginStartup.cs"
    Write-Host "   在 ConfigureServices 中注册服务"
    Write-Host "   在 Configure 中配置中间件"
    Write-Host ""
    
    Write-ColorOutput "4️⃣  编译项目" "Yellow"
    Write-Host "   dotnet build"
    Write-Host ""
    
    Write-ColorOutput "5️⃣  部署插件" "Yellow"
    Write-Host "   将编译输出复制到主应用的 Modules\$($config.PluginName) 目录"
    Write-Host ""
    
    Write-ColorOutput "📚 查看文档" "Yellow"
    Write-Host "   README.md           - 使用说明"
    Write-Host "   CONVERSION_REPORT.md - 转换报告"
    Write-Host ""
    
    # 询问是否打开输出目录
    if (Confirm-Action "是否在文件资源管理器中打开输出目录？") {
        Start-Process "explorer.exe" -ArgumentList $pluginPath
    }
}

# 主函数
function Main {
    try {
        # 显示欢迎界面
        Show-Welcome
        
        # 收集信息
        $basicInfo = Get-BasicInfo
        $pluginInfo = Get-PluginInfo -basicInfo $basicInfo
        $authorInfo = Get-AuthorInfo
        $outputOptions = Get-OutputOptions
        $advancedOptions = Get-AdvancedOptions
        
        # 合并配置
        $config = @{
            SourcePath = $basicInfo.SourcePath
            PluginName = $pluginInfo.PluginName
            DisplayName = $pluginInfo.DisplayName
            Version = $pluginInfo.Version
            Author = $authorInfo.Author
            Description = $authorInfo.Description
            Email = $authorInfo.Email
            OutputPath = $outputOptions.OutputPath
            Overwrite = $outputOptions.Overwrite
            CreateSampleData = $advancedOptions.CreateSampleData
            CreateTests = $advancedOptions.CreateTests
            CreateDocumentation = $advancedOptions.CreateDocumentation
            AddFreeSql = $advancedOptions.AddFreeSql
        }
        
        # 显示摘要
        Show-Summary -config $config
        
        # 确认执行
        if (-not (Confirm-Action "确认开始转换？")) {
            Write-Info "已取消转换"
            exit 0
        }
        
        # 执行转换
        $success = Start-Conversion -config $config
        
        if ($success) {
            # 显示下一步
            Show-NextSteps -config $config
            
            Write-Host ""
            Write-ColorOutput "╔═══════════════════════════════════════════════════════════╗" "Green"
            Write-ColorOutput "║              转换成功完成！感谢使用！                     ║" "Green"
            Write-ColorOutput "╚═══════════════════════════════════════════════════════════╝" "Green"
            Write-Host ""
        }
    }
    catch {
        Write-Error "发生错误: $($_.Exception.Message)"
        Write-Host $_.ScriptStackTrace
        exit 1
    }
}

# 运行主函数
Main
