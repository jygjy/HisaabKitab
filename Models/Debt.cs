namespace HisaabKitab.Models
{
    public class Debt
    {
        public decimal Amount { get; set; }
        public string Creditor { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool IsCleared { get; set; }
    }
}