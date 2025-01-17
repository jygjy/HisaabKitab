namespace HisaabKitab.Models
{
    public class Debt
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public decimal Amount { get; set; }
        public string Source { get; set; } = string.Empty;
        public DateTime ClearedDate { get; set; } // New property for cleared date
        public DateTime DueDate { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsCleared { get; set; }
    }
}