using System.Drawing;

namespace CompanyApi
{
    public class Company
    {
        public Company(string name)
        {
            Id = Guid.NewGuid().ToString();
            Name = name;
        }

        public string Id { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            return obj is Company c && Name == c.Name && Id == c.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        internal object? ToList()
        {
            throw new NotImplementedException();
        }
    }
}
