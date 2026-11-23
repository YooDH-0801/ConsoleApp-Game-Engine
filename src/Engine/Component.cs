namespace ConsoleApp_Game_Engine
{
    public abstract class Component : Object
    {
        protected Component() : base(typeof(Component).Name) { }

        public GameObject GameObject { get; internal set; } = null!;
        public Transform Transform => GameObject.Transform;

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            OnDestroy();
        }

        protected virtual void OnDestroy() { }

        internal virtual void InternalAwake() { }
        internal virtual void InternalStart() { }
        internal virtual void InternalUpdate() { }
        internal virtual void InternalRender() { }
    }
}
