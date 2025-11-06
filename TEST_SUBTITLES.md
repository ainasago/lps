# 字幕功能测试指南

## 🔍 问题诊断

如果看不到下载字幕按钮，可能的原因：

### 1. 字幕没有生成

**检查步骤：**

1. **确认勾选了"生成字幕"复选框**
   - 在表单中找到"生成字幕"选项
   - 必须勾选才会生成字幕

2. **查看浏览器控制台**
   - 按 F12 打开开发者工具
   - 切换到 Console 标签
   - 查看日志输出：
     ```
     字幕数据长度: 0
     字幕数据: null
     ```
   - 如果长度为 0 或 null，说明后端没有返回字幕

3. **查看后端日志**
   - 查看 TtsWebApp 控制台
   - 应该看到：
     ```
     字幕长度: XXX
     字幕前100字符: 1\n00:00:00,000 --> ...
     ```

### 2. 字幕容器被隐藏

**检查步骤：**

1. **打开浏览器开发者工具（F12）**
2. **切换到 Elements 标签**
3. **搜索 `subtitlesContainer`**
4. **查看 style 属性**
   - 如果是 `display: none`，说明容器被隐藏
   - 应该是 `display: block` 才能看到

### 3. JavaScript 错误

**检查步骤：**

1. **打开浏览器控制台（F12）**
2. **查看是否有红色错误信息**
3. **常见错误：**
   - `currentSubtitles is not defined`
   - `downloadSubtitles is not a function`

## 🧪 测试步骤

### 步骤 1：重启服务

```bash
# 重启 TtsWebApp
cd d:\1Dev\dev\webs\tts_turi\TtsWebApp
dotnet run
```

### 步骤 2：打开页面

1. 访问 http://localhost:5128/Tts/Index
2. 打开浏览器开发者工具（F12）

### 步骤 3：输入测试文本

```
你好，世界！这是一个测试。字幕功能很强大。
```

### 步骤 4：配置选项

1. ✅ 选择语言：中文（简体）
2. ✅ 选择配音员：任意
3. ✅ **勾选"生成字幕"** ← 重要！

### 步骤 5：生成语音

1. 点击"生成语音"按钮
2. 等待转换完成

### 步骤 6：查看日志

**浏览器控制台应该显示：**
```
收到响应: {success: true, audioData: "...", subtitles: "1\n00:00:00,000 --> ..."}
音频数据长度: XXXXX
字幕数据长度: XXX
字幕数据: 1
00:00:00,000 --> 00:00:01,500
你好，世界！
...
```

**后端日志应该显示：**
```
API响应内容长度: XXXXX
音频Base64长度: XXXXX
字幕长度: XXX
字幕前100字符: 1
00:00:00,000 --> 00:00:01,500
你好，世界！
...
```

### 步骤 7：查看字幕

1. 音频播放器下方应该出现"字幕内容"区域
2. 显示 SRT 格式的字幕
3. 右上角有"下载字幕"按钮

### 步骤 8：下载字幕

1. 点击"下载字幕"按钮
2. 浏览器应该下载 `tts_subtitles_时间戳.srt` 文件
3. 打开文件查看内容

## 🐛 常见问题

### 问题 1：看不到"生成字幕"复选框

**原因：** 页面没有正确加载

**解决：**
1. 清除浏览器缓存（Ctrl+Shift+Delete）
2. 硬刷新页面（Ctrl+Shift+R）
3. 重启 TtsWebApp

### 问题 2：勾选了但没有字幕

**原因：** 后端没有生成字幕

**检查：**
1. 查看 TtsWebApi 日志
2. 确认 `GenerateSubtitles: true` 被传递
3. 查看是否有错误日志

**解决：**
```csharp
// 在 TtsController.cs 中确认
GenerateSubtitles = request.GenerateSubtitles,  // 应该是 true
SubtitleOption = "mergeByPunctuation",
SubtitleWordCount = 10
```

### 问题 3：字幕显示但无法下载

**原因：** JavaScript 错误或浏览器阻止下载

**解决：**
1. 查看浏览器控制台错误
2. 检查浏览器下载设置
3. 尝试其他浏览器

### 问题 4：字幕格式不正确

**原因：** 后端生成的格式有问题

**检查：**
```
正确的 SRT 格式：
1
00:00:00,000 --> 00:00:01,500
你好，世界！

2
00:00:01,500 --> 00:00:03,000
这是一个测试。
```

## 🔧 手动测试字幕功能

### 在浏览器控制台执行：

```javascript
// 1. 检查变量
console.log('currentSubtitles:', currentSubtitles);

// 2. 手动显示字幕容器
$('#subtitlesContainer').show();
$('#subtitlesContent').text('测试字幕\n1\n00:00:00,000 --> 00:00:01,000\n测试');

// 3. 手动下载字幕
downloadSubtitles();
```

## 📋 完整的 UI 结构

```html
<form id="ttsForm">
    <!-- 文本输入 -->
    <textarea id="textInput"></textarea>
    
    <!-- 语言选择 -->
    <select id="languageSelect"></select>
    
    <!-- 配音员选择 -->
    <select id="voiceSelect"></select>
    
    <!-- 参数调节 -->
    <input id="pitchRange" type="range">
    <input id="rateRange" type="range">
    <input id="volumeRange" type="range">
    
    <!-- 生成字幕复选框 -->
    <input type="checkbox" id="generateSubtitles">
    <label for="generateSubtitles">生成字幕</label>
    
    <!-- 生成按钮 -->
    <button id="convertBtn">生成语音</button>
    
    <!-- 音频播放器 -->
    <div id="inlinePlayer" style="display: none;">
        <button id="playBtn">播放</button>
        <button id="downloadAudioBtn">下载音频</button>
    </div>
    
    <!-- 字幕容器 -->
    <div id="subtitlesContainer" style="display: none;">
        <h4>字幕内容</h4>
        <button id="downloadSubtitlesBtn">下载字幕</button>
        <div id="subtitlesContent"></div>
    </div>
</form>
```

## 🎯 预期行为

### 当勾选"生成字幕"时：

1. ✅ 请求中包含 `GenerateSubtitles: true`
2. ✅ 后端生成字幕
3. ✅ 响应中包含 `subtitles` 字段
4. ✅ 前端显示字幕容器
5. ✅ 可以查看字幕内容
6. ✅ 可以下载字幕文件

### 当不勾选"生成字幕"时：

1. ✅ 请求中包含 `GenerateSubtitles: false`
2. ✅ 后端不生成字幕
3. ✅ 响应中 `subtitles` 为 null
4. ✅ 字幕容器保持隐藏

## 📞 需要帮助？

如果按照上述步骤仍然看不到字幕，请提供：

1. **浏览器控制台的完整日志**
2. **TtsWebApp 后端日志**
3. **TtsWebApi 后端日志**
4. **是否勾选了"生成字幕"**
5. **浏览器和版本信息**

---

**最后更新：** 2025年1月1日
