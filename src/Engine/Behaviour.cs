namespace ConsoleGameEngine
{
    public abstract class Behaviour : Component
    {
        private bool _enabled = true;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                    return;

                bool wasActiveAndEnabled = IsActiveAndEnabled;
                _enabled = value;
                bool nowActiveAndEnabled = IsActiveAndEnabled;

                if (!wasActiveAndEnabled && nowActiveAndEnabled)
                    OnEnable();
                else if (wasActiveAndEnabled && !nowActiveAndEnabled)
                    OnDisable();
            }
        }

        public bool IsActiveAndEnabled => GameObject != null && GameObject.ActiveSelf && _enabled;

        internal bool HasAwoken { get; set; }
        internal bool HasStarted { get; set; }

        protected Behaviour(string name = "Behaviour") : base(name) { }

        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
    }
}
