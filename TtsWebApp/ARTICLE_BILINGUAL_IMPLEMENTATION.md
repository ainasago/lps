# 文章中英文支持实现方案（方案一）

## ✅ 已完成的修改

### 1. 数据模型修改
**文件**: `Models/AdminModels.cs`

添加了两个新字段到 `Article` 类：
```csharp
// 语言标识：zh-CN=中文, en-US=英文
[StringLength(10)]
public string Language { get; set; } = "zh-CN";

// 关联文章ID（用于关联中英文版本，指向另一个语言的文章）
public int? RelatedArticleId { get; set; }
```

### 2. 控制器修改
**文件**: `Controllers/ArticleController.cs`

#### Index 方法（文章列表）
- 添加 `lang` 参数，默认为 "zh-CN"
- 根据语言筛选文章：`a.Language == currentLang`
- 将当前语言传递给视图：`ViewBag.CurrentLang = currentLang`

#### Detail 方法（文章详情）
- 查找关联的其他语言版本文章
- 通过 `ViewBag.RelatedArticle` 传递给视图
- 用于显示语言切换按钮

### 3. 视图修改
**文件**: `Views/Article/Detail.cshtml`

添加了语言切换按钮：
```html
@if (ViewBag.RelatedArticle != null)
{
    var related = ViewBag.RelatedArticle as TtsWebApp.Models.Article;
    <a href="/Article/Detail/@related.Id" 
       class="px-4 py-2 bg-primary/10 text-primary rounded-lg hover:bg-primary/20 transition-colors flex items-center gap-2">
        <span class="material-symbols-outlined text-lg">translate</span>
        <span>@(related.Language == "zh-CN" ? "查看中文版" : "View English Version")</span>
    </a>
}
```

### 4. 国际化支持
**文件**: `wwwroot/js/i18n.js`

添加了文章相关翻译：
```javascript
// 中文
'article.backToList': '返回文章列表',
'article.viewChinese': '查看中文版',
'article.viewEnglish': 'View English Version'

// 英文
'article.backToList': 'Back to Article List',
'article.viewChinese': '查看中文版',
'article.viewEnglish': 'View English Version'
```

## 📋 待执行的步骤

### 1. 数据库迁移（需要停止应用后执行）
```bash
# 停止应用程序
# 然后执行：
cd d:\1Dev\dev\webs\tts_turi\TtsWebApp
dotnet ef migrations add AddArticleLanguageFields
dotnet ef database update
```

### 2. 更新现有数据（如果有）
```sql
-- 将所有现有文章设置为中文
UPDATE Articles 
SET Language = 'zh-CN' 
WHERE Language IS NULL OR Language = '';
```

## 🎯 使用方法

### 创建中英文文章对

#### 方法一：通过后台管理（推荐）
1. 创建中文文章
   - 标题：如何使用 TTS
   - 语言：zh-CN
   - 保存后记录文章 ID（假设为 1）

2. 创建英文文章
   - 标题：How to Use TTS
   - 语言：en-US
   - 关联文章ID：1
   - 保存后记录文章 ID（假设为 2）

3. 返回编辑中文文章
   - 设置关联文章ID：2
   - 保存

#### 方法二：通过数据库直接操作
```sql
-- 假设中文文章 ID = 1, 英文文章 ID = 2
UPDATE Articles SET RelatedArticleId = 2 WHERE Id = 1;
UPDATE Articles SET RelatedArticleId = 1 WHERE Id = 2;
```

### 访问文章

#### 中文文章列表
```
http://localhost:5128/Article/Index
或
http://localhost:5128/Article/Index?lang=zh-CN
```

#### 英文文章列表
```
http://localhost:5128/Article/Index?lang=en-US
```

#### 文章详情
```
http://localhost:5128/Article/Detail/1
```
- 如果有关联的其他语言版本，会显示语言切换按钮
- 点击按钮跳转到对应语言的文章

## 🔄 工作流程

### 用户浏览流程
```
1. 用户访问中文文章列表 (/Article/Index?lang=zh-CN)
   ↓
2. 看到所有中文文章
   ↓
3. 点击某篇文章进入详情页
   ↓
4. 如果有英文版本，看到"View English Version"按钮
   ↓
5. 点击按钮，跳转到英文版文章
   ↓
6. 在英文文章页面看到"查看中文版"按钮
   ↓
7. 可以随时切换回中文版
```

### 语言切换逻辑
```javascript
// 文章列表：通过 URL 参数筛选
/Article/Index?lang=zh-CN  → 显示中文文章
/Article/Index?lang=en-US  → 显示英文文章

// 文章详情：通过关联ID跳转
中文文章 (ID: 1, RelatedArticleId: 2) → 显示"View English Version"按钮
点击按钮 → 跳转到 /Article/Detail/2
英文文章 (ID: 2, RelatedArticleId: 1) → 显示"查看中文版"按钮
点击按钮 → 跳转到 /Article/Detail/1
```

## 🎨 界面展示

### 文章详情页布局
```
┌─────────────────────────────────────────────────┐
│ ← 返回文章列表              [🌐 View English Version] │
├─────────────────────────────────────────────────┤
│                                                 │
│  文章标题                                        │
│  👤 作者  📅 发布日期  👁 浏览次数               │
│  ─────────────────────────────────────────     │
│  #标签1 #标签2                                  │
│                                                 │
│  文章内容...                                    │
│                                                 │
└─────────────────────────────────────────────────┘
```

## 📊 数据库结构

### Articles 表新增字段
| 字段名 | 类型 | 长度 | 可空 | 默认值 | 说明 |
|--------|------|------|------|--------|------|
| Language | string | 10 | 否 | zh-CN | 语言标识 |
| RelatedArticleId | int | - | 是 | NULL | 关联文章ID |

### 示例数据
| Id | Title | Language | RelatedArticleId | IsPublished |
|----|-------|----------|------------------|-------------|
| 1 | 如何使用 TTS | zh-CN | 2 | true |
| 2 | How to Use TTS | en-US | 1 | true |
| 3 | TTS 最佳实践 | zh-CN | 4 | true |
| 4 | TTS Best Practices | en-US | 3 | true |

## 🚀 后续优化建议

### 1. 后台管理界面增强
- [ ] 添加语言选择下拉框
- [ ] 添加关联文章选择器（自动筛选相同类型的文章）
- [ ] 显示文章的语言标识
- [ ] 批量设置语言功能
- [ ] 创建文章时提供"创建翻译版本"快捷按钮

### 2. 前端体验优化
- [ ] 文章列表页添加语言切换按钮
- [ ] 记住用户的语言偏好
- [ ] 自动根据浏览器语言显示对应文章
- [ ] 添加"此文章暂无英文版本"提示

### 3. SEO 优化
- [ ] 添加 hreflang 标签
- [ ] 为中英文文章生成独立的 sitemap
- [ ] 优化 URL 结构（可选：/article/zh/xxx 和 /article/en/xxx）

### 4. API 支持
- [ ] 提供 API 接口获取文章的所有语言版本
- [ ] 支持通过 Accept-Language 头自动返回对应语言

## ⚠️ 注意事项

1. **双向关联**：RelatedArticleId 需要在两篇文章中都设置，形成双向关联
2. **语言一致性**：确保关联的文章语言不同（一个 zh-CN，一个 en-US）
3. **发布状态**：两篇关联文章可以有不同的发布状态，未发布的不会显示切换按钮
4. **页面类型**：Page 类型的文章（如 about, privacy）已通过 Slug 实现中英文，不使用此方案
5. **数据迁移**：执行数据库迁移前务必备份数据

## 📝 测试清单

- [ ] 创建中文文章，验证默认语言为 zh-CN
- [ ] 创建英文文章，设置语言为 en-US
- [ ] 设置两篇文章的关联关系
- [ ] 访问中文文章列表，确认只显示中文文章
- [ ] 访问英文文章列表，确认只显示英文文章
- [ ] 在文章详情页查看语言切换按钮
- [ ] 点击切换按钮，验证跳转正确
- [ ] 验证未关联的文章不显示切换按钮
- [ ] 测试国际化文本显示正确

## 🎉 完成状态

✅ 数据模型修改  
✅ 控制器逻辑更新  
✅ 视图界面调整  
✅ 国际化支持  
✅ 文档编写  
⏳ 数据库迁移（需要停止应用后执行）  
⏳ 后台管理界面更新（可选）  

---

**实现时间**: 2025-01-08  
**方案**: 语言字段方案（方案一）  
**状态**: 代码已完成，等待数据库迁移
