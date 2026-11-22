namespace ConsoleApp_Game_Engine
{
    public static class Time
    {
        private static TimeManager? _current;

        private static TimeManager Current =>
            _current ?? throw new InvalidOperationException("TimeManager가 바인딩되지 않았습니다.");

        public static bool IsBound => _current != null;

        public static void Bind(TimeManager manager) =>
            _current = manager ?? throw new ArgumentNullException(nameof(manager));

        public static void Unbind() => _current = null;

        public static float DeltaTime => Current.DeltaTime;
        public static float UnscaledDeltaTime => Current.UnscaledDeltaTime;
        public static float ElapsedTime => Current.ElapsedTime;
        public static float UnscaledElapsedTime => Current.UnscaledElapsedTime;
        public static long FrameCount => Current.FrameCount;

        public static float TimeScale
        {
            get => Current.TimeScale;
            set => Current.TimeScale = value;
        }

        public static double MaxDeltaTime
        {
            get => Current.MaxDeltaTime;
            set => Current.MaxDeltaTime = value;
        }
    }
}
