using YamlDotNet.Core.Tokens;

public class Person()
{
    public int Id {get; set;}
    public string? Name {get; set;}
    public DateTime Birthday {get; set;}
    public string Ssid => Birthday.ToString("ddMMyy") + Id;

    private Car? _car;
    public Car? Car
    {
        get => _car;
        set
        {
            _car = value;
            if(value != null && value.Owner != this)
            {
                value.Owner = this;
            }
        }
    }
}