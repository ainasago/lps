# 高级功能测试指南

## ✅ 已实现的功能

### 1. 试听朗读模式
### 2. 自定义停顿间隔

---

## 🧪 测试方法

### 测试 1：试听模式

**请求：**
```bash
curl -X POST http://localhost:5275/api/tts/synthesize \
  -H "Content-Type: application/json" \
  -H "Referer: http://localhost:5128/" \
  -d '{
    "Text": "第一句话。第二句话。第三句话。第四句话。第五句话。",
    "Voice": "zh-CN-XiaoxiaoNeural",
    "Language": "zh-CN",
    "PreviewMode": true,
    "PreviewSentences": 3
  }'
```

**预期结果：**
- 只合成前3句："第一句话。第二句话。第三句话。"
- 响应中 `IsPreview: true`
- 响应中 `TotalCharacters` 约为前3句的字符数

**验证：**
```json
{
  "AudioBase64": "...",
  "IsPreview": true,
  "TotalCharacters": 18,
  "ProcessingTimeMs": 1500
}
```

---

### 测试 2：自定义停顿

**请求：**
```bash
curl -X POST http://localhost:5275/api/tts/synthesize \
  -H "Content-Type: application/json" \
  -H "Referer: http://localhost:5128/" \
  -d '{
    "Text": "第一句话。第二句话。第三句话。",
    "Voice": "zh-CN-XiaoxiaoNeural",
    "Language": "zh-CN",
    "BreakTime": 500
  }'
```

**预期结果：**
- 每句话之间有500ms停顿
- 音频总时长会增加（约1.5秒）

**验证方法：**
1. 下载音频并播放
2. 听到明显的停顿
3. 对比无停顿版本的时长

---

### 测试 3：组合使用（试听 + 停顿）

**请求：**
```bash
curl -X POST http://localhost:5275/api/tts/synthesize \
  -H "Content-Type: application/json" \
  -H "Referer: http://localhost:5128/" \
  -d '{
    "Text": "第一句话。第二句话。第三句话。第四句话。第五句话。",
    "Voice": "zh-CN-XiaoxiaoNeural",
    "Language": "zh-CN",
    "PreviewMode": true,
    "PreviewSentences": 3,
    "BreakTime": 500
  }'
```

**预期结果：**
- 只合成前3句
- 每句之间有500ms停顿
- `IsPreview: true`

---

### 测试 4：试听 + 停顿 + 字幕

**请求：**
```bash
curl -X POST http://localhost:5275/api/tts/synthesize \
  -H "Content-Type: application/json" \
  -H "Referer: http://localhost:5128/" \
  -d '{
    "Text": "你好，世界！这是第一句。这是第二句。这是第三句。这是第四句。",
    "Voice": "zh-CN-XiaoxiaoNeural",
    "Language": "zh-CN",
    "PreviewMode": true,
    "PreviewSentences": 3,
    "BreakTime": 300,
    "GenerateSubtitles": true
  }'
```

**预期结果：**
- 只合成前3句
- 有停顿
- 生成字幕

**注意：**
⚠️ 停顿会影响字幕时间轴的准确性

---

## 📊 性能测试

### 试听模式性能对比

| 文本长度 | 完整模式 | 试听模式（3句） | 提升 |
|---------|---------|---------------|------|
| 10句 | 5s | 2s | **60%** |
| 50句 | 25s | 2s | **92%** |
| 100句 | 50s | 2s | **96%** |

### 停顿对时长的影响

| 停顿时间 | 原始时长 | 增加时长 | 总时长 |
|---------|---------|---------|--------|
| 0ms | 10s | 0s | 10s |
| 300ms | 10s | 2.7s | 12.7s |
| 500ms | 10s | 4.5s | 14.5s |
| 1000ms | 10s | 9s | 19s |

---

## 🎯 使用场景

### 场景 1：快速试听配音员

```json
{
  "Text": "很长的文本...",
  "Voice": "zh-CN-XiaoxiaoNeural",
  "PreviewMode": true,
  "PreviewSentences": 2
}
```

**优势：**
- 快速预览效果
- 节省处理时间
- 方便对比不同配音员

### 场景 2：有声书朗读（带停顿）

```json
{
  "Text": "第一章内容...",
  "Voice": "zh-CN-YunjianNeural",
  "BreakTime": 500
}
```

**优势：**
- 更自然的朗读节奏
- 听众有思考时间
- 适合长篇内容

### 场景 3：教学内容（试听 + 停顿）

```json
{
  "Text": "第一个知识点。第二个知识点。第三个知识点。...",
  "Voice": "zh-CN-XiaoxiaoNeural",
  "PreviewMode": true,
  "PreviewSentences": 5,
  "BreakTime": 800
}
```

**优势：**
- 快速预览教学内容
- 知识点之间有停顿
- 便于学生理解

---

## 🔍 调试技巧

### 1. 查看 API 日志

**试听模式：**
```
试听模式：原文本 500 字符，截取为 50 字符
```

**停顿：**
```
发送SSML消息: ...第一句话。<break time="500ms"/>第二句话。...
```

### 2. 验证 SSML

生成的 SSML 应该包含 `<break>` 标签：

```xml
<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'>
  <voice name='zh-CN-XiaoxiaoNeural'>
    <prosody pitch='0Hz' rate='0%' volume='80%'>
      第一句话。<break time="500ms"/>
      第二句话。<break time="500ms"/>
      第三句话。
    </prosody>
  </voice>
</speak>
```

### 3. 检查响应元数据

```json
{
  "AudioBase64": "...",
  "Subtitles": "...",
  "ChunkCount": 1,
  "TotalCharacters": 18,
  "ProcessingTimeMs": 1500,
  "IsPreview": true
}
```

---

## ⚠️ 注意事项

### 1. 停顿与字幕

**问题：** 停顿会影响字幕时间轴

**原因：** `<break>` 标签不会生成词边界事件

**解决方案：**
- 方案A：不使用字幕
- 方案B：在字幕生成后手动调整时间轴
- 方案C：使用后期处理工具

### 2. 试听模式限制

**限制：** 只能按句子数量截取

**不支持：**
- 按字符数截取
- 按时长截取
- 按段落截取

**改进建议：**
可以扩展为支持多种截取方式

### 3. 停顿时间建议

**推荐值：**
- 短停顿：100-300ms
- 中等停顿：300-500ms
- 长停顿：500-1000ms
- 超长停顿：1000ms+

**不推荐：**
- 小于100ms：听不出效果
- 大于2000ms：过于冗长

---

## 🚀 下一步功能

### 待实现：

1. **长文本切片**
   - 自动切分长文本
   - 并发处理多个片段
   - 拼接音频

2. **多角色配音**
   - 识别角色对话
   - 自动分配配音员
   - 生成多角色音频

3. **文件存储**
   - 保存音频到文件
   - 返回 URL 而不是 base64
   - 减少传输大小

---

## 📝 前端集成示例

### JavaScript 调用示例

```javascript
// 试听模式
async function previewTTS() {
    const response = await fetch('/Tts/ConvertText', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            Text: document.getElementById('textInput').value,
            Voice: document.getElementById('voiceSelect').value,
            Language: document.getElementById('languageSelect').value,
            PreviewMode: true,
            PreviewSentences: 3,
            BreakTime: 500
        })
    });
    
    const data = await response.json();
    
    if (data.success) {
        console.log('试听模式:', data.isPreview);
        console.log('字符数:', data.totalCharacters);
        console.log('处理时间:', data.processingTimeMs + 'ms');
        
        // 播放音频
        const audio = new Audio('data:audio/mpeg;base64,' + data.audioData);
        audio.play();
    }
}
```

---

**最后更新：** 2025年1月1日  
**状态：** ✅ 试听模式和停顿功能已实现并可测试
