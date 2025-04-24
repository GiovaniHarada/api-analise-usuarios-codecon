using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace AnlUsuarui.Models
{
    public class TeamInsightResponse
    {
        public long Timestamp { get; set; }
        public double ExecutionTimeMs { get; set; }
        public List<TeamResponse> Teams { get; set; } = new List<TeamResponse>();
    }

    public class TeamResponse
    {
        public string Team { get; set; }
        public int TotalMembers { get; set; } = 0;
        public int Leaders { get; set; }
        public int CompletedProjects { get; set; }
        public float ActivePercentage { get; set; }

        [JsonIgnore]
        public int TotalProjects { get; set; }
        [JsonIgnore]
        public int TotalMembersActive { get; set; }
    }
}
