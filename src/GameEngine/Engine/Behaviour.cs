namespace ConsoleApp_GameEngine
{
    public abstract class Behaviour : Component
    {
        private bool _enabled = true;
        private bool _isCurrentlyEnabledInLifecycle;

        protected Behaviour() : base(null) { }

        protected Behaviour(string? name) : base(name) { }

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

        public bool IsActiveAndEnabled =>
            !IsDestroyed &&
            GameObject != null &&
            !GameObject.IsDestroyed &&
            GameObject.ActiveInHierarchy &&
            _enabled;

        internal void SyncEnableState()
        {
            if (IsDestroyed || GameObject == null || GameObject.IsDestroyed)
            {
                if (_isCurrentlyEnabledInLifecycle)
                {
                    _isCurrentlyEnabledInLifecycle = false;
                    OnDisable();
                }
                return;
            }

            bool shouldBeActive = GameObject.ActiveInHierarchy && _enabled;

            if (shouldBeActive)
            {
                // MonoBehaviour의 경우 Awake가 먼저 끝난 뒤 활성화 콜백이 흐르도록 조율
                if (this is MonoBehaviour mono && !mono.HasAwoken)
                    return;

                if (!_isCurrentlyEnabledInLifecycle)
                {
                    _isCurrentlyEnabledInLifecycle = true;
                    OnEnable();
                }
            }
            else
            {
                if (_isCurrentlyEnabledInLifecycle)
                {
                    _isCurrentlyEnabledInLifecycle = false;
                    OnDisable();
                }
            }
        }

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            bool wasActiveLifecycle = _isCurrentlyEnabledInLifecycle;
            _enabled = false;
            _isCurrentlyEnabledInLifecycle = false;

            if (wasActiveLifecycle)
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
