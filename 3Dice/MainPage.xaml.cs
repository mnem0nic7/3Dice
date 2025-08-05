using Microsoft.Maui.Controls;
using Microsoft.Maui.Accessibility;
using System.Collections.ObjectModel;
using _3Dice.Models;
using _3Dice.Views;

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
            
            // Subscribe to 3D dice roll events
            DiceRollingView.DiceRolled += OnDiceRolled;
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
            DiceResultLabel.Text = "Select dice and roll to see realistic 3D physics!";
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

            // Start 3D dice rolling animation
            DiceRollingView.RollDice(selectedDice);
            DiceResultLabel.Text = "🎲 Rolling 3D dice with physics... Watch them bounce!";
        }

        private async void OnFullScreenRollClicked(object sender, EventArgs e)
        {
            var selectedDice = DiceTypes.Where(d => d.Count > 0).ToList();
            
            if (!selectedDice.Any())
            {
                await DisplayAlert("No Dice Selected", "Please select some dice before rolling!", "OK");
                return;
            }

            // Launch full-screen immersive dice rolling experience
            var fullScreenPage = new FullScreenDiceRollingPage(selectedDice);
            await Navigation.PushAsync(fullScreenPage);
        }

        private void OnDiceRolled(object? sender, DiceRolledEventArgs e)
        {
            // Process 3D dice results
            var allResults = new List<string>();
            int grandTotal = 0;

            foreach (var group in e.Results)
            {
                var diceType = DiceTypes.First(d => d.Sides == group.Sides);
                
                int groupTotal = group.Values.Sum();
                grandTotal += groupTotal;

                string groupResult;
                if (group.Values.Count == 1)
                {
                    groupResult = $"{diceType.Name}: {group.Values[0]}";
                }
                else
                {
                    string individualRolls = string.Join(", ", group.Values);
                    groupResult = $"{diceType.Name} ({group.Values.Count}x): [{individualRolls}] = {groupTotal}";
                }
                
                allResults.Add(groupResult);
            }

            string finalResult;
            if (e.Results.Count == 1 && e.Results[0].Values.Count == 1)
            {
                finalResult = $"🎲 3D Physics Result:\n{allResults[0]}";
            }
            else
            {
                finalResult = $"🎲 3D Physics Results:\n{string.Join("\n", allResults)}\n\n🎯 Grand Total: {grandTotal}";
            }

            // Update UI on main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DiceResultLabel.Text = finalResult;
                SemanticScreenReader.Announce($"3D dice rolled. Grand total: {grandTotal}");
            });
        }
    }
}
