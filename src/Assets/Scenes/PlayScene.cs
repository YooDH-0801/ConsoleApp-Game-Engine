using ConsoleApp_GameEngine;

public sealed class PlayScene : Scene
{
    private GameObject _player;

    private int _score;

    public PlayScene() : base("Play") { }

    public override void OnEnter()
    {
        _player = new GameObject("Player");
        _player.AddComponent<Player>();

        _player.Transform.X = Screen.Width / 2f;
        _player.Transform.Y = Screen.Height / 2f;
 
        _score = 0;
    }

    public override void Update()
    {
        if (Input.GetKeyDown(ConsoleKey.Escape))
        {
            SceneManager.LoadScene("Title");
            return;
        }

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

        

        Screen.Present();
    }

    public override void OnExit()
    {
        // 플레이 씬 이탈 시 정리할 리소스나 상태가 있다면 여기서 처리
    }
}