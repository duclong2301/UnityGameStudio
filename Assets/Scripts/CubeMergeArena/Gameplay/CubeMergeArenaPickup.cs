using System.Collections;
using TMPro;
using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public enum CubeMergeArenaPickupKind
    {
        Food,
        SpeedUp,
        MultiplierTwo,
        MultiplierHalf
    }

    [RequireComponent(typeof(Collider))]
    public sealed class CubeMergeArenaPickup : MonoBehaviour
    {
        [SerializeField] private TextMeshPro label;
        [SerializeField] private Renderer targetRenderer;

        public CubeMergeArenaPickupKind Kind { get; private set; }
        public int Number { get; private set; }

        private float spinSpeed;
        private Vector3 targetScale;

        public void SetupFood(int number, CubeMergeArenaBalance balance)
        {
            Kind = CubeMergeArenaPickupKind.Food;
            Number = number;
            spinSpeed = Random.Range(-40f, 40f);
            targetScale = Vector3.one * balance.GetCubeSize(number);
            SetColor(balance.GetNumberColor(number));
            SetLabel(number.ToString());
            PlaySpawnBounce();
        }

        public void SetupBooster(CubeMergeArenaPickupKind kind)
        {
            Kind = kind;
            Number = 0;
            spinSpeed = Random.Range(80f, 140f);
            targetScale = Vector3.one;

            switch (kind)
            {
                case CubeMergeArenaPickupKind.SpeedUp:
                    SetColor(new Color32(50, 205, 255, 255));
                    SetLabel(">>");
                    break;
                case CubeMergeArenaPickupKind.MultiplierTwo:
                    SetColor(new Color32(255, 205, 40, 255));
                    SetLabel("x2");
                    break;
                case CubeMergeArenaPickupKind.MultiplierHalf:
                    SetColor(new Color32(178, 80, 255, 255));
                    SetLabel("1/2");
                    break;
            }

            PlaySpawnBounce();
        }

        private void Awake()
        {
            EnsureReferences();
            var col = GetComponent<Collider>();
            col.isTrigger = true;
        }

        private void Update()
        {
            transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
        }

        private void EnsureReferences()
        {
            targetRenderer = targetRenderer != null ? targetRenderer : GetComponentInChildren<Renderer>();
            label = label != null ? label : GetComponentInChildren<TextMeshPro>();
        }

        private void SetColor(Color color)
        {
            EnsureReferences();
            if (targetRenderer == null) return;
            targetRenderer.sharedMaterial = new Material(Shader.Find("Standard")) { color = color };
        }

        private void SetLabel(string value)
        {
            EnsureReferences();
            if (label == null) return;
            label.text = value;
            label.color = Color.white;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
        }

        private void PlaySpawnBounce()
        {
            StopAllCoroutines();
            StartCoroutine(SpawnBounceRoutine());
        }

        private IEnumerator SpawnBounceRoutine()
        {
            var duration = 0.25f;
            var startPosition = transform.position + Vector3.up * 1.5f;
            var endPosition = transform.position;
            var startScale = new Vector3(targetScale.x * 1.1f, targetScale.y * 0.5f, targetScale.z * 1.1f);

            transform.position = startPosition;
            transform.localScale = startScale;

            for (var elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                var t = Mathf.Clamp01(elapsed / duration);
                var bounce = 1f - Mathf.Pow(1f - t, 3f);
                var back = BackOut(t);
                transform.position = Vector3.LerpUnclamped(startPosition, endPosition, bounce);
                transform.localScale = Vector3.LerpUnclamped(startScale, targetScale, back);
                yield return null;
            }

            transform.position = endPosition;
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
