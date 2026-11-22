namespace ConsoleApp_Game_Engine
{
    public abstract class Scene
    {
        public string Name { get; }

        protected Scene(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
        }

        /// <summary>
        /// 씬이 활성화되어 화면에 진입할 때 한 번 호출됩니다.
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 다른 씬으로 전환되기 직전에 한 번 호출됩니다.
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 매 프레임 게임 로직과 입력 처리를 수행합니다.
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// 매 프레임 백버퍼에 그리고 Screen.Present()로 출력합니다.
        /// </summary>
        public virtual void Render() { }
    }
}
