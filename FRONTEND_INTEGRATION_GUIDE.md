# 前端集成完成指南

## ✅ 已完成的前端功能

### 新增的 UI 控件

#### 1. 试听模式
- ✅ 复选框：启用/禁用试听模式
- ✅ 数字输入：设置试听句子数（1-10句）
- ✅ 默认值：3句

#### 2. 停顿间隔
- ✅ 数字输入：设置停顿时间（0-2000毫秒）
- ✅ 步进：100毫秒
- ✅ 默认值：0（无停顿）

#### 3. 长文本切片
- ✅ 复选框：启用/禁用切片
- ✅ 数字输入：设置每片字符数（100-2000字）
- ✅ 步进：100字
- ✅ 默认值：500字

### 元数据显示

在浏览器控制台显示：
- 🎧 试听模式标记
- 📄 切片数量
- 📝 总字符数
- ⏱️ 处理时间

---

## 🧪 测试步骤

### 1. 重启 TtsWebApp

```bash
# 停止当前服务（Ctrl+C）
cd d:\1Dev\dev\webs\tts_turi\TtsWebApp
dotnet run
```

### 2. 访问页面

打开浏览器访问：http://localhost:5128/Tts/Index

### 3. 测试试听模式

**步骤：**
1. 输入文本：`第一句话。第二句话。第三句话。第四句话。第五句话。`
2. 选择配音员：`zh-CN-XiaoxiaoNeural`
3. ✅ 勾选"试听模式"
4. 设置句子数：`3`
5. 点击"生成语音"

**预期结果：**
- 只生成前3句
- 控制台显示：`🎧 试听模式`
- 音频时长约为完整版的60%

### 4. 测试停顿功能

**步骤：**
1. 输入文本：`第一句话。第二句话。第三句话。`
2. 选择配音员：`zh-CN-XiaoxiaoNeural`
3. 设置停顿：`500` 毫秒
4. 点击"生成语音"

**预期结果：**
- 每句话之间有明显停顿
- 音频时长比无停顿版本长约1秒

### 5. 测试长文本切片

**步骤：**
1. 输入长文本（超过500字）
2. 选择配音员：`zh-CN-XiaoxiaoNeural`
3. ✅ 勾选"长文本切片"
4. 设置切片大小：`500` 字
5. 点击"生成语音"

**预期结果：**
- 控制台显示：`📄 X 个片段`
- 音频完整无缝拼接
- 处理时间显示

### 6. 组合测试

**步骤：**
1. 输入文本：`第一句。第二句。第三句。第四句。第五句。第六句。第七句。第八句。`
2. ✅ 勾选"试听模式"，设置 `3` 句
3. 设置停顿：`500` 毫秒
4. ✅ 勾选"生成字幕"
5. 点击"生成语音"

**预期结果：**
- 只生成前3句
- 每句有500ms停顿
- 生成字幕
- 控制台显示：`🎧 试听模式 | 📝 18 字符 | ⏱️ 2.5秒`

---

## 📊 UI 布局

```
┌─────────────────────────────────────────┐
│  文本输入框                              │
│  ┌────────────────────────────────────┐ │
│  │ 输入要转换的文本...                 │ │
│  └────────────────────────────────────┘ │
│                                          │
│  语言选择: [中文（简体）▼]               │
│  配音员: [zh-CN-XiaoxiaoNeural▼]        │
│                                          │
│  音调: ━━━●━━━ 0                        │
│  语速: ━━━●━━━ 0                        │
│  音量: ━━━━━━●━ 80                      │
│                                          │
│  ☑ 生成字幕                              │
│                                          │
│  ┌─ 高级功能 ──────────────────────┐    │
│  │ ☐ 试听模式（只合成前 [3] 句）    │    │
│  │ 句间停顿：[0] 毫秒 (0=无停顿)    │    │
│  │ ☐ 长文本切片（每片 [500] 字）    │    │
│  └──────────────────────────────────┘    │
│                                          │
│  [🎵 生成语音]                           │
└─────────────────────────────────────────┘
```

---

## 🎨 样式说明

### 高级功能面板

- 背景色：浅灰色（亮色模式）/ 深灰色（暗色模式）
- 边框：细边框
- 圆角：8px
- 内边距：16px
- 图标：Material Symbols `tune`

### 控件样式

- 复选框：4x4px，主题色
- 数字输入：小尺寸，带边框
- 标签：小字体，中等粗细

---

## 🔧 JavaScript 逻辑

### 读取参数

```javascript
const previewMode = $('#previewMode').is(':checked');
const previewSentences = parseInt($('#previewSentences').val()) || 3;
const breakTime = parseInt($('#breakTime').val()) || 0;
const enableLongTextSplit = $('#enableLongTextSplit').is(':checked');
const maxCharsPerChunk = parseInt($('#maxCharsPerChunk').val()) || 500;
```

### 构建请求

```javascript
const request = {
    Text: text,
    Voice: voiceId,
    Language: language,
    Pitch: pitch,
    Rate: rate,
    Volume: volume,
    GenerateSubtitles: generateSubtitles,
    // 高级功能
    PreviewMode: previewMode,
    PreviewSentences: previewSentences,
    BreakTime: breakTime,
    EnableLongTextSplit: enableLongTextSplit,
    MaxCharsPerChunk: maxCharsPerChunk
};
```

### 显示元数据

```javascript
if (response.isPreview) metaInfo.push('🎧 试听模式');
if (response.chunkCount > 1) metaInfo.push(`📄 ${response.chunkCount} 个片段`);
if (response.totalCharacters) metaInfo.push(`📝 ${response.totalCharacters} 字符`);
if (response.processingTimeMs) metaInfo.push(`⏱️ ${(response.processingTimeMs / 1000).toFixed(1)}秒`);
```

---

## 📝 控制台输出示例

### 普通模式

```
发送请求: {
  Text: "测试文本...",
  Voice: "zh-CN-XiaoxiaoNeural",
  PreviewMode: false,
  BreakTime: 0,
  EnableLongTextSplit: false
}

收到响应: {
  success: true,
  audioData: "...",
  chunkCount: 1,
  totalCharacters: 50,
  processingTimeMs: 1500
}

处理信息: 📝 50 字符 | ⏱️ 1.5秒
```

### 试听模式

```
发送请求: {
  PreviewMode: true,
  PreviewSentences: 3,
  ...
}

收到响应: {
  success: true,
  isPreview: true,
  totalCharacters: 18,
  processingTimeMs: 800
}

处理信息: 🎧 试听模式 | 📝 18 字符 | ⏱️ 0.8秒
```

### 长文本切片

```
发送请求: {
  EnableLongTextSplit: true,
  MaxCharsPerChunk: 500,
  ...
}

收到响应: {
  success: true,
  chunkCount: 5,
  totalCharacters: 2000,
  processingTimeMs: 8000
}

处理信息: 📄 5 个片段 | 📝 2000 字符 | ⏱️ 8.0秒
```

---

## ⚠️ 注意事项

### 1. 参数验证

- 试听句子数：1-10
- 停顿时间：0-2000ms
- 切片大小：100-2000字

### 2. 组合使用

**推荐组合：**
- ✅ 试听 + 停顿
- ✅ 长文本 + 字幕
- ✅ 长文本 + 停顿 + 字幕

**不推荐：**
- ❌ 试听 + 长文本切片（试听模式会先截取文本）

### 3. 性能考虑

- 长文本切片会增加处理时间
- 建议切片大小：500-1000字
- 试听模式可快速预览

---

## 🚀 下一步优化

### 可选功能：

1. **进度条显示**
   - 显示切片处理进度
   - 实时更新状态

2. **元数据面板**
   - 在页面上显示处理信息
   - 不仅在控制台

3. **预设配置**
   - 保存常用配置
   - 一键应用

4. **批量处理**
   - 支持多个文本
   - 队列处理

---

## 📞 故障排除

### 问题 1：控件不显示

**检查：**
- 清除浏览器缓存
- 硬刷新（Ctrl+Shift+R）

### 问题 2：参数不生效

**检查：**
- 打开控制台查看请求
- 确认参数正确传递

### 问题 3：样式错乱

**检查：**
- 确认 Tailwind CSS 加载
- 检查暗色模式

---

**最后更新：** 2025年1月1日  
**状态：** ✅ 前端集成完成，可以测试
