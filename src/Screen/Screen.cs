namespace ConsoleGameEngine
{
    public static class Screen
    {
        private static ScreenManager? _current;

        internal static void Bind(ScreenManager manager) => _current = manager;
        internal static void Unbind(ScreenManager manager)
        {
            if (ReferenceEquals(_current, manager))
                _current = null;
        }

        private static ScreenManager Current =>
            _current ?? throw new InvalidOperationException("Screen이 아직 GameLoop에 바인딩되지 않았습니다.");

        public static int Width => Current.Width;
        public static int Height => Current.Height;
        public static bool IsResized => Current.IsResized;

        public static ConsoleColor DefaultClearColor
        {
            get => Current.DefaultClearColor;
            set => Current.DefaultClearColor = value;
        }

        public static void SetResolution(int width, int height) => Current.SetResolution(width, height);
        public static void ClearBackBuffer(char fillChar = ' ', ConsoleColor? color = null) =>
            Current.ClearBackBuffer(fillChar, color);
        public static void DrawAt(int x, int y, char symbol, ConsoleColor color = ConsoleColor.Gray) =>
            Current.DrawAt(x, y, symbol, color);
        public static void DrawText(int x, int y, string text, ConsoleColor color = ConsoleColor.Gray) =>
            Current.DrawText(x, y, text, color);
        public static void Present() => Current.Present();
    }
}
