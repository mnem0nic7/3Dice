using Microsoft.Maui.Controls;
using Microsoft.Maui.Accessibility;
using System.Collections.ObjectModel;
using _3Dice.Models;

namespace _3Dice
{
    public partial class MainPage : ContentPage
    {
        Random random = new();
        public ObservableCollection<DiceType> DiceTypes { get; set; } = new();
        
        public MainPage()
        {
            InitializeComponent();
            InitializeDiceTypes();
            DiceTypesCollectionView.ItemsSource = DiceTypes;
        }

        private void InitializeDiceTypes()
        {
            DiceTypes.Add(new DiceType(4, "D4"));
            DiceTypes.Add(new DiceType(6, "D6"));
            DiceTypes.Add(new DiceType(8, "D8"));
            DiceTypes.Add(new DiceType(10, "D10"));
            DiceTypes.Add(new DiceType(12, "D12"));
            DiceTypes.Add(new DiceType(20, "D20"));
            DiceTypes.Add(new DiceType(100, "D100"));
        }

        private void OnDecreaseDiceClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DiceType diceType)
            {
                if (diceType.Count > 0)
                {
                    diceType.Count--;
                    UpdateSummary();
                }
            }
        }

        private void OnIncreaseDiceClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is DiceType diceType)
            {
                if (diceType.Count < 20) // Limit to 20 dice per type
                {
                    diceType.Count++;
                    UpdateSummary();
                }
            }
        }

        private void OnClearAllClicked(object sender, EventArgs e)
        {
            foreach (var diceType in DiceTypes)
            {
                diceType.Count = 0;
            }
            UpdateSummary();
            DiceResultLabel.Text = "Select dice and roll!";
        }

        private void UpdateSummary()
        {
            // Update summary display
            var selectedDice = DiceTypes.Where(d => d.Count > 0).ToList();
            if (selectedDice.Any())
            {
                var summaryText = string.Join(", ", selectedDice.Select(d => $"{d.Count}x {d.Name}"));
                SelectionSummaryLabel.Text = summaryText;
                ClearAllBtn.IsVisible = true;
            }
            else
            {
                SelectionSummaryLabel.Text = "No dice selected";
                ClearAllBtn.IsVisible = false;
            }
        }

        private void OnRollDiceClicked(object sender, EventArgs e)
        {
            var selectedDice = DiceTypes.Where(d => d.Count > 0).ToList();
            
            if (!selectedDice.Any())
            {
                DiceResultLabel.Text = "Please select some dice first!";
                return;
            }

            var allResults = new List<string>();
            int grandTotal = 0;

            foreach (var diceType in selectedDice)
            {
                var groupResults = new List<int>();
                for (int i = 0; i < diceType.Count; i++)
                {
                    groupResults.Add(random.Next(1, diceType.Sides + 1));
                }

                int groupTotal = groupResults.Sum();
                grandTotal += groupTotal;

                string groupResult;
                if (diceType.Count == 1)
                {
                    groupResult = $"{diceType.Name}: {groupResults[0]}";
                }
                else
                {
                    string individualRolls = string.Join(", ", groupResults);
                    groupResult = $"{diceType.Name} ({diceType.Count}x): [{individualRolls}] = {groupTotal}";
                }
                
                allResults.Add(groupResult);
            }

            string finalResult;
            if (selectedDice.Count == 1 && selectedDice[0].Count == 1)
            {
                finalResult = $"🎲 Result:\n{allResults[0]}";
            }
            else
            {
                finalResult = $"🎲 Results:\n{string.Join("\n", allResults)}\n\n🎯 Grand Total: {grandTotal}";
            }

            DiceResultLabel.Text = finalResult;
            SemanticScreenReader.Announce($"Rolled dice. Grand total: {grandTotal}");
        }
    }
}
