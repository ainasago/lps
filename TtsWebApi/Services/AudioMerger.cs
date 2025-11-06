using TtsWebApi.Controllers;

namespace TtsWebApi.Services
{
    /// <summary>
    /// 音频合并器 - 将多个音频片段合并为一个
    /// </summary>
    public class AudioMerger
    {
        private readonly ILogger<AudioMerger> _logger;

        public AudioMerger(ILogger<AudioMerger> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 合并多个 MP3 音频片段
        /// 注意：简单的字节拼接，可能需要更复杂的处理
        /// </summary>
        public byte[] MergeMP3Audio(List<byte[]> audioChunks)
        {
            if (audioChunks == null || audioChunks.Count == 0)
                return Array.Empty<byte>();

            if (audioChunks.Count == 1)
                return audioChunks[0];

            _logger.LogInformation("开始合并 {ChunkCount} 个音频片段", audioChunks.Count);

            var totalSize = audioChunks.Sum(chunk => chunk.Length);
            var mergedAudio = new byte[totalSize];
            var offset = 0;

            foreach (var chunk in audioChunks)
            {
                Buffer.BlockCopy(chunk, 0, mergedAudio, offset, chunk.Length);
                offset += chunk.Length;
            }

            _logger.LogInformation("音频合并完成，总大小: {TotalSize} 字节", totalSize);

            return mergedAudio;
        }

        /// <summary>
        /// 合并词边界列表，调整偏移量
        /// </summary>
        public List<WordBoundary> MergeWordBoundaries(List<List<WordBoundary>> boundaryChunks, List<long> chunkDurations)
        {
            var mergedBoundaries = new List<WordBoundary>();
            long cumulativeOffset = 0;

            for (int i = 0; i < boundaryChunks.Count; i++)
            {
                var boundaries = boundaryChunks[i];
                
                foreach (var boundary in boundaries)
                {
                    mergedBoundaries.Add(new WordBoundary
                    {
                        Text = boundary.Text,
                        Offset = (long.Parse(boundary.Offset) + cumulativeOffset).ToString(),
                        Duration = boundary.Duration
                    });
                }

                // 累加偏移量
                if (i < chunkDurations.Count)
                {
                    cumulativeOffset += chunkDurations[i];
                }
            }

            _logger.LogInformation("词边界合并完成，总数量: {TotalCount}", mergedBoundaries.Count);

            return mergedBoundaries;
        }
    }
}
