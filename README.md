# Tauri + Vue 3

使用Tauri开发的微软配音工具

## 注意事项

- 第一次使用Tauri发布公开软件，据说Tauri的兼容性不好，有问题请提交。
- 音频使用base64传输，文本过多时会有性能问题

## 已完成功能

- [x] 基本配音功能
- [x] 字幕生成功能
- [x] 配音界面的设置保存
- [x] 设置页面

## 后续计划

- [ ] 多角色配音
- [ ] 长文本切片（可提高性能）
- [ ] 试听朗读（可用于小说阅读）
- [ ] 自定义停顿间隔（暂时无法适应字幕）
- [ ] 多端适配（Mac,Linux,安卓）
- [ ] 解决base64引发的长音频性能问题

## 开发

### 克隆代码

```
git clone https://gitee.com/lieranhuasha/tts-tauri.git
或
git clone https://github.com/zs1083339604/tts-tauri.git
```

### 安装依赖

```
cd tts-tauri
npm install
```

### 运行

```
npm run tauri dev
```

### 注意事项

使用Tauri开发，需要完成前置条件：[Tauri前置条件](https://tauri.app/zh-cn/start/prerequisites/)

---

## ASP.NET Core 版本（新）

### 项目结构

```
tts_turi/
├── TtsWebApi/          # TTS API 服务
├── TtsWebApp/          # Web 应用程序
├── manage.bat          # 完整管理工具 
├── quick_start.bat     # 快速启动菜单
└── start_all.bat       # 一键启动（旧）
```

### 快速开始

#### 方式一：使用管理工具（推荐）

双击运行 `manage.bat`，提供完整的项目管理功能：

```
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
```

#### 方式二：快速启动

双击运行 `quick_start.bat`，快速启动开发环境。

#### 方式三：命令行

```bash
# 启动 API
cd TtsWebApi
dotnet run

# 启动 Web（新窗口）
cd TtsWebApp
dotnet run
```

### 访问地址

- **API**: http://localhost:5275
- **Web**: http://localhost:5261
- **管理后台**: http://localhost:5261/admin3

### 功能特性

#### 核心功能
- TTS 语音转换
- 多语言支持（中文/英文）
- 现代化 UI 设计
- 响应式布局

#### 管理后台
- TTS 转换记录管理
- 数据统计分析
- 文章/页面管理
- 文件管理
- 系统配置

#### JavaScript 混淆
- 类似 Node.js 的强混淆效果
- 自动备份和恢复
- 测试和生产两种模式
- 详细文档：`TtsWebApp/OBFUSCATION_README.md`

### 发布部署

#### 使用管理工具（推荐）

```bash
# 运行 manage.bat，选择 [9]
# 自动完成：JS混淆 → 发布 → 恢复原文件
```

#### 手动发布

```bash
# 发布 API
cd TtsWebApi
dotnet publish -c Release -o ./publish

# 发布 Web（含 JS 混淆）
cd TtsWebApp
npm install              # 首次需要
npm run obfuscate:prod   # 混淆 JS
dotnet publish -c Release -o ./publish
npm run restore          # 恢复原文件
```

### 环境要求

- .NET 9.0 SDK
- Node.js 18+（用于 JS 混淆，可选）
- SQLite（自动创建）

### 详细文档

- 管理工具使用: `MANAGE_README.md`
- JS 混淆详解: `TtsWebApp/OBFUSCATION_README.md`
- JS 混淆快速开始: `TtsWebApp/QUICK_START.md`

### 技术栈

#### 后端
- ASP.NET Core 9.0
- Entity Framework Core
- FreeSql（数据库同步）
- SQLite

#### 前端
- Bootstrap 5
- Tailwind CSS
- AdminLTE 4
- Chart.js
- Material Symbols

#### 工具
- javascript-obfuscator（JS 混淆）
- Serilog（日志）
- BCrypt（密码加密）

### 项目特色

1. **混合 ORM 架构**
   - FreeSql：自动同步数据库结构
   - EF Core：所有数据操作

2. **强 JavaScript 混淆**
   - RC4 字符串加密
   - 控制流扁平化
   - 死代码注入
   - 反调试保护

3. **完整管理系统**
   - 交互式管理脚本
   - 一键编译/发布
   - 自动化混淆流程

4. **数据统计分析**
   - 每日转换量趋势
   - 热门语言/音色统计
   - IP 访问频率分析

### 常见问题

**Q: 如何重置数据库？**
A: 运行 `manage.bat`，选择 `[E]` 删除数据库。

**Q: JS 混淆需要 Node.js 吗？**
A: 是的，但不是必需的。没有 Node.js 也可以发布，只是不会混淆 JS。

**Q: 如何查看混淆效果？**
A: 运行 `manage.bat`，选择 `[C]` 测试混淆，查看 `wwwroot/js/obfuscated/`。

**Q: 发布后如何部署？**
A: 将 `publish/` 目录复制到服务器，运行 `dotnet TtsWebApp.dll`。

### 开发建议

1. 使用 `manage.bat` 进行所有操作
2. 开发时使用原始 JS 文件
3. 发布前先测试混淆效果
4. 定期提交代码到 Git
5. 发布时选择 `[9]` 自动处理混淆

---

## 支持

如有问题，请查看详细文档或提交 Issue。