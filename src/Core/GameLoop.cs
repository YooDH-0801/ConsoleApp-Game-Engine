using System.Diagnostics;
using System.Text;

namespace ConsoleApp_GameEngine.Core
{
    public sealed class GameLoop
    {
        private readonly ApplicationManager _app;
        private readonly TimeManager _time;
        private readonly InputManager _input;
        private readonly ScreenManager _screen;

        public GameLoop(
        ApplicationManager app,
        TimeManager time,
        InputManager input,
        ScreenManager screen)
        {
            _app = app ?? throw new ArgumentNullException(nameof(app));
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _input = input ?? throw new ArgumentNullException(nameof(input));
            _screen = screen ?? throw new ArgumentNullException(nameof(screen));
        }

        public void Run()
        {
            // 1. 한글 및 특수문자 깨짐 방지 (UTF-8 설정)
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // 2. 커서 숨기기 (게임 화면에 깜빡이는 커서 제거)
            Console.CursorVisible = false;

            var stopwatch = Stopwatch.StartNew();

            try
            {
                Console.Clear();
                _screen.MarkDirtyAll();
                _app.IsPlaying = true;
                _time.Start();

                SceneManager.ApplyPendingLoad();

                while (_app.IsPlaying)
                {
                    double frameStart = stopwatch.Elapsed.TotalSeconds;

                    _time.Tick();
                    _input.Poll();

                    SceneManager.Update();
                    SceneManager.Render();

                    _app.WaitForNextFrame(stopwatch, frameStart);

                    SceneManager.ApplyPendingLoad();
                }
            }
            finally
            {
                SceneManager.Clear();
                _input.Clear();
                Console.ResetColor();
                Console.CursorVisible = true;
                Console.Clear();
            }
        }
    }
}