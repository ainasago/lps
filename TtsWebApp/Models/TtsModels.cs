using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace TtsWebApp.Models
{
    public class TtsRequest
    {
        [Required]
        [JsonProperty("Text")]
        public string Text { get; set; } = string.Empty;
        
        [Required]
        [JsonProperty("Voice")]
        public string Voice { get; set; } = string.Empty;
        
        [JsonProperty("Language")]
        public string? Language { get; set; }
        
        [Range(-50, 50)]
        [JsonProperty("Pitch")]
        public int Pitch { get; set; } = 0;
        
        [Range(-100, 200)]
        [JsonProperty("Rate")]
        public int Rate { get; set; } = 0;
        
        [Range(0, 100)]
        [JsonProperty("Volume")]
        public int Volume { get; set; } = 80;
        
        [JsonProperty("GenerateSubtitles")]
        public bool GenerateSubtitles { get; set; } = false;
        
        // 高级功能参数
        [JsonProperty("PreviewMode")]
        public bool PreviewMode { get; set; } = false;
        
        [JsonProperty("PreviewSentences")]
        [Range(1, 10)]
        public int PreviewSentences { get; set; } = 3;
        
        [JsonProperty("BreakTime")]
        [Range(0, 2000)]
        public int BreakTime { get; set; } = 0;
        
        [JsonProperty("EnableLongTextSplit")]
        public bool EnableLongTextSplit { get; set; } = false;
        
        [JsonProperty("MaxCharsPerChunk")]
        [Range(100, 2000)]
        public int MaxCharsPerChunk { get; set; } = 500;
    }
    
    public class VoiceInfo
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string LocalName { get; set; } = string.Empty;
        public string ShortName { get; set; } = string.Empty;
        public string Locale { get; set; } = string.Empty;
        public string LocaleName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string SampleRate { get; set; } = string.Empty;
        public string VoiceType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
    
    public class TtsResponse
    {
        [JsonProperty("audioData")]
        public string AudioData { get; set; } = string.Empty;
        
        [JsonProperty("subtitles")]
        public string? Subtitles { get; set; }
        
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("errorMessage")]
        public string? ErrorMessage { get; set; }
        
        // 元数据字段
        [JsonProperty("chunkCount")]
        public int ChunkCount { get; set; }
        
        [JsonProperty("totalCharacters")]
        public int TotalCharacters { get; set; }
        
        [JsonProperty("processingTimeMs")]
        public double ProcessingTimeMs { get; set; }
        
        [JsonProperty("isPreview")]
        public bool IsPreview { get; set; }
    }
    
    public class AppSettings
    {
        public string SavePath { get; set; } = string.Empty;
        public bool OpenFolders { get; set; } = true;
        public bool SaveTTSOptions { get; set; } = true;
    }
}