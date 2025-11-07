using TtsWebApp.Models;

namespace TtsWebApp.Services;

/// <summary>
/// 文件管理服务接口
/// </summary>
public interface IFileManagerService
{
    /// <summary>
    /// 获取指定路径下的文件和文件夹列表
    /// </summary>
    Task<List<FileItem>> GetFilesAsync(string relativePath);

    /// <summary>
    /// 获取文件内容
    /// </summary>
    Task<FileContentResponse> GetFileContentAsync(string relativePath);

    /// <summary>
    /// 保存文件内容
    /// </summary>
    Task<FileOperationResult> SaveFileAsync(string relativePath, string content);

    /// <summary>
    /// 删除文件或文件夹
    /// </summary>
    Task<FileOperationResult> DeleteAsync(string relativePath);

    /// <summary>
    /// 重命名文件或文件夹
    /// </summary>
    Task<FileOperationResult> RenameAsync(string relativePath, string newName);

    /// <summary>
    /// 创建文件夹
    /// </summary>
    Task<FileOperationResult> CreateFolderAsync(string parentPath, string folderName);

    /// <summary>
    /// 上传文件
    /// </summary>
    Task<FileOperationResult> UploadFileAsync(string relativePath, IFormFile file);

    /// <summary>
    /// 获取面包屑导航
    /// </summary>
    List<BreadcrumbItem> GetBreadcrumbs(string relativePath);

    /// <summary>
    /// 验证路径是否安全
    /// </summary>
    bool IsPathSafe(string relativePath);

    /// <summary>
    /// 获取物理路径
    /// </summary>
    string GetPhysicalPath(string relativePath);

    /// <summary>
    /// 获取根目录的绝对路径
    /// </summary>
    string GetRootPath();
}
