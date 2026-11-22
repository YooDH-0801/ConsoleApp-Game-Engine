using ConsoleGameEngine.Core;

namespace ConsoleGameEngine
{
    public sealed class GameObject : Object
    {
        private readonly List<Component> _components = new List<Component>();

        public bool ActiveSelf { get; private set; } = true;
        public int X { get; set; }
        public int Y { get; set; }

        public IReadOnlyList<Component> Components => _components;

        public GameObject(string name = "GameObject", int x = 0, int y = 0) : base(name)
        {
            X = x;
            Y = y;
        }

        public void SetActive(bool active)
        {
            if (ActiveSelf == active)
                return;

            bool prevActiveInHierarchy = ActiveSelf;
            ActiveSelf = active;

            foreach (Component comp in _components)
            {
                if (comp is Behaviour behaviour && behaviour.Enabled)
                {
                    if (!prevActiveInHierarchy && ActiveSelf)
                        behaviour.OnEnable();
                    else if (prevActiveInHierarchy && !ActiveSelf)
                        behaviour.OnDisable();
                }
            }
        }

        public T AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            component.GameObject = this;
            if (string.IsNullOrEmpty(component.Name) || component.Name == "Component")
            {
                component.Name = component.GetType().Name;
            }

            _components.Add(component);
            return component;
        }

        public T? GetComponent<T>() where T : Component
        {
            foreach (Component comp in _components)
            {
                if (comp is T matched)
                    return matched;
            }
            return null;
        }
    }
}
