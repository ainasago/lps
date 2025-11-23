// JavaScript 混淆脚本
// 使用 javascript-obfuscator 进行强混淆

const JavaScriptObfuscator = require('javascript-obfuscator');
const fs = require('fs');
const path = require('path');

// 混淆配置（类似 Node.js 项目的强混淆）
const obfuscationOptions = {
    compact: true,
    controlFlowFlattening: true,
    controlFlowFlatteningThreshold: 0.75,
    deadCodeInjection: true,
    deadCodeInjectionThreshold: 0.4,
    debugProtection: false,
    debugProtectionInterval: 0,
    disableConsoleOutput: false,
    identifierNamesGenerator: 'hexadecimal',
    log: false,
    numbersToExpressions: true,
    renameGlobals: false,
    selfDefending: true,
    simplify: true,
    splitStrings: true,
    splitStringsChunkLength: 10,
    stringArray: true,
    stringArrayCallsTransform: true,
    stringArrayCallsTransformThreshold: 0.75,
    stringArrayEncoding: ['base64'],
    stringArrayIndexShift: true,
    stringArrayRotate: true,
    stringArrayShuffle: true,
    stringArrayWrappersCount: 2,
    stringArrayWrappersChainedCalls: true,
    stringArrayWrappersParametersMaxCount: 4,
    stringArrayWrappersType: 'function',
    stringArrayThreshold: 0.75,
    transformObjectKeys: true,
    unicodeEscapeSequence: false
};

// 要混淆的文件列表
const filesToObfuscate = [
    'wwwroot/js/tts.js',
    'wwwroot/js/i18n.js',
    'wwwroot/js/site.js'
];

// 输出目录
const outputDir = 'wwwroot/js/obfuscated';

// 创建输出目录
if (!fs.existsSync(outputDir)) {
    fs.mkdirSync(outputDir, { recursive: true });
}

console.log('开始混淆 JavaScript 文件...\n');

filesToObfuscate.forEach(filePath => {
    try {
        if (!fs.existsSync(filePath)) {
            console.log(`⚠️  文件不存在: ${filePath}`);
            return;
        }

        // 读取源文件
        const sourceCode = fs.readFileSync(filePath, 'utf8');
        
        // 混淆代码
        const obfuscationResult = JavaScriptObfuscator.obfuscate(sourceCode, obfuscationOptions);
        
        // 生成输出文件路径
        const fileName = path.basename(filePath);
        const outputPath = path.join(outputDir, fileName);
        
        // 写入混淆后的代码
        fs.writeFileSync(outputPath, obfuscationResult.getObfuscatedCode());
        
        const originalSize = (sourceCode.length / 1024).toFixed(2);
        const obfuscatedSize = (obfuscationResult.getObfuscatedCode().length / 1024).toFixed(2);
        
        console.log(`✅ ${fileName}`);
        console.log(`   原始大小: ${originalSize} KB`);
        console.log(`   混淆后: ${obfuscatedSize} KB`);
        console.log(`   输出: ${outputPath}\n`);
        
    } catch (error) {
        console.error(`❌ 混淆失败: ${filePath}`);
        console.error(`   错误: ${error.message}\n`);
    }
});

console.log('混淆完成！');
console.log(`混淆后的文件保存在: ${outputDir}`);
