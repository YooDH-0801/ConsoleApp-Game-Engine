namespace ConsoleApp_GameEngine
{
    public sealed class SceneManager
    {
        private static readonly Dictionary<string, Scene> _scenes = new(StringComparer.Ordinal);
        private static Scene? _pendingScene;

        public static Scene? ActiveScene { get; private set; }
        public static string? ActiveSceneName => ActiveScene?.Name;

        public static void Register(Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException(nameof(scene));

            _scenes[scene.Name] = scene;
        }

        public static bool TryGetScene(string sceneName, out Scene? scene) =>
            _scenes.TryGetValue(sceneName, out scene);

        public static void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
                throw new ArgumentException("씬 이름이 비어 있을 수 없습니다.", nameof(sceneName));

            if (!_scenes.TryGetValue(sceneName, out Scene? targetScene))
                throw new InvalidOperationException($"등록되지 않은 씬입니다: '{sceneName}'");

            _pendingScene = targetScene;
        }

        internal static void ApplyPendingLoad()
        {
            if (_pendingScene == null)
                return;

            Scene nextScene = _pendingScene;
            _pendingScene = null;

            if (ReferenceEquals(ActiveScene, nextScene))
                return;

            ActiveScene?.OnExit();

            ActiveScene = nextScene;

            if (Input.IsBound)
                Input.Clear();

            if (Screen.IsBound)
                Screen.MarkDirtyAll();

            ActiveScene.OnEnter();
        }

        internal static void Update()
        {
            ActiveScene?.Update();
        }

        internal static void Render()
        {
            ActiveScene?.Render();
        }

        internal static void Clear()
        {
            ActiveScene?.OnExit();
            ActiveScene = null;
            _pendingScene = null;
            _scenes.Clear();
        }
    }
}
