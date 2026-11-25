namespace ConsoleApp_GameEngine
{
    public abstract class Object : IEquatable<Object>
    {
        private static int _nextInstanceId = 1;
        private static readonly HashSet<Object> _aliveObjects = new();
        private static readonly HashSet<Object> _dontDestroyOnLoadObjects = new();

        private readonly int _instanceId;
        private string _name;

        public string Name
        {
            get => _name;
            set => _name = value ?? string.Empty;
        }

        public HideFlags HideFlags { get; set; } = HideFlags.None;

        public bool IsDestroyed { get; internal set; }

        protected Object() : this(null)
        {
        }

        protected Object(string? name)
        {
            _instanceId = _nextInstanceId++;
            _name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
            _aliveObjects.Add(this);
        }

        public int GetInstanceID() => _instanceId;

        // --- Destroy 계열 ---

        public static void Destroy(Object? obj) => Destroy(obj, 0f);

        public static void Destroy(Object? obj, float t)
        {
            if (obj == null || obj.IsDestroyed)
                return;

            if (t <= 0f)
            {
                obj.PerformDestroy(immediate: false);
            }
            else
            {
                // 실제 엔진의 프레임/타이머 스케줄러가 있다면 지연 파괴 큐에 등록하세요.
                obj.ScheduleDestroy(t);
            }
        }

        public static void DestroyImmediate(Object? obj) => DestroyImmediate(obj, false);

        public static void DestroyImmediate(Object? obj, bool allowDestroyingAssets)
        {
            if (obj == null || obj.IsDestroyed)
                return;

            obj.PerformDestroy(immediate: true);
        }

        protected virtual void ScheduleDestroy(float delay)
        {
            // 기본 구현: 지연 스케줄러가 연결되지 않은 환경에서는 즉시 처리하거나 외부 매니저에서 처리
            PerformDestroy(immediate: false);
        }

        private void PerformDestroy(bool immediate)
        {
            if (IsDestroyed)
                return;

            if (this is GameObject go)
            {
                go.MarkDestroyed();
            }
            else if (this is Component comp)
            {
                comp.MarkDestroyed();
            }
            else
            {
                MarkDestroyed();
            }

            IsDestroyed = true;
            _aliveObjects.Remove(this);
            _dontDestroyOnLoadObjects.Remove(this);
            OnDestroyed(immediate);
        }

        protected virtual void OnDestroyed(bool immediate) { }

        internal virtual void MarkDestroyed() { }

        // --- DontDestroyOnLoad ---

        public static void DontDestroyOnLoad(Object? target)
        {
            if (target == null || target.IsDestroyed)
                return;

            if (target is Component comp && comp.GameObject != null)
            {
                _dontDestroyOnLoadObjects.Add(comp.GameObject);
            }
            else
            {
                _dontDestroyOnLoadObjects.Add(target);
            }
        }

        public static bool IsMarkedDontDestroyOnLoad(Object? target)
        {
            return target != null && _dontDestroyOnLoadObjects.Contains(target);
        }

        // --- Find 계열 ---

        public static T? FindObjectOfType<T>() where T : Object
        {
            return FindObjectsOfType<T>().FirstOrDefault();
        }

        public static T[] FindObjectsOfType<T>() where T : Object
        {
            return _aliveObjects
                .OfType<T>()
                .Where(o => !o.IsDestroyed)
                .ToArray();
        }

        // --- Instantiate 계열 ---

        public static T Instantiate<T>(T original) where T : Object
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            if (original.IsDestroyed)
                throw new InvalidOperationException("Cannot instantiate a destroyed object.");

            return (T)original.CloneObject(parent: null);
        }

        public static T Instantiate<T>(T original, Transform? parent) where T : Object
        {
            if (original == null)
                throw new ArgumentNullException(nameof(original));
            if (original.IsDestroyed)
                throw new InvalidOperationException("Cannot instantiate a destroyed object.");

            return (T)original.CloneObject(parent);
        }

        protected virtual Object CloneObject(Transform? parent)
        {
            var clone = (Object)MemberwiseClone();
            clone._name = $"{_name} (Clone)";
            clone.IsDestroyed = false;
            clone.HideFlags = HideFlags.None;
            _aliveObjects.Add(clone);
            return clone;
        }

        // --- 문자열 및 동등성 비교 (유니티의 "파괴된 객체 == null" 감각 반영) ---

        public override string ToString()
        {
            return IsDestroyed ? $"{_name} (Destroyed)" : _name;
        }

        public bool Equals(Object? other)
        {
            bool thisNull = IsNull(this);
            bool otherNull = IsNull(other);

            if (thisNull && otherNull) return true;
            if (thisNull || otherNull) return false;

            return ReferenceEquals(this, other);
        }

        public override bool Equals(object? obj) => Equals(obj as Object);

        public override int GetHashCode() => _instanceId.GetHashCode();

        public static bool operator ==(Object? left, Object? right)
        {
            bool leftNull = IsNull(left);
            bool rightNull = IsNull(right);

            if (leftNull && rightNull) return true;
            if (leftNull || rightNull) return false;

            return ReferenceEquals(left, right);
        }

        public static bool operator !=(Object? left, Object? right) => !(left == right);

        public static implicit operator bool(Object? exists) => !IsNull(exists);

        private static bool IsNull(Object? obj) => ReferenceEquals(obj, null) || obj.IsDestroyed;
    }
}