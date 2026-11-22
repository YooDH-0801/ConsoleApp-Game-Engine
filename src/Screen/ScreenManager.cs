using ConsoleGameEngine.Structs;

namespace ConsoleGameEngine
{
    public sealed class ScreenManager
    {
        private readonly char[,] _frontChar;
        private readonly char[,] _backChar;
        private readonly ConsoleColor[,] _frontFg;
        private readonly ConsoleColor[,] _backFg;
        private readonly ConsoleColor[,] _frontBg;
        private readonly ConsoleColor[,] _backBg;

        private int _lastConsoleWidth = -1;
        private int _lastConsoleHeight = -1;

        public int Width { get; }
        public int Height { get; }

        public ScreenManager(int width = 64, int height = 24)
        {
            Width = Math.Max(10, width);
            Height = Math.Max(5, height);

            _frontChar = new char[Width, Height];
            _backChar = new char[Width, Height];
            _frontFg = new ConsoleColor[Width, Height];
            _backFg = new ConsoleColor[Width, Height];
            _frontBg = new ConsoleColor[Width, Height];
            _backBg = new ConsoleColor[Width, Height];

            Clear(' ', ConsoleColor.Gray, ConsoleColor.Black);
            MarkDirtyAll();
        }

        public void Clear(
            char glyph = ' ',
            ConsoleColor foreground = ConsoleColor.Gray,
            ConsoleColor background = ConsoleColor.Black)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _backChar[x, y] = glyph;
                    _backFg[x, y] = foreground;
                    _backBg[x, y] = background;
                }
        }

        public void DrawPixel(
            int x,
            int y,
            char glyph,
            ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            _backChar[x, y] = glyph;
            _backFg[x, y] = foreground;
            _backBg[x, y] = background;
        }

        public void DrawText(
            int x,
            int y,
            string? text,
            ConsoleColor foreground = ConsoleColor.White,
            ConsoleColor background = ConsoleColor.Black)
        {
            if (y < 0 || y >= Height || string.IsNullOrEmpty(text))
                return;

            for (int i = 0; i < text.Length; i++)
            {
                int px = x + i;
                if (px >= 0 && px < Width)
                {
                    _backChar[px, y] = text[i];
                    _backFg[px, y] = foreground;
                    _backBg[px, y] = background;
                }
            }
        }

        public void Present()
        {
            if (!TryGetSafeConsoleBounds(out int bufferWidth, out int bufferHeight, out int windowWidth, out int windowHeight))
                return;

            if (bufferWidth <= 0 || bufferHeight <= 0 || windowWidth <= 0 || windowHeight <= 0)
                return;

            if (_lastConsoleWidth != bufferWidth || _lastConsoleHeight != bufferHeight)
            {
                _lastConsoleWidth = bufferWidth;
                _lastConsoleHeight = bufferHeight;
                MarkDirtyAll();
            }

            // 마지막 열/행 출력 시 발생할 수 있는 줄바꿈/스크롤을 피하기 위해 안전 한계 설정
            int maxDrawableX = Math.Min(Width, Math.Min(bufferWidth, windowWidth)) - 1;
            int maxDrawableY = Math.Min(Height, Math.Min(bufferHeight, windowHeight)) - 1;

            if (maxDrawableX < 0 || maxDrawableY < 0)
                return;

            ConsoleColor currentFg = Console.ForegroundColor;
            ConsoleColor currentBg = Console.BackgroundColor;

            try
            {
                for (int y = 0; y <= maxDrawableY; y++)
                    for (int x = 0; x <= maxDrawableX; x++)
                    {
                        char targetChar = _backChar[x, y];
                        ConsoleColor targetFg = _backFg[x, y];
                        ConsoleColor targetBg = _backBg[x, y];

                        if (targetChar != _frontChar[x, y] ||
                            targetFg != _frontFg[x, y] ||
                            targetBg != _frontBg[x, y])
                        {
                            Console.SetCursorPosition(x, y);

                            if (targetFg != currentFg)
                            {
                                Console.ForegroundColor = targetFg;
                                currentFg = targetFg;
                            }

                            if (targetBg != currentBg)
                            {
                                Console.BackgroundColor = targetBg;
                                currentBg = targetBg;
                            }

                            Console.Write(targetChar);

                            _frontChar[x, y] = targetChar;
                            _frontFg[x, y] = targetFg;
                            _frontBg[x, y] = targetBg;
                        }
                    }
            }
            catch (IOException)
            {
                MarkDirtyAll();
            }
            catch (ArgumentOutOfRangeException)
            {
                MarkDirtyAll();
            }
        }

        public void MarkDirtyAll()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    _frontChar[x, y] = '\0';
                    _frontFg[x, y] = (ConsoleColor)(-1);
                    _frontBg[x, y] = (ConsoleColor)(-1);
                }
        }

        private static bool TryGetSafeConsoleBounds(
            out int bufferWidth,
            out int bufferHeight,
            out int windowWidth,
            out int windowHeight)
        {
            bufferWidth = 0;
            bufferHeight = 0;
            windowWidth = 0;
            windowHeight = 0;

            try
            {
                bufferWidth = Console.BufferWidth;
                bufferHeight = Console.BufferHeight;
                windowWidth = Console.WindowWidth;
                windowHeight = Console.WindowHeight;
                return true;
            }
            catch (IOException)
            {
                return false;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
        }
    }
}
