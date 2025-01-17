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
        //public void AddDebt(Debt debt)
        //{
        //    var debts = LoadDebts();
        //    debts.Add(debt);
        //    SaveDebts(debts);
        //}
        public void AddDebt(Debt debt)
        {
            var debts = LoadDebts();
            debts.Add(debt);
            SaveDebts(debts);

            // Record the debt as a cash inflow to increase the balance
            var inflows = LoadCashInFlows();
            inflows.Add(new CIF
            {
                Id = Guid.NewGuid(),
                Amount = debt.Amount,
                Source = debt.Source,
                Date = debt.DueDate,
                Tags = "Debt",
                Notes = $"Debt added from {debt.Source}"
            });
            SaveCashInFlows(inflows);
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
            return LoadDebts().Where(d => d.IsCleared).ToList();
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
        public List<dynamic> LoadAllTransactions()
        {
            var inflows = LoadCashInFlows().Select(i => new
            {
                Id = i.Id,
                Type = "Inflow",
                Amount = i.Amount,
                Category = i.Source, // Display Source as Category for inflows
                Date = i.Date,
                Tags = i.Tags,
                Notes = i.Notes
            });

            var outflows = LoadCashOutFlows().Select(o => new
            {
                Id = o.Id,
                Type = "Outflow",
                Amount = o.Amount,
                Category = o.Category,
                Date = o.Date,
                Tags = o.Tags,
                Notes = o.Notes
            });
            var debts = LoadDebts().Select(d => new
            {
                Id = d.Id,
                Type = "Debt",
                Amount = d.Amount,
                Category = d.Source, // Display Source as Category for debts
                Date = d.DueDate,
                Tags = "Debt",
                Notes = d.Notes,
                IsCleared = d.IsCleared // Include cleared status for debts
            });
            return inflows.Concat(outflows).ToList<dynamic>();
        }






        public void UpdateTransaction(string type, Guid id, decimal amount, string category, DateTime date, string tags, string notes)
        {
            if (type == "Inflow")
            {
                var inflows = LoadCashInFlows();
                var inflow = inflows.FirstOrDefault(i => i.Id == id);
                if (inflow != null)
                {
                    inflow.Amount = amount;
                    inflow.Source = category; // Update Source for inflows
                    inflow.Date = date;
                    inflow.Tags = tags;
                    inflow.Notes = notes;
                    SaveCashInFlows(inflows);
                }
            }
            else if (type == "Outflow")
            {
                var outflows = LoadCashOutFlows();
                var outflow = outflows.FirstOrDefault(o => o.Id == id);
                if (outflow != null)
                {
                    outflow.Amount = amount;
                    outflow.Category = category;
                    outflow.Date = date;
                    outflow.Tags = tags;
                    outflow.Notes = notes;
                    SaveCashOutFlows(outflows);
                }
            }
        }
        public void DeleteTransaction(string type, Guid id)
        {
            if (type == "Inflow")
            {
                var inflows = LoadCashInFlows();
                inflows.RemoveAll(i => i.Id == id);
                SaveCashInFlows(inflows);
            }
            else if (type == "Outflow")
            {
                var outflows = LoadCashOutFlows();
                outflows.RemoveAll(o => o.Id == id);
                SaveCashOutFlows(outflows);
            }

        }


        public void ClearDebt(Guid debtId)
        {
            var debts = LoadDebts();
            var debt = debts.FirstOrDefault(d => d.Id == debtId && !d.IsCleared);

            if (debt != null && GetBalance() >= debt.Amount)
            {
                debt.IsCleared = true;
                debt.ClearedDate = DateTime.Now;
                debt.Notes = $"Cleared on {DateTime.Now.ToShortDateString()}"; // Example note
                SaveDebts(debts);

                // Optionally, deduct the cleared debt amount from balance by recording it as an outflow
                var outflows = LoadCashOutFlows();
                outflows.Add(new COF
                {
                    Id = Guid.NewGuid(),
                    Amount = debt.Amount,
                    Category = "Debt Clearance",
                    Date = DateTime.Now,
                    Tags = "Debt",
                    Notes = $"Cleared debt from {debt.Source}"
                });
                SaveCashOutFlows(outflows);
            }
            else if (debt != null)
            {
                throw new InvalidOperationException("Insufficient balance to clear the debt.");
            }
            else
            {
                throw new InvalidOperationException("Debt not found or already cleared.");
            }
        }





    }
}
