// 生产环境 JavaScript 混淆脚本
// 直接替换原文件，用于发布时

const JavaScriptObfuscator = require('javascript-obfuscator');
const fs = require('fs');
const path = require('path');

// 生产环境混淆配置（最强混淆）
const obfuscationOptions = {
    compact: true,
    controlFlowFlattening: true,
    controlFlowFlatteningThreshold: 1,
    deadCodeInjection: true,
    deadCodeInjectionThreshold: 0.5,
    debugProtection: true,
    debugProtectionInterval: 2000,
    disableConsoleOutput: true,
    identifierNamesGenerator: 'hexadecimal',
    log: false,
    numbersToExpressions: true,
    renameGlobals: false,
    selfDefending: true,
    simplify: true,
    splitStrings: true,
    splitStringsChunkLength: 5,
    stringArray: true,
    stringArrayCallsTransform: true,
    stringArrayCallsTransformThreshold: 1,
    stringArrayEncoding: ['rc4'],
    stringArrayIndexShift: true,
    stringArrayRotate: true,
    stringArrayShuffle: true,
    stringArrayWrappersCount: 5,
    stringArrayWrappersChainedCalls: true,
    stringArrayWrappersParametersMaxCount: 5,
    stringArrayWrappersType: 'function',
    stringArrayThreshold: 1,
    transformObjectKeys: true,
    unicodeEscapeSequence: false
};

// 要混淆的文件列表
const filesToObfuscate = [
    'wwwroot/js/tts.js',
    'wwwroot/js/i18n.js',
    'wwwroot/js/site.js'
];

console.log('🚀 开始生产环境混淆...\n');
console.log('⚠️  警告：此操作将直接替换原文件！\n');

// 备份目录
const backupDir = 'wwwroot/js/backup';
if (!fs.existsSync(backupDir)) {
    fs.mkdirSync(backupDir, { recursive: true });
}

let successCount = 0;
let failCount = 0;

filesToObfuscate.forEach(filePath => {
    try {
        if (!fs.existsSync(filePath)) {
            console.log(`⚠️  文件不存在: ${filePath}`);
            failCount++;
            return;
        }

        // 读取源文件
        const sourceCode = fs.readFileSync(filePath, 'utf8');
        
        // 备份原文件
        const fileName = path.basename(filePath);
        const backupPath = path.join(backupDir, `${fileName}.backup`);
        fs.writeFileSync(backupPath, sourceCode);
        
        // 混淆代码
        console.log(`🔄 正在混淆: ${fileName}...`);
        const obfuscationResult = JavaScriptObfuscator.obfuscate(sourceCode, obfuscationOptions);
        
        // 直接替换原文件
        fs.writeFileSync(filePath, obfuscationResult.getObfuscatedCode());
        
        const originalSize = (sourceCode.length / 1024).toFixed(2);
        const obfuscatedSize = (obfuscationResult.getObfuscatedCode().length / 1024).toFixed(2);
        const ratio = ((obfuscatedSize / originalSize) * 100).toFixed(1);
        
        console.log(`✅ ${fileName} 混淆成功`);
        console.log(`   原始: ${originalSize} KB → 混淆: ${obfuscatedSize} KB (${ratio}%)`);
        console.log(`   备份: ${backupPath}\n`);
        
        successCount++;
        
    } catch (error) {
        console.error(`❌ 混淆失败: ${filePath}`);
        console.error(`   错误: ${error.message}\n`);
        failCount++;
    }
});

console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━');
console.log(`✨ 混淆完成！`);
console.log(`   成功: ${successCount} 个文件`);
console.log(`   失败: ${failCount} 个文件`);
console.log(`   备份位置: ${backupDir}`);
console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');

if (successCount > 0) {
    console.log('📝 提示：');
    console.log('   1. 原文件已被混淆版本替换');
    console.log('   2. 原始文件已备份到 wwwroot/js/backup/');
    console.log('   3. 如需恢复，请从备份目录复制回来');
    console.log('   4. 现在可以执行 dotnet publish 发布项目\n');
}
