using System.Text.Json.Serialization;
using YamlDotNet.Core.Tokens;

public class Person
{
    public int Id {get; set;}
    public string? Name {get; set;}
    public DateTime Birthday {get; set;}
    public string Ssid => Birthday.ToString("ddMMyy") + Id;
    [JsonIgnore]
    public List<Car>? Cars {get; set;}
    public int CarsOwned => Cars?.Count ?? 0;
}