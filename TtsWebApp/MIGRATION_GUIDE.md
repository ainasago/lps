# 文章中英文支持 - 数据库迁移指南

## 概述
本次更新为 Article 模型添加了两个新字段以支持中英文文章：
- `Language`: 语言标识（zh-CN 或 en-US）
- `RelatedArticleId`: 关联文章ID（用于关联中英文版本）

## 迁移步骤

### 1. 停止应用程序
确保应用程序已停止运行，否则无法执行数据库迁移。

### 2. 创建迁移
```bash
cd d:\1Dev\dev\webs\tts_turi\TtsWebApp
dotnet ef migrations add AddArticleLanguageFields
```

### 3. 更新数据库
```bash
dotnet ef database update
```

### 4. 更新现有数据（可选）
如果数据库中已有文章数据，需要手动设置语言字段：

```sql
-- 将所有现有文章设置为中文
UPDATE Articles 
SET Language = 'zh-CN' 
WHERE Language IS NULL OR Language = '';
```

## 新字段说明

### Language 字段
- **类型**: string (最大长度 10)
- **默认值**: "zh-CN"
- **可选值**: 
  - "zh-CN" - 中文
  - "en-US" - 英文
- **用途**: 标识文章的语言

### RelatedArticleId 字段
- **类型**: int? (可为空)
- **默认值**: null
- **用途**: 指向同一篇文章的另一个语言版本的 ID

## 使用示例

### 创建中英文文章对
1. 创建中文文章（ID: 1）
   - Title: "如何使用 TTS"
   - Language: "zh-CN"
   - RelatedArticleId: 2

2. 创建英文文章（ID: 2）
   - Title: "How to Use TTS"
   - Language: "en-US"
   - RelatedArticleId: 1

这样两篇文章就互相关联了，用户可以在文章详情页切换语言。

## 功能说明

### 文章列表页
- URL: `/Article/Index?lang=zh-CN` 或 `/Article/Index?lang=en-US`
- 根据 `lang` 参数筛选对应语言的文章
- 默认显示中文文章

### 文章详情页
- 如果文章有关联的其他语言版本，会显示语言切换按钮
- 点击按钮可以跳转到对应语言的文章

## 后台管理更新（待实现）
后续需要在后台管理界面添加：
1. 语言选择下拉框
2. 关联文章选择器
3. 批量设置语言功能

## 注意事项
1. 新创建的文章默认为中文（zh-CN）
2. RelatedArticleId 是双向关联，需要手动设置两篇文章的关联
3. 页面类型（Type = Page）的文章已经通过 Slug 实现了中英文（如 about 和 about-en），不需要使用这个字段
