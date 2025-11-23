# 📋 项目管理脚本使用说明

## 🚀 快速开始

### Windows CMD（命令提示符）
双击运行 `manage.bat` 即可打开交互式管理菜单。

### PowerShell
```powershell
# 方式一：直接运行
.\manage.ps1

# 方式二：如果提示执行策略错误
Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass
.\manage.ps1

# 方式三：使用 bat 文件（推荐）
cmd /c manage.bat
```

**提示**: 如果在 PowerShell 中运行 `.bat` 文件没反应，请使用 `.\manage.ps1` 或 `cmd /c manage.bat`。

## 📖 功能说明

### 【编译构建】

#### [1] 编译 API 项目
- 编译 TtsWebApi 项目
- 检查代码错误
- 生成 Debug 版本

#### [2] 编译 Web 项目
- 编译 TtsWebApp 项目
- 检查代码错误
- 生成 Debug 版本

#### [3] 编译所有项目
- 依次编译 API 和 Web 项目
- 一次性检查所有代码

---

### 【开发运行】

#### [4] 启动 API（开发模式）
- 启动 TtsWebApi 开发服务器
- 地址: http://localhost:5275
- 支持热重载

#### [5] 启动 Web（开发模式）
- 启动 TtsWebApp 开发服务器
- 地址: http://localhost:5261
- 支持热重载

#### [6] 启动所有服务（开发模式）
- 同时启动 API 和 Web
- 完整开发环境
- 两个独立的命令行窗口

**提示**: 
- 每个服务在独立窗口运行
- 关闭窗口即停止服务
- 修改代码后自动重新编译

---

### 【发布部署】

#### [7] 发布 API（Release）
- 编译 Release 版本
- 优化性能
- 输出到 `TtsWebApi/publish/`

#### [8] 发布 Web（Release + JS混淆）
- 编译 Release 版本
- **自动混淆 JavaScript 文件**
- 自动备份原文件
- 发布后自动恢复原文件
- 输出到 `TtsWebApp/publish/`

**流程**:
1. 检查 Node.js 环境
2. 安装 npm 依赖（首次）
3. 混淆 JS 文件（tts.js, i18n.js, site.js）
4. 发布项目
5. 恢复原始 JS 文件

#### [9] 发布所有项目（Release + JS混淆）
- 发布 API 和 Web
- Web 项目包含 JS 混淆
- 一键完成所有发布

**输出目录**:
```
TtsWebApi/publish/     ← API 发布文件
TtsWebApp/publish/     ← Web 发布文件（JS已混淆）
```

---

### 【维护工具】

#### [A] 清理编译文件
- 删除 `bin/` 目录
- 删除 `obj/` 目录
- 删除 `publish/` 目录
- 删除混淆临时文件

**用途**:
- 解决编译错误
- 释放磁盘空间
- 重新开始编译

#### [B] 恢复 JS 原文件
- 从备份恢复原始 JS 文件
- 用于发布后继续开发

**场景**:
- 发布时混淆了 JS
- 需要继续开发调试
- 恢复可读的原始代码

#### [C] 测试 JS 混淆效果
- 混淆 JS 文件到独立目录
- **不修改原文件**
- 可以查看混淆效果

**输出**: `wwwroot/js/obfuscated/`

**用途**:
- 预览混淆效果
- 测试混淆配置
- 安全测试

#### [D] 查看项目信息
- 显示项目路径
- 检查项目状态
- 显示服务地址
- 检查数据库状态
- 检查 Node.js 环境

#### [E] 删除数据库（重置）
- 删除 `tts_admin.db`
- 重置所有数据
- 下次启动自动创建新库

**警告**: 会删除所有数据！

---

## 🎯 常用操作流程

### 开发流程

```
1. 启动服务
   → 选择 [6] 启动所有服务

2. 修改代码
   → 自动热重载

3. 测试功能
   → 访问 http://localhost:5261
```

### 发布流程

```
1. 清理旧文件（可选）
   → 选择 [A] 清理编译文件

2. 测试混淆（可选）
   → 选择 [C] 测试 JS 混淆效果
   → 查看 wwwroot/js/obfuscated/

3. 发布项目
   → 选择 [9] 发布所有项目
   
4. 检查输出
   → TtsWebApi/publish/
   → TtsWebApp/publish/
```

### 维护流程

```
1. 重置数据库
   → 选择 [E] 删除数据库

2. 清理编译文件
   → 选择 [A] 清理编译文件

3. 重新编译
   → 选择 [3] 编译所有项目
```

---

## 📁 目录结构

```
tts_turi/
├── manage.bat              ← 主管理脚本
├── start_all.bat           ← 快速启动脚本（旧）
├── TtsWebApi/
│   ├── publish/            ← API 发布输出
│   ├── bin/                ← 编译输出
│   └── obj/                ← 编译临时文件
└── TtsWebApp/
    ├── publish/            ← Web 发布输出（JS已混淆）
    ├── bin/                ← 编译输出
    ├── obj/                ← 编译临时文件
    ├── wwwroot/js/
    │   ├── obfuscated/     ← 测试混淆输出
    │   └── backup/         ← JS 备份文件
    ├── package.json        ← npm 配置
    └── node_modules/       ← npm 依赖
```

---

## ⚙️ 环境要求

### 必需
- ✅ .NET 9.0 SDK
- ✅ Windows 操作系统

### 可选（用于 JS 混淆）
- 📦 Node.js 18+ 
- 📦 npm

**检查方法**:
```bash
dotnet --version    # 检查 .NET
node --version      # 检查 Node.js
npm --version       # 检查 npm
```

---

## 🔧 首次使用

### 1. 检查环境

运行 `manage.bat`，选择 `[D] 查看项目信息`

### 2. 安装 npm 依赖（用于 JS 混淆）

如果要使用 JS 混淆功能：

```bash
cd TtsWebApp
npm install
```

或者直接选择 `[C] 测试 JS 混淆效果`，会自动安装依赖。

### 3. 编译项目

选择 `[3] 编译所有项目`

### 4. 启动服务

选择 `[6] 启动所有服务`

---

## 💡 提示和技巧

### 1. 快速开发

开发时直接选择 `[6]` 启动所有服务，无需每次编译。

### 2. 发布前测试

发布前先选择 `[C]` 测试混淆效果，确保混淆正常。

### 3. 清理问题

遇到奇怪的编译错误时，选择 `[A]` 清理编译文件。

### 4. 数据库重置

测试时需要重置数据，选择 `[E]` 删除数据库。

### 5. 查看状态

不确定项目状态时，选择 `[D]` 查看项目信息。

---

## ❓ 常见问题

### Q: 选择 [8] 或 [9] 时提示"未检测到 Node.js"？

**A**: 需要安装 Node.js 才能使用 JS 混淆功能。

**解决方案**:
1. 下载安装 Node.js: https://nodejs.org/
2. 重新打开 `manage.bat`
3. 或者选择 `[7]` 发布 API（不需要 Node.js）

### Q: npm install 失败？

**A**: 网络问题或权限问题。

**解决方案**:
```bash
# 使用淘宝镜像
npm config set registry https://registry.npmmirror.com
npm install
```

### Q: 发布后 JS 文件被混淆了，如何恢复？

**A**: 选择 `[B] 恢复 JS 原文件`

### Q: 如何只编译不运行？

**A**: 选择 `[1]`、`[2]` 或 `[3]` 进行编译。

### Q: 如何停止运行的服务？

**A**: 关闭对应的命令行窗口即可。

### Q: 发布文件在哪里？

**A**: 
- API: `TtsWebApi/publish/`
- Web: `TtsWebApp/publish/`

### Q: 如何部署到服务器？

**A**: 
1. 选择 `[9]` 发布所有项目
2. 将 `publish/` 目录复制到服务器
3. 在服务器上运行 `dotnet TtsWebApi.dll` 和 `dotnet TtsWebApp.dll`

---

## 🔒 JS 混淆说明

### 自动混淆

选择 `[8]` 或 `[9]` 发布时，会自动：
1. 混淆 JS 文件（最强配置）
2. 备份原文件到 `wwwroot/js/backup/`
3. 发布项目
4. 恢复原文件

### 手动混淆

如果需要手动控制：

```bash
cd TtsWebApp

# 测试混淆（不修改原文件）
npm run obfuscate

# 生产混淆（替换原文件）
npm run obfuscate:prod

# 恢复原文件
npm run restore
```

### 混淆的文件

- `wwwroot/js/tts.js`
- `wwwroot/js/i18n.js`
- `wwwroot/js/site.js`

详细说明请查看 `TtsWebApp/OBFUSCATION_README.md`

---

## 📊 脚本功能对比

| 功能 | manage.bat | start_all.bat |
|------|-----------|---------------|
| 交互式菜单 | ✅ | ❌ |
| 编译项目 | ✅ | ❌ |
| 启动服务 | ✅ | ✅ |
| 发布项目 | ✅ | ❌ |
| JS 混淆 | ✅ | ❌ |
| 清理文件 | ✅ | ❌ |
| 数据库管理 | ✅ | ❌ |
| 项目信息 | ✅ | ❌ |

**推荐**: 使用 `manage.bat` 进行所有操作。

---

## 🎨 界面预览

```
╔════════════════════════════════════════════════════════════════╗
║                    TTS 项目管理工具 v1.0                      ║
╚════════════════════════════════════════════════════════════════╝

 【编译构建】
 [1] 编译 API 项目
 [2] 编译 Web 项目
 [3] 编译所有项目

 【开发运行】
 [4] 启动 API（开发模式）
 [5] 启动 Web（开发模式）
 [6] 启动所有服务（开发模式）

 【发布部署】
 [7] 发布 API（Release）
 [8] 发布 Web（Release + JS混淆）
 [9] 发布所有项目（Release + JS混淆）

 【维护工具】
 [A] 清理编译文件
 [B] 恢复 JS 原文件
 [C] 测试 JS 混淆效果
 [D] 查看项目信息
 [E] 删除数据库（重置）

 [0] 退出

════════════════════════════════════════════════════════════════
请选择操作 [0-9/A-E]: _
```

---

## 📞 技术支持

如有问题，请查看：
- JS 混淆详细文档: `TtsWebApp/OBFUSCATION_README.md`
- JS 混淆快速指南: `TtsWebApp/QUICK_START.md`

---

**提示**: 建议将此脚本添加到快捷方式，方便日常使用！
