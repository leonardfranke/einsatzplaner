namespace Web.Models
{
    public class RequirementGroup
    {
        public string Id { get; set; }

        public Dictionary<string, uint> Requirements { get; set; }
    }
}
