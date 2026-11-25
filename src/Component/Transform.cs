using System;
using System.Collections;
using System.Collections.Generic;

namespace ConsoleApp_GameEngine
{
    public sealed class Transform : Component, IEnumerable<Transform>
    {
        private readonly List<Transform> _children = new();
        private Transform? _parent;

        private float _localPositionX;
        private float _localPositionY;
        private float _localRotation;   // 도(degree) 단위 기준
        private float _localScaleX = 1f;
        private float _localScaleY = 1f;

        public Transform? Parent
        {
            get => _parent;
            set => SetParent(value, worldPositionStays: true);
        }

        public int ChildCount => _children.Count;

        public IReadOnlyList<Transform> Children => _children;

        // --- 로컬 좌표 / 회전 / 스케일 ---

        public float LocalPositionX
        {
            get => _localPositionX;
            set => _localPositionX = value;
        }

        public float LocalPositionY
        {
            get => _localPositionY;
            set => _localPositionY = value;
        }

        public float LocalRotation
        {
            get => _localRotation;
            set => _localRotation = value;
        }

        public float LocalScaleX
        {
            get => _localScaleX;
            set => _localScaleX = value;
        }

        public float LocalScaleY
        {
            get => _localScaleY;
            set => _localScaleY = value;
        }

        // --- 월드 좌표 / 회전 / LossyScale (부모 계층 누적) ---

        public float PositionX
        {
            get
            {
                if (_parent == null)
                    return _localPositionX;

                (float wx, _) = _parent.TransformPoint(_localPositionX, _localPositionY);
                return wx;
            }
            set => SetPosition(value, PositionY);
        }

        public float PositionY
        {
            get
            {
                if (_parent == null)
                    return _localPositionY;

                (_, float wy) = _parent.TransformPoint(_localPositionX, _localPositionY);
                return wy;
            }
            set => SetPosition(PositionX, value);
        }

        // 기존 X, Y 프로퍼티를 월드 좌표 별칭으로 유지해 하위 호환성 확보
        public float X
        {
            get => PositionX;
            set => PositionX = value;
        }

        public float Y
        {
            get => PositionY;
            set => PositionY = value;
        }

        public float Rotation
        {
            get => (_parent?.Rotation ?? 0f) + _localRotation;
            set
            {
                float parentRot = _parent?.Rotation ?? 0f;
                _localRotation = value - parentRot;
            }
        }

        public float LossyScaleX => (_parent?.LossyScaleX ?? 1f) * _localScaleX;

        public float LossyScaleY => (_parent?.LossyScaleY ?? 1f) * _localScaleY;

        public Transform Root
        {
            get
            {
                Transform current = this;
                while (current._parent != null)
                    current = current._parent;

                return current;
            }
        }

        // --- 이동 / 회전 / 좌표 설정 ---

        public void SetPosition(float x, float y)
        {
            if (_parent == null)
            {
                _localPositionX = x;
                _localPositionY = y;
                return;
            }

            (float lx, float ly) = _parent.InverseTransformPoint(x, y);
            _localPositionX = lx;
            _localPositionY = ly;
        }

        public void SetLocalPosition(float x, float y)
        {
            _localPositionX = x;
            _localPositionY = y;
        }

        public void Translate(float dx, float dy)
        {
            _localPositionX += dx;
            _localPositionY += dy;
        }

        public void Rotate(float angleDegrees)
        {
            _localRotation += angleDegrees;
        }

        // --- 좌표 변환 헬퍼 (로컬 <-> 월드) ---

        public (float x, float y) TransformPoint(float localX, float localY)
        {
            float sx = LossyScaleX;
            float sy = LossyScaleY;
            float rad = Rotation * (MathF.PI / 180f);
            float cos = MathF.Cos(rad);
            float sin = MathF.Sin(rad);

            float scaledX = localX * sx;
            float scaledY = localY * sy;

            float rotX = scaledX * cos - scaledY * sin;
            float rotY = scaledX * sin + scaledY * cos;

            // 부모 원점 기준 오프셋
            float originX = _parent != null ? _parent.PositionX : 0f;
            float originY = _parent != null ? _parent.PositionY : 0f;

            // 부모가 있는 경우 TransformPoint 호출 시 누적 스케일/회전을 다시 곱하지 않도록 로컬 기준 계산
            if (_parent == null)
                return (rotX, rotY);

            float parentRad = _parent.Rotation * (MathF.PI / 180f);
            float pCos = MathF.Cos(parentRad);
            float pSin = MathF.Sin(parentRad);

            float pLocalX = localX * _parent.LossyScaleX;
            float pLocalY = localY * _parent.LossyScaleY;

            float worldX = originX + (pLocalX * pCos - pLocalY * pSin);
            float worldY = originY + (pLocalX * pSin + pLocalY * pCos);
            return (worldX, worldY);
        }

        public (float localX, float localY) InverseTransformPoint(float worldX, float worldY)
        {
            float originX = _parent != null ? _parent.PositionX : 0f;
            float originY = _parent != null ? _parent.PositionY : 0f;

            float dx = worldX - originX;
            float dy = worldY - originY;

            float rad = -(_parent?.Rotation ?? 0f) * (MathF.PI / 180f);
            float cos = MathF.Cos(rad);
            float sin = MathF.Sin(rad);

            float unrotX = dx * cos - dy * sin;
            float unrotY = dx * sin + dy * cos;

            float scaleX = _parent?.LossyScaleX ?? 1f;
            float scaleY = _parent?.LossyScaleY ?? 1f;

            float lx = MathF.Abs(scaleX) > 1e-6f ? unrotX / scaleX : 0f;
            float ly = MathF.Abs(scaleY) > 1e-6f ? unrotY / scaleY : 0f;
            return (lx, ly);
        }

        // --- 계층 구조 조작 및 탐색 ---

        public void SetParent(Transform? newParent, bool worldPositionStays = true)
        {
            if (ReferenceEquals(_parent, newParent))
                return;

            if (newParent != null && (ReferenceEquals(newParent, this) || newParent.IsChildOf(this)))
                throw new InvalidOperationException("자기 자신이나 자신의 하위 자식을 부모로 설정할 수 없습니다.");

            float oldWorldX = PositionX;
            float oldWorldY = PositionY;
            float oldWorldRot = Rotation;

            _parent?._children.Remove(this);
            _parent = newParent;
            _parent?._children.Add(this);

            if (worldPositionStays)
            {
                SetPosition(oldWorldX, oldWorldY);
                Rotation = oldWorldRot;
            }
        }

        public void DetachChildren()
        {
            for (int i = _children.Count - 1; i >= 0; i--)
            {
                Transform child = _children[i];
                _children.RemoveAt(i);
                child._parent = null;
            }
        }

        public Transform GetChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            return _children[index];
        }

        public Transform? Find(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            for (int i = 0; i < _children.Count; i++)
            {
                Transform child = _children[i];
                if (child.GameObject != null && child.GameObject.Name == name)
                    return child;

                Transform? foundInSubtree = child.Find(name);
                if (foundInSubtree != null)
                    return foundInSubtree;
            }

            return null;
        }

        public bool IsChildOf(Transform parent)
        {
            if (parent == null)
                return false;

            Transform? current = _parent;
            while (current != null)
            {
                if (ReferenceEquals(current, parent))
                    return true;

                current = current._parent;
            }

            return false;
        }

        public int GetSiblingIndex()
        {
            return _parent != null ? _parent._children.IndexOf(this) : -1;
        }

        public void SetSiblingIndex(int index)
        {
            if (_parent == null)
                return;

            int currentIndex = _parent._children.IndexOf(this);
            if (currentIndex < 0)
                return;

            _parent._children.RemoveAt(currentIndex);
            int clamped = Math.Clamp(index, 0, _parent._children.Count);
            _parent._children.Insert(clamped, this);
        }

        public void SetAsFirstSibling() => SetSiblingIndex(0);

        public void SetAsLastSibling() => SetSiblingIndex(_parent?._children.Count ?? 0);

        public IEnumerator<Transform> GetEnumerator() => _children.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        internal override void MarkDestroyed()
        {
            if (IsDestroyed)
                return;

            _parent?._children.Remove(this);
            _parent = null;
            _children.Clear();
            base.MarkDestroyed();
        }
    }
}
