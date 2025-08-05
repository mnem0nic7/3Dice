namespace _3Dice.Models
{
    public class DiceGroup
    {
        public int Sides { get; set; }
        public int Count { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        
        public DiceGroup(int sides, int count, string displayName)
        {
            Sides = sides;
            Count = count;
            DisplayName = displayName;
        }
    }
}