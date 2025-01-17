namespace HisaabKitab.Models
{
    public class CIF
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Tags { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }
}