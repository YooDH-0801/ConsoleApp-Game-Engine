using System.Diagnostics;

namespace ConsoleApp_Game_Engine
{
    public sealed class ApplicationManager
    {
        private int _targetFrameRate = 60;
        private double _targetFrameTime = 1.0 / 60;

        public bool IsPlaying { get; set; }

        public int TargetFrameRate
        {
            get => _targetFrameRate;
            set
            {
                _targetFrameRate = value;
                _targetFrameTime = value > 0 ? 1.0 / value : 0.0;
            }
        }

        public void Quit() => IsPlaying = false;

        public void WaitForNextFrame(Stopwatch stopwatch, double frameStartSeconds)
        {
            if (_targetFrameRate <= 0 || _targetFrameTime <= 0.0) return;

            double deadline = frameStartSeconds + _targetFrameTime;

            while (true)
            {
                double remainingMs = (deadline - stopwatch.Elapsed.TotalSeconds) * 1000.0;
                if (remainingMs <= 2.0) break;

                int sleepMs = (int)(remainingMs - 1.0);
                if (sleepMs > 0)
                {
                    Thread.Sleep(sleepMs);
                }
                else
                {
                    break;
                }
            }

            while (stopwatch.Elapsed.TotalSeconds < deadline)
            {
                Thread.SpinWait(10);
            }
        }
    }
}
