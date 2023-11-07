using CompanyApi.DTO;

namespace CompanyApi
{
    public class Company
    {
        public List<Employee> EmployeesList { get; set; }
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
            EmployeesList = new List<Employee>();
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public static implicit operator Company(HttpResponseMessage v)
        {
            throw new NotImplementedException();
        }
    }
}
