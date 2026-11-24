using System.Runtime.InteropServices;

namespace ConsoleApp_Game_Engine
{
    public sealed class InputManager
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // 중복 열거형 값(예: PrintScreen/Snapshot 등)을 제거해 순회 효율 최적화
        private static readonly ConsoleKey[] _allKeys = GetUniqueConsoleKeys();

        private readonly HashSet<ConsoleKey> _previousHeld = new();
        private readonly HashSet<ConsoleKey> _currentHeld = new();
        private readonly HashSet<ConsoleKey> _downThisFrame = new();
        private readonly HashSet<ConsoleKey> _upThisFrame = new();
        private readonly List<ConsoleKeyInfo> _frameKeyInfos = new();

        public IReadOnlyList<ConsoleKeyInfo> FrameKeyInfos => _frameKeyInfos;

        private static ConsoleKey[] GetUniqueConsoleKeys()
        {
            var seenValues = new HashSet<int>();
            var keys = new List<ConsoleKey>();

            foreach (ConsoleKey key in Enum.GetValues<ConsoleKey>())
            {
                int vk = (int)key;
                if (vk > 0 && seenValues.Add(vk))
                {
                    keys.Add(key);
                }
            }

            return keys.ToArray();
        }

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
            foreach (ConsoleKey key in _allKeys)
            {
                int vk = (int)key;
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
    }
}