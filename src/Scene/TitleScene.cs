namespace ConsoleApp_Game_Engine
{
    public sealed class TitleScene : Scene
    {
        public TitleScene() : base("Title") { }

        public override void OnEnter()
        {
            // 타이틀 씬 진입 시 필요한 초기화나 안내 메시지 리셋
        }

        public override void Update()
        {
            if (Input.GetKeyDown(ConsoleKey.Enter))
            {
                SceneManager.LoadScene("Play");
            }
            else if (Input.GetKeyDown(ConsoleKey.Escape))
            {
                Application.Quit();
            }
        }

        public override void Render()
        {
            Screen.Clear(' ', ConsoleColor.Gray, ConsoleColor.Black);

            Screen.DrawText(16, 6, "=== CONSOLE GAME ENGINE ===", ConsoleColor.Cyan);
            Screen.DrawText(14, 10, "[Enter]  Start Game (PlayScene)", ConsoleColor.White);
            Screen.DrawText(14, 12, "[Escape] Quit", ConsoleColor.DarkGray);
            Screen.DrawText(14, 16, $"Elapsed: {Time.ElapsedTime:F1}s", ConsoleColor.Yellow);

            Screen.Present();
        }
    }
}
