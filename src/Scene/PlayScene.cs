namespace ConsoleApp_GameEngine
{
    public sealed class PlayScene : Scene
    {
        private float _playerX;
        private float _playerY;
        private readonly float _moveSpeed = 18f;
        private int _score;

        public PlayScene() : base("Play") { }

        public override void OnEnter()
        {

            _playerX = Screen.Width / 2f;
            _playerY = Screen.Height / 2f;
            _score = 0;
        }

        public override void Update()
        {
            if (Input.GetKeyDown(ConsoleKey.Escape))
            {
                SceneManager.LoadScene("Title");
                return;
            }

            float dx = 0f;
            float dy = 0f;

            if (Input.GetKey(ConsoleKey.LeftArrow)) dx -= 1f;
            if (Input.GetKey(ConsoleKey.RightArrow)) dx += 1f;
            if (Input.GetKey(ConsoleKey.UpArrow)) dy -= 1f;
            if (Input.GetKey(ConsoleKey.DownArrow)) dy += 1f;

            if (dx != 0f && dy != 0f)
            {
                dx *= 0.7071f;
                dy *= 0.7071f;
            }

            _playerX = Math.Clamp(_playerX + dx * _moveSpeed * Time.DeltaTime, 1, Screen.Width - 2);
            _playerY = Math.Clamp(_playerY + dy * _moveSpeed * Time.DeltaTime, 2, Screen.Height - 2);

            if (Input.GetKeyDown(ConsoleKey.Spacebar))
            {
                _score += 10;
            }
        }

        public override void Render()
        {
            Screen.Clear(' ', ConsoleColor.Gray, ConsoleColor.Black);

            Screen.DrawText(2, 0, $"Score: {_score}  |  FPS Target: {Application.TargetFrameRate}", ConsoleColor.Green);
            Screen.DrawText(2, 1, "[Arrows] Move   [Space] +10 Score   [Esc] Title", ConsoleColor.DarkGray);

            int drawX = (int)MathF.Round(_playerX);
            int drawY = (int)MathF.Round(_playerY);
            Screen.DrawPixel(drawX, drawY, '@', ConsoleColor.Yellow);

            Screen.Present();
        }

        public override void OnExit()
        {
            // 플레이 씬 이탈 시 정리할 리소스나 상태가 있다면 여기서 처리
        }
    }
}
