using System.Text.RegularExpressions;

namespace TtsWebApi.Services
{
    /// <summary>
    /// 文本切片器 - 将长文本切分为适合 TTS 的片段
    /// </summary>
    public class TextSplitter
    {
        private readonly int _maxCharsPerChunk;
        private readonly ILogger<TextSplitter> _logger;

        public TextSplitter(ILogger<TextSplitter> logger, int maxCharsPerChunk = 500)
        {
            _logger = logger;
            _maxCharsPerChunk = maxCharsPerChunk;
        }

        /// <summary>
        /// 将文本切分为多个片段
        /// </summary>
        public List<TextChunk> SplitText(string text, SplitStrategy strategy = SplitStrategy.BySentence)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<TextChunk>();

            switch (strategy)
            {
                case SplitStrategy.BySentence:
                    return SplitBySentence(text);
                case SplitStrategy.ByParagraph:
                    return SplitByParagraph(text);
                case SplitStrategy.ByLength:
                    return SplitByLength(text);
                default:
                    return SplitBySentence(text);
            }
        }

        /// <summary>
        /// 按句子切分
        /// </summary>
        private List<TextChunk> SplitBySentence(string text)
        {
            var chunks = new List<TextChunk>();
            
            // 中英文句子分隔符
            var sentencePattern = @"[。！？；\.\?!;]+";
            var sentences = Regex.Split(text, sentencePattern);
            
            var currentChunk = "";
            var chunkIndex = 0;

            foreach (var sentence in sentences)
            {
                var trimmedSentence = sentence.Trim();
                if (string.IsNullOrEmpty(trimmedSentence))
                    continue;

                // 如果当前句子加上已有内容超过限制，先保存当前块
                if (currentChunk.Length + trimmedSentence.Length > _maxCharsPerChunk && !string.IsNullOrEmpty(currentChunk))
                {
                    chunks.Add(new TextChunk
                    {
                        Index = chunkIndex++,
                        Text = currentChunk.Trim(),
                        CharCount = currentChunk.Length
                    });
                    currentChunk = "";
                }

                currentChunk += trimmedSentence + "。";
            }

            // 添加最后一个块
            if (!string.IsNullOrEmpty(currentChunk))
            {
                chunks.Add(new TextChunk
                {
                    Index = chunkIndex,
                    Text = currentChunk.Trim(),
                    CharCount = currentChunk.Length
                });
            }

            _logger.LogInformation("按句子切分，原文本长度: {OriginalLength}, 切分为 {ChunkCount} 个片段", 
                text.Length, chunks.Count);

            return chunks;
        }

        /// <summary>
        /// 按段落切分
        /// </summary>
        private List<TextChunk> SplitByParagraph(string text)
        {
            var chunks = new List<TextChunk>();
            var paragraphs = text.Split(new[] { "\n\n", "\r\n\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            
            var chunkIndex = 0;
            foreach (var paragraph in paragraphs)
            {
                var trimmed = paragraph.Trim();
                if (string.IsNullOrEmpty(trimmed))
                    continue;

                // 如果段落太长，进一步按句子切分
                if (trimmed.Length > _maxCharsPerChunk)
                {
                    var subChunks = SplitBySentence(trimmed);
                    foreach (var subChunk in subChunks)
                    {
                        subChunk.Index = chunkIndex++;
                        chunks.Add(subChunk);
                    }
                }
                else
                {
                    chunks.Add(new TextChunk
                    {
                        Index = chunkIndex++,
                        Text = trimmed,
                        CharCount = trimmed.Length
                    });
                }
            }

            _logger.LogInformation("按段落切分，原文本长度: {OriginalLength}, 切分为 {ChunkCount} 个片段", 
                text.Length, chunks.Count);

            return chunks;
        }

        /// <summary>
        /// 按固定长度切分
        /// </summary>
        private List<TextChunk> SplitByLength(string text)
        {
            var chunks = new List<TextChunk>();
            var chunkIndex = 0;

            for (int i = 0; i < text.Length; i += _maxCharsPerChunk)
            {
                var length = Math.Min(_maxCharsPerChunk, text.Length - i);
                var chunk = text.Substring(i, length);

                chunks.Add(new TextChunk
                {
                    Index = chunkIndex++,
                    Text = chunk,
                    CharCount = chunk.Length
                });
            }

            _logger.LogInformation("按长度切分，原文本长度: {OriginalLength}, 切分为 {ChunkCount} 个片段", 
                text.Length, chunks.Count);

            return chunks;
        }
    }

    /// <summary>
    /// 文本片段
    /// </summary>
    public class TextChunk
    {
        public int Index { get; set; }
        public string Text { get; set; }
        public int CharCount { get; set; }
    }

    /// <summary>
    /// 切分策略
    /// </summary>
    public enum SplitStrategy
    {
        BySentence,   // 按句子
        ByParagraph,  // 按段落
        ByLength      // 按固定长度
    }
}
