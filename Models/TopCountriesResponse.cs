namespace AnlUsuarui.Models
{
    public class TopCountriesResponse
    {
        public long Timestamp { get; set; }
        public double ExecutionTimeMs { get; set; }
        public List<CountryResponse> Countries { get; set; } = new List<CountryResponse>();

    }

    public class CountryResponse
    {
        public string Country { get; set; }
        public int Total { get; set; }
    }
}
