namespace DTO
{
    public class UpdateEnteringsDTO
    {
        public IEnumerable<string> Members { get; set; }
        public EnteringType? EnteringType { get; set; }
    }
}