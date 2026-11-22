using ConsoleApp_Game_Engine.Core;

namespace ConsoleApp_Game_Engine
{
    public abstract class Component : Object
    {
        public GameObject GameObject { get; internal set; } = null!;
        internal GameLoop? Loop { get; set; }

        protected Component(string name = "Component") : base(name) { }
    }
}
