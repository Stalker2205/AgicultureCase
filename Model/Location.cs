namespace AgroCase.Model
{
    public class Location
    {
        public List<double> Center { get; set; } = new List<double>(); // [lat, lng]
        public List<List<double>> Polygon { get; set; } = new List<List<double>>(); // [[lat, lng]]
    }
}
