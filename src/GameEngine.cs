using ConsoleApp_Game_Engine.Core;

namespace ConsoleApp_Game_Engine
{
    public class GameEngine
    {
        static void Main(string[] args)
        {
            var appManager = new ApplicationManager { TargetFrameRate = 30 };
            var timeManager = new TimeManager();
            var inputManager = new InputManager();
            var screenManager = new ScreenManager(width: 64, height: 24);

            Application.Bind(appManager);
            Time.Bind(timeManager);
            Input.Bind(inputManager);
            Screen.Bind(screenManager);

            try
            {
                // TODO:: 이거 하고 오브젝트 계열 구현
                // 예시:
                SceneManager.Register(new TitleScene());
                SceneManager.Register(new PlayScene());
                SceneManager.LoadScene("Title");

                var loop = new GameLoop(appManager, timeManager, inputManager, screenManager);
                loop.Run();
            }
            finally
            {
                SceneManager.Clear();
                Screen.Unbind();
                Input.Unbind();
                Time.Unbind();
                Application.Unbind();
            }
        }
    }
}
