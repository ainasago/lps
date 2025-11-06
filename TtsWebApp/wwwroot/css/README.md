# VoiceGen UI 样式指南

## 概述

VoiceGen 的 UI 样式采用现代化的设计系统，支持亮色/暗色主题切换，使用 Tailwind CSS 框架和自定义 CSS 类。

## 文件说明

### 1. `voicegen.css`
独立的 CSS 文件，包含所有自定义样式类。可以在不使用 Tailwind 的情况下使用这些预定义的类。

### 2. `tailwind.config.js`
Tailwind CSS 配置文件，定义了自定义颜色、字体和其他主题扩展。

## 设计系统

### 颜色方案

```css
--color-primary: #5b13ec          /* 主色调 - 紫色 */
--color-background-light: #f6f6f8 /* 浅色背景 */
--color-background-dark: #161022  /* 深色背景 */
```

### 字体

- **主字体**: Space Grotesk (Google Fonts)
- **图标**: Material Symbols Outlined (Google Icons)

### 主题切换

通过在 `<html>` 标签上添加/移除 `dark` 类来切换主题：

```html
<!-- 亮色主题 -->
<html lang="en">

<!-- 暗色主题 -->
<html class="dark" lang="en">
```

## 主要组件类

### 布局组件

- `.voicegen-container` - 响应式容器
- `.voicegen-section-padding` - 标准区块内边距
- `.voicegen-header` - 粘性头部
- `.voicegen-footer` - 页脚

### 按钮组件

- `.voicegen-btn-primary` - 主要按钮
- `.voicegen-btn-primary-lg` - 大号主要按钮
- `.voicegen-play-btn` - 播放按钮
- `.voicegen-download-btn` - 下载按钮

### 表单组件

- `.voicegen-textarea` - 文本输入框
- `.voicegen-select` - 下拉选择框
- `.voicegen-range` - 滑块控件
- `.voicegen-voice-btn` - 语音选择按钮
  - 添加 `.active` 类表示选中状态

### 卡片组件

- `.voicegen-tool-card` - 主工具卡片
- `.voicegen-feature-card` - 功能特性卡片
- `.voicegen-usecase-card` - 使用场景卡片

### 音频播放器

- `.voicegen-audio-player` - 播放器容器
- `.voicegen-progress-bar` - 进度条
- `.voicegen-time-display` - 时间显示

### 导航组件

- `.voicegen-nav` - 导航菜单
- `.voicegen-nav-link` - 导航链接
- `.voicegen-logo` - Logo 容器
- `.voicegen-logo-icon` - Logo 图标
- `.voicegen-logo-text` - Logo 文字

## 使用方式

### 方式一：使用 Tailwind CSS（推荐用于新页面）

```html
<!DOCTYPE html>
<html class="dark" lang="en">
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>VoiceGen - AI Text to Speech</title>
    
    <!-- Tailwind CSS CDN -->
    <script src="https://cdn.tailwindcss.com?plugins=forms,container-queries"></script>
    
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;700&display=swap" rel="stylesheet"/>
    
    <!-- Material Icons -->
    <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" rel="stylesheet"/>
    
    <!-- Tailwind Config -->
    <script src="/css/tailwind.config.js"></script>
    
    <!-- Custom Styles -->
    <link href="/css/voicegen.css" rel="stylesheet"/>
</head>
<body class="bg-background-light dark:bg-background-dark font-display">
    <!-- 内容 -->
</body>
</html>
```

### 方式二：仅使用自定义 CSS 类

```html
<!DOCTYPE html>
<html class="dark" lang="en">
<head>
    <meta charset="utf-8"/>
    <meta name="viewport" content="width=device-width, initial-scale=1.0"/>
    <title>VoiceGen</title>
    
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;700&display=swap" rel="stylesheet"/>
    
    <!-- Material Icons -->
    <link href="https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined" rel="stylesheet"/>
    
    <!-- VoiceGen Styles -->
    <link href="/css/voicegen.css" rel="stylesheet"/>
</head>
<body>
    <header class="voicegen-header">
        <div class="voicegen-container">
            <div class="voicegen-logo">
                <span class="material-symbols-outlined voicegen-logo-icon">audiotrack</span>
                <h2 class="voicegen-logo-text">VoiceGen</h2>
            </div>
        </div>
    </header>
    
    <main>
        <section class="voicegen-section-padding">
            <div class="voicegen-container">
                <h1 class="voicegen-hero-title">Your Title</h1>
                <p class="voicegen-hero-subtitle">Your subtitle</p>
            </div>
        </section>
    </main>
</body>
</html>
```

## 响应式设计

所有组件都支持响应式设计，主要断点：

- **sm**: 640px
- **md**: 768px
- **lg**: 1024px
- **xl**: 1280px

## 示例：创建一个按钮

```html
<!-- 使用自定义类 -->
<button class="voicegen-btn-primary">
    <span class="material-symbols-outlined">play_arrow</span>
    <span>播放</span>
</button>

<!-- 使用 Tailwind（如果已加载） -->
<button class="flex items-center justify-center px-4 py-2 bg-primary text-white rounded-lg">
    <span class="material-symbols-outlined">play_arrow</span>
    <span>播放</span>
</button>
```

## 示例：创建一个语音选择按钮

```html
<button class="voicegen-voice-btn active">
    <span class="material-symbols-outlined voicegen-voice-btn-icon">man</span>
    <p class="voicegen-voice-btn-text">David</p>
</button>
```

## 暗色主题切换脚本

```javascript
// 切换暗色主题
function toggleDarkMode() {
    document.documentElement.classList.toggle('dark');
    
    // 保存到 localStorage
    const isDark = document.documentElement.classList.contains('dark');
    localStorage.setItem('darkMode', isDark);
}

// 页面加载时恢复主题设置
window.addEventListener('DOMContentLoaded', () => {
    const isDark = localStorage.getItem('darkMode') === 'true';
    if (isDark) {
        document.documentElement.classList.add('dark');
    }
});
```

## 自定义和扩展

如需自定义颜色或样式，可以：

1. 修改 `voicegen.css` 中的 CSS 变量
2. 修改 `tailwind.config.js` 中的主题配置
3. 添加新的自定义类到 `voicegen.css`

## 浏览器兼容性

- Chrome/Edge: 最新版本
- Firefox: 最新版本
- Safari: 最新版本
- 移动浏览器: iOS Safari, Chrome Mobile

## 性能优化建议

1. 在生产环境中使用本地 Tailwind CSS 构建而非 CDN
2. 压缩 CSS 文件
3. 使用字体子集减少加载时间
4. 考虑使用 CSS 预加载

```html
<link rel="preload" href="/css/voicegen.css" as="style">
```
