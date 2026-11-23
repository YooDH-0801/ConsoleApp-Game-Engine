namespace ConsoleApp_Game_Engine
{
    public sealed class GameObject : Object
    {
        private readonly List<Component> _components = new();
        private bool _activeSelf = true;

        public Transform Transform { get; }
        public Scene? OwnerScene { get; internal set; }

        public bool ActiveSelf
        {
            get => _activeSelf;
            set
            {
                if (_activeSelf == value)
                    return;

                _activeSelf = value;
                SyncComponentsEnableState();
            }
        }

        public bool ActiveInHierarchy => !IsDestroyed && _activeSelf;

        public GameObject(string name = "GameObject") : base(name)
        {
            Transform = new Transform
            {
                Name = "Transform",
                GameObject = this
            };
            _components.Add(Transform);
        }

        public T AddComponent<T>() where T : Component, new()
        {
            if (IsDestroyed)
                throw new InvalidOperationException($"파괴된 GameObject('{Name}')에는 컴포넌트를 추가할 수 없습니다.");

            var component = new T
            {
                Name = typeof(T).Name,
                GameObject = this
            };

            _components.Add(component);

            if (component is MonoBehaviour mono && ActiveInHierarchy)
            {
                mono.InternalAwake();
                if (mono.Enabled)
                    mono.InternalStart();
            }

            return component;
        }

        public T? GetComponent<T>() where T : Component
        {
            if (IsDestroyed)
                return null;

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed && comp is T matched)
                    return matched;
            }

            return null;
        }

        public bool TryGetComponent<T>(out T? component) where T : Component
        {
            component = GetComponent<T>();
            return component != null;
        }

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            // 역순으로 순회하며 컴포넌트 파괴 (Transform 등 기본 컴포넌트도 안전하게 정리)
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed)
                    comp.MarkDestroyed();
            }
        }

        internal void FlushLifecycle()
        {
            if (IsDestroyed || !ActiveInHierarchy)
                return;

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (comp.IsDestroyed)
                    continue;

                if (comp is MonoBehaviour mono && mono.Enabled)
                {
                    mono.InternalAwake();
                    mono.InternalStart();
                }
            }
        }

        internal void UpdateComponents()
        {
            if (IsDestroyed || !ActiveInHierarchy)
                return;

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed)
                    comp.InternalUpdate();
            }
        }

        internal void RenderComponents()
        {
            if (IsDestroyed || !ActiveInHierarchy)
                return;

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed)
                    comp.InternalRender();
            }
        }

        internal void CleanupDestroyedComponents()
        {
            for (int i = _components.Count - 1; i >= 0; i--)
            {
                if (_components[i].IsDestroyed)
                    _components.RemoveAt(i);
            }
        }

        private void SyncComponentsEnableState()
        {
            if (IsDestroyed)
                return;

            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is Behaviour behaviour && !behaviour.IsDestroyed)
                    behaviour.SyncEnableState();
            }
        }
    }
}
