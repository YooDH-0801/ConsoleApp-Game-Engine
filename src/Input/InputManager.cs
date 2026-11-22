namespace ConsoleGameEngine
{
    public sealed class InputManager
    {
        private readonly HashSet<ConsoleKey> _pressedThisFrame = new HashSet<ConsoleKey>();
        private readonly HashSet<ConsoleKey> _heldKeys = new HashSet<ConsoleKey>();
        private readonly Dictionary<ConsoleKey, int> _keyHoldFrames = new Dictionary<ConsoleKey, int>();

        private const int DefaultHoldFrames = 2;

        public void Poll()
        {
            _pressedThisFrame.Clear();

            var keysToRemove = new List<ConsoleKey>();
            foreach (var pair in _keyHoldFrames)
            {
                int nextFrames = pair.Value - 1;
                if (nextFrames <= 0)
                {
                    keysToRemove.Add(pair.Key);
                }
                else
                {
                    _keyHoldFrames[pair.Key] = nextFrames;
                }
            }

            foreach (ConsoleKey key in keysToRemove)
            {
                _keyHoldFrames.Remove(key);
                _heldKeys.Remove(key);
            }

            while (Console.KeyAvailable)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(intercept: true);
                ConsoleKey key = keyInfo.Key;

                if (!_heldKeys.Contains(key))
                {
                    _pressedThisFrame.Add(key);
                }

                _heldKeys.Add(key);
                _keyHoldFrames[key] = DefaultHoldFrames;
            }
        }

        public bool GetKeyDown(ConsoleKey key) => _pressedThisFrame.Contains(key);

        public bool GetKey(ConsoleKey key) => _heldKeys.Contains(key);

        public int GetAxisRaw(string axisName)
        {
            if (string.Equals(axisName, "Horizontal", StringComparison.OrdinalIgnoreCase))
            {
                int value = 0;
                if (GetKey(ConsoleKey.LeftArrow) || GetKey(ConsoleKey.A)) value -= 1;
                if (GetKey(ConsoleKey.RightArrow) || GetKey(ConsoleKey.D)) value += 1;
                return value;
            }

            if (string.Equals(axisName, "Vertical", StringComparison.OrdinalIgnoreCase))
            {
                int value = 0;
                if (GetKey(ConsoleKey.UpArrow) || GetKey(ConsoleKey.W)) value -= 1;
                if (GetKey(ConsoleKey.DownArrow) || GetKey(ConsoleKey.S)) value += 1;
                return value;
            }

            return 0;
        }
    }
}
