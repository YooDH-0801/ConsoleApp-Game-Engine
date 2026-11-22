namespace ConsoleGameEngine.Core
{
    public sealed class GameLoop
    {
        private readonly TimeManager _timeManager = new TimeManager(targetFps: 60.0);
        private readonly InputManager _inputManager = new InputManager();
        private readonly ScreenManager _screenManager = new ScreenManager();

        public bool IsApplicationPlaying { get; set; }

        public void Run()
        {
            Time.Bind(_timeManager);
            Input.Bind(_inputManager);
            Screen.Bind(_screenManager);

            _screenManager.Initialize(requestedWidth: 80, requestedHeight: 25);
            _timeManager.Reset();
            IsApplicationPlaying = true;


            try
            {
                while (IsApplicationPlaying)
                {
                    _timeManager.Tick();
                    _screenManager.RefreshResolution();
                    _inputManager.Poll();

                    // TODO::생명주기 Awake() 호출 위치.

                    // TODO::생명주기 Start() 호출 위치.

                    while (_timeManager.ConsumeFixedStep())
                    {
                        // TODO::생명주기 FixedUpdate() 호출 위치.
                    }

                    _screenManager.ClearBackBuffer(' ');

                    // TODO::생명주기 Update() 호출 위치.

                    // TODO::생명주기 LateUpdate() 호출 위치.

                    _screenManager.Present();
                    _timeManager.SleepToMaintainFrameRate();
                }
            }
            finally
            {
                OnApplicationQuit();
            }
        }

        private void OnApplicationQuit()
        {
            Time.Unbind(_timeManager);
            Input.Unbind(_inputManager);
            Screen.Unbind(_screenManager);

            _screenManager.Restore();
            Console.WriteLine("애플리케이션이 종료되었습니다.");
        }
    }
}


//TODO:: 전체적으로 흐름 파악 후, 리펙토링할 것.