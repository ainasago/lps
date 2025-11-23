# JavaScript 混淆使用说明

## 📋 概述

本项目使用 `javascript-obfuscator` 实现类似 Node.js 项目的强混淆效果，保护前端代码不被轻易逆向。

## 🚀 快速开始

### 1. 安装依赖

首次使用需要安装 Node.js 依赖：

```bash
npm install
```

### 2. 混淆模式

#### 开发模式混淆（推荐用于测试）

生成混淆后的文件到 `wwwroot/js/obfuscated/` 目录，**不会修改原文件**：

```bash
npm run obfuscate
```

**特点**：
- ✅ 不修改原文件
- ✅ 输出到独立目录
- ✅ 可以对比混淆效果
- ✅ 安全测试

#### 生产模式混淆（用于发布）

**直接替换原文件**，用于正式发布：

```bash
npm run obfuscate:prod
```

**特点**：
- ⚠️ 会直接替换原文件
- ✅ 自动备份原文件到 `wwwroot/js/backup/`
- ✅ 最强混淆配置
- ✅ 启用调试保护
- ✅ 禁用 console 输出

### 3. 一键发布

混淆 + 发布一步完成：

```bash
npm run publish
```

这个命令会：
1. 执行生产模式混淆
2. 自动执行 `dotnet publish -c Release`

### 4. 恢复原文件

如果需要恢复被混淆的原文件：

```bash
npm run restore
```

## 📁 文件结构

```
TtsWebApp/
├── wwwroot/js/
│   ├── tts.js              # 原始文件（生产混淆后会被替换）
│   ├── i18n.js             # 原始文件（生产混淆后会被替换）
│   ├── site.js             # 原始文件（生产混淆后会被替换）
│   ├── obfuscated/         # 开发模式混淆输出目录
│   │   ├── tts.js
│   │   ├── i18n.js
│   │   └── site.js
│   └── backup/             # 生产模式备份目录
│       ├── tts.js.backup
│       ├── i18n.js.backup
│       └── site.js.backup
├── obfuscate.js            # 开发模式混淆脚本
├── obfuscate-production.js # 生产模式混淆脚本
├── restore-js.js           # 恢复脚本
└── package.json            # NPM 配置
```

## 🔒 混淆特性

### 开发模式配置

适合测试和预览混淆效果：

- ✅ 控制流扁平化（75%）
- ✅ 死代码注入（40%）
- ✅ 字符串数组编码（Base64）
- ✅ 标识符混淆（十六进制）
- ✅ 自我保护
- ✅ 字符串分割

### 生产模式配置（最强）

用于正式发布，提供最强保护：

- 🔥 控制流扁平化（100%）
- 🔥 死代码注入（50%）
- 🔥 字符串数组编码（RC4）
- 🔥 调试保护（启用）
- 🔥 禁用 console 输出
- 🔥 标识符混淆（十六进制）
- 🔥 自我保护
- 🔥 字符串分割（更小块）
- 🔥 对象键转换
- 🔥 数字转表达式

## 📊 混淆效果对比

### 原始代码示例

```javascript
function convertText() {
    const text = document.getElementById('inputText').value;
    const voice = document.getElementById('voiceSelect').value;
    
    if (!text) {
        alert('请输入文本');
        return;
    }
    
    fetch('/api/tts/convert', {
        method: 'POST',
        body: JSON.stringify({ text, voice })
    });
}
```

### 混淆后效果

```javascript
var _0x4a2b=['getElementById','value','inputText','voiceSelect','请输入文本','alert','/api/tts/convert','POST','stringify'];(function(_0x2d8f05,_0x4b81bb){var _0x4d74cb=function(_0x32719f){while(--_0x32719f){_0x2d8f05['push'](_0x2d8f05['shift']());}};_0x4d74cb(++_0x4b81bb);}(_0x4a2b,0x1f4));var _0x4d74=function(_0x2d8f05,_0x4b81bb){_0x2d8f05=_0x2d8f05-0x0;var _0x4d74cb=_0x4a2b[_0x2d8f05];return _0x4d74cb;};function convertText(){const _0x3a7c8b=document[_0x4d74('0x0')](_0x4d74('0x2'))[_0x4d74('0x1')];const _0x2e9d4a=document[_0x4d74('0x0')](_0x4d74('0x3'))[_0x4d74('0x1')];if(!_0x3a7c8b){alert(_0x4d74('0x4'));return;}fetch(_0x4d74('0x6'),{'method':_0x4d74('0x7'),'body':JSON[_0x4d74('0x8')]({'text':_0x3a7c8b,'voice':_0x2e9d4a})});}
```

**特点**：
- 变量名全部混淆为十六进制
- 字符串提取到数组并编码
- 控制流被打乱
- 添加了反调试代码
- 几乎不可读

## 🔄 工作流程

### 开发阶段

1. 正常编写和修改 JS 文件
2. 使用 `npm run obfuscate` 测试混淆效果
3. 查看 `wwwroot/js/obfuscated/` 中的混淆结果

### 发布阶段

#### 方案一：一键发布（推荐）

```bash
npm run publish
```

#### 方案二：分步执行

```bash
# 1. 混淆
npm run obfuscate:prod

# 2. 发布
dotnet publish -c Release

# 3. （可选）恢复原文件继续开发
npm run restore
```

## ⚠️ 注意事项

### 1. 备份重要性

生产模式混淆会**直接替换原文件**，虽然会自动备份，但建议：
- ✅ 使用 Git 版本控制
- ✅ 定期提交代码
- ✅ 发布前确保代码已提交

### 2. 性能影响

强混淆会增加文件大小和运行时开销：
- 文件大小：通常增加 50-100%
- 运行性能：轻微影响（约 5-10%）
- 加载时间：略有增加

### 3. 调试困难

混淆后的代码几乎无法调试：
- ❌ 无法设置断点
- ❌ 变量名不可读
- ❌ 控制流混乱

**建议**：
- 开发时使用原始文件
- 只在发布时混淆
- 保留原始文件用于调试

### 4. 兼容性

混淆后的代码可能在某些情况下出现问题：
- 使用 `eval()` 的代码
- 依赖变量名的反射代码
- 某些第三方库

**解决方案**：
- 在 `obfuscate-production.js` 中设置 `renameGlobals: false`
- 使用 `reservedNames` 保留特定变量名

## 🛠️ 自定义配置

### 修改混淆强度

编辑 `obfuscate-production.js`：

```javascript
const obfuscationOptions = {
    // 降低混淆强度（提高性能）
    controlFlowFlatteningThreshold: 0.5,  // 从 1 降到 0.5
    deadCodeInjectionThreshold: 0.2,      // 从 0.5 降到 0.2
    
    // 或提高混淆强度（更强保护）
    stringArrayEncoding: ['rc4', 'base64'], // 双重编码
    selfDefending: true,                    // 启用自我保护
};
```

### 添加更多文件

编辑 `obfuscate-production.js`：

```javascript
const filesToObfuscate = [
    'wwwroot/js/tts.js',
    'wwwroot/js/i18n.js',
    'wwwroot/js/site.js',
    'wwwroot/js/custom.js',  // 添加新文件
];
```

## 📈 混淆级别对比

| 特性 | 开发模式 | 生产模式 |
|------|---------|---------|
| 控制流扁平化 | 75% | 100% |
| 死代码注入 | 40% | 50% |
| 字符串编码 | Base64 | RC4 |
| 调试保护 | ❌ | ✅ |
| Console 禁用 | ❌ | ✅ |
| 自我保护 | ✅ | ✅ |
| 文件大小增加 | ~60% | ~80% |
| 性能影响 | 轻微 | 中等 |

## 🎯 最佳实践

1. **开发时**：使用原始文件，不混淆
2. **测试时**：使用 `npm run obfuscate` 测试混淆效果
3. **发布前**：确保代码已提交到 Git
4. **发布时**：使用 `npm run publish` 一键发布
5. **发布后**：使用 `npm run restore` 恢复原文件

## 🔍 故障排除

### 问题：混淆后代码不工作

**解决方案**：
1. 检查是否使用了 `eval()`
2. 设置 `renameGlobals: false`
3. 使用 `reservedNames` 保留关键变量

### 问题：文件太大

**解决方案**：
1. 降低 `deadCodeInjectionThreshold`
2. 禁用 `stringArrayEncoding`
3. 减少 `stringArrayWrappersCount`

### 问题：性能下降明显

**解决方案**：
1. 降低 `controlFlowFlatteningThreshold`
2. 禁用 `numbersToExpressions`
3. 减少 `splitStringsChunkLength`

## 📚 参考资源

- [javascript-obfuscator 官方文档](https://github.com/javascript-obfuscator/javascript-obfuscator)
- [在线混淆工具](https://obfuscator.io/)
- [配置选项详解](https://github.com/javascript-obfuscator/javascript-obfuscator#options)

---

**提示**：混淆只是代码保护的一部分，不能完全防止逆向工程。核心业务逻辑应该放在服务器端。
