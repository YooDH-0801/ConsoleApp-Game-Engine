using ConsoleApp_GameEngine.Core;

namespace ConsoleApp_GameEngine
{
    public class GameEngine
    {
        static void Main(string[] args)
        {
            var appManager = new ApplicationManager { TargetFrameRate = 30 };
            var timeManager = new TimeManager();
            var inputManager = new InputManager();
            var screenManager = new ScreenManager(width: 128, height: 48);
            var sceneManager = new SceneManager();

            Application.Bind(appManager);
            Time.Bind(timeManager);
            Input.Bind(inputManager);
            Screen.Bind(screenManager);

            try
            {
                SceneBuilder.Build();

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
