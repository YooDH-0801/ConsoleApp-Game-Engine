using System.Runtime.InteropServices;

namespace ConsoleApp_Game_Engine
{
    public sealed class InputManager
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        private readonly HashSet<ConsoleKey> _previousHeld = new();
        private readonly HashSet<ConsoleKey> _currentHeld = new();
        private readonly HashSet<ConsoleKey> _downThisFrame = new();
        private readonly HashSet<ConsoleKey> _upThisFrame = new();
        private readonly List<ConsoleKeyInfo> _frameKeyInfos = new();

        // 감시할 키 목록 (필요한 키를 추가)
        private static readonly ConsoleKey[] _watchedKeys =
        {
        ConsoleKey.LeftArrow,
        ConsoleKey.RightArrow,
        ConsoleKey.UpArrow,
        ConsoleKey.DownArrow,
        ConsoleKey.W,
        ConsoleKey.A,
        ConsoleKey.S,
        ConsoleKey.D,
        ConsoleKey.Spacebar,
        ConsoleKey.Enter,
        ConsoleKey.Escape
    };

        public IReadOnlyList<ConsoleKeyInfo> FrameKeyInfos => _frameKeyInfos;

        public void Poll()
        {
            _previousHeld.Clear();
            foreach (ConsoleKey key in _currentHeld)
            {
                _previousHeld.Add(key);
            }

            _currentHeld.Clear();
            _downThisFrame.Clear();
            _upThisFrame.Clear();
            _frameKeyInfos.Clear();

            // 콘솔 버퍼에 쌓인 키 문자/정보는 비워서 콘솔 화면에 출력되지 않게 유지
            DrainConsoleBuffer();

            if (OperatingSystem.IsWindows())
            {
                PollWindowsAsyncKeys();
            }
        }

        public bool GetKeyDown(ConsoleKey key) => _downThisFrame.Contains(key);
        public bool GetKey(ConsoleKey key) => _currentHeld.Contains(key);
        public bool GetKeyUp(ConsoleKey key) => _upThisFrame.Contains(key);

        public void Clear()
        {
            _previousHeld.Clear();
            _currentHeld.Clear();
            _downThisFrame.Clear();
            _upThisFrame.Clear();
            _frameKeyInfos.Clear();
        }

        private void PollWindowsAsyncKeys()
        {
            foreach (ConsoleKey key in _watchedKeys)
            {
                int vk = ToVirtualKey(key);
                if (vk == 0) continue;

                bool isDownNow = (GetAsyncKeyState(vk) & 0x8000) != 0;
                if (isDownNow)
                {
                    _currentHeld.Add(key);
                    if (!_previousHeld.Contains(key))
                    {
                        _downThisFrame.Add(key);
                    }
                }
                else if (_previousHeld.Contains(key))
                {
                    _upThisFrame.Add(key);
                }
            }
        }

        private void DrainConsoleBuffer()
        {
            try
            {
                while (Console.KeyAvailable)
                {
                    _frameKeyInfos.Add(Console.ReadKey(intercept: true));
                }
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static int ToVirtualKey(ConsoleKey key) => key switch
        {
            ConsoleKey.LeftArrow => 0x25,
            ConsoleKey.UpArrow => 0x26,
            ConsoleKey.RightArrow => 0x27,
            ConsoleKey.DownArrow => 0x28,
            ConsoleKey.Spacebar => 0x20,
            ConsoleKey.Enter => 0x0D,
            ConsoleKey.Escape => 0x1B,
            >= ConsoleKey.A and <= ConsoleKey.Z => (int)key,
            _ => 0
        };
    }
}