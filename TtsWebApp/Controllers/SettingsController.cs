using Microsoft.AspNetCore.Mvc;
using TtsWebApp.Models;

namespace TtsWebApp.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ILogger<SettingsController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            var model = new AppSettings
            {
                SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                OpenFolders = true,
                SaveTTSOptions = true
            };
            
            return View(model);
        }

        [HttpPost]
        public IActionResult SaveSettings(AppSettings settings)
        {
            // 在实际应用中，这里应该将设置保存到数据库或配置文件
            // 这里只是模拟保存操作
            _logger.LogInformation($"Settings saved: SavePath={settings.SavePath}, OpenFolders={settings.OpenFolders}, SaveTTSOptions={settings.SaveTTSOptions}");
            
            // 返回成功状态
            return Json(new { success = true, message = "设置保存成功" });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = HttpContext.TraceIdentifier });
        }
    }
}