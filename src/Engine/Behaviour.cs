namespace ConsoleApp_Game_Engine
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

                _enabled = value;
                SyncEnableState();
            }
        }

        public bool IsActiveAndEnabled => !IsDestroyed && GameObject.ActiveInHierarchy && _enabled;

        internal void SyncEnableState()
        {
            if (IsDestroyed)
                return;

            bool shouldBeActive = GameObject.ActiveInHierarchy && _enabled;
            if (shouldBeActive)
            {
                if (this is MonoBehaviour mono && !mono.HasAwoken)
                    return;

                OnEnable();
            }
            else
            {
                OnDisable();
            }
        }

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            bool wasActive = GameObject.ActiveInHierarchy && _enabled;
            _enabled = false;

            if (wasActive)
                OnDisable();

            base.MarkDestroyed();
        }

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }

        internal override void InternalUpdate()
        {
            if (IsActiveAndEnabled)
                Update();
        }

        internal override void InternalRender()
        {
            if (IsActiveAndEnabled)
                Render();
        }

        protected virtual void Update() { }
        protected virtual void Render() { }
    }
}
