namespace ConsoleGameEngine
{
    public abstract class Object
    {
        private static int _nextInstanceId = 1;

        public int InstanceId { get; } = _nextInstanceId++;
        public string Name { get; set; }

        protected Object(string name = "Object")
        {
            Name = name;
        }

        public override string ToString() => $"{Name} ({GetType().Name})";
    }
}