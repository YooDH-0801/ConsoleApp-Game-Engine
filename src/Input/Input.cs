namespace ConsoleApp_Game_Engine
{
    public static class Input
    {
        private static InputManager? _current;

        private static InputManager Current =>
            _current ?? throw new InvalidOperationException("InputManager가 바인딩되지 않았습니다. Input.Bind(...)를 먼저 호출하세요.");

        public static bool IsBound => _current != null;

        public static IReadOnlyList<ConsoleKeyInfo> FrameKeyInfos => Current.FrameKeyInfos;

        public static void Bind(InputManager manager)
        {
            _current = manager ?? throw new ArgumentNullException(nameof(manager));
            _current.Clear();
        }

        public static void Unbind()
        {
            _current?.Clear();
            _current = null;
        }

        public static bool GetKeyDown(ConsoleKey key) => Current.GetKeyDown(key);

        public static bool GetKey(ConsoleKey key) => Current.GetKey(key);

        public static bool GetKeyUp(ConsoleKey key) => Current.GetKeyUp(key);

        public static void Clear() => Current.Clear();
    }
}
