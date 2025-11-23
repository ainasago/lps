// 恢复备份的 JavaScript 文件

const fs = require('fs');
const path = require('path');

const backupDir = 'wwwroot/js/backup';
const targetDir = 'wwwroot/js';

console.log('🔄 开始恢复 JavaScript 文件...\n');

if (!fs.existsSync(backupDir)) {
    console.log('❌ 备份目录不存在！');
    console.log(`   请确保 ${backupDir} 目录存在\n`);
    process.exit(1);
}

const backupFiles = fs.readdirSync(backupDir).filter(f => f.endsWith('.backup'));

if (backupFiles.length === 0) {
    console.log('⚠️  没有找到备份文件！\n');
    process.exit(0);
}

let restoredCount = 0;

backupFiles.forEach(backupFile => {
    try {
        const originalFileName = backupFile.replace('.backup', '');
        const backupPath = path.join(backupDir, backupFile);
        const targetPath = path.join(targetDir, originalFileName);
        
        // 读取备份文件
        const backupContent = fs.readFileSync(backupPath, 'utf8');
        
        // 恢复到原位置
        fs.writeFileSync(targetPath, backupContent);
        
        console.log(`✅ 已恢复: ${originalFileName}`);
        restoredCount++;
        
    } catch (error) {
        console.error(`❌ 恢复失败: ${backupFile}`);
        console.error(`   错误: ${error.message}`);
    }
});

console.log(`\n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━`);
console.log(`✨ 恢复完成！共恢复 ${restoredCount} 个文件`);
console.log('━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n');
