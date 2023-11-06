namespace CompanyApi
{
    public class Company
    {
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Company c && Name == c.Name && Id == c.Id;
        }

        public string Id { get; set; }

        public string Name { get; set; }
    }
}
