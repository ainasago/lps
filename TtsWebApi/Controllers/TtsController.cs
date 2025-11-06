using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TtsWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TtsController : ControllerBase
    {
        private readonly ITtsService _ttsService;
        private readonly ILogger<TtsController> _logger;

        public TtsController(ITtsService ttsService, ILogger<TtsController> logger)
        {
            _ttsService = ttsService;
            _logger = logger;
        }

        [HttpGet("voices")]
        public async Task<IActionResult> GetVoices()
        {
            try
            {
                var voices = await _ttsService.GetVoicesAsync();
                return Ok(voices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("synthesize")]
        public async Task<IActionResult> SynthesizeSpeech([FromBody] TtsRequest request)
        {
            try
            {
                _logger.LogInformation("API收到合成请求");
                
                // 验证模型
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("模型验证失败: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { error = "模型验证失败", details = errors });
                }
                
                if (request == null)
                {
                    _logger.LogWarning("请求对象为 null");
                    return BadRequest(new { error = "请求对象为 null" });
                }
                
                _logger.LogInformation($"请求参数 - Text长度: {request.Text?.Length ?? 0}, Voice: {request.Voice}, GenerateSubtitles: {request.GenerateSubtitles}, PreviewMode: {request.PreviewMode}, BreakTime: {request.BreakTime}");
                
                var result = await _ttsService.SynthesizeSpeechAsync(request);
                _logger.LogInformation($"API返回结果，Subtitles长度: {result.Subtitles?.Length ?? 0}");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "合成语音时发生错误");
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
    }

    public interface ITtsService
    {
        Task<List<VoiceInfo>> GetVoicesAsync();
        Task<TtsResponse> SynthesizeSpeechAsync(TtsRequest request);
    }

    public class VoiceInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string LocalName { get; set; }
        public string ShortName { get; set; }
        public string Locale { get; set; }
        public string LocaleName { get; set; }
        public string Gender { get; set; }
        public string SampleRate { get; set; }
        public string VoiceType { get; set; }
        public string Status { get; set; }
    }

    public class TtsRequest
    {
        public string Text { get; set; }
        public string Voice { get; set; }
        public string Language { get; set; }
        public string OutputFormat { get; set; } = "audio-24khz-48kbitrate-mono-mp3";
        public string Pitch { get; set; } = "0";
        public string Rate { get; set; } = "0";
        public string Volume { get; set; } = "100";
        public bool GenerateSubtitles { get; set; } = false;
        public string SubtitleOption { get; set; } = "mergeByPunctuation";
        public int SubtitleWordCount { get; set; } = 10;
        
        // 新增功能参数
        public bool EnableLongTextSplit { get; set; } = false;  // 启用长文本切片
        public int MaxCharsPerChunk { get; set; } = 500;        // 每个切片最大字符数
        public bool EnableMultiRole { get; set; } = false;       // 启用多角色配音
        public Dictionary<string, string>? RoleVoiceMap { get; set; }  // 角色-配音员映射
        public bool PreviewMode { get; set; } = false;           // 试听模式（只合成前几句）
        public int PreviewSentences { get; set; } = 3;           // 试听句子数
        public int BreakTime { get; set; } = 0;                  // 停顿时间（毫秒）
        public bool SaveToFile { get; set; } = false;            // 保存到文件而不是返回 base64
    }

    public class TtsResponse
    {
        public string AudioBase64 { get; set; }
        public string AudioUrl { get; set; }              // 音频文件 URL（SaveToFile 模式）
        public string Subtitles { get; set; }
        public List<WordBoundary> WordBoundaries { get; set; }
        public int ChunkCount { get; set; }               // 切片数量
        public int TotalCharacters { get; set; }          // 总字符数
        public double ProcessingTimeMs { get; set; }      // 处理时间（毫秒）
        public bool IsPreview { get; set; }               // 是否为试听模式
    }

    public class WordBoundary
    {
        public string Text { get; set; }
        public string Offset { get; set; }
        public string Duration { get; set; }
    }
}