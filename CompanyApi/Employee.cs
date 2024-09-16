namespace CompanyApi;

public class Employee
{
    public Employee(string name, string id)
    {
        Id = id;
        Name = name;
    }

    public string Id { get; set; }

    public string Name { get; set; }
}
