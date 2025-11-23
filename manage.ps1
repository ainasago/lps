# TTS 项目管理工具 PowerShell 版本
# 设置控制台编码为 UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8
$Host.UI.RawUI.WindowTitle = "TTS 项目管理工具"

function Show-Menu {
    Clear-Host
    Write-Host "╔════════════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
    Write-Host "║                    TTS 项目管理工具 v1.0                      ║" -ForegroundColor Cyan
    Write-Host "╚════════════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
    Write-Host ""
    Write-Host " 【编译构建】" -ForegroundColor Yellow
    Write-Host " [1] 编译 API 项目"
    Write-Host " [2] 编译 Web 项目"
    Write-Host " [3] 编译所有项目"
    Write-Host ""
    Write-Host " 【开发运行】" -ForegroundColor Yellow
    Write-Host " [4] 启动 API（开发模式）"
    Write-Host " [5] 启动 Web（开发模式）"
    Write-Host " [6] 启动所有服务（开发模式）"
    Write-Host ""
    Write-Host " 【发布部署】" -ForegroundColor Yellow
    Write-Host " [7] 发布 API（Release）"
    Write-Host " [8] 发布 Web（Release + JS混淆）"
    Write-Host " [9] 发布所有项目（Release + JS混淆）"
    Write-Host ""
    Write-Host " 【维护工具】" -ForegroundColor Yellow
    Write-Host " [A] 清理编译文件"
    Write-Host " [B] 恢复 JS 原文件"
    Write-Host " [C] 测试 JS 混淆效果"
    Write-Host " [D] 查看项目信息"
    Write-Host " [E] 删除数据库（重置）"
    Write-Host ""
    Write-Host " [0] 退出"
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
}

function Build-Api {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 正在编译 TtsWebApi..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApi"
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ API 编译成功！" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ API 编译失败！" -ForegroundColor Red
    }
    Write-Host ""
    Read-Host "按回车键继续"
}

function Build-Web {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 正在编译 TtsWebApp..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApp"
    dotnet build
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Web 编译成功！" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Web 编译失败！" -ForegroundColor Red
    }
    Write-Host ""
    Read-Host "按回车键继续"
}

function Build-All {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 正在编译所有项目..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "[1/2] 编译 TtsWebApi..." -ForegroundColor Yellow
    Set-Location "$PSScriptRoot\TtsWebApi"
    dotnet build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ API 编译失败！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    Write-Host "✅ API 编译成功！" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "[2/2] 编译 TtsWebApp..." -ForegroundColor Yellow
    Set-Location "$PSScriptRoot\TtsWebApp"
    dotnet build
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Web 编译失败！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    Write-Host "✅ Web 编译成功！" -ForegroundColor Green
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ 所有项目编译完成！" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Read-Host "按回车键继续"
}

function Run-Api {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 启动 TtsWebApi（开发模式）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\TtsWebApi'; dotnet run"
    
    Write-Host ""
    Write-Host "✅ API 正在启动..." -ForegroundColor Green
    Write-Host "📍 地址: http://localhost:5275" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Run-Web {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 启动 TtsWebApp（开发模式）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\TtsWebApp'; dotnet run"
    
    Write-Host ""
    Write-Host "✅ Web 正在启动..." -ForegroundColor Green
    Write-Host "📍 地址: http://localhost:5261" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Run-All {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 启动所有服务（开发模式）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "[1/2] 启动 TtsWebApi..." -ForegroundColor Yellow
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\TtsWebApi'; dotnet run"
    Write-Host "✅ API 正在启动..." -ForegroundColor Green
    Write-Host ""
    
    Write-Host "[2/2] 启动 TtsWebApp..." -ForegroundColor Yellow
    Start-Process pwsh -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\TtsWebApp'; dotnet run"
    Write-Host "✅ Web 正在启动..." -ForegroundColor Green
    Write-Host ""
    
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ 所有服务正在启动..." -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📍 TtsWebApi: http://localhost:5275" -ForegroundColor Cyan
    Write-Host "📍 TtsWebApp: http://localhost:5261" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Publish-Api {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 发布 TtsWebApi（Release）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApi"
    dotnet publish -c Release -o ./publish
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ API 发布成功！" -ForegroundColor Green
        Write-Host "📁 输出目录: $PSScriptRoot\TtsWebApi\publish" -ForegroundColor Cyan
    } else {
        Write-Host ""
        Write-Host "❌ API 发布失败！" -ForegroundColor Red
    }
    Write-Host ""
    Read-Host "按回车键继续"
}

function Publish-Web {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 发布 TtsWebApp（Release + JS混淆）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApp"
    
    # 检查 Node.js
    $nodeExists = Get-Command node -ErrorAction SilentlyContinue
    if (-not $nodeExists) {
        Write-Host "⚠️  未检测到 Node.js，跳过 JS 混淆..." -ForegroundColor Yellow
        Write-Host ""
        dotnet publish -c Release -o ./publish
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "✅ Web 发布成功！" -ForegroundColor Green
            Write-Host "📁 输出目录: $PSScriptRoot\TtsWebApp\publish" -ForegroundColor Cyan
        }
        Read-Host "按回车键继续"
        return
    }
    
    # 检查 npm 依赖
    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 首次发布，正在安装 npm 依赖..." -ForegroundColor Yellow
        npm install
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ npm 依赖安装失败！" -ForegroundColor Red
            Read-Host "按回车键继续"
            return
        }
    }
    
    Write-Host ""
    Write-Host "[1/3] 混淆 JavaScript 文件..." -ForegroundColor Yellow
    npm run obfuscate:prod
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ JS 混淆失败！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    Write-Host "✅ JS 混淆完成！" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "[2/3] 发布项目..." -ForegroundColor Yellow
    dotnet publish -c Release -o ./publish
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ 项目发布失败！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    
    Write-Host ""
    Write-Host "[3/3] 恢复原始 JS 文件..." -ForegroundColor Yellow
    npm run restore
    Write-Host "✅ 原始文件已恢复！" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ Web 发布成功！" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📁 输出目录: $PSScriptRoot\TtsWebApp\publish" -ForegroundColor Cyan
    Write-Host "🔒 JS 文件已混淆保护" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Publish-All {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 发布所有项目（Release + JS混淆）..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "┌────────────────────────────────────────────────────────────┐" -ForegroundColor Cyan
    Write-Host "│ [1/2] 发布 TtsWebApi...                                    │" -ForegroundColor Cyan
    Write-Host "└────────────────────────────────────────────────────────────┘" -ForegroundColor Cyan
    Set-Location "$PSScriptRoot\TtsWebApi"
    dotnet publish -c Release -o ./publish
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ API 发布失败！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    Write-Host "✅ API 发布成功！" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "┌────────────────────────────────────────────────────────────┐" -ForegroundColor Cyan
    Write-Host "│ [2/2] 发布 TtsWebApp（含 JS 混淆）...                     │" -ForegroundColor Cyan
    Write-Host "└────────────────────────────────────────────────────────────┘" -ForegroundColor Cyan
    Set-Location "$PSScriptRoot\TtsWebApp"
    
    $nodeExists = Get-Command node -ErrorAction SilentlyContinue
    if (-not $nodeExists) {
        Write-Host "⚠️  未检测到 Node.js，跳过 JS 混淆..." -ForegroundColor Yellow
        dotnet publish -c Release -o ./publish
    } else {
        if (-not (Test-Path "node_modules")) {
            Write-Host "📦 安装 npm 依赖..." -ForegroundColor Yellow
            npm install
        }
        
        Write-Host "🔒 混淆 JavaScript..." -ForegroundColor Yellow
        npm run obfuscate:prod
        
        Write-Host "📦 发布项目..." -ForegroundColor Yellow
        dotnet publish -c Release -o ./publish
        
        Write-Host "🔄 恢复原始文件..." -ForegroundColor Yellow
        npm run restore
    }
    
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ 所有项目发布完成！" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📁 API 输出: $PSScriptRoot\TtsWebApi\publish" -ForegroundColor Cyan
    Write-Host "📁 Web 输出: $PSScriptRoot\TtsWebApp\publish" -ForegroundColor Cyan
    Write-Host "🔒 JS 文件已混淆保护" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Clean-Files {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 清理编译文件..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    
    $confirm = Read-Host "⚠️  确定要清理所有编译文件吗？(Y/N)"
    if ($confirm -ne "Y" -and $confirm -ne "y") {
        return
    }
    
    Write-Host ""
    Write-Host "清理 TtsWebApi..." -ForegroundColor Yellow
    Set-Location "$PSScriptRoot\TtsWebApi"
    Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ API 清理完成" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "清理 TtsWebApp..." -ForegroundColor Yellow
    Set-Location "$PSScriptRoot\TtsWebApp"
    Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "publish" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "wwwroot\js\obfuscated" -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item -Path "wwwroot\js\backup" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "✅ Web 清理完成" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ 清理完成！" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Read-Host "按回车键继续"
}

function Restore-Js {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 恢复 JavaScript 原文件..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApp"
    
    if (-not (Test-Path "wwwroot\js\backup")) {
        Write-Host "⚠️  没有找到备份文件！" -ForegroundColor Yellow
        Read-Host "按回车键继续"
        return
    }
    
    $nodeExists = Get-Command node -ErrorAction SilentlyContinue
    if (-not $nodeExists) {
        Write-Host "❌ 未检测到 Node.js！" -ForegroundColor Red
        Read-Host "按回车键继续"
        return
    }
    
    npm run restore
    Write-Host ""
    Write-Host "✅ 原始文件已恢复！" -ForegroundColor Green
    Read-Host "按回车键继续"
}

function Test-Obfuscate {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 测试 JavaScript 混淆效果..." -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    
    Set-Location "$PSScriptRoot\TtsWebApp"
    
    $nodeExists = Get-Command node -ErrorAction SilentlyContinue
    if (-not $nodeExists) {
        Write-Host "❌ 未检测到 Node.js！" -ForegroundColor Red
        Write-Host "📥 请先安装 Node.js: https://nodejs.org/" -ForegroundColor Yellow
        Read-Host "按回车键继续"
        return
    }
    
    if (-not (Test-Path "node_modules")) {
        Write-Host "📦 首次使用，正在安装 npm 依赖..." -ForegroundColor Yellow
        npm install
    }
    
    Write-Host ""
    npm run obfuscate
    Write-Host ""
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "✅ 混淆测试完成！" -ForegroundColor Green
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host "📁 混淆文件位置: wwwroot\js\obfuscated\" -ForegroundColor Cyan
    Write-Host "💡 提示: 原文件未被修改，可以安全查看混淆效果" -ForegroundColor Yellow
    Write-Host ""
    Read-Host "按回车键继续"
}

function Show-Info {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 项目信息" -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "📦 项目名称: TTS 语音转换系统" -ForegroundColor Cyan
    Write-Host "📂 项目路径: $PSScriptRoot" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "┌─ TtsWebApi ────────────────────────────────────────────────┐" -ForegroundColor Cyan
    if (Test-Path "$PSScriptRoot\TtsWebApi\TtsWebApi.csproj") {
        Write-Host "│ ✅ 项目存在" -ForegroundColor Green
        Write-Host "│ 📍 开发地址: http://localhost:5275" -ForegroundColor Cyan
        Write-Host "│ 📁 项目路径: $PSScriptRoot\TtsWebApi" -ForegroundColor Cyan
    } else {
        Write-Host "│ ❌ 项目不存在" -ForegroundColor Red
    }
    Write-Host "└────────────────────────────────────────────────────────────┘" -ForegroundColor Cyan
    Write-Host ""
    
    Write-Host "┌─ TtsWebApp ────────────────────────────────────────────────┐" -ForegroundColor Cyan
    if (Test-Path "$PSScriptRoot\TtsWebApp\TtsWebApp.csproj") {
        Write-Host "│ ✅ 项目存在" -ForegroundColor Green
        Write-Host "│ 📍 开发地址: http://localhost:5261" -ForegroundColor Cyan
        Write-Host "│ 📁 项目路径: $PSScriptRoot\TtsWebApp" -ForegroundColor Cyan
        
        if (Test-Path "$PSScriptRoot\TtsWebApp\tts_admin.db") {
            Write-Host "│ 💾 数据库: 已创建" -ForegroundColor Green
        } else {
            Write-Host "│ 💾 数据库: 未创建" -ForegroundColor Yellow
        }
        
        $nodeExists = Get-Command node -ErrorAction SilentlyContinue
        if ($nodeExists) {
            Write-Host "│ 🟢 Node.js: 已安装" -ForegroundColor Green
            if (Test-Path "$PSScriptRoot\TtsWebApp\node_modules") {
                Write-Host "│ 📦 npm 依赖: 已安装" -ForegroundColor Green
            } else {
                Write-Host "│ 📦 npm 依赖: 未安装" -ForegroundColor Yellow
            }
        } else {
            Write-Host "│ 🔴 Node.js: 未安装" -ForegroundColor Red
        }
    } else {
        Write-Host "│ ❌ 项目不存在" -ForegroundColor Red
    }
    Write-Host "└────────────────────────────────────────────────────────────┘" -ForegroundColor Cyan
    Write-Host ""
    Read-Host "按回车键继续"
}

function Reset-Database {
    Clear-Host
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host " 重置数据库" -ForegroundColor Yellow
    Write-Host "════════════════════════════════════════════════════════════════" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "⚠️  警告: 此操作将删除所有数据！" -ForegroundColor Red
    Write-Host ""
    
    $confirm = Read-Host "确定要重置数据库吗？(Y/N)"
    if ($confirm -ne "Y" -and $confirm -ne "y") {
        return
    }
    
    Set-Location "$PSScriptRoot\TtsWebApp"
    if (Test-Path "tts_admin.db") {
        Remove-Item "tts_admin.db" -Force
        Write-Host "✅ 数据库已删除！" -ForegroundColor Green
        Write-Host "💡 下次启动时将自动创建新数据库" -ForegroundColor Yellow
    } else {
        Write-Host "⚠️  数据库文件不存在" -ForegroundColor Yellow
    }
    Write-Host ""
    Read-Host "按回车键继续"
}

# 主循环
while ($true) {
    Show-Menu
    $choice = Read-Host "请选择操作 [0-9/A-E]"
    
    switch ($choice.ToUpper()) {
        "1" { Build-Api }
        "2" { Build-Web }
        "3" { Build-All }
        "4" { Run-Api }
        "5" { Run-Web }
        "6" { Run-All }
        "7" { Publish-Api }
        "8" { Publish-Web }
        "9" { Publish-All }
        "A" { Clean-Files }
        "B" { Restore-Js }
        "C" { Test-Obfuscate }
        "D" { Show-Info }
        "E" { Reset-Database }
        "0" { 
            Clear-Host
            Write-Host ""
            Write-Host "👋 感谢使用 TTS 项目管理工具！" -ForegroundColor Cyan
            Write-Host ""
            Start-Sleep -Seconds 1
            exit 
        }
        default {
            Write-Host ""
            Write-Host "❌ 无效选择，请重新输入！" -ForegroundColor Red
            Start-Sleep -Seconds 2
        }
    }
}
