namespace ConsoleGameEngine
{
    public sealed class SceneManager
    {
        private readonly List<Scene> _sceneList = new List<Scene>();
        private string? _pendingSceneName;

        public Scene? ActiveScene { get; private set; }

        public SceneManager()
        {

        }

        public void LoadScene(string sceneName)
        {
            _pendingSceneName = sceneName;
        }

        public void LoadScene(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= _sceneList.Count)
                throw new ArgumentOutOfRangeException(nameof(buildIndex));

            _pendingSceneName = _sceneList[buildIndex].Name;
        }
    }
}
