namespace ConsoleGameEngine
{
    public static class Time
    {
        private static TimeManager? _current;

        internal static void Bind(TimeManager manager) => _current = manager;
        internal static void Unbind(TimeManager manager)
        {
            if (ReferenceEquals(_current, manager))
                _current = null;
        }

        private static TimeManager Current =>
            _current ?? throw new InvalidOperationException("Time이 아직 GameLoop에 바인딩되지 않았습니다.");

        public static double FixedDeltaTime => Current.FixedDeltaTime;
        public static double MaxDeltaTime => Current.MaxDeltaTime;
        public static double DeltaTime => Current.DeltaTime;
        public static double UnscaledDeltaTime => Current.UnscaledDeltaTime;
        public static double TimeSinceLevelLoad => Current.TimeSinceLevelLoad;
        public static int FrameCount => Current.FrameCount;

        public static double TimeScale
        {
            get => Current.TimeScale;
            set => Current.TimeScale = value;
        }
    }
}
