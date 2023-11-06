namespace CompanyApi.DTOs
{
    public class EmployeeCreate
    {
        public required string Name {  get; set; }

        public string Position { get; set; }

        public string CompanyId {  get; set; }
    }
}
