namespace AnlUsuarui.Models
{
    public class ActiveUsersResponse
    {
        public long Timestamp { get; set; }
        public double ExecutionTimeMs { get; set; }
        public List<ActiveResponse> Logins { get; set; } = new List<ActiveResponse>();
    }

    public class ActiveResponse {
        public DateOnly Date { get; set; }
        public int Total { get; set; }
    }

}
