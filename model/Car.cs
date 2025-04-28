using System.Text.Json.Serialization;

public class Car
{
    public int Id  {get; set;}
    public bool IsRegistered {get; set;}
    public string? Model {get; set;}
    public string? Make {get; set;}
    public int BuildYear {get; set;}
    public int PersonId {get; set;}
    [JsonIgnore]
    public Person? Owner {get; private set;}
    public void SetOwner(Person owner)
    {
        Owner = owner;
        PersonId = owner.Id;
    }
}