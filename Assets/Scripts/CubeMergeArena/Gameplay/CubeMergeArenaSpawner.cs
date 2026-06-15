using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaSpawner : MonoBehaviour
    {
        [SerializeField] private CubeMergeArenaBalance balance;

        private readonly List<CubeMergeArenaPickup> pickups = new List<CubeMergeArenaPickup>();
        private float refillTimer;
        private float boosterTimer;
        private bool spawningBatch;

        public void Initialize(CubeMergeArenaBalance config)
        {
            balance = config;
            refillTimer = balance.freeBlockRefillCooldown;
            boosterTimer = balance.generateBoosterTime;

            for (var i = 0; i < balance.initialFreeBlocks; i++)
            {
                SpawnFood(balance.PickFreeBlockNumber());
            }
        }

        private void Update()
        {
            if (balance == null) return;

            CleanupDestroyedPickups();
            UpdateFoodRefill();
            UpdateBoosterSpawn();
        }

        private void UpdateFoodRefill()
        {
            if (spawningBatch || CountFood() >= balance.numberOfFreeBlocks) return;

            refillTimer -= Time.deltaTime;
            if (refillTimer > 0f) return;

            refillTimer = balance.freeBlockRefillCooldown;
            StartCoroutine(SpawnFoodBatch(balance.numberOfFreeBlockSpawn, balance.freeBlockSpawnInterval));
        }

        private void UpdateBoosterSpawn()
        {
            boosterTimer -= Time.deltaTime;
            if (boosterTimer > 0f) return;

            boosterTimer = balance.generateBoosterTime;
            if (Random.value <= balance.boosterChance)
            {
                SpawnBooster(balance.PickBoosterKind());
            }
        }

        private IEnumerator SpawnFoodBatch(int count, float interval)
        {
            spawningBatch = true;

            for (var i = 0; i < count; i++)
            {
                SpawnFood(balance.PickFreeBlockNumber());
                if (interval > 0f) yield return new WaitForSeconds(interval);
            }

            spawningBatch = false;
        }

        private void SpawnFood(int number)
        {
            var pickup = CreatePickup("Food_" + number, GetRandomEmptyPosition());
            pickup.SetupFood(number, balance);
            pickups.Add(pickup);
        }

        private void SpawnBooster(CubeMergeArenaPickupKind kind)
        {
            var pickup = CreatePickup("Booster_" + kind, GetRandomEmptyPosition() + Vector3.up * balance.boosterSpawnHeight);
            pickup.SetupBooster(kind);
            pickups.Add(pickup);
        }

        private CubeMergeArenaPickup CreatePickup(string objectName, Vector3 position)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = objectName;
            cube.transform.SetParent(transform, true);
            cube.transform.position = position;

            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(cube.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            labelObject.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            labelObject.transform.localScale = Vector3.one * 0.12f;

            var label = labelObject.AddComponent<TextMeshPro>();
            label.fontSize = 8f;
            label.enableWordWrapping = false;
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;

            return cube.AddComponent<CubeMergeArenaPickup>();
        }

        private Vector3 GetRandomEmptyPosition()
        {
            for (var attempt = 0; attempt < 80; attempt++)
            {
                var candidate = new Vector3(
                    Random.Range(-balance.mapWidth * 0.48f, balance.mapWidth * 0.48f),
                    0.55f,
                    Random.Range(-balance.mapHeight * 0.48f, balance.mapHeight * 0.48f));

                if (IsFarEnough(candidate))
                {
                    return candidate;
                }
            }

            return new Vector3(
                Random.Range(-balance.mapWidth * 0.45f, balance.mapWidth * 0.45f),
                0.55f,
                Random.Range(-balance.mapHeight * 0.45f, balance.mapHeight * 0.45f));
        }

        private bool IsFarEnough(Vector3 candidate)
        {
            for (var i = 0; i < pickups.Count; i++)
            {
                if (pickups[i] == null) continue;
                if (Vector3.Distance(candidate, pickups[i].transform.position) < balance.emptySpawnRadius)
                {
                    return false;
                }
            }

            for (var i = 0; i < CubeSnake.Snakes.Count; i++)
            {
                var snake = CubeSnake.Snakes[i];
                if (snake == null || !snake.IsAlive) continue;
                if (Vector3.Distance(candidate, snake.HeadPosition) < balance.emptySpawnRadius)
                {
                    return false;
                }
            }

            return true;
        }

        private int CountFood()
        {
            var count = 0;
            for (var i = 0; i < pickups.Count; i++)
            {
                if (pickups[i] != null && pickups[i].Kind == CubeMergeArenaPickupKind.Food)
                {
                    count++;
                }
            }

            return count;
        }

        private void CleanupDestroyedPickups()
        {
            for (var i = pickups.Count - 1; i >= 0; i--)
            {
                if (pickups[i] == null)
                {
                    pickups.RemoveAt(i);
                }
            }
        }
    }
}
