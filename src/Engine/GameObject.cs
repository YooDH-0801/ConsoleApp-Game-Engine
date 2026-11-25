namespace ConsoleApp_GameEngine
{
    public sealed class GameObject : Object
    {
        private readonly List<Component> _components = new();
        private bool _activeSelf = true;
        private string _tag = "Untagged";
        private int _layer = 0;

        public Transform Transform { get; }
        public Scene? Scene { get; internal set; }

        public bool ActiveSelf => _activeSelf;

        public bool ActiveInHierarchy
        {
            get
            {
                if (IsDestroyed || !_activeSelf)
                    return false;

                // Transform 계층 구조가 연결되어 있다면 부모도 활성 상태여야 함
                Transform? parent = Transform?.Parent;
                while (parent != null)
                {
                    GameObject? parentGo = parent.GameObject;
                    if (parentGo == null || parentGo.IsDestroyed || !parentGo._activeSelf)
                        return false;

                    parent = parent.Parent;
                }

                return true;
            }
        }

        public string Tag
        {
            get => _tag;
            set => _tag = string.IsNullOrWhiteSpace(value) ? "Untagged" : value;
        }

        public int Layer
        {
            get => _layer;
            set => _layer = value;
        }

        public bool IsStatic { get; set; }

        public GameObject(string name = "GameObject") : base(name)
        {
            Transform = new Transform
            {
                Name = "Transform",
                GameObject = this
            };
            _components.Add(Transform);
        }

        // --- 활성 상태 제어 ---

        public void SetActive(bool value)
        {
            if (IsDestroyed || _activeSelf == value)
                return;

            bool wasActiveInHierarchy = ActiveInHierarchy;
            _activeSelf = value;
            bool nowActiveInHierarchy = ActiveInHierarchy;

            if (wasActiveInHierarchy != nowActiveInHierarchy)
            {
                SyncComponentsEnableState();
                SyncChildrenActiveState();
            }
        }

        // --- 컴포넌트 추가 / 조회 ---

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

        public T[] GetComponents<T>() where T : Component
        {
            var result = new List<T>();
            if (IsDestroyed)
                return result.ToArray();

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed && comp is T matched)
                    result.Add(matched);
            }

            return result.ToArray();
        }

        public void GetComponents<T>(List<T> results) where T : Component
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            if (IsDestroyed)
                return;

            for (int i = 0; i < _components.Count; i++)
            {
                Component comp = _components[i];
                if (!comp.IsDestroyed && comp is T matched)
                    results.Add(matched);
            }
        }

        public bool TryGetComponent<T>(out T? component) where T : Component
        {
            component = GetComponent<T>();
            return component != null;
        }

        public T? GetComponentInChildren<T>(bool includeInactive = false) where T : Component
        {
            if (IsDestroyed)
                return null;

            if (includeInactive || ActiveInHierarchy)
            {
                T? selfComp = GetComponent<T>();
                if (selfComp != null)
                    return selfComp;
            }

            foreach (Transform child in EnumerateChildren(Transform))
            {
                GameObject? childGo = child.GameObject;
                if (childGo == null || childGo.IsDestroyed)
                    continue;
                if (!includeInactive && !childGo.ActiveInHierarchy)
                    continue;

                T? found = childGo.GetComponent<T>();
                if (found != null)
                    return found;
            }

            return null;
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : Component
        {
            var results = new List<T>();
            if (IsDestroyed)
                return results.ToArray();

            if (includeInactive || ActiveInHierarchy)
            {
                foreach (Component comp in _components)
                {
                    if (!comp.IsDestroyed && comp is T matched)
                        results.Add(matched);
                }
            }

            foreach (Transform child in EnumerateChildren(Transform))
            {
                GameObject? childGo = child.GameObject;
                if (childGo == null || childGo.IsDestroyed)
                    continue;
                if (!includeInactive && !childGo.ActiveInHierarchy)
                    continue;

                foreach (Component comp in childGo._components)
                {
                    if (!comp.IsDestroyed && comp is T matched)
                        results.Add(matched);
                }
            }

            return results.ToArray();
        }

        public T? GetComponentInParent<T>() where T : Component
        {
            if (IsDestroyed)
                return null;

            T? selfComp = GetComponent<T>();
            if (selfComp != null)
                return selfComp;

            Transform? current = Transform?.Parent;
            while (current != null)
            {
                GameObject? parentGo = current.GameObject;
                if (parentGo != null && !parentGo.IsDestroyed)
                {
                    T? found = parentGo.GetComponent<T>();
                    if (found != null)
                        return found;
                }

                current = current.Parent;
            }

            return null;
        }

        // --- Static 탐색 및 생성 헬퍼 ---

        public static GameObject? Find(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (!go.IsDestroyed && go.Name == name)
                    return go;
            }

            return null;
        }

        public static GameObject? FindWithTag(string tag)
        {
            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (!go.IsDestroyed && go.Tag == tag)
                    return go;
            }

            return null;
        }

        public static GameObject[] FindGameObjectsWithTag(string tag)
        {
            var matches = new List<GameObject>();
            foreach (GameObject go in FindObjectsOfType<GameObject>())
            {
                if (!go.IsDestroyed && go.Tag == tag)
                    matches.Add(go);
            }

            return matches.ToArray();
        }

        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            var go = new GameObject(type.ToString());
            // 렌더러/메시 컴포넌트가 있다면 여기서 부착하도록 확장하세요.
            // 예: go.AddComponent<MeshFilter>(); go.AddComponent<MeshRenderer>();
            return go;
        }

        // --- 복제 (Object.Instantiate 연동) ---

        protected override Object CloneObject(Transform? parent)
        {
            var clone = new GameObject($"{Name} (Clone)")
            {
                _activeSelf = _activeSelf,
                _tag = _tag,
                _layer = _layer,
                IsStatic = IsStatic,
                Scene = Scene,
                HideFlags = HideFlags
            };

            if (parent != null)
            {
                clone.Transform.Parent = parent;
            }

            // 기존 Transform 외 컴포넌트 복제 (실제 엔진의 컴포넌트 복제 규칙에 맞게 확장)
            for (int i = 0; i < _components.Count; i++)
            {
                Component sourceComp = _components[i];
                if (sourceComp is Transform || sourceComp.IsDestroyed)
                    continue;

                var clonedComp = (Component)Activator.CreateInstance(sourceComp.GetType())!;
                clonedComp.Name = sourceComp.Name;
                clonedComp.GameObject = clone;
                clone._components.Add(clonedComp);
            }

            return clone;
        }

        // --- 파괴 및 엔진 루프 연동 ---

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;

            // 자식 오브젝트가 있다면 먼저 파괴 처리
            var children = new List<Transform>();
            foreach (Transform child in EnumerateDirectChildren(Transform))
                children.Add(child);

            for (int i = children.Count - 1; i >= 0; i--)
            {
                GameObject? childGo = children[i].GameObject;
                if (childGo != null && !childGo.IsDestroyed)
                    Destroy(childGo);
            }

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

        private void SyncChildrenActiveState()
        {
            if (IsDestroyed)
                return;

            foreach (Transform child in EnumerateDirectChildren(Transform))
            {
                GameObject? childGo = child.GameObject;
                if (childGo != null && !childGo.IsDestroyed)
                {
                    childGo.SyncComponentsEnableState();
                    childGo.SyncChildrenActiveState();
                }
            }
        }

        // Transform 계층 순회 헬퍼 (Transform 쪽 자식 컬렉션 프로퍼티명에 맞게 조정)
        private static IEnumerable<Transform> EnumerateDirectChildren(Transform root)
        {
            // 예시: root.Children (IEnumerable<Transform>) 또는 GetChild(i) / ChildCount 형태로 연결
            yield break;
        }

        private static IEnumerable<Transform> EnumerateChildren(Transform root)
        {
            var stack = new Stack<Transform>();
            foreach (Transform direct in EnumerateDirectChildren(root))
                stack.Push(direct);

            while (stack.Count > 0)
            {
                Transform current = stack.Pop();
                yield return current;

                foreach (Transform next in EnumerateDirectChildren(current))
                    stack.Push(next);
            }
        }
    }
}
