using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Diagnostics;
using TtsWebApp.Models;
using TtsWebApp.Data;
using TtsWebApp.Resources;

namespace TtsWebApp.Controllers
{
    public class TtsController : Controller
    {
        private readonly ILogger<TtsController> _logger;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly string _apiBaseUrl;
        private readonly string _apiReferer;

        public TtsController(ILogger<TtsController> logger, HttpClient httpClient, IConfiguration configuration, AppDbContext context)
        {
            _logger = logger;
            _httpClient = httpClient;
            _configuration = configuration;
            _context = context;
            _apiBaseUrl = _configuration["AppSettings:TtsApiUrl"] ?? "http://localhost:5275/api/tts";
            _apiReferer = _configuration["AppSettings:TtsApiReferer"] ?? "http://localhost:5128/";
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetVoices()
        {
            try
            {
                _logger.LogInformation($"Calling API: {_apiBaseUrl}/voices");
                
                // 创建请求并添加 Referer 头
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiBaseUrl}/voices");
                request.Headers.Add("Referer", _apiReferer);
                
                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation($"API response status code: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API response content length: {content.Length}");
                    var voices = JsonConvert.DeserializeObject<List<VoiceInfo>>(content);
                    _logger.LogInformation($"Deserialization successful, voice count: {voices?.Count ?? 0}");
                    return Json(voices);
                }
                else
                {
                    _logger.LogWarning($"API call failed, status code: {response.StatusCode}");
                    return Json(new List<VoiceInfo>());
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voices");
                return Json(new List<VoiceInfo>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertText([FromBody] TtsRequest request)
        {
            try
            {
                    _logger.LogInformation($"Received conversion request, GenerateSubtitles: {request.GenerateSubtitles}");
                
                // 创建API请求对象
                var apiRequest = new
                {
                    Text = request.Text,
                    Voice = request.Voice,
                    Language = request.Language,
                    OutputFormat = "audio-24khz-48kbitrate-mono-mp3",
                    Pitch = request.Pitch.ToString(),
                    Rate = request.Rate.ToString(),
                    Volume = request.Volume.ToString(),
                    GenerateSubtitles = request.GenerateSubtitles,
                    SubtitleOption = "mergeByPunctuation",
                    SubtitleWordCount = 10,
                    // 高级功能参数
                    PreviewMode = request.PreviewMode,
                    PreviewSentences = request.PreviewSentences,
                    BreakTime = request.BreakTime,
                    EnableLongTextSplit = request.EnableLongTextSplit,
                    MaxCharsPerChunk = request.MaxCharsPerChunk
                };
                
                _logger.LogInformation($"API request object, GenerateSubtitles: {apiRequest.GenerateSubtitles}");
                
                var jsonContent = JsonConvert.SerializeObject(apiRequest);
                _logger.LogInformation($"JSON sent to API: {jsonContent}");
                var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                
                // 创建请求并添加 Referer 头
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"{_apiBaseUrl}/synthesize");
                httpRequest.Content = content;
                httpRequest.Headers.Add("Referer", _apiReferer);
                
                var response = await _httpClient.SendAsync(httpRequest);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation($"API response content length: {responseContent.Length}");
                    
                    var apiResult = JsonConvert.DeserializeObject<dynamic>(responseContent);
                    
                    string audioBase64 = apiResult.audioBase64?.ToString() ?? apiResult.AudioBase64?.ToString();
                    string subtitles = apiResult.subtitles?.ToString() ?? apiResult.Subtitles?.ToString();
                    
                    // 提取元数据
                    int chunkCount = apiResult.chunkCount ?? apiResult.ChunkCount ?? 1;
                    int totalCharacters = apiResult.totalCharacters ?? apiResult.TotalCharacters ?? 0;
                    double processingTimeMs = apiResult.processingTimeMs ?? apiResult.ProcessingTimeMs ?? 0.0;
                    bool isPreview = apiResult.isPreview ?? apiResult.IsPreview ?? false;
                    
                    _logger.LogInformation($"Audio Base64 length: {audioBase64?.Length ?? 0}");
                    _logger.LogInformation($"Subtitle length: {subtitles?.Length ?? 0}");
                    _logger.LogInformation($"Metadata - Chunks: {chunkCount}, Characters: {totalCharacters}, Processing time: {processingTimeMs}ms, Preview: {isPreview}");
                    if (!string.IsNullOrEmpty(subtitles))
                    {
                        _logger.LogInformation($"First 100 characters of subtitles: {subtitles.Substring(0, Math.Min(100, subtitles.Length))}");
                    }
                    
                    // 保存转换记录到数据库
                    try
                    {
                        var record = new TtsConversionRecord
                        {
                            Text = request.Text,
                            Language = request.Language,
                            Voice = request.Voice,
                            Speed = request.Rate,
                            Pitch = request.Pitch,
                            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                            UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                            CreatedAt = DateTime.Now,
                            IsSuccess = true
                        };
                        
                        _context.TtsConversionRecords.Add(record);
                        await _context.SaveChangesAsync();
                        _logger.LogInformation($"Conversion record saved, ID: {record.Id}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, Resources.ErrorMessages.SaveConversionRecordFailed);
                        // 不影响主流程，继续返回结果
                    }
                    
                    // 转换API响应为前端响应格式
                    var result = new TtsResponse 
                    { 
                        Success = true,
                        AudioData = audioBase64,
                        Subtitles = subtitles,
                        ChunkCount = chunkCount,
                        TotalCharacters = totalCharacters,
                        ProcessingTimeMs = processingTimeMs,
                        IsPreview = isPreview
                    };
                    return Json(result);
                }
                
                // API调用失败，保存失败记录
                try
                {
                    var record = new TtsConversionRecord
                    {
                        Text = request.Text,
                        Language = request.Language,
                        Voice = request.Voice,
                        Speed = request.Rate,
                        Pitch = request.Pitch,
                        IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
                        UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                        CreatedAt = DateTime.Now,
                        IsSuccess = false,
                        ErrorMessage = Resources.ErrorMessages.ApiCallFailed
                    };
                    
                    _context.TtsConversionRecords.Add(record);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, Resources.ErrorMessages.SaveFailureRecordError);
                }
                
                return Json(new TtsResponse { Success = false, ErrorMessage = Resources.ErrorMessages.ApiCallFailed });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting text");
                return Json(new TtsResponse { Success = false, ErrorMessage = string.Format(Resources.ErrorMessages.ConversionError, ex.Message) });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}