using System;
using System.Collections.Generic;
using System.Reflection;

namespace ConsoleApp_GameEngine
{
    public abstract class Component : Object
    {
        protected Component() : base(null) { }

        protected Component(string? name) : base(name) { }

        public GameObject GameObject { get; internal set; } = null!;

        public Transform Transform => GameObject.Transform;

        public string Tag
        {
            get => GameObject != null && !GameObject.IsDestroyed ? GameObject.Tag : "Untagged";
            set
            {
                if (GameObject != null && !GameObject.IsDestroyed)
                    GameObject.Tag = value;
            }
        }

        public bool CompareTag(string tag)
        {
            if (GameObject == null || GameObject.IsDestroyed)
                return false;

            return GameObject.Tag == tag;
        }

        // --- 컴포넌트 조회 (GameObject 계층/컴포넌트 검색 위임) ---

        public T? GetComponent<T>() where T : Component
        {
            return GameObject != null && !GameObject.IsDestroyed
                ? GameObject.GetComponent<T>()
                : null;
        }

        public T[] GetComponents<T>() where T : Component
        {
            return GameObject != null && !GameObject.IsDestroyed
                ? GameObject.GetComponents<T>()
                : Array.Empty<T>();
        }

        public void GetComponents<T>(List<T> results) where T : Component
        {
            if (results == null)
                throw new ArgumentNullException(nameof(results));

            results.Clear();
            if (GameObject != null && !GameObject.IsDestroyed)
                GameObject.GetComponents(results);
        }

        public bool TryGetComponent<T>(out T? component) where T : Component
        {
            component = GetComponent<T>();
            return component != null;
        }

        public T? GetComponentInChildren<T>(bool includeInactive = false) where T : Component
        {
            return GameObject != null && !GameObject.IsDestroyed
                ? GameObject.GetComponentInChildren<T>(includeInactive)
                : null;
        }

        public T[] GetComponentsInChildren<T>(bool includeInactive = false) where T : Component
        {
            return GameObject != null && !GameObject.IsDestroyed
                ? GameObject.GetComponentsInChildren<T>(includeInactive)
                : Array.Empty<T>();
        }

        public T? GetComponentInParent<T>() where T : Component
        {
            return GameObject != null && !GameObject.IsDestroyed
                ? GameObject.GetComponentInParent<T>()
                : null;
        }

        public T[] GetComponentsInParent<T>(bool includeInactive = false) where T : Component
        {
            var results = new List<T>();
            if (GameObject == null || GameObject.IsDestroyed)
                return results.ToArray();

            if (includeInactive || GameObject.ActiveInHierarchy)
            {
                foreach (T comp in GameObject.GetComponents<T>())
                    results.Add(comp);
            }

            Transform? current = Transform?.Parent;
            while (current != null)
            {
                GameObject? parentGo = current.GameObject;
                if (parentGo != null && !parentGo.IsDestroyed && (includeInactive || parentGo.ActiveInHierarchy))
                {
                    foreach (T comp in parentGo.GetComponents<T>())
                        results.Add(comp);
                }

                current = current.Parent;
            }

            return results.ToArray();
        }

        // --- 메시지 전달 (리플렉션 기반 기본 구현) ---

        public void SendMessage(string methodName, object? value = null, bool requireReceiver = true)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("메서드 이름이 비어 있을 수 없습니다.", nameof(methodName));
            if (IsDestroyed || GameObject == null || GameObject.IsDestroyed)
                return;

            int invokedCount = InvokeMethodOnGameObject(GameObject, methodName, value);
            if (invokedCount == 0 && requireReceiver)
            {
                throw new MissingMethodException(
                    $"GameObject '{GameObject.Name}'에 '{methodName}' 메서드를 가진 컴포넌트가 없습니다.");
            }
        }

        public void BroadcastMessage(string methodName, object? value = null, bool requireReceiver = true)
        {
            if (string.IsNullOrWhiteSpace(methodName))
                throw new ArgumentException("메서드 이름이 비어 있을 수 없습니다.", nameof(methodName));
            if (IsDestroyed || GameObject == null || GameObject.IsDestroyed)
                return;

            int totalInvoked = 0;
            var queue = new Queue<GameObject>();
            queue.Enqueue(GameObject);

            while (queue.Count > 0)
            {
                GameObject current = queue.Dequeue();
                if (current.IsDestroyed)
                    continue;

                totalInvoked += InvokeMethodOnGameObject(current, methodName, value);

                // Transform 자식 순회 연동 시 하위 GameObject도 큐에 추가 가능
                // foreach (Transform child in current.Transform.Children) ...
            }

            if (totalInvoked == 0 && requireReceiver)
            {
                throw new MissingMethodException(
                    $"계층 구조에서 '{methodName}' 메서드를 가진 컴포넌트를 찾지 못했습니다.");
            }
        }

        private static int InvokeMethodOnGameObject(GameObject targetGo, string methodName, object? value)
        {
            int count = 0;
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (Component comp in targetGo.GetComponents<Component>())
            {
                if (comp == null || comp.IsDestroyed)
                    continue;

                Type type = comp.GetType();
                MethodInfo? method = value != null
                    ? type.GetMethod(methodName, flags, binder: null, types: new[] { value.GetType() }, modifiers: null)
                        ?? type.GetMethod(methodName, flags, binder: null, types: Type.EmptyTypes, modifiers: null)
                    : type.GetMethod(methodName, flags, binder: null, types: Type.EmptyTypes, modifiers: null);

                if (method == null)
                    continue;

                if (method.GetParameters().Length == 0)
                    method.Invoke(comp, null);
                else
                    method.Invoke(comp, new[] { value });

                count++;
            }

            return count;
        }

        // --- 파괴 및 내부 루프 훅 ---

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            IsDestroyed = true;
            OnDestroy();
        }

        protected virtual void OnDestroy() { }

        internal virtual void InternalAwake() { }
        internal virtual void InternalStart() { }
        internal virtual void InternalUpdate() { }
        internal virtual void InternalRender() { }
    }
}
