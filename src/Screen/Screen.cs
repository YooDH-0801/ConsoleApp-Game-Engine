namespace ConsoleGameEngine
{
    public static class Screen
    {
        private static ScreenManager? _current;

        private static ScreenManager Current =>
            _current ?? throw new InvalidOperationException("ScreenManager가 바인딩되지 않았습니다. Screen.Bind(...)를 먼저 호출하세요.");

        public static bool IsBound => _current != null;

        public static void Bind(ScreenManager manager) =>
            _current = manager ?? throw new ArgumentNullException(nameof(manager));

        public static void Unbind() => _current = null;

        public static int Width => Current.Width;
        public static int Height => Current.Height;

        public static void Clear(
            char glyph = ' ',
            ConsoleColor foreground = ConsoleColor.Gray,
            ConsoleColor background = ConsoleColor.Black) =>
            Current.Clear(glyph, foreground, background);

        public static void DrawPixel(
            int x,
            int y,
            char glyph,
            ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black) =>
            Current.DrawPixel(x, y, glyph, foreground, background);

        public static void DrawText(
            int x,
            int y,
            string? text,
            ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black) =>
            Current.DrawText(x, y, text, foreground, background);

        public static void Present() => Current.Present();

        public static void MarkDirtyAll() => Current.MarkDirtyAll();
    }
}
