# 多语言功能说明

## 已实现的功能

### 1. 浏览器语言自动检测

**功能描述：**
- 自动检测用户浏览器的语言设置
- 根据浏览器语言自动选择对应的 TTS 配音语言
- 自动选择该语言的第一个配音员

**支持的语言映射：**
```javascript
{
    'zh': 'zh-CN',      // 中文 → 简体中文
    'zh-CN': 'zh-CN',   // 简体中文
    'zh-TW': 'zh-TW',   // 繁体中文（台湾）
    'zh-HK': 'zh-HK',   // 繁体中文（香港）
    'en': 'en-US',      // 英文 → 美式英语
    'en-US': 'en-US',   // 美式英语
    'en-GB': 'en-GB',   // 英式英语
    'ja': 'ja-JP',      // 日语
    'ko': 'ko-KR',      // 韩语
    'fr': 'fr-FR',      // 法语
    'de': 'de-DE',      // 德语
    'es': 'es-ES'       // 西班牙语
}
```

**匹配逻辑：**
1. 首先尝试完全匹配（如 zh-CN 匹配 zh-CN）
2. 如果没有完全匹配，尝试匹配语言代码（如 zh-CN 匹配 zh）
3. 检查匹配的语言是否在可用语言列表中
4. 如果精确匹配失败，尝试模糊匹配（如 zh-CN 匹配 zh-*）
5. **如果都没有匹配到，默认选择英文（en-US）**
6. 如果没有 en-US，选择任何 en- 开头的语言
7. 如果还是没有，选择第一个可用语言

**实现位置：**
- 文件：`TtsWebApp/Views/Tts/Index.cshtml`
- 函数：`autoSelectLanguage()`

### 2. 网站多语言切换

**功能描述：**
- 支持中文（zh-CN）和英文（en-US）两种界面语言
- 语言设置保存在 localStorage，刷新页面后保持
- 点击语言按钮即可切换界面语言
- 所有静态文本自动翻译

**语言切换按钮位置：**
- 位于网站右上角，主题切换按钮旁边
- 显示为 "中文 | EN" 的切换按钮
- 当前选中的语言会高亮显示（紫色背景）

**实现位置：**
- 语言资源文件：`TtsWebApp/wwwroot/js/i18n.js`
- 布局文件：`TtsWebApp/Views/Shared/_Layout.cshtml`

### 3. 多语言资源

**已翻译的内容：**

#### 导航菜单
- 首页 / Home
- 关于我们 / About Us
- 联系我们 / Contact

#### TTS 工具界面
- 标题和副标题
- 表单标签（输入文本、语言、配音员、音调、语速、音量）
- 按钮文本（生成语音、播放、下载音频、下载字幕）
- 提示信息

#### Features 部分
- 标题和描述
- 4个特性卡片的标题和描述

#### Use Cases 部分
- 标题和描述
- 3个使用场景的标题和描述

#### 页脚
- 品牌描述
- 各栏目标题
- 版权信息

## 使用方法

### 用户使用

1. **自动语言检测：**
   - 首次访问网站时，系统会自动检测浏览器语言
   - 自动选择对应的 TTS 配音语言
   - 无需手动设置

2. **手动切换语言：**
   - 点击右上角的语言切换按钮
   - 选择 "中文" 或 "EN"
   - 页面会立即切换到选定的语言
   - 设置会自动保存

### 开发者使用

#### 添加新的翻译文本

1. 在 `i18n.js` 中添加翻译键值对：

```javascript
const translations = {
    'zh-CN': {
        'your.key': '中文文本'
    },
    'en-US': {
        'your.key': 'English Text'
    }
};
```

2. 在 HTML 元素中添加 `data-i18n` 属性：

```html
<p data-i18n="your.key">中文文本</p>
```

#### 添加占位符替换

```javascript
// 在 i18n.js 中
'home.charCount': '{0} / 5000'

// 在 JavaScript 中使用
const text = t('home.charCount', currentLength);
```

#### 为 input/textarea 添加翻译

```html
<input placeholder="默认文本" data-i18n="your.key" />
<textarea placeholder="默认文本" data-i18n="your.key"></textarea>
```

## 技术实现

### 核心文件

1. **i18n.js** - 多语言核心库
   - 翻译资源存储
   - 语言切换逻辑
   - 页面更新函数

2. **_Layout.cshtml** - 布局文件
   - 语言切换按钮
   - i18n.js 引用
   - 语言按钮样式

3. **Index.cshtml** - TTS 主页
   - 浏览器语言检测
   - 自动选择配音语言
   - 多语言属性标记

### 工作流程

```
页面加载
    ↓
检查 localStorage 中的语言设置
    ↓
如果没有设置，检测浏览器语言
    ↓
加载对应的翻译资源
    ↓
更新页面所有 data-i18n 元素
    ↓
用户点击语言切换按钮
    ↓
保存语言设置到 localStorage
    ↓
重新更新页面内容
```

## 浏览器兼容性

- ✅ Chrome 90+
- ✅ Firefox 88+
- ✅ Safari 14+
- ✅ Edge 90+
- ✅ 移动端浏览器

## 注意事项

1. **localStorage 依赖**
   - 语言设置保存在 localStorage
   - 如果用户清除浏览器数据，设置会重置

2. **JavaScript 必需**
   - 多语言功能依赖 JavaScript
   - 如果 JavaScript 被禁用，将显示默认文本（中文）

3. **翻译完整性**
   - 目前主要页面已翻译
   - 其他页面（关于我们、联系我们等）需要继续添加翻译

4. **SEO 考虑**
   - 当前实现是客户端翻译
   - 搜索引擎会索引默认语言（中文）
   - 如需更好的 SEO，建议实现服务端多语言路由

## 未来改进

### 短期计划
- [ ] 完成所有页面的翻译
- [ ] 添加更多语言支持（日语、韩语等）
- [ ] 优化翻译质量

### 长期计划
- [ ] 实现服务端多语言路由（/zh-CN/, /en-US/）
- [ ] 添加 hreflang 标签提升 SEO
- [ ] 支持用户自定义翻译
- [ ] 添加语言检测 API（基于 IP 地址）

## 测试清单

- [x] 浏览器语言检测功能
- [x] 语言切换按钮功能
- [x] localStorage 持久化
- [x] 页面刷新后保持语言设置
- [x] 中文界面显示正确
- [x] 英文界面显示正确
- [x] 自动选择配音语言
- [x] 响应式设计兼容

## 示例代码

### 检测浏览器语言

```javascript
const browserLang = navigator.language || navigator.userLanguage;
console.log('浏览器语言:', browserLang); // 输出: zh-CN, en-US, etc.
```

### 切换语言

```javascript
// 切换到英文
switchLanguage('en-US');

// 切换到中文
switchLanguage('zh-CN');
```

### 获取翻译文本

```javascript
// 简单翻译
const text = t('nav.home'); // 返回: "首页" 或 "Home"

// 带占位符的翻译
const text = t('home.charCount', 100); // 返回: "100 / 5000"
```

## 相关文件

- `TtsWebApp/wwwroot/js/i18n.js` - 多语言核心库
- `TtsWebApp/Views/Shared/_Layout.cshtml` - 布局文件（包含语言切换按钮）
- `TtsWebApp/Views/Tts/Index.cshtml` - TTS 主页（包含自动语言检测）

## 联系方式

如有问题或建议，请联系：contact@annietts.com

---

**最后更新：** 2025年1月1日  
**版本：** 1.0
