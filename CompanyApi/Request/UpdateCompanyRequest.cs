namespace CompanyApi
{
    public class UpdateCompanyRequest
    {
        public UpdateCompanyRequest(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
