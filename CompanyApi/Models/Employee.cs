namespace CompanyApi.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public string CompanyName { get; set; }

        public Employee()
        {
        }

        public Employee(string name, string title, string companyName)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            Title = title;
            CompanyName = companyName;
        }
    }
}
