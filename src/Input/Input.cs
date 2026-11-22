namespace ConsoleGameEngine
{
    public static class Input
    {
        private static InputManager? _current;

        internal static void Bind(InputManager manager) => _current = manager;
        internal static void Unbind(InputManager manager)
        {
            if (ReferenceEquals(_current, manager))
                _current = null;
        }

        private static InputManager Current =>
            _current ?? throw new InvalidOperationException("Input이 아직 GameLoop에 바인딩되지 않았습니다.");

        public static bool GetKeyDown(ConsoleKey key) => Current.GetKeyDown(key);
        public static bool GetKey(ConsoleKey key) => Current.GetKey(key);
        public static int GetAxisRaw(string axisName) => Current.GetAxisRaw(axisName);
    }
}
