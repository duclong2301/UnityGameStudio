using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeSnake : MonoBehaviour
    {
        private static readonly List<CubeSnake> ActiveSnakes = new List<CubeSnake>();

        [SerializeField] private CubeMergeArenaBalance balance;
        [SerializeField] private bool isPlayer;
        [SerializeField] private bool isBot;
        [SerializeField] private bool isDummy;
        [SerializeField] private CubeSnakeSegment segmentPrefab;

        private readonly List<CubeSnakeSegment> segments = new List<CubeSnakeSegment>();
        private readonly List<Vector3> breadcrumbs = new List<Vector3>(512);

        private float currentAngle;
        private float desiredAngle;
        private float movementLerp;
        private float currentSpeed;
        private float speedBoostTimer;
        private float tinySpeedCooldownTimer;
        private bool speedBoosted;

        public static IReadOnlyList<CubeSnake> Snakes => ActiveSnakes;
        public CubeMergeArenaBalance Balance => balance;
        public bool IsPlayer => isPlayer;
        public bool IsBot => isBot;
        public bool IsDummy => isDummy;
        public bool IsAlive => segments.Count > 0;
        public Vector3 HeadPosition => IsAlive ? segments[0].transform.position : transform.position;
        public int HeadNumber => IsAlive ? segments[0].Number : 0;
        public float DesiredAngle => desiredAngle;

        public void SetSegmentPrefab(CubeSnakeSegment prefab)
        {
            if (prefab != null)
            {
                segmentPrefab = prefab;
            }
        }

        public void Initialize(
            CubeMergeArenaBalance config,
            int[] numbers,
            Vector3 worldPosition,
            float startAngle,
            bool player,
            bool bot,
            bool dummy)
        {
            balance = config;
            isPlayer = player;
            isBot = bot;
            isDummy = dummy;
            currentAngle = startAngle;
            desiredAngle = startAngle;
            movementLerp = isBot ? balance.botMovementLerp : balance.playerMovementLerp;
            currentSpeed = balance.MoveSpeedUnitsPerSecond;
            transform.position = worldPosition;

            ClearSegments();
            var safeNumbers = numbers != null && numbers.Length > 0 ? numbers : new[] { balance.playerSnakeNumber };
            for (var i = 0; i < safeNumbers.Length; i++)
            {
                AddSegment(safeNumbers[i], i == 0);
            }

            breadcrumbs.Clear();
            for (var i = 0; i < 256; i++)
            {
                breadcrumbs.Add(HeadPosition - DirectionFromAngle(currentAngle) * i * 0.08f);
            }
        }

        public void SetDesiredAngle(float angleRadians)
        {
            desiredAngle = NormalizeRadians(angleRadians);
        }

        public void SetDesiredDirection(Vector3 worldDirection)
        {
            worldDirection.y = 0f;
            if (worldDirection.sqrMagnitude < 0.0001f) return;

            var directionAngle = Mathf.Atan2(worldDirection.z, worldDirection.x);
            SetDesiredAngle(directionAngle + Mathf.PI * 0.5f);
        }

        public void TinySpeedUp()
        {
            if (speedBoosted || tinySpeedCooldownTimer > 0f || !IsAlive) return;
            StartSpeedBoost(balance.tinySpeedMultiplier, balance.tinySpeedDuration, balance.tinySpeedCooldown);
        }

        public void StartSpeedBoost(float multiplier, float duration, float cooldown = 0f)
        {
            speedBoosted = true;
            currentSpeed = balance.MoveSpeedUnitsPerSecond * Mathf.Max(0.1f, multiplier);
            speedBoostTimer = Mathf.Max(0.05f, duration);
            tinySpeedCooldownTimer = Mathf.Max(tinySpeedCooldownTimer, cooldown);
        }

        public void ConsumePickup(CubeMergeArenaPickup pickup)
        {
            if (pickup == null || !IsAlive) return;

            switch (pickup.Kind)
            {
                case CubeMergeArenaPickupKind.Food:
                    AddNumberAndResolve(pickup.Number);
                    break;
                case CubeMergeArenaPickupKind.SpeedUp:
                    StartSpeedBoost(balance.speedBoosterMultiplier, balance.speedBoosterDuration);
                    break;
                case CubeMergeArenaPickupKind.MultiplierTwo:
                    segments[0].SetNumber(HeadNumber * 2, balance);
                    break;
                case CubeMergeArenaPickupKind.MultiplierHalf:
                    segments[0].SetNumber(Mathf.Max(2, HeadNumber / 2), balance);
                    break;
            }

            Destroy(pickup.gameObject);
        }

        public void ResolveSnakeContact(CubeSnakeSegment targetSegment)
        {
            if (targetSegment == null || targetSegment.Snake == null || targetSegment.Snake == this) return;
            if (!IsAlive || !targetSegment.Snake.IsAlive) return;

            var opponent = targetSegment.Snake;
            if (HeadNumber > opponent.HeadNumber)
            {
                ConsumeOpponentSegment(opponent, targetSegment);
            }
            else if (HeadNumber < opponent.HeadNumber)
            {
                opponent.ConsumeOpponentSegment(this, segments[0]);
            }
        }

        public void ReduceDummyHeadIfNeeded()
        {
            if (!isDummy || !IsAlive || HeadNumber < balance.dummyAILimit) return;
            segments[0].SetNumber(Mathf.Max(2, HeadNumber / 2), balance);
        }

        private void OnEnable()
        {
            if (!ActiveSnakes.Contains(this))
            {
                ActiveSnakes.Add(this);
            }
        }

        private void OnDisable()
        {
            ActiveSnakes.Remove(this);
        }

        private void Update()
        {
            if (balance == null || !IsAlive) return;

            if (isPlayer)
            {
                UpdatePlayerInput();
            }

            UpdateSpeedTimers();
            MoveHead();
            UpdateBreadcrumbs();
            UpdateTail();
        }

        private void UpdatePlayerInput()
        {
            var keyDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            if (keyDirection.sqrMagnitude > 0.01f)
            {
                SetDesiredDirection(keyDirection);
                return;
            }

            if (!Input.GetMouseButton(0) || Camera.main == null) return;

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            var plane = new Plane(Vector3.up, Vector3.zero);
            if (!plane.Raycast(ray, out var distance)) return;

            var target = ray.GetPoint(distance);
            SetDesiredDirection(target - HeadPosition);
        }

        private void UpdateSpeedTimers()
        {
            if (tinySpeedCooldownTimer > 0f)
            {
                tinySpeedCooldownTimer -= Time.deltaTime;
            }

            if (!speedBoosted) return;

            speedBoostTimer -= Time.deltaTime;
            if (speedBoostTimer <= 0f)
            {
                speedBoosted = false;
                currentSpeed = balance.MoveSpeedUnitsPerSecond;
            }
        }

        private void MoveHead()
        {
            var lerpAmount = Mathf.Clamp01(movementLerp * Time.deltaTime * 60f);
            var delta = Mathf.DeltaAngle(currentAngle * Mathf.Rad2Deg, desiredAngle * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            currentAngle = NormalizeRadians(currentAngle + delta * lerpAmount);

            var head = segments[0].transform;
            var nextPosition = head.position + DirectionFromAngle(currentAngle) * currentSpeed * Time.deltaTime;
            nextPosition.x = Mathf.Clamp(nextPosition.x, -balance.mapWidth * 0.5f, balance.mapWidth * 0.5f);
            nextPosition.z = Mathf.Clamp(nextPosition.z, -balance.mapHeight * 0.5f, balance.mapHeight * 0.5f);
            nextPosition.y = 0.55f;

            head.position = nextPosition;
            head.rotation = Quaternion.Euler(0f, -currentAngle * Mathf.Rad2Deg, 0f);
        }

        private void UpdateBreadcrumbs()
        {
            if (breadcrumbs.Count == 0 || Vector3.Distance(breadcrumbs[0], HeadPosition) > 0.05f)
            {
                breadcrumbs.Insert(0, HeadPosition);
            }

            var maxBreadcrumbs = Mathf.Max(128, segments.Count * 80);
            while (breadcrumbs.Count > maxBreadcrumbs)
            {
                breadcrumbs.RemoveAt(breadcrumbs.Count - 1);
            }
        }

        private void UpdateTail()
        {
            for (var i = 1; i < segments.Count; i++)
            {
                var target = SampleBreadcrumb(i * balance.segmentSpacing);
                var segmentTransform = segments[i].transform;
                var sharpness = 1f - Mathf.Exp(-balance.segmentFollowSharpness * Time.deltaTime);
                segmentTransform.position = Vector3.Lerp(segmentTransform.position, target, sharpness);

                var toPrevious = segments[i - 1].transform.position - segmentTransform.position;
                toPrevious.y = 0f;
                if (toPrevious.sqrMagnitude > 0.001f)
                {
                    segmentTransform.rotation = Quaternion.LookRotation(toPrevious.normalized, Vector3.up);
                }
            }
        }

        private Vector3 SampleBreadcrumb(float targetDistance)
        {
            if (breadcrumbs.Count == 0) return HeadPosition;

            var walked = 0f;
            for (var i = 1; i < breadcrumbs.Count; i++)
            {
                var segmentDistance = Vector3.Distance(breadcrumbs[i - 1], breadcrumbs[i]);
                if (walked + segmentDistance >= targetDistance)
                {
                    var t = Mathf.InverseLerp(walked, walked + segmentDistance, targetDistance);
                    return Vector3.Lerp(breadcrumbs[i - 1], breadcrumbs[i], t);
                }

                walked += segmentDistance;
            }

            return breadcrumbs[breadcrumbs.Count - 1];
        }

        private void AddNumberAndResolve(int number)
        {
            AddSegment(number, false);

            for (var i = segments.Count - 1; i > 0; i--)
            {
                if (segments[i].Number != segments[i - 1].Number) continue;

                segments[i - 1].SetNumber(segments[i - 1].Number * 2, balance);
                var removed = segments[i];
                segments.RemoveAt(i);
                Destroy(removed.gameObject);
            }
        }

        private void ConsumeOpponentSegment(CubeSnake opponent, CubeSnakeSegment targetSegment)
        {
            if (opponent == null || targetSegment == null || targetSegment.Snake != opponent) return;

            AddNumberAndResolve(targetSegment.Number);
            opponent.RemoveSegment(targetSegment);
        }

        private void RemoveSegment(CubeSnakeSegment targetSegment)
        {
            var index = segments.IndexOf(targetSegment);
            if (index < 0) return;

            segments.RemoveAt(index);
            Destroy(targetSegment.gameObject);

            if (segments.Count == 0)
            {
                Destroy(gameObject);
                return;
            }

            if (index == 0)
            {
                PromoteHead();
            }
        }

        private void PromoteHead()
        {
            var newHead = segments[0];
            newHead.Configure(this, newHead.Number, true, balance);
        }

        private void AddSegment(int number, bool isHeadSegment)
        {
            var segment = CreateSegment(isHeadSegment ? "Head" : "Segment_" + segments.Count);
            segment.transform.position = transform.position - DirectionFromAngle(currentAngle) * balance.segmentSpacing * segments.Count + Vector3.up * 0.55f;
            segment.Configure(this, number, isHeadSegment, balance);
            segment.PlaySpawnAnimation();
            segments.Add(segment);
        }

        private CubeSnakeSegment CreateSegment(string segmentName)
        {
            if (segmentPrefab != null)
            {
                var instance = Instantiate(segmentPrefab, transform);
                instance.name = segmentName;
                return instance;
            }

            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = segmentName;
            cube.transform.SetParent(transform, true);

            var labelObject = new GameObject("NumberLabel");
            labelObject.transform.SetParent(cube.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            labelObject.transform.localScale = Vector3.one * 0.12f;

            var label = labelObject.AddComponent<TextMeshPro>();
            label.fontSize = 8f;
            label.enableWordWrapping = false;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;

            return cube.AddComponent<CubeSnakeSegment>();
        }

        private void ClearSegments()
        {
            for (var i = segments.Count - 1; i >= 0; i--)
            {
                if (segments[i] != null)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(segments[i].gameObject);
                    }
                    else
                    {
                        DestroyImmediate(segments[i].gameObject);
                    }
                }
            }

            segments.Clear();
        }

        public static Vector3 DirectionFromAngle(float angleRadians)
        {
            var moveAngle = angleRadians - Mathf.PI * 0.5f;
            return new Vector3(Mathf.Cos(moveAngle), 0f, Mathf.Sin(moveAngle));
        }

        public static float AngleToPosition(Vector3 from, Vector3 to)
        {
            var direction = to - from;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return 0f;
            return Mathf.Atan2(direction.z, direction.x) + Mathf.PI * 0.5f;
        }

        private static float NormalizeRadians(float value)
        {
            while (value < 0f) value += Mathf.PI * 2f;
            while (value >= Mathf.PI * 2f) value -= Mathf.PI * 2f;
            return value;
        }
    }
}
