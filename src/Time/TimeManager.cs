using System.Diagnostics;

namespace ConsoleApp_GameEngine
{
    public sealed class TimeManager
    {
        private readonly Stopwatch _stopwatch = new();
        private double _lastFrameSeconds;
        private float _timeScale = 1.0f;

        public double MaxDeltaTime { get; set; } = 0.1;

        public float DeltaTime { get; private set; }
        public float UnscaledDeltaTime { get; private set; }
        public float ElapsedTime { get; private set; }
        public float UnscaledElapsedTime { get; private set; }
        public long FrameCount { get; private set; }

        public float TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Max(0f, value);
        }

        public void Start()
        {
            _stopwatch.Restart();
            _lastFrameSeconds = _stopwatch.Elapsed.TotalSeconds;
            DeltaTime = 0f;
            UnscaledDeltaTime = 0f;
            ElapsedTime = 0f;
            UnscaledElapsedTime = 0f;
            FrameCount = 0;
        }

        public void Tick()
        {
            double now = _stopwatch.Elapsed.TotalSeconds;
            double rawDelta = Math.Max(0.0, now - _lastFrameSeconds);
            _lastFrameSeconds = now;

            double clampedDelta = Math.Min(rawDelta, MaxDeltaTime);

            UnscaledDeltaTime = (float)clampedDelta;
            DeltaTime = (float)(clampedDelta * _timeScale);

            UnscaledElapsedTime += UnscaledDeltaTime;
            ElapsedTime += DeltaTime;
            FrameCount++;
        }
    }
}
