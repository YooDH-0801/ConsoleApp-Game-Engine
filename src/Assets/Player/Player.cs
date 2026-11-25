using ConsoleApp_GameEngine;

public class Player : MonoBehaviour
{
    private float _moveSpeed = 18f;


    protected override void Awake()
    {
        
    }

    protected override void OnEnable()
    {
        
    }

    protected override void Start()
    {
        
    }

    protected override void Update()
    {
        base.Update();

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

        Transform.X = Math.Clamp(Transform.X + dx * _moveSpeed * Time.DeltaTime, 1, Screen.Width - 2);
        Transform.Y = Math.Clamp(Transform.Y + dy * _moveSpeed * Time.DeltaTime, 2, Screen.Height - 2);
    }

    protected override void Render()
    {
        base.Render();

        int drawX = (int)MathF.Round(Transform.X);
        int drawY = (int)MathF.Round(Transform.Y);
        Screen.DrawPixel(drawX, drawY, '@', ConsoleColor.Yellow);
    }
}