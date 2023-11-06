namespace CompanyApi.DTO
{
    public class Employee
    {
        public Employee()
        {
            EmployeeId = Guid.NewGuid().ToString();
        }

        public Employee(string id, string emloyeeName)
        {
            CompanyId = id;
            Name = emloyeeName;
            EmployeeId = Guid.NewGuid().ToString();
        }

        public string EmployeeId { get; set; }

        public string Name { get; set; }

        public string CompanyId {  get; set; }
        
    }
}
