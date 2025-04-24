namespace AnlUsuarui.Models
{
    public class SuperUsersResponse
    {
        public long Timestamp { get; set; }
        public double ExecutionTimeMs { get; set; }
        public int Count { get; set; }
        public IEnumerable<UsuarioModel> Data { get; set; }
    }
}
