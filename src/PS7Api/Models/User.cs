namespace PS7Api.Models;

public class User
{
    public int Id { get; set; }
    public UserType Type { get; set; }
}

public enum UserType
{
    CustomsOfficer,
    Administrator
}