namespace ConsoleGameEngine
{
    public sealed class InputManager
    {
        private readonly HashSet<ConsoleKey> _previousHeld = new();
        private readonly HashSet<ConsoleKey> _currentHeld = new();
        private readonly HashSet<ConsoleKey> _downThisFrame = new();
        private readonly HashSet<ConsoleKey> _upThisFrame = new();
        private readonly Dictionary<ConsoleKey, float> _holdTimers = new();
        private readonly List<ConsoleKey> _expiredBuffer = new();
        private readonly List<ConsoleKeyInfo> _frameKeyInfos = new();

        /// <summary>
        /// 키 입력 이벤트 후 "눌림 유지"로 간주할 유예 시간(초).
        /// </summary>
        public float HoldGraceDuration { get; set; } = 0.12f;

        /// <summary>
        /// 이번 프레임에 읽어들인 원본 콘솔 키 정보 목록입니다.
        /// </summary>
        public IReadOnlyList<ConsoleKeyInfo> FrameKeyInfos => _frameKeyInfos;

        public void Poll()
        {
            _previousHeld.Clear();
            foreach (ConsoleKey key in _currentHeld)
            {
                _previousHeld.Add(key);
            }

            _currentHeld.Clear();
            _downThisFrame.Clear();
            _upThisFrame.Clear();
            _frameKeyInfos.Clear();

            UpdateHoldTimers(Time.UnscaledDeltaTime);
            ReadConsoleInputBuffer();
            ComputeKeyUpStates();
        }

        public bool GetKeyDown(ConsoleKey key) => _downThisFrame.Contains(key);

        public bool GetKey(ConsoleKey key) => _currentHeld.Contains(key);

        public bool GetKeyUp(ConsoleKey key) => _upThisFrame.Contains(key);

        public void Clear()
        {
            _previousHeld.Clear();
            _currentHeld.Clear();
            _downThisFrame.Clear();
            _upThisFrame.Clear();
            _holdTimers.Clear();
            _expiredBuffer.Clear();
            _frameKeyInfos.Clear();
        }

        private void UpdateHoldTimers(float deltaSeconds)
        {
            if (_holdTimers.Count == 0)
                return;

            _expiredBuffer.Clear();
            float step = Math.Max(0f, deltaSeconds);

            foreach (KeyValuePair<ConsoleKey, float> pair in _holdTimers)
            {
                float remaining = pair.Value - step;
                if (remaining <= 0f)
                {
                    _expiredBuffer.Add(pair.Key);
                }
                else
                {
                    _holdTimers[pair.Key] = remaining;
                    _currentHeld.Add(pair.Key);
                }
            }

            for (int i = 0; i < _expiredBuffer.Count; i++)
            {
                _holdTimers.Remove(_expiredBuffer[i]);
            }
        }

        private void ReadConsoleInputBuffer()
        {
            try
            {
                while (Console.KeyAvailable)
                {
                    ConsoleKeyInfo info = Console.ReadKey(intercept: true);
                    ConsoleKey key = info.Key;

                    _frameKeyInfos.Add(info);
                    _holdTimers[key] = HoldGraceDuration;
                    _currentHeld.Add(key);

                    if (!_previousHeld.Contains(key))
                    {
                        _downThisFrame.Add(key);
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // 입력 스트림 리다이렉트 또는 콘솔 핸들 부재 환경 방어
            }
        }

        private void ComputeKeyUpStates()
        {
            foreach (ConsoleKey key in _previousHeld)
            {
                if (!_currentHeld.Contains(key))
                {
                    _upThisFrame.Add(key);
                }
            }
        }
    }
}