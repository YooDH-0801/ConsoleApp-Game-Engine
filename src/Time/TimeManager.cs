using System.Diagnostics;

namespace ConsoleGameEngine
{
    internal sealed class TimeManager
    {
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _previousTime;

        public double FixedDeltaTime { get; }
        public double MaxDeltaTime { get; }
        public double DeltaTime { get; private set; }
        public double UnscaledDeltaTime { get; private set; }
        public double TimeScale { get; set; } = 1.0;
        public double TimeSinceLevelLoad { get; private set; }
        public double Accumulator { get; private set; }
        public int FrameCount { get; private set; }

        public TimeManager(double targetFps = 60.0, double maxDeltaTime = 0.25)
        {
            FixedDeltaTime = 1.0 / targetFps;
            MaxDeltaTime = maxDeltaTime;
        }

        public void Reset()
        {
            _stopwatch.Restart();
            _previousTime = 0.0;
            DeltaTime = 0.0;
            UnscaledDeltaTime = 0.0;
            TimeSinceLevelLoad = 0.0;
            Accumulator = 0.0;
            FrameCount = 0;
        }

        public void Tick()
        {
            double currentTime = _stopwatch.Elapsed.TotalSeconds;
            double rawDelta = currentTime - _previousTime;
            _previousTime = currentTime;

            if (rawDelta > MaxDeltaTime)
            {
                rawDelta = MaxDeltaTime;
            }

            UnscaledDeltaTime = rawDelta;
            DeltaTime = rawDelta * TimeScale;
            TimeSinceLevelLoad += DeltaTime;
            Accumulator += DeltaTime;
            FrameCount++;
        }

        public bool ConsumeFixedStep()
        {
            if (Accumulator >= FixedDeltaTime)
            {
                Accumulator -= FixedDeltaTime;
                return true;
            }
            return false;
        }

        public void SleepToMaintainFrameRate()
        {
            double elapsedInFrame = _stopwatch.Elapsed.TotalSeconds - _previousTime;
            double remaining = FixedDeltaTime - elapsedInFrame;
            if (remaining > 0.002)
            {
                Thread.Sleep((int)((remaining - 0.001) * 1000));
            }
        }
    }
}
