using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private CubeMergeArenaBalance balance;
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool clearExistingArenaOnBuild = true;
        [SerializeField] private Transform runtimeContentRoot;
        [SerializeField] private GameObject ground;
        [SerializeField] private Camera arenaCamera;
        [SerializeField] private Light arenaLight;
        [SerializeField] private CubeSnake snakePrefab;
        [SerializeField] private CubeSnakeSegment segmentPrefab;
        [SerializeField] private CubeMergeArenaSpawner spawnerPrefab;
        [SerializeField] private CubeMergeArenaPickup pickupPrefab;

        private static readonly Vector2[] AiSpawnPositions =
        {
            new Vector2(-37f, -37f), new Vector2(-3f, -38f), new Vector2(17f, -32f), new Vector2(39f, -23f),
            new Vector2(25f, -15f), new Vector2(-21f, -26f), new Vector2(-27f, -6f), new Vector2(-19f, 25f),
            new Vector2(-1f, 37f), new Vector2(29f, 38f), new Vector2(38f, 22f), new Vector2(-39f, 12f),
            new Vector2(14f, 23f), new Vector2(38f, 2f), new Vector2(25f, -3f), new Vector2(-4f, -22f),
            new Vector2(-32f, 40f), new Vector2(-38f, 35f), new Vector2(33f, 5f), new Vector2(-38f, -17f)
        };

        public bool IsBuilt { get; private set; }

        private Transform RuntimeContentRoot
        {
            get
            {
                if (runtimeContentRoot != null)
                {
                    return runtimeContentRoot;
                }

                var existing = transform.Find("RuntimeContent");
                if (existing != null)
                {
                    runtimeContentRoot = existing;
                    return runtimeContentRoot;
                }

                var root = new GameObject("RuntimeContent");
                root.transform.SetParent(transform, false);
                runtimeContentRoot = root.transform;
                return runtimeContentRoot;
            }
        }

        private void Start()
        {
            if (buildOnStart)
            {
                BuildArena();
            }
        }

        [ContextMenu("Build Arena")]
        public void BuildArena()
        {
            ResolveSceneReferences();

            if (clearExistingArenaOnBuild)
            {
                ClearArena();
            }

            if (balance == null)
            {
                balance = ScriptableObject.CreateInstance<CubeMergeArenaBalance>();
                balance.name = "Runtime Cube Merge Arena Balance";
            }

            var player = SpawnSnake("PlayerSnake", new[] { balance.playerSnakeNumber }, Vector3.zero, 0f, true, false, false);
            CreateEnvironment(player);
            SpawnAis(player);
            CreateSpawner();
            IsBuilt = true;
        }

        [ContextMenu("Clear Arena")]
        public void ClearArena()
        {
            var root = RuntimeContentRoot;
            for (var i = root.childCount - 1; i >= 0; i--)
            {
                var child = root.GetChild(i).gameObject;
                if (Application.isPlaying)
                {
                    Destroy(child);
                }
                else
                {
                    DestroyImmediate(child);
                }
            }

            IsBuilt = false;
        }

        private void CreateEnvironment(CubeSnake player)
        {
            if (ground != null)
            {
                ground.transform.position = new Vector3(0f, -0.05f, 0f);
                ground.transform.localScale = new Vector3(balance.mapWidth, 0.1f, balance.mapHeight);
            }

            if (arenaLight != null)
            {
                arenaLight.type = LightType.Directional;
                arenaLight.intensity = 1.2f;
                arenaLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            if (arenaCamera == null)
            {
                Debug.LogWarning("CubeMergeArenaRuntimeBootstrap is missing an arena camera reference.", this);
                return;
            }

            arenaCamera.transform.position = new Vector3(0f, 62f, -48f);
            arenaCamera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            arenaCamera.fieldOfView = 45f;
            arenaCamera.clearFlags = CameraClearFlags.SolidColor;
            arenaCamera.backgroundColor = new Color32(0, 45, 140, 255);

            var follow = arenaCamera.GetComponent<CubeMergeArenaCameraFollow>();
            if (follow == null)
            {
                follow = arenaCamera.gameObject.AddComponent<CubeMergeArenaCameraFollow>();
            }

            follow.Initialize(player);
        }

        private void SpawnAis(CubeSnake player)
        {
            var playerHeadNumber = player != null ? player.HeadNumber : balance.playerSnakeNumber;

            for (var i = 0; i < balance.numberOfAIs; i++)
            {
                var position = AiSpawnPositions[i % AiSpawnPositions.Length];
                var snake = SpawnSnake(
                    "BotSnake_" + (i + 1),
                    balance.PickBotSnakeNumbers(playerHeadNumber),
                    new Vector3(position.x, 0f, position.y),
                    Random.Range(0f, Mathf.PI * 2f),
                    false,
                    true,
                    false);
                snake.gameObject.AddComponent<CubeSnakeBotBrain>();
            }

            for (var i = 0; i < balance.numberOfDummyAIs; i++)
            {
                var position = GetRandomSpawnPosition();
                var snake = SpawnSnake(
                    "DummySnake_" + (i + 1),
                    new[] { balance.playerSnakeNumber },
                    position,
                    Random.Range(0f, Mathf.PI * 2f),
                    false,
                    true,
                    true);
                snake.gameObject.AddComponent<CubeSnakeBotBrain>();
            }
        }

        private CubeSnake SpawnSnake(
            string objectName,
            int[] numbers,
            Vector3 position,
            float angle,
            bool player,
            bool bot,
            bool dummy)
        {
            CubeSnake snake;
            if (snakePrefab != null)
            {
                snake = Instantiate(snakePrefab, RuntimeContentRoot);
                snake.name = objectName;
            }
            else
            {
                var snakeObject = new GameObject(objectName);
                snakeObject.transform.SetParent(RuntimeContentRoot, true);
                snake = snakeObject.AddComponent<CubeSnake>();
            }

            snake.SetSegmentPrefab(segmentPrefab);
            snake.Initialize(balance, numbers, position, angle, player, bot, dummy);
            return snake;
        }

        private void CreateSpawner()
        {
            CubeMergeArenaSpawner spawner;
            if (spawnerPrefab != null)
            {
                spawner = Instantiate(spawnerPrefab, RuntimeContentRoot);
                spawner.name = "PickupSpawner";
            }
            else
            {
                var spawnerObject = new GameObject("PickupSpawner");
                spawnerObject.transform.SetParent(RuntimeContentRoot, false);
                spawner = spawnerObject.AddComponent<CubeMergeArenaSpawner>();
            }

            spawner.Initialize(balance, pickupPrefab);
        }

        private void ResolveSceneReferences()
        {
            if (ground == null)
            {
                var groundTransform = transform.Find("ArenaGround");
                if (groundTransform != null)
                {
                    ground = groundTransform.gameObject;
                }
            }

            if (arenaCamera == null)
            {
                arenaCamera = GetComponentInChildren<Camera>(true);
            }

            if (arenaLight == null)
            {
                arenaLight = GetComponentInChildren<Light>(true);
            }
        }

        private Vector3 GetRandomSpawnPosition()
        {
            return new Vector3(
                Random.Range(-balance.mapWidth * 0.42f, balance.mapWidth * 0.42f),
                0f,
                Random.Range(-balance.mapHeight * 0.42f, balance.mapHeight * 0.42f));
        }
    }
}
