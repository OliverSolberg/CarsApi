public class Car
{
    public int Id  {get; set;}
    public bool IsRegistered {get; set;}
    public string? Model {get; set;}
    public string? Make {get; set;}
    public int BuildYear {get; set;}
    private Person? _owner;
    public Person? Owner
    {
        get => _owner;
        set
        {
            _owner = value;
            if(value != null && value.Car != this)
            {
                value.Car = this;
            }
        }
    }
}