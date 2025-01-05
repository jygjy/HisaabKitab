using System.Text.Json;
using HisaabKitab.Models;

namespace HisaabKitab.Services
{
    public class CashFlowService
    {
        private static readonly string DesktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        private static readonly string FolderPath = Path.Combine(DesktopPath, "DataET");
        private static readonly string CashInFlowFilePath = Path.Combine(FolderPath, "cashinflows.json");
        private static readonly string CashOutFlowFilePath = Path.Combine(FolderPath, "cashoutflows.json");
        private static readonly string DebtFilePath = Path.Combine(FolderPath, "debts.json");
        private static readonly string RemindersFilePath = Path.Combine(FolderPath, "reminders.json");

        public CashFlowService()
        {
            Directory.CreateDirectory(FolderPath); // Ensure the directory exists on the desktop
        }

        // Load Cash Inflows
        public List<CIF> LoadCashInFlows()
        {
            if (!File.Exists(CashInFlowFilePath)) File.WriteAllText(CashInFlowFilePath, "[]");
            var json = File.ReadAllText(CashInFlowFilePath);
            return JsonSerializer.Deserialize<List<CIF>>(json) ?? new List<CIF>();
        }

        // Load Cash Outflows
        public List<COF> LoadCashOutFlows()
        {
            if (!File.Exists(CashOutFlowFilePath)) File.WriteAllText(CashOutFlowFilePath, "[]");
            var json = File.ReadAllText(CashOutFlowFilePath);
            return JsonSerializer.Deserialize<List<COF>>(json) ?? new List<COF>();
        }

        // Load Debts
        public List<Debt> LoadDebts()
        {
            if (!File.Exists(DebtFilePath)) File.WriteAllText(DebtFilePath, "[]");
            var json = File.ReadAllText(DebtFilePath);
            return JsonSerializer.Deserialize<List<Debt>>(json) ?? new List<Debt>();
        }

        // Save Cash Inflows
        private void SaveCashInFlows(List<CIF> cashInFlows)
        {
            var json = JsonSerializer.Serialize(cashInFlows);
            File.WriteAllText(CashInFlowFilePath, json);
        }

        // Save Cash Outflows
        private void SaveCashOutFlows(List<COF> cashOutFlows)
        {
            var json = JsonSerializer.Serialize(cashOutFlows);
            File.WriteAllText(CashOutFlowFilePath, json);
        }

        // Save Debts
        private void SaveDebts(List<Debt> debts)
        {
            var json = JsonSerializer.Serialize(debts);
            File.WriteAllText(DebtFilePath, json);
        }

        // Add a New Cash Inflow
        public void AddCashInFlow(CIF cashInFlow)
        {
            var inflows = LoadCashInFlows();
            inflows.Add(cashInFlow);
            SaveCashInFlows(inflows);
        }

        // Add a New Cash Outflow
        public void AddCashOutFlow(COF cashOutFlow)
        {
            var outflows = LoadCashOutFlows();
            outflows.Add(cashOutFlow);
            SaveCashOutFlows(outflows);
        }

        // Add a New Debt
        public void AddDebt(Debt debt)
        {
            var debts = LoadDebts();
            debts.Add(debt);
            SaveDebts(debts);
        }

        // Get Total Inflows
        public decimal GetTotalInflows()
        {
            var inflows = LoadCashInFlows();
            return inflows.Sum(i => i.Amount);
        }

        // Get Total Outflows
        public decimal GetTotalOutflows()
        {
            var outflows = LoadCashOutFlows();
            return outflows.Sum(o => o.Amount);
        }

        // Get Balance
        public decimal GetBalance()
        {
            return GetTotalInflows() - GetTotalOutflows();
        }

        // Get Cleared Debts
        public List<Debt> GetClearedDebts()
        {
            var debts = LoadDebts();
            return debts.Where(d => d.IsCleared).ToList();
        }

        // Save and Load Reminders
        public List<string> LoadReminders()
        {
            if (!File.Exists(RemindersFilePath)) File.WriteAllText(RemindersFilePath, "[]");
            var json = File.ReadAllText(RemindersFilePath);
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }

        public void SaveReminders(List<string> reminders)
        {
            var json = JsonSerializer.Serialize(reminders);
            File.WriteAllText(RemindersFilePath, json);
        }

        // Get Category Breakdown
        public Dictionary<string, decimal> GetCategoryBreakdown()
        {
            var outflows = LoadCashOutFlows();
            var totalOutflow = outflows.Sum(o => o.Amount);
            if (totalOutflow == 0) return new Dictionary<string, decimal>();

            return outflows.GroupBy(o => o.Category)
                           .ToDictionary(g => g.Key, g => Math.Round((g.Sum(o => o.Amount) / totalOutflow) * 100, 2));
        }

        // Filter Cash Inflows by Date Range and Tag
        public List<CIF> FilterCashInFlows(DateTime? startDate, DateTime? endDate, string tag = null)
        {
            var inflows = LoadCashInFlows();

            if (startDate.HasValue && endDate.HasValue)
            {
                inflows = inflows.Where(i => i.Date >= startDate.Value && i.Date <= endDate.Value).ToList();
            }

            if (!string.IsNullOrEmpty(tag))
            {
                inflows = inflows.Where(i => i.Tags?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            return inflows;
        }

        // Filter Cash Outflows by Date Range and Tag
        public List<COF> FilterCashOutFlows(DateTime? startDate, DateTime? endDate, string tag = null)
        {
            var outflows = LoadCashOutFlows();

            if (startDate.HasValue && endDate.HasValue)
            {
                outflows = outflows.Where(o => o.Date >= startDate.Value && o.Date <= endDate.Value).ToList();
            }

            if (!string.IsNullOrEmpty(tag))
            {
                outflows = outflows.Where(o => o.Tags?.Contains(tag, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            return outflows;
        }

        // Sort Cash Inflows by Date or Amount
        public List<CIF> SortCashInFlows(List<CIF> inflows, string sortBy, bool ascending = true)
        {
            return sortBy.ToLower() switch
            {
                "date" => ascending ? inflows.OrderBy(i => i.Date).ToList() : inflows.OrderByDescending(i => i.Date).ToList(),
                "amount" => ascending ? inflows.OrderBy(i => i.Amount).ToList() : inflows.OrderByDescending(i => i.Amount).ToList(),
                _ => inflows
            };
        }

        // Sort Cash Outflows by Date or Amount
        public List<COF> SortCashOutFlows(List<COF> outflows, string sortBy, bool ascending = true)
        {
            return sortBy.ToLower() switch
            {
                "date" => ascending ? outflows.OrderBy(o => o.Date).ToList() : outflows.OrderByDescending(o => o.Date).ToList(),
                "amount" => ascending ? outflows.OrderBy(o => o.Amount).ToList() : outflows.OrderByDescending(o => o.Amount).ToList(),
                _ => outflows
            };
        }
    }
}
