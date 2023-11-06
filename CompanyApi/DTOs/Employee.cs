namespace CompanyApi.DTOs
{
    public class Employee
    {
        public Employee()
        {
            Id = Guid.NewGuid().ToString();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public string Position { get; set; }

        public string CompanyId { get; set; }
    }
}
