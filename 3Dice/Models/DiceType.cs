namespace _3Dice.Models
{
    public class DiceType
    {
        public int Sides { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Count { get; set; } = 0;
        
        public DiceType(int sides, string name)
        {
            Sides = sides;
            Name = name;
            Count = 0;
        }
    }
}