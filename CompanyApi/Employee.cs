namespace CompanyApi
{
    public class Employee
    {
        public Employee(string name, string companyId)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            CompanyId = companyId;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string CompanyId { get; set; }
    }
}
