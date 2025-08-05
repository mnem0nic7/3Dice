using System.ComponentModel;

namespace _3Dice.Models
{
    public class DiceType : INotifyPropertyChanged
    {
        private int _count = 0;
        
        public int Sides { get; set; }
        public string Name { get; set; } = string.Empty;
        
        public int Count 
        { 
            get => _count;
            set
            {
                if (_count != value)
                {
                    _count = value;
                    OnPropertyChanged(nameof(Count));
                }
            }
        }
        
        public DiceType(int sides, string name)
        {
            Sides = sides;
            Name = name;
            Count = 0;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}