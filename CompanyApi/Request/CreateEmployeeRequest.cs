namespace CompanyApi.Request
{
    public class CreateEmployeeRequest
    {
        public CreateEmployeeRequest(string name)
        {
            Name = name;
        }
        public string Name { get; set; }
    }
}
