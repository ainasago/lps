# 字幕生成功能说明

## ✅ 功能状态

字幕生成功能已经完整实现！包括前端和后端。

## 🎯 功能特性

### 1. 自动生成字幕
- ✅ 基于词边界（Word Boundary）生成精确时间轴
- ✅ 支持 SRT 格式字幕文件
- ✅ 自动时间戳同步

### 2. 字幕合并选项

#### 选项 A：按标点符号合并（推荐）
```
mergeByPunctuation
```
- 根据句子标点符号自动分段
- 支持中英文标点：。！？，；等
- 生成自然的字幕分段

#### 选项 B：按词数合并
```
mergeByNumber
```
- 指定每条字幕包含的词数
- 适合固定长度的字幕需求

### 3. 字幕显示和下载
- ✅ 实时显示生成的字幕内容
- ✅ 一键下载 SRT 格式字幕文件
- ✅ 自动命名：`tts_subtitles_时间戳.srt`

## 📋 使用方法

### 前端使用

1. **勾选"生成字幕"选项**
   - 在 TTS 转换表单中找到"生成字幕"复选框
   - 勾选后会生成字幕

2. **输入文本并转换**
   - 输入要转换的文本
   - 选择语言和配音员
   - 点击"生成语音"

3. **查看字幕**
   - 转换成功后，字幕会显示在音频播放器下方
   - 字幕内容以 SRT 格式显示

4. **下载字幕**
   - 点击"下载字幕"按钮
   - 自动下载 `.srt` 文件

### 后端 API

#### 请求参数

```json
{
  "Text": "要转换的文本",
  "Voice": "zh-CN-XiaoxiaoNeural",
  "Language": "zh-CN",
  "OutputFormat": "audio-24khz-48kbitrate-mono-mp3",
  "Pitch": "0",
  "Rate": "0",
  "Volume": "80",
  "GenerateSubtitles": true,
  "SubtitleOption": "mergeByPunctuation",
  "SubtitleWordCount": 10
}
```

**参数说明：**
- `GenerateSubtitles`: `true` 生成字幕，`false` 不生成
- `SubtitleOption`: 
  - `"mergeByPunctuation"` - 按标点符号合并（推荐）
  - `"mergeByNumber"` - 按词数合并
- `SubtitleWordCount`: 当使用 `mergeByNumber` 时，每条字幕的词数

#### 响应格式

```json
{
  "success": true,
  "audioBase64": "base64编码的音频数据",
  "subtitles": "1\n00:00:00,000 --> 00:00:01,500\n你好，世界\n\n2\n00:00:01,500 --> 00:00:03,000\n这是字幕测试\n\n"
}
```

## 🔧 技术实现

### 后端实现（TtsService.cs）

#### 1. 词边界收集
```csharp
private List<WordBoundary> wordBoundaries = new List<WordBoundary>();

// 在接收 WebSocket 消息时收集词边界
if (message.Contains("\"Type\":\"WordBoundary\""))
{
    var wordBoundary = JsonConvert.DeserializeObject<WordBoundaryMessage>(message);
    wordBoundaries.Add(new WordBoundary
    {
        Text = wordBoundary.Data.Text,
        Offset = wordBoundary.Data.Offset.ToString(),
        Duration = wordBoundary.Data.Duration.ToString()
    });
}
```

#### 2. 字幕生成
```csharp
private string GenerateSubtitles(List<WordBoundary> wordBoundaries, 
                                 string fullText, 
                                 string option, 
                                 int wordCount)
{
    var cues = new List<Cue>();
    
    // 为每个词创建字幕条目
    foreach (var boundary in wordBoundaries)
    {
        cues.Add(new Cue
        {
            Index = cues.Count + 1,
            Start = MicrosecondsToTime(offset),
            End = MicrosecondsToTime(offset + duration),
            Content = boundary.Text
        });
    }
    
    // 根据选项合并字幕
    if (option == "mergeByPunctuation")
        cues = MergeCuesByPunctuation(cues, fullText);
    else if (option == "mergeByNumber")
        cues = MergeCuesByNumber(cues, wordCount);
    
    return ConvertToSrt(cues);
}
```

#### 3. 按标点符号合并
```csharp
private List<Cue> MergeCuesByPunctuation(List<Cue> cues, string fullText)
{
    // 使用正则表达式匹配标点符号
    var punctuationRegex = new Regex(@"[。！？？，；,()\[\]（）【】{}、\.\?!;:<>《》「」『』""''…\n]+");
    
    // 根据标点符号位置合并字幕
    // ...
}
```

#### 4. SRT 格式转换
```csharp
private string ConvertToSrt(List<Cue> cues)
{
    var sb = new StringBuilder();
    foreach (var cue in cues)
    {
        sb.AppendLine(cue.Index.ToString());
        sb.AppendLine($"{cue.Start} --> {cue.End}");
        sb.AppendLine(cue.Content);
        sb.AppendLine();
    }
    return sb.ToString();
}
```

### 前端实现（Index.cshtml）

#### 1. 字幕容器
```html
<div id="subtitlesContainer" style="display: none;">
    <h4>字幕内容</h4>
    <button id="downloadSubtitlesBtn">下载字幕</button>
    <div id="subtitlesContent"></div>
</div>
```

#### 2. 显示字幕
```javascript
if (currentSubtitles) {
    $('#subtitlesContainer').show();
    $('#subtitlesContent').text(currentSubtitles);
} else {
    $('#subtitlesContainer').hide();
}
```

#### 3. 下载字幕
```javascript
function downloadSubtitles() {
    if (currentSubtitles) {
        const a = document.createElement('a');
        a.href = 'data:text/plain;charset=utf-8,' + 
                 encodeURIComponent(currentSubtitles);
        a.download = 'tts_subtitles_' + new Date().getTime() + '.srt';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
    }
}
```

## 📝 SRT 格式示例

```srt
1
00:00:00,000 --> 00:00:01,500
你好，世界！

2
00:00:01,500 --> 00:00:03,200
这是一个字幕测试。

3
00:00:03,200 --> 00:00:05,000
字幕会自动同步音频。
```

**格式说明：**
- 第一行：字幕序号
- 第二行：时间轴（开始时间 --> 结束时间）
- 第三行：字幕内容
- 空行：分隔不同的字幕条目

## 🎬 使用场景

### 1. 视频制作
- 为视频添加字幕
- 导入到视频编辑软件（如 Premiere、Final Cut Pro）
- 自动同步音频和字幕

### 2. 在线教育
- 为课程视频添加字幕
- 提高可访问性
- 支持多语言学习

### 3. 播客和音频内容
- 生成音频转录
- 提供文字版本
- 便于搜索和引用

### 4. 无障碍访问
- 为听障用户提供字幕
- 符合无障碍标准
- 提升用户体验

## 🧪 测试步骤

### 1. 基本测试

1. 访问 TTS 页面
2. 输入测试文本：
   ```
   你好，世界！这是一个测试。字幕功能很强大。
   ```
3. 勾选"生成字幕"
4. 选择语言和配音员
5. 点击"生成语音"
6. 查看字幕是否显示
7. 点击"下载字幕"
8. 检查下载的 SRT 文件

### 2. 标点符号合并测试

**输入文本：**
```
第一句话。第二句话！第三句话？第四句话，继续说。
```

**预期结果：**
- 每个句子生成一条字幕
- 按标点符号自动分段

### 3. 词数合并测试

**设置：**
- SubtitleOption: `mergeByNumber`
- SubtitleWordCount: `5`

**预期结果：**
- 每条字幕包含约 5 个词
- 固定长度分段

### 4. 长文本测试

**输入文本：**
```
这是一段很长的文本，用来测试字幕生成功能是否能够正确处理长文本。
字幕应该根据标点符号自动分段，每一段都有准确的时间轴。
这样可以确保字幕和音频完美同步。
```

**预期结果：**
- 正确分段
- 时间轴准确
- 字幕完整

## ⚠️ 注意事项

### 1. 字幕质量
- 字幕质量取决于 TTS 引擎的词边界准确性
- 标点符号合并效果最好
- 建议使用标准标点符号

### 2. 文本格式
- 支持中英文混合
- 支持常见标点符号
- 避免使用特殊字符

### 3. 文件大小
- 长文本会生成较大的字幕文件
- 建议分段处理超长文本

### 4. 浏览器兼容性
- 支持所有现代浏览器
- 下载功能使用 HTML5 API
- 无需额外插件

## 🔍 故障排除

### 问题 1：字幕不显示

**可能原因：**
- 没有勾选"生成字幕"
- 后端字幕生成失败

**解决方法：**
1. 确认勾选了"生成字幕"复选框
2. 查看浏览器控制台是否有错误
3. 查看后端日志

### 问题 2：字幕时间不准确

**可能原因：**
- TTS 引擎词边界不准确
- 文本格式问题

**解决方法：**
1. 使用标准标点符号
2. 避免特殊字符
3. 尝试不同的配音员

### 问题 3：下载失败

**可能原因：**
- 浏览器阻止下载
- 字幕内容为空

**解决方法：**
1. 检查浏览器下载设置
2. 确认字幕已生成
3. 尝试其他浏览器

## 📚 相关文档

- `TtsWebApi/Services/TtsService.cs` - 字幕生成逻辑
- `TtsWebApp/Views/Tts/Index.cshtml` - 前端实现
- `TtsWebApp/Controllers/TtsController.cs` - API 调用

## 🎉 总结

字幕生成功能已经完整实现，包括：

1. ✅ **后端实现**
   - 词边界收集
   - 字幕生成
   - 标点符号合并
   - 词数合并
   - SRT 格式转换

2. ✅ **前端实现**
   - 字幕显示
   - 字幕下载
   - UI 交互

3. ✅ **功能特性**
   - 自动时间轴同步
   - 多种合并选项
   - SRT 格式支持

现在可以直接使用字幕功能了！🚀

---

**最后更新：** 2025年1月1日  
**状态：** ✅ 功能完整实现
