using ConsoleGameEngine.Core;

namespace ConsoleGameEngine
{
    public abstract class Component : Object
    {
        public GameObject GameObject { get; internal set; } = null!;
        internal GameLoop? Loop { get; set; }

        protected Component(string name = "Component") : base(name) { }
    }
}
