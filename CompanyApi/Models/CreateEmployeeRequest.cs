namespace CompanyApi.Models
{
    public class CreateEmployeeRequest
    {
        public string Name { get; set; }
        public string CompanyName { get; set; }
        public string Title { get; set; }

        public CreateEmployeeRequest(string name, string companyName, string title)
        {
            Name = name;
            CompanyName = companyName;
            Title = title;
        }
    }
}
