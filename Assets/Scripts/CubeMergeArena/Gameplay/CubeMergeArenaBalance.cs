using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    [CreateAssetMenu(menuName = "Cube Merge Arena/Gameplay Balance", fileName = "CubeMergeArenaBalance")]
    public sealed class CubeMergeArenaBalance : ScriptableObject
    {
        [Header("World")]
        public float mapWidth = 90f;
        public float mapHeight = 90f;
        public float emptySpawnRadius = 2.25f;
        public float playerSafeSpawnRadius = 13f;

        [Header("Snakes")]
        public int playerSnakeNumber = 2;
        public int numberOfAIs = 16;
        public int numberOfDummyAIs = 5;
        public int dummyAILimit = 8;
        public int maxReviveCount = 1;
        public float reviveTimer = 7f;
        public float baseSpeedRaw = 0.09f;
        public float playerMovementLerp = 0.15f;
        public float botMovementLerp = 0.07f;
        public float segmentSpacing = 1.05f;
        public float segmentFollowSharpness = 18f;

        [Header("Tiny Speed")]
        public float tinySpeedMultiplier = 1.5f;
        public float tinySpeedDuration = 2f;
        public float tinySpeedCooldown = 6f;
        [Range(0f, 1f)] public float botTinySpeedChancePerSecond = 0.2f;

        [Header("AI")]
        public float aiFollowRange = 10f;
        public float aiAvoidRange = 12f;
        public float aiAvoidPowerRatioLimit = 500f;
        public Vector2 randomStateDuration = new Vector2(2f, 4f);
        public Vector2 followStateDuration = new Vector2(3f, 7f);
        public Vector2 avoidStateDuration = new Vector2(3f, 7f);
        public float randomRetargetInterval = 2f;
        public float aiDecisionInterval = 1f;
        public float wallProbeDistance = 2f;

        [Header("Food")]
        public int initialFreeBlocks = 50;
        public int numberOfFreeBlocks = 40;
        public int numberOfFreeBlockSpawn = 25;
        public float freeBlockSpawnInterval = 0.015f;
        public float freeBlockRefillCooldown = 4f;
        public int[] freeBlockExponentWeights = { 60, 10, 3, 3, 1 };

        [Header("Boosters")]
        public float boosterChance = 0.5f;
        public float generateBoosterTime = 6f;
        public float speedBoosterMultiplier = 2f;
        public float speedBoosterDuration = 2f;
        public float boosterSpawnHeight = 0.55f;

        [Header("Bot Spawn Numbers")]
        public int[] botExponentWeights = { 55, 35, 13, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
        public Vector2Int botExtraSegmentRange = new Vector2Int(1, 6);

        public float MoveSpeedUnitsPerSecond => baseSpeedRaw * 60f;

        public int PickFreeBlockNumber()
        {
            var exponent = WeightedIndex(freeBlockExponentWeights) + 1;
            return 1 << exponent;
        }

        public CubeMergeArenaPickupKind PickBoosterKind()
        {
            var roll = Random.value;
            if (roll < 0.7f) return CubeMergeArenaPickupKind.SpeedUp;
            if (roll < 0.8f) return CubeMergeArenaPickupKind.MultiplierTwo;
            return CubeMergeArenaPickupKind.MultiplierHalf;
        }

        public int[] PickBotSnakeNumbers(int playerHeadNumber)
        {
            var adjusted = new int[botExponentWeights.Length];
            var bonus = Mathf.FloorToInt(playerHeadNumber / 250f);
            for (var i = 0; i < adjusted.Length; i++)
            {
                adjusted[i] = Mathf.Max(1, botExponentWeights[i] + bonus);
            }

            var topExponent = WeightedIndex(adjusted) + 1;
            var extraCount = Random.Range(botExtraSegmentRange.x, botExtraSegmentRange.y + 1);
            var numbers = new int[extraCount + 1];
            numbers[0] = 1 << topExponent;

            for (var i = 1; i < numbers.Length; i++)
            {
                var exponent = Random.Range(1, Mathf.Max(2, topExponent));
                numbers[i] = 1 << exponent;
            }

            System.Array.Sort(numbers);
            System.Array.Reverse(numbers);
            return DeduplicateSorted(numbers);
        }

        public float GetCubeSize(int number)
        {
            var exponent = Mathf.Max(1f, Mathf.Log(Mathf.Max(2, number), 2f));
            return 0.9f + 0.1f * (exponent - 1f);
        }

        public Color GetNumberColor(int number)
        {
            if (number <= 2) return new Color32(232, 55, 45, 255);
            if (number <= 4) return new Color32(33, 160, 255, 255);
            if (number <= 8) return new Color32(78, 205, 48, 255);
            if (number <= 16) return new Color32(255, 149, 30, 255);
            if (number <= 32) return new Color32(255, 204, 36, 255);
            if (number <= 64) return new Color32(186, 94, 255, 255);
            return new Color32(255, 229, 151, 255);
        }

        private static int WeightedIndex(int[] weights)
        {
            var total = 0;
            for (var i = 0; i < weights.Length; i++)
            {
                total += Mathf.Max(0, weights[i]);
            }

            if (total <= 0) return 0;

            var roll = Random.Range(0, total);
            for (var i = 0; i < weights.Length; i++)
            {
                roll -= Mathf.Max(0, weights[i]);
                if (roll < 0) return i;
            }

            return weights.Length - 1;
        }

        private static int[] DeduplicateSorted(int[] sortedDescending)
        {
            var list = new System.Collections.Generic.List<int>(sortedDescending.Length);
            for (var i = 0; i < sortedDescending.Length; i++)
            {
                if (!list.Contains(sortedDescending[i]))
                {
                    list.Add(sortedDescending[i]);
                }
            }

            return list.ToArray();
        }
    }
}
