using System.Collections;
using TMPro;
using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class CubeSnakeSegment : MonoBehaviour
    {
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Renderer targetRenderer;

        public CubeSnake Snake { get; private set; }
        public int Number { get; private set; }
        public bool IsHead { get; private set; }

        private Vector3 targetScale;

        public void Configure(CubeSnake snake, int number, bool isHead, CubeMergeArenaBalance balance)
        {
            Snake = snake;
            IsHead = isHead;
            SetNumber(number, balance);

            var collider = GetComponent<BoxCollider>();
            collider.isTrigger = isHead;

            if (isHead && GetComponent<Rigidbody>() == null)
            {
                var body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }
        }

        public void SetNumber(int number, CubeMergeArenaBalance balance)
        {
            Number = Mathf.Max(2, number);
            EnsureReferences();

            var size = balance.GetCubeSize(Number);
            targetScale = Vector3.one * size;
            transform.localScale = targetScale;

            if (targetRenderer != null)
            {
                targetRenderer.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = balance.GetNumberColor(Number)
                };
            }

            if (label != null)
            {
                label.text = Number.ToString();
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.Center;
                label.fontStyle = FontStyles.Bold;
            }
        }

        public void PlaySpawnAnimation()
        {
            StopAllCoroutines();
            StartCoroutine(SpawnRoutine());
        }

        private void Awake()
        {
            EnsureReferences();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsHead || Snake == null) return;

            if (other.TryGetComponent(out CubeMergeArenaPickup pickup))
            {
                Snake.ConsumePickup(pickup);
                return;
            }

            if (other.TryGetComponent(out CubeSnakeSegment snakeSegment))
            {
                Snake.ResolveSnakeContact(snakeSegment);
            }
        }

        private void EnsureReferences()
        {
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
            label = label != null ? label : GetComponentInChildren<TextMeshPro>();
        }

        private IEnumerator SpawnRoutine()
        {
            var duration = 0.4f;
            transform.localScale = Vector3.zero;

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                transform.localScale = Vector3.LerpUnclamped(Vector3.zero, targetScale, BackOut(t));
                yield return null;
            }

            transform.localScale = targetScale;
        }

        private static float BackOut(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
