using ConsoleGameEngine.Structs;

namespace ConsoleGameEngine
{
    public sealed class ScreenManager
    {
        private ScreenCell[,] _backBuffer = new ScreenCell[1, 1];
        private ScreenCell[,] _frontBuffer = new ScreenCell[1, 1];
        private bool _forceFullRedraw = true;

        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsResized { get; private set; }
        public ConsoleColor DefaultClearColor { get; set; } = ConsoleColor.Gray;

        public void Initialize(int? requestedWidth = null, int? requestedHeight = null)
        {
            Console.CursorVisible = false;

            if (requestedWidth.HasValue && requestedHeight.HasValue)
            {
                SetResolution(requestedWidth.Value, requestedHeight.Value);
            }
            else
            {
                RefreshResolution();
            }

            AllocateBuffers(Width, Height);
            Console.Clear();
            _forceFullRedraw = true;
        }

        public void SetResolution(int width, int height)
        {
            int maxW = Math.Max(20, Console.LargestWindowWidth);
            int maxH = Math.Max(10, Console.LargestWindowHeight);
            int targetW = Math.Clamp(width, 20, maxW);
            int targetH = Math.Clamp(height, 10, maxH);

#pragma warning disable CA1416
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.SetWindowSize(targetW, targetH);
                    Console.SetBufferSize(targetW, targetH);
                }
                catch
                {
                }
            }
#pragma warning restore CA1416

            RefreshResolution();
        }

        public void RefreshResolution()
        {
            int currentW = Math.Max(1, Console.WindowWidth);
            int currentH = Math.Max(1, Console.WindowHeight);

            IsResized = (currentW != Width || currentH != Height);
            if (IsResized)
            {
                Width = currentW;
                Height = currentH;
                AllocateBuffers(Width, Height);
                Console.Clear();
                _forceFullRedraw = true;
            }
        }

        private void AllocateBuffers(int width, int height)
        {
            _backBuffer = new ScreenCell[width, height];
            _frontBuffer = new ScreenCell[width, height];
            var blank = new ScreenCell(' ', DefaultClearColor);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _backBuffer[x, y] = blank;
                    _frontBuffer[x, y] = new ScreenCell('\0', DefaultClearColor);
                }
            }
        }

        public void ClearBackBuffer(char fillChar = ' ', ConsoleColor? color = null)
        {
            ConsoleColor targetColor = color ?? DefaultClearColor;
            var blank = new ScreenCell(fillChar, targetColor);

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _backBuffer[x, y] = blank;
                }
            }
        }

        public void DrawAt(int x, int y, char symbol, ConsoleColor color = ConsoleColor.Gray)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;

            _backBuffer[x, y] = new ScreenCell(symbol, color);
        }

        public void DrawText(int x, int y, string text, ConsoleColor color = ConsoleColor.Gray)
        {
            if (y < 0 || y >= Height || x >= Width || string.IsNullOrEmpty(text))
                return;

            for (int i = 0; i < text.Length; i++)
            {
                int targetX = x + i;
                if (targetX < 0)
                    continue;
                if (targetX >= Width)
                    break;

                _backBuffer[targetX, y] = new ScreenCell(text[i], color);
            }
        }

        public void Present()
        {
            ConsoleColor? currentColor = null;

            for (int y = 0; y < Height; y++)
            {
                int x = 0;
                while (x < Width)
                {
                    ScreenCell desired = _backBuffer[x, y];
                    ScreenCell current = _frontBuffer[x, y];

                    if (!_forceFullRedraw && desired.Equals(current))
                    {
                        x++;
                        continue;
                    }

                    Console.SetCursorPosition(x, y);
                    if (currentColor != desired.Color)
                    {
                        Console.ForegroundColor = desired.Color;
                        currentColor = desired.Color;
                    }

                    while (x < Width)
                    {
                        ScreenCell cell = _backBuffer[x, y];
                        if (!_forceFullRedraw && cell.Equals(_frontBuffer[x, y]))
                            break;

                        if (cell.Color != desired.Color)
                            break;

                        Console.Write(cell.Symbol);
                        _frontBuffer[x, y] = cell;
                        x++;
                    }
                }
            }

            Console.ResetColor();
            _forceFullRedraw = false;
        }

        public void Restore()
        {
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.Clear();
        }
    }
}
