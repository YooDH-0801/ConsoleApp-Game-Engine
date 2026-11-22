namespace ConsoleGameEngine.Structs
{
    public readonly struct ScreenCell : IEquatable<ScreenCell>
    {
        public char Symbol { get; }
        public ConsoleColor Color { get; }

        public ScreenCell(char symbol, ConsoleColor color)
        {
            Symbol = symbol;
            Color = color;
        }

        public bool Equals(ScreenCell other) => Symbol == other.Symbol && Color == other.Color;
        public override bool Equals(object? obj) => obj is ScreenCell other && Equals(other);
        public override int GetHashCode() => HashCode.Combine(Symbol, Color);
    }
}
