namespace TtsWebApp.Models;

/// <summary>
/// 文件管理配置
/// </summary>
public class FileManagerConfig
{
    public string RootPath { get; set; } = "publish";
    public long MaxFileSize { get; set; } = 20971520; // 20MB
    public List<string> AllowedUploadExtensions { get; set; } = new();
    public List<string> ImageExtensions { get; set; } = new();
    public List<string> ExcludedPaths { get; set; } = new();
}

/// <summary>
/// 文件项类型
/// </summary>
public enum FileItemType
{
    Directory,      // 文件夹
    TextFile,       // 文本文件
    ImageFile,      // 图片文件
    BinaryFile,     // 二进制文件
    CodeFile        // 代码文件
}

/// <summary>
/// 文件/文件夹信息
/// </summary>
public class FileItem
{
    /// <summary>
    /// 文件/文件夹名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 完整物理路径
    /// </summary>
    public string FullPath { get; set; } = string.Empty;

    /// <summary>
    /// 相对于根目录的路径
    /// </summary>
    public string RelativePath { get; set; } = string.Empty;

    /// <summary>
    /// 是否是文件夹
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 格式化的文件大小
    /// </summary>
    public string FormattedSize { get; set; } = string.Empty;

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// 文件扩展名
    /// </summary>
    public string Extension { get; set; } = string.Empty;

    /// <summary>
    /// 文件类型
    /// </summary>
    public FileItemType Type { get; set; }

    /// <summary>
    /// 是否是图片
    /// </summary>
    public bool IsImage { get; set; }

    /// <summary>
    /// 图标CSS类名
    /// </summary>
    public string IconClass { get; set; } = string.Empty;
}

/// <summary>
/// 文件操作请求基类
/// </summary>
public class FileOperationRequest
{
    /// <summary>
    /// 相对路径
    /// </summary>
    public string Path { get; set; } = string.Empty;
}

/// <summary>
/// 保存文件请求
/// </summary>
public class SaveFileRequest : FileOperationRequest
{
    /// <summary>
    /// 文件内容
    /// </summary>
    public string Content { get; set; } = string.Empty;
}

/// <summary>
/// 重命名请求
/// </summary>
public class RenameRequest : FileOperationRequest
{
    /// <summary>
    /// 新名称
    /// </summary>
    public string NewName { get; set; } = string.Empty;
}

/// <summary>
/// 创建文件夹请求
/// </summary>
public class CreateFolderRequest
{
    /// <summary>
    /// 父目录路径
    /// </summary>
    public string ParentPath { get; set; } = string.Empty;

    /// <summary>
    /// 文件夹名称
    /// </summary>
    public string FolderName { get; set; } = string.Empty;
}

/// <summary>
/// 文件操作结果
/// </summary>
public class FileOperationResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 数据
    /// </summary>
    public object? Data { get; set; }
}

/// <summary>
/// 文件内容响应
/// </summary>
public class FileContentResponse
{
    /// <summary>
    /// 文件内容
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 是否可以作为文本读取
    /// </summary>
    public bool IsTextFile { get; set; }

    /// <summary>
    /// 文件信息
    /// </summary>
    public FileItem? FileInfo { get; set; }

    /// <summary>
    /// 错误消息
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 面包屑导航项
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// 显示名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    public string Path { get; set; } = string.Empty;

    /// <summary>
    /// 是否是当前项
    /// </summary>
    public bool IsCurrent { get; set; }
}
