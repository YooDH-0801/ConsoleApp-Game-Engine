namespace ConsoleApp_Game_Engine
{
    public sealed class Transform : Component
    {
        public float X { get; set; }
        public float Y { get; set; }

        public void Translate(float dx, float dy)
        {
            X += dx;
            Y += dy;
        }

        public void SetPosition(float x, float y)
        {
            X = x;
            Y = y;
        }
    }
}
