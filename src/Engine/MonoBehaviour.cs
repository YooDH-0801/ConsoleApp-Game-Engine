namespace ConsoleGameEngine
{
    public abstract class MonoBehaviour : Behaviour
    {
        protected MonoBehaviour(string name = "MonoBehaviour") : base(name) { }

        public virtual void Awake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
        public virtual void LateUpdate() { }
        public virtual void OnDestroy() { }
    }
}
