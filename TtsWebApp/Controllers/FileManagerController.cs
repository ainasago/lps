using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TtsWebApp.Models;
using TtsWebApp.Services;

namespace TtsWebApp.Controllers;

/// <summary>
/// 文件管理控制器
/// </summary>
[Authorize]
public class FileManagerController : Controller
{
    private readonly IFileManagerService _fileManager;
    private readonly ILogger<FileManagerController> _logger;
    private readonly IConfiguration _configuration;

    public FileManagerController(
        IFileManagerService fileManager,
        ILogger<FileManagerController> logger,
        IConfiguration configuration)
    {
        _fileManager = fileManager;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// 文件浏览主页
    /// </summary>
    public async Task<IActionResult> Index(string? path)
    {
        try
        {
            path = path ?? string.Empty;

            if (!_fileManager.IsPathSafe(path))
            {
                TempData["Error"] = "路径不安全";
                return RedirectToAction(nameof(Index));
            }

            var files = await _fileManager.GetFilesAsync(path);
            var breadcrumbs = _fileManager.GetBreadcrumbs(path);

            ViewBag.CurrentPath = path;
            ViewBag.Breadcrumbs = breadcrumbs;
            ViewBag.AdminPath = _configuration["AppSettings:AdminPath"] ?? "admin3";
            ViewBag.RootPath = _fileManager.GetRootPath();

            return View(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "浏览文件失败: {Path}", path);
            TempData["Error"] = "浏览文件失败";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 编辑文件页面
    /// </summary>
    public async Task<IActionResult> Edit(string path, bool create = false)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                TempData["Error"] = "文件路径不能为空";
                return RedirectToAction(nameof(Index));
            }

            if (!_fileManager.IsPathSafe(path))
            {
                TempData["Error"] = "路径不安全";
                return RedirectToAction(nameof(Index));
            }

            FileContentResponse result;

            // 如果是创建新文件
            if (create)
            {
                result = new FileContentResponse
                {
                    Content = "",
                    IsTextFile = true
                };
            }
            else
            {
                result = await _fileManager.GetFileContentAsync(path);

                if (!result.IsTextFile)
                {
                    TempData["Error"] = result.ErrorMessage ?? "无法编辑此文件";
                    return RedirectToAction(nameof(Index), new { path = GetParentPath(path) });
                }
            }

            ViewBag.FilePath = path;
            ViewBag.FileName = Path.GetFileName(path);
            ViewBag.FileExtension = Path.GetExtension(path).TrimStart('.');
            ViewBag.AdminPath = _configuration["AppSettings:AdminPath"] ?? "admin3";
            ViewBag.IsNewFile = create;

            return View(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开文件失败: {Path}", path);
            TempData["Error"] = "打开文件失败";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 保存文件
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save([FromForm] SaveFileRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Path))
            {
                return Json(new FileOperationResult
                {
                    Success = false,
                    Message = "文件路径不能为空"
                });
            }

            var result = await _fileManager.SaveFileAsync(request.Path, request.Content);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存文件失败: {Path}", request.Path);
            return Json(new FileOperationResult
            {
                Success = false,
                Message = $"保存失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(string path, IFormFile file)
    {
        try
        {
            path = path ?? string.Empty;

            if (!_fileManager.IsPathSafe(path))
            {
                return Json(new FileOperationResult
                {
                    Success = false,
                    Message = "路径不安全"
                });
            }

            var result = await _fileManager.UploadFileAsync(path, file);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上传文件失败");
            return Json(new FileOperationResult
            {
                Success = false,
                Message = $"上传失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 创建文件夹
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateFolder([FromForm] CreateFolderRequest request)
    {
        try
        {
            var result = await _fileManager.CreateFolderAsync(
                request.ParentPath ?? string.Empty,
                request.FolderName);

            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建文件夹失败");
            return Json(new FileOperationResult
            {
                Success = false,
                Message = $"创建失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 删除文件或文件夹
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete([FromForm] FileOperationRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Path))
            {
                return Json(new FileOperationResult
                {
                    Success = false,
                    Message = "路径不能为空"
                });
            }

            var result = await _fileManager.DeleteAsync(request.Path);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除失败: {Path}", request.Path);
            return Json(new FileOperationResult
            {
                Success = false,
                Message = $"删除失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 重命名文件或文件夹
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Rename([FromForm] RenameRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Path) || string.IsNullOrEmpty(request.NewName))
            {
                return Json(new FileOperationResult
                {
                    Success = false,
                    Message = "参数不能为空"
                });
            }

            var result = await _fileManager.RenameAsync(request.Path, request.NewName);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "重命名失败: {Path}", request.Path);
            return Json(new FileOperationResult
            {
                Success = false,
                Message = $"重命名失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 下载文件
    /// </summary>
    public IActionResult Download(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                TempData["Error"] = "文件路径不能为空";
                return RedirectToAction(nameof(Index));
            }

            if (!_fileManager.IsPathSafe(path))
            {
                TempData["Error"] = "路径不安全";
                return RedirectToAction(nameof(Index));
            }

            var physicalPath = _fileManager.GetPhysicalPath(path);

            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["Error"] = "文件不存在";
                return RedirectToAction(nameof(Index));
            }

            var fileName = Path.GetFileName(path);
            var fileBytes = System.IO.File.ReadAllBytes(physicalPath);
            var contentType = GetContentType(path);

            return File(fileBytes, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "下载文件失败: {Path}", path);
            TempData["Error"] = "下载文件失败";
            return RedirectToAction(nameof(Index));
        }
    }

    /// <summary>
    /// 获取文件内容（API）
    /// </summary>
    public async Task<IActionResult> GetContent(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                return Json(new FileContentResponse
                {
                    IsTextFile = false,
                    ErrorMessage = "文件路径不能为空"
                });
            }

            if (!_fileManager.IsPathSafe(path))
            {
                return Json(new FileContentResponse
                {
                    IsTextFile = false,
                    ErrorMessage = "路径不安全"
                });
            }

            var result = await _fileManager.GetFileContentAsync(path);
            return Json(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取文件内容失败: {Path}", path);
            return Json(new FileContentResponse
            {
                IsTextFile = false,
                ErrorMessage = $"获取失败: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// 图片预览页面
    /// </summary>
    public async Task<IActionResult> Preview(string path)
    {
        try
        {
            if (string.IsNullOrEmpty(path))
            {
                TempData["Error"] = "文件路径不能为空";
                return RedirectToAction(nameof(Index));
            }

            if (!_fileManager.IsPathSafe(path))
            {
                TempData["Error"] = "路径不安全";
                return RedirectToAction(nameof(Index));
            }

            var physicalPath = _fileManager.GetPhysicalPath(path);

            if (!System.IO.File.Exists(physicalPath))
            {
                TempData["Error"] = "文件不存在";
                return RedirectToAction(nameof(Index));
            }

            // 获取同目录下的所有图片
            var parentPath = GetParentPath(path);
            var allFiles = await _fileManager.GetFilesAsync(parentPath);
            var images = allFiles.Where(f => f.IsImage).ToList();

            ViewBag.CurrentPath = path;
            ViewBag.ParentPath = parentPath;
            ViewBag.Images = images;
            ViewBag.AdminPath = _configuration["AppSettings:AdminPath"] ?? "admin3";

            return View();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "预览图片失败: {Path}", path);
            TempData["Error"] = "预览图片失败";
            return RedirectToAction(nameof(Index));
        }
    }

    #region 私有辅助方法

    /// <summary>
    /// 获取父路径
    /// </summary>
    private string GetParentPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return string.Empty;

        var lastSlash = path.LastIndexOfAny(new[] { '/', '\\' });
        if (lastSlash <= 0)
            return string.Empty;

        return path.Substring(0, lastSlash);
    }

    /// <summary>
    /// 获取Content-Type
    /// </summary>
    private string GetContentType(string path)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        
        return extension switch
        {
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".pdf" => "application/pdf",
            ".zip" => "application/zip",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }

    #endregion
}
