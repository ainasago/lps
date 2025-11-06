using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TtsWebApi.Controllers;

namespace TtsWebApi.Services
{
    public class TtsService : ITtsService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TtsService> _logger;
        private readonly TextSplitter _textSplitter;
        private readonly MultiRoleParser _multiRoleParser;
        private readonly AudioMerger _audioMerger;
        // 使用与src-tauri相同的TrustedClientToken
        private const string TrustedClientToken = "6A5AA1D4EAFF4E9FB37E23D68491D6F4";

        public TtsService(IConfiguration configuration, HttpClient httpClient, ILogger<TtsService> logger, 
            TextSplitter textSplitter, MultiRoleParser multiRoleParser, AudioMerger audioMerger)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _logger = logger;
            _textSplitter = textSplitter;
            _multiRoleParser = multiRoleParser;
            _audioMerger = audioMerger;
        }

        public async Task<List<VoiceInfo>> GetVoicesAsync()
        {
            try
            {
                _logger.LogInformation("开始获取语音列表");
                
                var url = $"https://speech.platform.bing.com/consumer/speech/synthesize/readaloud/voices/list?trustedclienttoken={TrustedClientToken}";
                
                var response = await _httpClient.GetStringAsync(url);
                var voices = JsonConvert.DeserializeObject<List<VoiceInfo>>(response);
                
                // 处理LocaleName为null的情况，使用语言代码映射
                var localeNameMap = GetLocaleNameMap();
                foreach (var voice in voices)
                {
                    if (string.IsNullOrEmpty(voice.LocaleName) && !string.IsNullOrEmpty(voice.Locale))
                    {
                        if (localeNameMap.TryGetValue(voice.Locale, out var localeName))
                        {
                            voice.LocaleName = localeName;
                        }
                    }
                }
                
                _logger.LogInformation("成功获取到 {Count} 个语音", voices?.Count ?? 0);
                return voices;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取语音列表时发生错误");
                throw;
            }
        }
        
        private Dictionary<string, string> GetLocaleNameMap()
        {
            return new Dictionary<string, string>
            {
                { "af-ZA", "Afrikaans (South Africa)" },
                { "sq-AL", "Albanian (Albania)" },
                { "am-ET", "Amharic (Ethiopia)" },
                { "ar-DZ", "Arabic (Algeria)" },
                { "ar-BH", "Arabic (Bahrain)" },
                { "ar-EG", "Arabic (Egypt)" },
                { "ar-IQ", "Arabic (Iraq)" },
                { "ar-JO", "Arabic (Jordan)" },
                { "ar-KW", "Arabic (Kuwait)" },
                { "ar-LB", "Arabic (Lebanon)" },
                { "ar-LY", "Arabic (Libya)" },
                { "ar-MA", "Arabic (Morocco)" },
                { "ar-OM", "Arabic (Oman)" },
                { "ar-QA", "Arabic (Qatar)" },
                { "ar-SA", "Arabic (Saudi Arabia)" },
                { "ar-SY", "Arabic (Syria)" },
                { "ar-TN", "Arabic (Tunisia)" },
                { "ar-AE", "Arabic (UAE)" },
                { "ar-YE", "Arabic (Yemen)" },
                { "hy-AM", "Armenian (Armenia)" },
                { "as-IN", "Assamese (India)" },
                { "az-AZ", "Azerbaijani (Azerbaijan)" },
                { "bn-BD", "Bengali (Bangladesh)" },
                { "bn-IN", "Bengali (India)" },
                { "bs-BA", "Bosnian (Bosnia and Herzegovina)" },
                { "bg-BG", "Bulgarian (Bulgaria)" },
                { "yue-CN", "Cantonese (Mainland China)" },
                { "ca-ES", "Catalan (Spain)" },
                { "zh-HK", "Chinese (Hong Kong)" },
                { "zh-CN", "Chinese (Mainland)" },
                { "zh-TW", "Chinese (Taiwan)" },
                { "hr-HR", "Croatian (Croatia)" },
                { "cs-CZ", "Czech (Czech Republic)" },
                { "da-DK", "Danish (Denmark)" },
                { "nl-BE", "Dutch (Belgium)" },
                { "nl-NL", "Dutch (Netherlands)" },
                { "en-AU", "English (Australia)" },
                { "en-CA", "English (Canada)" },
                { "en-GH", "English (Ghana)" },
                { "en-HK", "English (Hong Kong)" },
                { "en-IN", "English (India)" },
                { "en-IE", "English (Ireland)" },
                { "en-KE", "English (Kenya)" },
                { "en-NZ", "English (New Zealand)" },
                { "en-NG", "English (Nigeria)" },
                { "en-PH", "English (Philippines)" },
                { "en-SG", "English (Singapore)" },
                { "en-ZA", "English (South Africa)" },
                { "en-TZ", "English (Tanzania)" },
                { "en-GB", "English (United Kingdom)" },
                { "en-US", "English (United States)" },
                { "et-EE", "Estonian (Estonia)" },
                { "fil-PH", "Filipino (Philippines)" },
                { "fi-FI", "Finnish (Finland)" },
                { "fr-BE", "French (Belgium)" },
                { "fr-CA", "French (Canada)" },
                { "fr-FR", "French (France)" },
                { "fr-CH", "French (Switzerland)" },
                { "gl-ES", "Galician (Spain)" },
                { "ka-GE", "Georgian (Georgia)" },
                { "de-AT", "German (Austria)" },
                { "de-DE", "German (Germany)" },
                { "de-CH", "German (Switzerland)" },
                { "el-GR", "Greek (Greece)" },
                { "gu-IN", "Gujarati (India)" },
                { "he-IL", "Hebrew (Israel)" },
                { "hi-IN", "Hindi (India)" },
                { "hu-HU", "Hungarian (Hungary)" },
                { "is-IS", "Icelandic (Iceland)" },
                { "id-ID", "Indonesian (Indonesia)" },
                { "ga-IE", "Irish (Ireland)" },
                { "it-IT", "Italian (Italy)" },
                { "ja-JP", "Japanese (Japan)" },
                { "jv-ID", "Javanese (Indonesia)" },
                { "kn-IN", "Kannada (India)" },
                { "kk-KZ", "Kazakh (Kazakhstan)" },
                { "km-KH", "Khmer (Cambodia)" },
                { "ko-KR", "Korean (South Korea)" },
                { "lo-LA", "Lao (Laos)" },
                { "lv-LV", "Latvian (Latvia)" },
                { "lt-LT", "Lithuanian (Lithuania)" },
                { "mk-MK", "Macedonian (North Macedonia)" },
                { "ms-MY", "Malay (Malaysia)" },
                { "ml-IN", "Malayalam (India)" },
                { "mt-MT", "Maltese (Malta)" },
                { "mr-IN", "Marathi (India)" },
                { "mn-MN", "Mongolian (Mongolia)" },
                { "my-MM", "Myanmar (Myanmar)" },
                { "nb-NO", "Norwegian (Bokmål, Norway)" },
                { "ps-AF", "Pashto (Afghanistan)" },
                { "fa-IR", "Persian (Iran)" },
                { "pl-PL", "Polish (Poland)" },
                { "pt-BR", "Portuguese (Brazil)" },
                { "pt-PT", "Portuguese (Portugal)" },
                { "pa-IN", "Punjabi (India)" },
                { "ro-RO", "Romanian (Romania)" },
                { "ru-RU", "Russian (Russia)" },
                { "sr-RS", "Serbian (Serbia)" },
                { "si-LK", "Sinhala (Sri Lanka)" },
                { "sk-SK", "Slovak (Slovakia)" },
                { "sl-SI", "Slovenian (Slovenia)" },
                { "so-SO", "Somali (Somalia)" },
                { "es-AR", "Spanish (Argentina)" },
                { "es-BO", "Spanish (Bolivia)" },
                { "es-CL", "Spanish (Chile)" },
                { "es-CO", "Spanish (Colombia)" },
                { "es-CR", "Spanish (Costa Rica)" },
                { "es-CU", "Spanish (Cuba)" },
                { "es-DO", "Spanish (Dominican Republic)" },
                { "es-EC", "Spanish (Ecuador)" },
                { "es-SV", "Spanish (El Salvador)" },
                { "es-GQ", "Spanish (Equatorial Guinea)" },
                { "es-GT", "Spanish (Guatemala)" },
                { "es-HN", "Spanish (Honduras)" },
                { "es-MX", "Spanish (Mexico)" },
                { "es-NI", "Spanish (Nicaragua)" },
                { "es-PA", "Spanish (Panama)" },
                { "es-PY", "Spanish (Paraguay)" },
                { "es-PE", "Spanish (Peru)" },
                { "es-PR", "Spanish (Puerto Rico)" },
                { "es-ES", "Spanish (Spain)" },
                { "es-UY", "Spanish (Uruguay)" },
                { "es-VE", "Spanish (Venezuela)" },
                { "sw-KE", "Swahili (Kenya)" },
                { "sv-SE", "Swedish (Sweden)" },
                { "ta-IN", "Tamil (India)" },
                { "ta-LK", "Tamil (Sri Lanka)" },
                { "te-IN", "Telugu (India)" },
                { "th-TH", "Thai (Thailand)" },
                { "tr-TR", "Turkish (Turkey)" },
                { "uk-UA", "Ukrainian (Ukraine)" },
                { "ur-PK", "Urdu (Pakistan)" },
                { "uz-UZ", "Uzbek (Uzbekistan)" },
                { "vi-VN", "Vietnamese (Vietnam)" },
                { "cy-GB", "Welsh (United Kingdom)" },
                { "zu-ZA", "Zulu (South Africa)" }
            };
        }

        public async Task<TtsResponse> SynthesizeSpeechAsync(TtsRequest request)
        {
            var startTime = DateTime.Now;
            
            try
            {
                // 处理试听模式
                var processedText = request.Text;
                if (request.PreviewMode)
                {
                    processedText = GetPreviewText(request.Text, request.PreviewSentences);
                    _logger.LogInformation("试听模式：原文本 {OriginalLength} 字符，截取为 {PreviewLength} 字符", 
                        request.Text.Length, processedText.Length);
                }
                
                _logger.LogInformation("开始合成语音，请求参数：Text={Text}, Voice={Voice}, OutputFormat={OutputFormat}, BreakTime={BreakTime}, PreviewMode={PreviewMode}, EnableLongTextSplit={EnableLongTextSplit}", 
                    processedText.Substring(0, Math.Min(50, processedText.Length)) + "...", 
                    request.Voice, request.OutputFormat, request.BreakTime, request.PreviewMode, request.EnableLongTextSplit);

                // 检查是否需要长文本切片
                if (request.EnableLongTextSplit && processedText.Length > request.MaxCharsPerChunk)
                {
                    _logger.LogInformation("启用长文本切片，文本长度: {TextLength}, 切片大小: {ChunkSize}", 
                        processedText.Length, request.MaxCharsPerChunk);
                    return await SynthesizeLongTextAsync(request, processedText, startTime);
                }
                
                var requestId = Guid.NewGuid().ToString();
                var secMsGec = GenerateSecMsGec();
                
                _logger.LogInformation("生成请求ID: {RequestId}, SecMsGec: {SecMsGec}", requestId, secMsGec);
                
                // 使用与src-tauri相同的WebSocket URL格式
                var webSocketUrl = $"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={TrustedClientToken}&Sec-MS-GEC={secMsGec}&Sec-MS-GEC-Version=1-130.0.2849.68&ConnectionId={requestId}";
                
                _logger.LogInformation("连接WebSocket: {WebSocketUrl}", webSocketUrl);
                
                using var clientWebSocket = new ClientWebSocket();
                
                // 添加与src-tauri相同的请求头
                clientWebSocket.Options.SetRequestHeader("Pragma", "no-cache");
                clientWebSocket.Options.SetRequestHeader("Cache-Control", "no-cache");
                clientWebSocket.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
                clientWebSocket.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");
                clientWebSocket.Options.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
                clientWebSocket.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
                
                await clientWebSocket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
                
                _logger.LogInformation("WebSocket连接成功，状态: {State}", clientWebSocket.State);
                
                // 发送配置消息
                var configMessage = ConvertToAudioFormatWebSocketString(request.OutputFormat);
                _logger.LogInformation("发送配置消息: {ConfigMessage}", configMessage);
                await SendWebSocketMessage(clientWebSocket, configMessage);
                
                // 发送SSML消息（使用处理后的文本和停顿参数）
                var ssmlMessage = ConvertToSsmlWebSocketString(requestId, request.Voice, processedText, request.Pitch, request.Rate, request.Volume, request.BreakTime);
                _logger.LogInformation("发送SSML消息: {SsmlMessage}", ssmlMessage);
                await SendWebSocketMessage(clientWebSocket, ssmlMessage);
                
                _logger.LogInformation("消息发送完成，开始接收响应");
                
                // 接收响应
                var audioData = new List<byte>();
                var wordBoundaries = new List<WordBoundary>();
                var buffer = new byte[4096];
                var messageCount = 0;
                var binaryDelim = "Path:audio\r\n";
                
                while (clientWebSocket.State == WebSocketState.Open)
                {
                    var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    messageCount++;
                    
                    _logger.LogInformation("接收到消息 #{MessageCount}, 类型: {MessageType}, 大小: {Count}", 
                        messageCount, result.MessageType, result.Count);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("接收到关闭消息，状态: {CloseStatus}, 描述: {CloseStatusDescription}", 
                            result.CloseStatus, result.CloseStatusDescription);
                        await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        break;
                    }
                    
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation("收到文本消息: {Message}", receivedText.Length > 200 ? receivedText.Substring(0, 200) + "..." : receivedText);
                        
                        if (receivedText.Contains("Path:turn.end"))
                        {
                            _logger.LogInformation("接收到turn.end消息，结束接收");
                            break;
                        }
                        else if (receivedText.Contains("Path:audio.metadata"))
                        {
                            _logger.LogInformation("接收到元数据消息，大小: {Size}", result.Count);
                            // 找到第一个空行之后的第一个非空行，这应该是 JSON 数据的开始
                            if (receivedText.Contains("\r\n\r\n"))
                            {
                                var startIndex = receivedText.IndexOf("\r\n\r\n") + 4;
                                var jsonPart = receivedText.Substring(startIndex);
                                
                                try
                                {
                                    // 尝试解析为包含 Metadata 数组的格式
                                    var metadataWrapper = JsonConvert.DeserializeObject<MetadataWrapper>(jsonPart);
                                    if (metadataWrapper?.Metadata != null)
                                    {
                                        foreach (var metadata in metadataWrapper.Metadata)
                                        {
                                            if (metadata.Type == "WordBoundary")
                                            {
                                                _logger.LogInformation("接收到词边界: Text={Text}, Offset={Offset}, Duration={Duration}", 
                                                    metadata.Data.Text.Text, metadata.Data.Offset, metadata.Data.Duration);
                                                    
                                                wordBoundaries.Add(new WordBoundary
                                                {
                                                    Text = metadata.Data.Text.Text,
                                                    Offset = metadata.Data.Offset.ToString(),
                                                    Duration = metadata.Data.Duration.ToString()
                                                });
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    _logger.LogWarning(ex, "解析元数据消息失败: {Message}", ex.Message);
                                }
                            }
                        }
                    }
                    else if (result.MessageType == WebSocketMessageType.Binary)
                    {
                        var receivedData = new byte[result.Count];
                        Array.Copy(buffer, receivedData, result.Count);
                        
                        // 查找二进制分隔符
                        var binaryDelimBytes = Encoding.UTF8.GetBytes(binaryDelim);
                        var index = FindBytes(receivedData, binaryDelimBytes);
                        
                        if (index != -1)
                        {
                            var audioBytes = new byte[receivedData.Length - index - binaryDelimBytes.Length];
                            Array.Copy(receivedData, index + binaryDelimBytes.Length, audioBytes, 0, audioBytes.Length);
                            
                            _logger.LogInformation("接收到音频数据，大小: {Size}", audioBytes.Length);
                            audioData.AddRange(audioBytes);
                        }
                        else
                        {
                            _logger.LogInformation("接收到音频数据（无分隔符），大小: {Size}", receivedData.Length);
                            audioData.AddRange(receivedData);
                        }
                    }
                }
                
                _logger.LogInformation("接收完成，音频数据大小: {AudioSize}, 词边界数量: {WordBoundaryCount}", 
                    audioData.Count, wordBoundaries.Count);
                
                var processingTime = (DateTime.Now - startTime).TotalMilliseconds;
                
                var response = new TtsResponse
                {
                    AudioBase64 = Convert.ToBase64String(audioData.ToArray()),
                    WordBoundaries = wordBoundaries,
                    ChunkCount = 1,
                    TotalCharacters = processedText.Length,
                    ProcessingTimeMs = processingTime,
                    IsPreview = request.PreviewMode
                };
                
                if (request.GenerateSubtitles)
                {
                    _logger.LogInformation("生成字幕，选项: {SubtitleOption}, 词数: {SubtitleWordCount}", 
                        request.SubtitleOption, request.SubtitleWordCount);
                    response.Subtitles = GenerateSubtitles(wordBoundaries, processedText, request.SubtitleOption, request.SubtitleWordCount);
                }
                
                _logger.LogInformation("语音合成完成，处理时间: {ProcessingTime}ms, 字符数: {CharCount}, 试听模式: {IsPreview}", 
                    processingTime, processedText.Length, request.PreviewMode);
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "合成语音时发生错误");
                throw;
            }
        }

        private string GenerateSecMsGec()
        {
            // 使用与src-tauri相同的算法
            var now = DateTime.UtcNow;
            var unixTimestamp = ((DateTimeOffset)now).ToUnixTimeSeconds();
            var ticks = unixTimestamp + 11644473600.0;
            ticks = Math.Floor(ticks / 300.0) * 300.0;
            var ticksInt = (long)(ticks * 1e7);
            
            // 使用与src-tauri相同的哈希算法
            var strToHash = $"{NumToStr(ticksInt.ToString())}{TrustedClientToken}";
            
            using var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(strToHash));
            var hash = Convert.ToHexString(hashBytes).ToUpperInvariant();
            
            return hash;
        }

        private string NumToStr(string num)
        {
            if (!num.Contains('e'))
            {
                return num;
            }
            
            var numStr = num.Trim().Replace("\"", "").Replace("=", "").Replace("'", "");
            var result = new StringBuilder();
            var n = long.Parse(numStr);
            
            while (n > 0)
            {
                var v = n % 10;
                n /= 10;
                result.Insert(0, v.ToString());
            }
            
            return result.ToString();
        }

        private string ConvertToAudioFormatWebSocketString(string outputFormat)
        {
            // 使用与src-tauri相同的格式
            var timestamp = DateTime.UtcNow.ToString("ddd MMM dd yyyy HH:mm:ss \"GMT+0000 (Coordinated Universal Time)\"");
            return $"X-Timestamp:{timestamp}\r\nContent-Type:application/json; charset=utf-8\r\nPath:speech.config\r\n\r\n{{\"context\":{{\"synthesis\":{{\"audio\":{{\"metadataoptions\":{{\"sentenceBoundaryEnabled\":\"false\",\"wordBoundaryEnabled\":\"true\"}},\"outputFormat\":\"{outputFormat}\"}}}}}}}}";
        }

        private string ConvertToSsmlWebSocketString(string requestId, string voice, string text, string pitch, string rate, string volume, int breakTime = 0)
        {
            // 策略：先插入占位符，再转义，最后替换占位符为真实的 break 标签
            const string BREAK_PLACEHOLDER = "___BREAK___";
            
            _logger.LogInformation("转义前文本（前100字符）: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            
            // 如果设置了停顿时间，先插入占位符
            // 需要考虑引号的情况：在引号后插入，如果没有引号则在句号后插入
            // 暂时禁用停顿功能，因为与字幕不兼容
            if (false && breakTime > 0)
            {
                // 先处理带引号的情况（引号在句号后），使用特殊占位符标记已处理
                const string PROCESSED_MARKER = "___PROCESSED___";
                var patternsWithQuotes = new[]
                {
                    ("\u3002\u201D", "\u3002\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),  // 。"
                    ("\uFF01\u201D", "\uFF01\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),  // ！"
                    ("\uFF1F\u201D", "\uFF1F\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),  // ？"
                    (".\u201D", ".\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),            // ."
                    ("!\u201D", "!\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),            // !"
                    ("?\u201D", "?\u201D" + BREAK_PLACEHOLDER + PROCESSED_MARKER),            // ?"
                };
                
                foreach (var (pattern, replacement) in patternsWithQuotes)
                {
                    text = text.Replace(pattern, replacement);
                }
                
                // 然后处理没有引号的情况（但跳过已处理的）
                var sentenceEndings = new[] { "\u3002", "\uFF01", "\uFF1F", ".", "!", "?" };  // 。！？
                foreach (var ending in sentenceEndings)
                {
                    // 只替换后面没有 PROCESSED_MARKER 的句号
                    text = text.Replace(ending, ending + BREAK_PLACEHOLDER);
                }
                
                // 移除处理标记
                text = text.Replace(PROCESSED_MARKER, "");
                
                _logger.LogInformation("插入占位符后（前100字符）: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            }
            
            // 进行 XML 转义（手动转义特殊字符，包括中文引号）
            text = text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")  // 英文引号
                       .Replace("'", "&apos;")   // 英文单引号
                       .Replace("\u201C", "&quot;")   // 中文左引号 "
                       .Replace("\u201D", "&quot;")   // 中文右引号 "
                       .Replace("\u2018", "&apos;")   // 中文左单引号 '
                       .Replace("\u2019", "&apos;");  // 中文右单引号 '
            
            _logger.LogInformation("XML转义后（前100字符）: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            
            // 替换占位符为真实的 break 标签（这些标签不会被转义）
            if (breakTime > 0)
            {
                text = text.Replace(BREAK_PLACEHOLDER, $"<break time=\"{breakTime}ms\"/>");
                _logger.LogInformation("替换break标签后（前100字符）: {Text}", text.Substring(0, Math.Min(100, text.Length)));
            }

            // 使用与src-tauri相同的格式
            var timestamp = DateTime.UtcNow.ToString("ddd MMM dd yyyy HH:mm:ss \"GMT+0000 (Coordinated Universal Time)\"");
            var ssml = $"<speak version='1.0' xmlns='http://www.w3.org/2001/10/synthesis' xml:lang='en-US'><voice name='{voice}'><prosody pitch='{pitch}Hz' rate='{rate}%' volume='{volume}%'>{text}</prosody></voice></speak>";
            return $"X-RequestId:{requestId}\r\nContent-Type:application/ssml+xml\r\nX-Timestamp:{timestamp}Z\r\nPath:ssml\r\n\r\n{ssml}";
        }

        private string GetPreviewText(string text, int sentenceCount)
        {
            if (string.IsNullOrEmpty(text) || sentenceCount <= 0)
                return text;

            // 按句子分割
            var sentencePattern = @"[。！？；\.\?!;]+";
            var sentences = System.Text.RegularExpressions.Regex.Split(text, sentencePattern);
            
            var result = new System.Text.StringBuilder();
            var count = 0;
            
            foreach (var sentence in sentences)
            {
                if (string.IsNullOrWhiteSpace(sentence))
                    continue;
                    
                result.Append(sentence.Trim());
                result.Append("。");
                count++;
                
                if (count >= sentenceCount)
                    break;
            }
            
            return result.ToString();
        }

        private async Task SendWebSocketMessage(ClientWebSocket webSocket, string message)
        {
            var buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private int FindBytes(byte[] src, byte[] pattern)
        {
            for (int i = 0; i < src.Length - pattern.Length + 1; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (src[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }

        private string GenerateSubtitles(List<WordBoundary> wordBoundaries, string fullText, string option, int wordCount)
        {
            var cues = new List<Cue>();
            
            foreach (var boundary in wordBoundaries)
            {
                var offset = long.Parse(boundary.Offset);
                var duration = long.Parse(boundary.Duration);
                
                cues.Add(new Cue
                {
                    Index = cues.Count + 1,
                    Start = MicrosecondsToTime(offset),
                    End = MicrosecondsToTime(offset + duration),
                    Content = boundary.Text
                });
            }
            
            if (option == "mergeByNumber")
            {
                cues = MergeCuesByNumber(cues, wordCount);
            }
            else if (option == "mergeByPunctuation")
            {
                cues = MergeCuesByPunctuation(cues, fullText);
            }
            
            return ConvertToSrt(cues);
        }

        private List<Cue> MergeCuesByNumber(List<Cue> cues, int wordsPerCue)
        {
            if (wordsPerCue <= 0 || cues.Count == 0)
                return cues;
                
            var mergedCues = new List<Cue>();
            var currentCue = cues[0];
            
            for (int i = 1; i < cues.Count; i++)
            {
                var wordCount = currentCue.Content.Split(' ').Length;
                
                if (wordCount < wordsPerCue)
                {
                    currentCue.End = cues[i].End;
                    currentCue.Content += " " + cues[i].Content;
                }
                else
                {
                    mergedCues.Add(currentCue);
                    currentCue = cues[i];
                }
            }
            
            mergedCues.Add(currentCue);
            
            // 重新编号
            for (int i = 0; i < mergedCues.Count; i++)
            {
                mergedCues[i].Index = i + 1;
            }
            
            return mergedCues;
        }

        private List<Cue> MergeCuesByPunctuation(List<Cue> cues, string fullText)
        {
            if (string.IsNullOrEmpty(fullText) || cues.Count == 0)
                return cues;
                
            var mergedCues = new List<Cue>();
            var punctuationRegex = new System.Text.RegularExpressions.Regex(@"[。！？？，；,()\[\]（）【】{}、\.\?!;:<>《》「」『』""''…\n]+");
            var currentIndex = 0;
            var lastMatchEnd = -1;
            var matchStartIndex = 0;
            
            for (int i = 0; i < cues.Count; i++)
            {
                var cue = cues[i];
                var searchText = cue.Content;
                
                var position = fullText.IndexOf(searchText, currentIndex);
                if (position >= 0)
                {
                    if (lastMatchEnd == -1)
                    {
                        lastMatchEnd = position;
                    }
                    
                    var nextPosition = position + searchText.Length;
                    var maxWhile = 0;
                    
                    // 过滤空格
                    while (nextPosition < fullText.Length && fullText[nextPosition] == ' ' && maxWhile < 10)
                    {
                        nextPosition++;
                        maxWhile++;
                    }
                    
                    // 检查下一个字符是否是标点
                    if (nextPosition < fullText.Length && punctuationRegex.IsMatch(fullText[nextPosition].ToString()))
                    {
                        var start = cues[matchStartIndex].Start;
                        var endPos = nextPosition;
                        var extractedText = fullText.Substring(lastMatchEnd, endPos - lastMatchEnd);
                        
                        mergedCues.Add(new Cue
                        {
                            Index = cue.Index,
                            Start = start,
                            End = cue.End,
                            Content = extractedText
                        });
                        
                        lastMatchEnd = -1;
                        matchStartIndex = i + 1;
                    }
                    
                    currentIndex = position;
                }
            }
            
            return mergedCues;
        }

        private string ConvertToSrt(List<Cue> cues)
        {
            var srtBuilder = new StringBuilder();
            
            foreach (var cue in cues)
            {
                srtBuilder.AppendLine(cue.Index.ToString());
                srtBuilder.AppendLine($"{cue.Start} --> {cue.End}");
                srtBuilder.AppendLine(cue.Content.Trim());
                srtBuilder.AppendLine();
            }
            
            return srtBuilder.ToString();
        }

        /// <summary>
        /// 长文本合成 - 切片后分批合成并拼接
        /// </summary>
        private async Task<TtsResponse> SynthesizeLongTextAsync(TtsRequest request, string text, DateTime startTime)
        {
            // 切分文本
            var chunks = _textSplitter.SplitText(text, SplitStrategy.BySentence);
            _logger.LogInformation("文本切分完成，共 {ChunkCount} 个片段", chunks.Count);

            var audioChunks = new List<byte[]>();
            var wordBoundaryChunks = new List<List<WordBoundary>>();
            var chunkDurations = new List<long>();

            // 逐个合成片段
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];
                _logger.LogInformation("正在合成第 {Index}/{Total} 个片段，长度: {Length} 字符", 
                    i + 1, chunks.Count, chunk.CharCount);

                try
                {
                    // 创建单个片段的请求（复用现有的单次合成逻辑）
                    var chunkText = chunk.Text;
                    var chunkAudio = await SynthesizeSingleTextAsync(request, chunkText);
                    
                    audioChunks.Add(chunkAudio.audioData);
                    wordBoundaryChunks.Add(chunkAudio.wordBoundaries);
                    
                    // 计算片段时长
                    if (chunkAudio.wordBoundaries != null && chunkAudio.wordBoundaries.Count > 0)
                    {
                        var lastBoundary = chunkAudio.wordBoundaries.Last();
                        var duration = long.Parse(lastBoundary.Offset) + long.Parse(lastBoundary.Duration);
                        chunkDurations.Add(duration);
                    }
                    else
                    {
                        chunkDurations.Add(0);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "合成第 {Index} 个片段失败", i + 1);
                    throw;
                }
            }

            // 合并音频
            var mergedAudio = _audioMerger.MergeMP3Audio(audioChunks);
            var mergedBoundaries = _audioMerger.MergeWordBoundaries(wordBoundaryChunks, chunkDurations);

            var processingTime = (DateTime.Now - startTime).TotalMilliseconds;

            var response = new TtsResponse
            {
                AudioBase64 = Convert.ToBase64String(mergedAudio),
                WordBoundaries = mergedBoundaries,
                ChunkCount = chunks.Count,
                TotalCharacters = text.Length,
                ProcessingTimeMs = processingTime,
                IsPreview = request.PreviewMode
            };

            // 生成字幕
            if (request.GenerateSubtitles)
            {
                _logger.LogInformation("生成合并后的字幕");
                response.Subtitles = GenerateSubtitles(mergedBoundaries, text, request.SubtitleOption, request.SubtitleWordCount);
            }

            _logger.LogInformation("长文本合成完成，片段数: {ChunkCount}, 总字符数: {TotalChars}, 处理时间: {ProcessingTime}ms", 
                chunks.Count, text.Length, processingTime);

            return response;
        }

        /// <summary>
        /// 合成单个文本（提取现有逻辑）
        /// </summary>
        private async Task<(byte[] audioData, List<WordBoundary> wordBoundaries)> SynthesizeSingleTextAsync(TtsRequest request, string text)
        {
            var requestId = Guid.NewGuid().ToString();
            var secMsGec = GenerateSecMsGec();
            
            var webSocketUrl = $"wss://speech.platform.bing.com/consumer/speech/synthesize/readaloud/edge/v1?TrustedClientToken={TrustedClientToken}&Sec-MS-GEC={secMsGec}&Sec-MS-GEC-Version=1-130.0.2849.68&ConnectionId={requestId}";
            
            using var clientWebSocket = new ClientWebSocket();
            
            clientWebSocket.Options.SetRequestHeader("Pragma", "no-cache");
            clientWebSocket.Options.SetRequestHeader("Cache-Control", "no-cache");
            clientWebSocket.Options.SetRequestHeader("Origin", "chrome-extension://jdiccldimpdaibmpdkjnbmckianbfold");
            clientWebSocket.Options.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0");
            clientWebSocket.Options.SetRequestHeader("Accept-Encoding", "gzip, deflate, br");
            clientWebSocket.Options.SetRequestHeader("Accept-Language", "en-US,en;q=0.9");
            
            await clientWebSocket.ConnectAsync(new Uri(webSocketUrl), CancellationToken.None);
            
            var configMessage = ConvertToAudioFormatWebSocketString(request.OutputFormat);
            await SendWebSocketMessage(clientWebSocket, configMessage);
            
            var ssmlMessage = ConvertToSsmlWebSocketString(requestId, request.Voice, text, request.Pitch, request.Rate, request.Volume, request.BreakTime);
            await SendWebSocketMessage(clientWebSocket, ssmlMessage);
            
            var audioData = new List<byte>();
            var wordBoundaries = new List<WordBoundary>();
            var buffer = new byte[4096];
            var binaryDelim = "Path:audio\r\n";
            
            while (clientWebSocket.State == WebSocketState.Open)
            {
                var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    break;
                }
                
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var receivedText = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                    if (receivedText.Contains("Path:turn.end"))
                    {
                        break;
                    }
                    else if (receivedText.Contains("Path:audio.metadata"))
                    {
                        if (receivedText.Contains("\r\n\r\n"))
                        {
                            var startIndex = receivedText.IndexOf("\r\n\r\n") + 4;
                            var jsonPart = receivedText.Substring(startIndex);
                            
                            try
                            {
                                var metadataWrapper = JsonConvert.DeserializeObject<MetadataWrapper>(jsonPart);
                                if (metadataWrapper?.Metadata != null)
                                {
                                    foreach (var metadata in metadataWrapper.Metadata)
                                    {
                                        if (metadata.Type == "WordBoundary")
                                        {
                                            wordBoundaries.Add(new WordBoundary
                                            {
                                                Text = metadata.Data.Text.Text,
                                                Offset = metadata.Data.Offset.ToString(),
                                                Duration = metadata.Data.Duration.ToString()
                                            });
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                // 忽略解析错误
                            }
                        }
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Binary)
                {
                    var receivedData = new byte[result.Count];
                    Array.Copy(buffer, receivedData, result.Count);
                    
                    var binaryDelimBytes = Encoding.UTF8.GetBytes(binaryDelim);
                    var index = FindBytes(receivedData, binaryDelimBytes);
                    
                    if (index >= 0)
                    {
                        var audioBytes = new byte[receivedData.Length - index - binaryDelimBytes.Length];
                        Array.Copy(receivedData, index + binaryDelimBytes.Length, audioBytes, 0, audioBytes.Length);
                        audioData.AddRange(audioBytes);
                    }
                    else
                    {
                        audioData.AddRange(receivedData);
                    }
                }
            }
            
            return (audioData.ToArray(), wordBoundaries);
        }

        private string MicrosecondsToTime(long microseconds)
        {
            var milliseconds = microseconds / 10000;
            var hours = milliseconds / 3600000;
            var minutes = (milliseconds % 3600000) / 60000;
            var seconds = (milliseconds % 60000) / 1000;
            var ms = milliseconds % 1000;
            
            return $"{hours:D2}:{minutes:D2}:{seconds:D2},{ms:D3}";
        }
    }

    public class MetadataWrapper
    {
        public List<MetadataMessage> Metadata { get; set; }
    }

    public class MetadataMessage
    {
        public string Type { get; set; }
        public MetadataData Data { get; set; }
    }

    public class MetadataData
    {
        public long Offset { get; set; }
        public long Duration { get; set; }
        public TextData Text { get; set; }
    }

    public class TextData
    {
        public string Text { get; set; }
    }

    public class Cue
    {
        public int Index { get; set; }
        public string Start { get; set; }
        public string End { get; set; }
        public string Content { get; set; }
    }
}