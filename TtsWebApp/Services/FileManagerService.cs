using Microsoft.Extensions.Options;
using System.Text;
using TtsWebApp.Models;

namespace TtsWebApp.Services;

/// <summary>
/// 文件管理服务实现
/// </summary>
public class FileManagerService : IFileManagerService
{
    private readonly FileManagerConfig _config;
    private readonly ILogger<FileManagerService> _logger;
    private readonly string _rootPath;

    public FileManagerService(
        IOptions<FileManagerConfig> config,
        ILogger<FileManagerService> logger,
        IWebHostEnvironment env)
    {
        _config = config.Value;
        _logger = logger;
        
        // 获取根目录的完整路径
        _rootPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, _config.RootPath));
        
        _logger.LogInformation("文件管理器根目录: {RootPath}", _rootPath);
    }

    public async Task<List<FileItem>> GetFilesAsync(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);
            
            if (!Directory.Exists(physicalPath))
            {
                _logger.LogWarning("目录不存在: {Path}", physicalPath);
                return new List<FileItem>();
            }

            var items = new List<FileItem>();

            // 获取文件夹
            var directories = Directory.GetDirectories(physicalPath);
            foreach (var dir in directories)
            {
                var dirInfo = new DirectoryInfo(dir);
                
                // 跳过排除的路径
                if (_config.ExcludedPaths.Contains(dirInfo.Name))
                    continue;

                items.Add(new FileItem
                {
                    Name = dirInfo.Name,
                    FullPath = dir,
                    RelativePath = CombinePathWithForwardSlash(relativePath, dirInfo.Name),
                    IsDirectory = true,
                    LastModified = dirInfo.LastWriteTime,
                    Type = FileItemType.Directory,
                    IconClass = "folder"
                });
            }

            // 获取文件
            var files = Directory.GetFiles(physicalPath);
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var extension = fileInfo.Extension.ToLowerInvariant();
                var isImage = _config.ImageExtensions.Contains(extension);

                items.Add(new FileItem
                {
                    Name = fileInfo.Name,
                    FullPath = file,
                    RelativePath = CombinePathWithForwardSlash(relativePath, fileInfo.Name),
                    IsDirectory = false,
                    Size = fileInfo.Length,
                    FormattedSize = FormatFileSize(fileInfo.Length),
                    LastModified = fileInfo.LastWriteTime,
                    Extension = extension,
                    Type = DetermineFileType(extension, isImage),
                    IsImage = isImage,
                    IconClass = GetIconClass(extension, isImage)
                });
            }

            // 排序：文件夹在前，然后按名称排序
            return items.OrderBy(x => !x.IsDirectory).ThenBy(x => x.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件列表失败: {Path}", relativePath);
            throw;
        }
    }

    public async Task<FileContentResponse> GetFileContentAsync(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);
            
            if (!File.Exists(physicalPath))
            {
                return new FileContentResponse
                {
                    IsTextFile = false,
                    ErrorMessage = "文件不存在"
                };
            }

            var fileInfo = new FileInfo(physicalPath);
            
            // 尝试读取文件内容
            try
            {
                // 使用 UTF-8 编码读取，如果失败则尝试其他编码
                string content;
                try
                {
                    content = await File.ReadAllTextAsync(physicalPath, Encoding.UTF8);
                }
                catch
                {
                    // 尝试使用默认编码
                    content = await File.ReadAllTextAsync(physicalPath);
                }

                // 检查是否包含过多的不可打印字符（可能是二进制文件）
                if (IsBinaryContent(content))
                {
                    return new FileContentResponse
                    {
                        IsTextFile = false,
                        ErrorMessage = "此文件是二进制文件，无法作为文本编辑",
                        FileInfo = await GetFileInfoAsync(relativePath)
                    };
                }

                return new FileContentResponse
                {
                    Content = content,
                    IsTextFile = true,
                    FileInfo = await GetFileInfoAsync(relativePath)
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "无法读取文件作为文本: {Path}", relativePath);
                return new FileContentResponse
                {
                    IsTextFile = false,
                    ErrorMessage = $"无法读取文件: {ex.Message}",
                    FileInfo = await GetFileInfoAsync(relativePath)
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件内容失败: {Path}", relativePath);
            return new FileContentResponse
            {
                IsTextFile = false,
                ErrorMessage = $"获取文件失败: {ex.Message}"
            };
        }
    }

    public async Task<FileOperationResult> SaveFileAsync(string relativePath, string content)
    {
        try
        {
            if (!IsPathSafe(relativePath))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "路径不安全"
                };
            }

            var physicalPath = GetPhysicalPath(relativePath);
            
            // 创建备份
            if (File.Exists(physicalPath))
            {
                var backupPath = physicalPath + ".bak";
                File.Copy(physicalPath, backupPath, true);
                _logger.LogInformation("创建备份: {BackupPath}", backupPath);
            }

            await File.WriteAllTextAsync(physicalPath, content, Encoding.UTF8);
            
            _logger.LogInformation("文件保存成功: {Path}", relativePath);
            
            return new FileOperationResult
            {
                Success = true,
                Message = "文件保存成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存文件失败: {Path}", relativePath);
            return new FileOperationResult
            {
                Success = false,
                Message = $"保存失败: {ex.Message}"
            };
        }
    }

    public async Task<FileOperationResult> DeleteAsync(string relativePath)
    {
        try
        {
            if (!IsPathSafe(relativePath))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "路径不安全"
                };
            }

            var physicalPath = GetPhysicalPath(relativePath);

            if (Directory.Exists(physicalPath))
            {
                Directory.Delete(physicalPath, true);
                _logger.LogInformation("删除文件夹: {Path}", relativePath);
            }
            else if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
                _logger.LogInformation("删除文件: {Path}", relativePath);
            }
            else
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "文件或文件夹不存在"
                };
            }

            return new FileOperationResult
            {
                Success = true,
                Message = "删除成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除失败: {Path}", relativePath);
            return new FileOperationResult
            {
                Success = false,
                Message = $"删除失败: {ex.Message}"
            };
        }
    }

    public async Task<FileOperationResult> RenameAsync(string relativePath, string newName)
    {
        try
        {
            if (!IsPathSafe(relativePath) || string.IsNullOrWhiteSpace(newName))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "参数无效"
                };
            }

            // 验证新名称不包含非法字符
            if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "文件名包含非法字符"
                };
            }

            var physicalPath = GetPhysicalPath(relativePath);
            var directory = Path.GetDirectoryName(physicalPath);
            var newPath = Path.Combine(directory!, newName);

            if (File.Exists(newPath) || Directory.Exists(newPath))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "目标文件或文件夹已存在"
                };
            }

            if (Directory.Exists(physicalPath))
            {
                Directory.Move(physicalPath, newPath);
            }
            else if (File.Exists(physicalPath))
            {
                File.Move(physicalPath, newPath);
            }
            else
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "源文件或文件夹不存在"
                };
            }

            _logger.LogInformation("重命名成功: {OldPath} -> {NewPath}", relativePath, newName);

            return new FileOperationResult
            {
                Success = true,
                Message = "重命名成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重命名失败: {Path}", relativePath);
            return new FileOperationResult
            {
                Success = false,
                Message = $"重命名失败: {ex.Message}"
            };
        }
    }

    public async Task<FileOperationResult> CreateFolderAsync(string parentPath, string folderName)
    {
        try
        {
            if (!IsPathSafe(parentPath) || string.IsNullOrWhiteSpace(folderName))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "参数无效"
                };
            }

            // 验证文件夹名称不包含非法字符
            if (folderName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "文件夹名包含非法字符"
                };
            }

            var parentPhysicalPath = GetPhysicalPath(parentPath);
            var newFolderPath = Path.Combine(parentPhysicalPath, folderName);

            if (Directory.Exists(newFolderPath))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "文件夹已存在"
                };
            }

            Directory.CreateDirectory(newFolderPath);
            
            _logger.LogInformation("创建文件夹: {Path}", newFolderPath);

            return new FileOperationResult
            {
                Success = true,
                Message = "文件夹创建成功"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建文件夹失败: {ParentPath}/{FolderName}", parentPath, folderName);
            return new FileOperationResult
            {
                Success = false,
                Message = $"创建失败: {ex.Message}"
            };
        }
    }

    public async Task<FileOperationResult> UploadFileAsync(string relativePath, IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = "文件为空"
                };
            }

            if (file.Length > _config.MaxFileSize)
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = $"文件大小超过限制 ({FormatFileSize(_config.MaxFileSize)})"
                };
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (_config.AllowedUploadExtensions.Count > 0 && 
                !_config.AllowedUploadExtensions.Contains(extension))
            {
                return new FileOperationResult
                {
                    Success = false,
                    Message = $"不允许上传 {extension} 类型的文件"
                };
            }

            var physicalPath = GetPhysicalPath(relativePath);
            var targetPath = Path.Combine(physicalPath, file.FileName);

            // 如果文件已存在，添加时间戳
            if (File.Exists(targetPath))
            {
                var nameWithoutExt = Path.GetFileNameWithoutExtension(file.FileName);
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var newFileName = $"{nameWithoutExt}_{timestamp}{extension}";
                targetPath = Path.Combine(physicalPath, newFileName);
            }

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("文件上传成功: {FileName} -> {Path}", file.FileName, targetPath);

            return new FileOperationResult
            {
                Success = true,
                Message = "文件上传成功",
                Data = new { FileName = Path.GetFileName(targetPath) }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "文件上传失败: {FileName}", file?.FileName);
            return new FileOperationResult
            {
                Success = false,
                Message = $"上传失败: {ex.Message}"
            };
        }
    }

    public List<BreadcrumbItem> GetBreadcrumbs(string relativePath)
    {
        var breadcrumbs = new List<BreadcrumbItem>
        {
            new BreadcrumbItem
            {
                Name = "根目录",
                Path = "",
                IsCurrent = string.IsNullOrEmpty(relativePath)
            }
        };

        if (string.IsNullOrEmpty(relativePath))
            return breadcrumbs;

        var parts = relativePath.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        var currentPath = "";

        for (int i = 0; i < parts.Length; i++)
        {
            currentPath = string.IsNullOrEmpty(currentPath) 
                ? parts[i] 
                : CombinePathWithForwardSlash(currentPath, parts[i]);

            breadcrumbs.Add(new BreadcrumbItem
            {
                Name = parts[i],
                Path = currentPath,
                IsCurrent = i == parts.Length - 1
            });
        }

        return breadcrumbs;
    }

    public bool IsPathSafe(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return true;

        // 防止路径遍历攻击
        if (relativePath.Contains("..") || 
            relativePath.Contains("~") ||
            Path.IsPathRooted(relativePath))
        {
            _logger.LogWarning("检测到不安全的路径: {Path}", relativePath);
            return false;
        }

        var physicalPath = GetPhysicalPath(relativePath);
        var fullPath = Path.GetFullPath(physicalPath);

        // 确保路径在根目录内
        if (!fullPath.StartsWith(_rootPath, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("路径超出根目录: {Path}", relativePath);
            return false;
        }

        return true;
    }

    public string GetPhysicalPath(string relativePath)
    {
        if (string.IsNullOrEmpty(relativePath))
            return _rootPath;

        // Convert forward slashes to platform-specific path separator
        var normalizedPath = relativePath.Replace("/", Path.DirectorySeparatorChar.ToString());
        return Path.Combine(_rootPath, normalizedPath);
    }

    #region 私有辅助方法

    private async Task<FileItem?> GetFileInfoAsync(string relativePath)
    {
        try
        {
            var physicalPath = GetPhysicalPath(relativePath);
            var fileInfo = new FileInfo(physicalPath);
            var extension = fileInfo.Extension.ToLowerInvariant();
            var isImage = _config.ImageExtensions.Contains(extension);

            return new FileItem
            {
                Name = fileInfo.Name,
                FullPath = physicalPath,
                RelativePath = relativePath,
                IsDirectory = false,
                Size = fileInfo.Length,
                FormattedSize = FormatFileSize(fileInfo.Length),
                LastModified = fileInfo.LastWriteTime,
                Extension = extension,
                Type = DetermineFileType(extension, isImage),
                IsImage = isImage,
                IconClass = GetIconClass(extension, isImage)
            };
        }
        catch
        {
            return null;
        }
    }

    private FileItemType DetermineFileType(string extension, bool isImage)
    {
        if (isImage)
            return FileItemType.ImageFile;

        var codeExtensions = new[] { ".cs", ".cshtml", ".js", ".css", ".html", ".json", ".xml", ".sql", ".md" };
        if (codeExtensions.Contains(extension))
            return FileItemType.CodeFile;

        var textExtensions = new[] { ".txt", ".log", ".config" };
        if (textExtensions.Contains(extension))
            return FileItemType.TextFile;

        return FileItemType.BinaryFile;
    }

    private string GetIconClass(string extension, bool isImage)
    {
        if (isImage)
            return "image";

        return extension switch
        {
            ".cs" => "code",
            ".cshtml" => "code",
            ".js" => "javascript",
            ".css" => "css",
            ".html" => "html",
            ".json" => "data_object",
            ".xml" => "data_object",
            ".sql" => "database",
            ".md" => "description",
            ".txt" => "description",
            ".pdf" => "picture_as_pdf",
            ".zip" or ".rar" => "folder_zip",
            _ => "insert_drive_file"
        };
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }

    private bool IsBinaryContent(string content)
    {
        // 检查前1000个字符中不可打印字符的比例
        var checkLength = Math.Min(1000, content.Length);
        var nonPrintableCount = 0;

        for (int i = 0; i < checkLength; i++)
        {
            var c = content[i];
            // 允许常见的控制字符：换行、回车、制表符
            if (char.IsControl(c) && c != '\n' && c != '\r' && c != '\t')
            {
                nonPrintableCount++;
            }
        }

        // 如果超过10%是不可打印字符，认为是二进制文件
        return (double)nonPrintableCount / checkLength > 0.1;
    }

    /// <summary>
    /// 获取根目录的绝对路径
    /// </summary>
    public string GetRootPath()
    {
        return _rootPath;
    }

    /// <summary>
    /// 使用正斜杠组合路径
    /// </summary>
    private string CombinePathWithForwardSlash(string path1, string path2)
    {
        if (string.IsNullOrEmpty(path1))
            return path2;
        if (string.IsNullOrEmpty(path2))
            return path1;
        
        return $"{path1}/{path2}";
    }

    #endregion
}
