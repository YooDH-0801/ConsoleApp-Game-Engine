using ConsoleApp_Game_Engine;

namespace ConsoleApp_Game_Engine
{
    public static class Application
    {
        private static ApplicationManager? _current;

        private static ApplicationManager Current =>
            _current ?? throw new InvalidOperationException("ApplicationManager가 바인딩되지 않았습니다.");

        public static bool IsBound => _current != null;

        public static void Bind(ApplicationManager manager) =>
            _current = manager ?? throw new ArgumentNullException(nameof(manager));

        public static void Unbind() => _current = null;

        public static bool IsPlaying => Current.IsPlaying;

        public static int TargetFrameRate
        {
            get => Current.TargetFrameRate;
            set => Current.TargetFrameRate = value;
        }

        public static void Quit() => Current.Quit();
    }
}
