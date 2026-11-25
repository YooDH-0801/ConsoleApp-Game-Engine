using System.Collections;

namespace ConsoleApp_GameEngine
{
    public abstract class MonoBehaviour : Behaviour
    {
        private readonly List<ScheduledInvocation> _invocations = new();
        private readonly List<CoroutineHandle> _coroutines = new();

        internal bool HasAwoken { get; set; }
        internal bool HasStarted { get; set; }

        protected MonoBehaviour() : base(null) { }

        protected MonoBehaviour(string? name) : base(name) { }

        // --- 유니티 주요 생명주기 훅 ---

        protected virtual void Awake() { }
        protected virtual void Start() { }

        internal override void InternalAwake()
        {
            if (HasAwoken || IsDestroyed)
                return;

            HasAwoken = true;
            Awake();

            // Awake() 내부에서 Enabled나 GameObject 상태를 바꿨을 수도 있으므로 동기화 수행
            SyncEnableState();
        }

        internal override void InternalStart()
        {
            if (!HasAwoken || HasStarted || IsDestroyed || !IsActiveAndEnabled)
                return;

            HasStarted = true;
            Start();
        }

        internal override void InternalUpdate()
        {
            if (!IsActiveAndEnabled || !HasAwoken)
                return;

            // Awake가 끝났지만 아직 Start가 안 된 케이스(예: 런타임 Enabled 전환 등) 방어
            if (!HasStarted)
            {
                HasStarted = true;
                Start();
                if (IsDestroyed || !IsActiveAndEnabled)
                    return;
            }

            float deltaTime = GetDeltaTime();
            TickInvocations(deltaTime);
            TickCoroutines(deltaTime);

            if (IsActiveAndEnabled)
                Update();
        }

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            StopAllCoroutines();
            CancelInvoke();
            base.MarkDestroyed();
        }

        // --- Invoke 계열 ---

        public void Invoke(string methodName, float time)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("메서드 이름이 비어 있을 수 없습니다.", nameof(methodName));
            if (IsDestroyed)
                return;

            _invocations.Add(new ScheduledInvocation(methodName, MathF.Max(0f, time), repeatRate: 0f));
        }

        public void InvokeRepeating(string methodName, float time, float repeatRate)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("메서드 이름이 비어 있을 수 없습니다.", nameof(methodName));
            if (repeatRate <= 0.0001f)
                throw new ArgumentOutOfRangeException(nameof(repeatRate), "repeatRate는 0보다 커야 합니다.");
            if (IsDestroyed)
                return;

            _invocations.Add(new ScheduledInvocation(methodName, MathF.Max(0f, time), repeatRate));
        }

        public void CancelInvoke()
        {
            _invocations.Clear();
        }

        public void CancelInvoke(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return;

            for (int i = _invocations.Count - 1; i >= 0; i--)
            {
                if (_invocations[i].MethodName == methodName)
                    _invocations.RemoveAt(i);
            }
        }

        public bool IsInvoking()
        {
            return _invocations.Count > 0;
        }

        public bool IsInvoking(string methodName)
        {
            if (string.IsNullOrEmpty(methodName))
                return false;

            for (int i = 0; i < _invocations.Count; i++)
            {
                if (_invocations[i].MethodName == methodName)
                    return true;
            }

            return false;
        }

        private void TickInvocations(float deltaTime)
        {
            if (_invocations.Count == 0 || deltaTime <= 0f)
                return;

            for (int i = _invocations.Count - 1; i >= 0; i--)
            {
                ScheduledInvocation item = _invocations[i];
                item.RemainingTime -= deltaTime;

                if (item.RemainingTime <= 0f)
                {
                    SendMessage(item.MethodName, value: null, requireReceiver: false);

                    if (IsDestroyed)
                        return;

                    if (item.RepeatRate > 0f)
                    {
                        item.RemainingTime += item.RepeatRate;
                    }
                    else
                    {
                        _invocations.RemoveAt(i);
                    }
                }
            }
        }

        // --- 코루틴 계열 (IEnumerator 기반 경량 실행기) ---

        public CoroutineHandle StartCoroutine(IEnumerator routine)
        {
            if (routine == null)
                throw new ArgumentNullException(nameof(routine));
            if (IsDestroyed)
                throw new InvalidOperationException($"파괴된 MonoBehaviour('{Name}')에서 코루틴을 시작할 수 없습니다.");

            var handle = new CoroutineHandle(routine);
            _coroutines.Add(handle);
            return handle;
        }

        public void StopCoroutine(CoroutineHandle? coroutine)
        {
            if (coroutine == null)
                return;

            coroutine.IsStopped = true;
            _coroutines.Remove(coroutine);
        }

        public void StopCoroutine(IEnumerator? routine)
        {
            if (routine == null)
                return;

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(_coroutines[i].Routine, routine))
                {
                    _coroutines[i].IsStopped = true;
                    _coroutines.RemoveAt(i);
                }
            }
        }

        public void StopAllCoroutines()
        {
            for (int i = 0; i < _coroutines.Count; i++)
                _coroutines[i].IsStopped = true;

            _coroutines.Clear();
        }

        private void TickCoroutines(float deltaTime)
        {
            if (_coroutines.Count == 0)
                return;

            for (int i = _coroutines.Count - 1; i >= 0; i--)
            {
                CoroutineHandle handle = _coroutines[i];
                if (handle.IsStopped || IsDestroyed)
                {
                    _coroutines.RemoveAt(i);
                    continue;
                }

                if (handle.WaitSeconds > 0f)
                {
                    handle.WaitSeconds -= MathF.Max(0f, deltaTime);
                    continue;
                }

                bool hasNext;
                try
                {
                    hasNext = handle.Routine.MoveNext();
                }
                catch
                {
                    _coroutines.RemoveAt(i);
                    throw;
                }

                if (!hasNext)
                {
                    _coroutines.RemoveAt(i);
                    continue;
                }

                // yield return float / double 값을 대기 초(seconds)로 지원하는 간단한 규칙
                if (handle.Routine.Current is float waitFloat && waitFloat > 0f)
                {
                    handle.WaitSeconds = waitFloat;
                }
                else if (handle.Routine.Current is double waitDouble && waitDouble > 0.0)
                {
                    handle.WaitSeconds = (float)waitDouble;
                }
            }
        }

        // 실제 엔진의 Time.deltaTime 클래스가 있다면 해당 값으로 연결하세요.
        protected virtual float GetDeltaTime() => 0.016f;

        private sealed class ScheduledInvocation
        {
            public string MethodName { get; }
            public float RemainingTime;
            public float RepeatRate { get; }

            public ScheduledInvocation(string methodName, float delay, float repeatRate)
            {
                MethodName = methodName;
                RemainingTime = delay;
                RepeatRate = repeatRate;
            }
        }

        public sealed class CoroutineHandle
        {
            internal IEnumerator Routine { get; }
            internal float WaitSeconds { get; set; }
            public bool IsStopped { get; internal set; }

            internal CoroutineHandle(IEnumerator routine)
            {
                Routine = routine;
            }
        }
    }
}
