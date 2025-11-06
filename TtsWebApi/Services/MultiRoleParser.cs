using System.Text.RegularExpressions;

namespace TtsWebApi.Services
{
    /// <summary>
    /// 多角色解析器 - 解析文本中的角色对话
    /// </summary>
    public class MultiRoleParser
    {
        private readonly ILogger<MultiRoleParser> _logger;

        public MultiRoleParser(ILogger<MultiRoleParser> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 解析多角色文本
        /// 支持格式：
        /// 1. 角色名：对话内容
        /// 2. 【角色名】对话内容
        /// 3. "对话内容" - 角色名
        /// </summary>
        public List<RoleSegment> ParseRoles(string text, Dictionary<string, string>? roleVoiceMap = null)
        {
            var segments = new List<RoleSegment>();

            // 模式1: 角色名：对话内容
            var pattern1 = @"([^：:\n]+)[：:]\s*([^\n]+)";
            
            // 模式2: 【角色名】对话内容
            var pattern2 = @"【([^】]+)】\s*([^\n]+)";
            
            var matches1 = Regex.Matches(text, pattern1);
            var matches2 = Regex.Matches(text, pattern2);

            var allMatches = new List<(int Index, string Role, string Content)>();

            foreach (Match match in matches1)
            {
                allMatches.Add((match.Index, match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim()));
            }

            foreach (Match match in matches2)
            {
                allMatches.Add((match.Index, match.Groups[1].Value.Trim(), match.Groups[2].Value.Trim()));
            }

            // 按位置排序
            allMatches = allMatches.OrderBy(m => m.Index).ToList();

            // 如果没有匹配到角色，返回整个文本作为旁白
            if (allMatches.Count == 0)
            {
                segments.Add(new RoleSegment
                {
                    Index = 0,
                    Role = "旁白",
                    Content = text,
                    Voice = roleVoiceMap?.GetValueOrDefault("旁白")
                });
                return segments;
            }

            // 处理匹配到的角色对话
            int segmentIndex = 0;
            int lastEndIndex = 0;

            foreach (var match in allMatches)
            {
                // 添加角色对话之前的旁白
                if (match.Index > lastEndIndex)
                {
                    var narration = text.Substring(lastEndIndex, match.Index - lastEndIndex).Trim();
                    if (!string.IsNullOrEmpty(narration))
                    {
                        segments.Add(new RoleSegment
                        {
                            Index = segmentIndex++,
                            Role = "旁白",
                            Content = narration,
                            Voice = roleVoiceMap?.GetValueOrDefault("旁白")
                        });
                    }
                }

                // 添加角色对话
                segments.Add(new RoleSegment
                {
                    Index = segmentIndex++,
                    Role = match.Role,
                    Content = match.Content,
                    Voice = roleVoiceMap?.GetValueOrDefault(match.Role)
                });

                lastEndIndex = match.Index + match.Role.Length + match.Content.Length + 2;
            }

            // 添加最后的旁白
            if (lastEndIndex < text.Length)
            {
                var narration = text.Substring(lastEndIndex).Trim();
                if (!string.IsNullOrEmpty(narration))
                {
                    segments.Add(new RoleSegment
                    {
                        Index = segmentIndex,
                        Role = "旁白",
                        Content = narration,
                        Voice = roleVoiceMap?.GetValueOrDefault("旁白")
                    });
                }
            }

            _logger.LogInformation("解析多角色文本，识别到 {RoleCount} 个角色，{SegmentCount} 个片段", 
                segments.Select(s => s.Role).Distinct().Count(), segments.Count);

            return segments;
        }

        /// <summary>
        /// 自动分配配音员
        /// </summary>
        public Dictionary<string, string> AutoAssignVoices(List<string> roles, List<string> availableVoices)
        {
            var assignment = new Dictionary<string, string>();
            
            // 为每个角色分配一个配音员
            for (int i = 0; i < roles.Count; i++)
            {
                var voice = availableVoices[i % availableVoices.Count];
                assignment[roles[i]] = voice;
            }

            return assignment;
        }
    }

    /// <summary>
    /// 角色片段
    /// </summary>
    public class RoleSegment
    {
        public int Index { get; set; }
        public string Role { get; set; }
        public string Content { get; set; }
        public string Voice { get; set; }
    }
}
