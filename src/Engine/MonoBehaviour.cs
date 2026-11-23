namespace ConsoleApp_Game_Engine
{
    public abstract class MonoBehaviour : Behaviour
    {
        internal bool HasAwoken { get; set; }
    internal bool HasStarted { get; set; }

    protected virtual void Awake() { }
    protected virtual void Start() { }

    internal override void InternalAwake()
    {
        if (HasAwoken || IsDestroyed)
            return;

        HasAwoken = true;
        Awake();

        if (IsActiveAndEnabled)
            OnEnable();
    }

    internal override void InternalStart()
    {
        if (!HasAwoken || HasStarted || IsDestroyed || !IsActiveAndEnabled)
            return;

        HasStarted = true;
        Start();
    }
    }
}
