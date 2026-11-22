namespace ConsoleGameEngine
{
    public sealed class Scene
    {
        private readonly List<GameObject> _gameObjectList = new List<GameObject>();

        public string Name { get; }
        public IReadOnlyList<GameObject> GameObjectList => _gameObjectList;

        internal Scene(string name)
        {
            Name = name;
        }

        internal GameObject AddGameObject(string name = "GameObject")
        {
            var go = new GameObject(name);
            _gameObjectList.Add(go);
            return go;
        }

        internal void MoveGameObjectTo(GameObject go, Scene targetScene)
        {
            if (ReferenceEquals(this, targetScene))
                return;

            if (_gameObjectList.Remove(go))
            {
                targetScene._gameObjectList.Add(go);
            }
        }

        internal void UnloadAndDestroy()
        {
            //TODO:: 추가할 것.
            /*for (int i = _gameObjectList.Count - 1; i >= 0; i--)
            {
                _gameObjectList[i].DestroyAllComponents();
            }*/

            _gameObjectList.Clear();
        }
    }
}
