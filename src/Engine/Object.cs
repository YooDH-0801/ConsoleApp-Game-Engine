namespace ConsoleApp_Game_Engine
{
    public abstract class Object
    {
        public string Name { get; set; }
        public bool IsDestroyed { get; internal set; }

        protected Object(string name)
        {
            Name = string.IsNullOrWhiteSpace(name) ? GetType().Name : name;
        }

        public static void Destroy(Object? obj)
        {
            if (obj == null || obj.IsDestroyed)
                return;

            if (obj is GameObject go)
            {
                go.MarkDestroyed();
            }
            else if (obj is Component comp)
            {
                comp.MarkDestroyed();
            }
            else
            {
                obj.IsDestroyed = true;
            }
        }

        public override string ToString() => Name;

        internal virtual void MarkDestroyed() { }
    }
}