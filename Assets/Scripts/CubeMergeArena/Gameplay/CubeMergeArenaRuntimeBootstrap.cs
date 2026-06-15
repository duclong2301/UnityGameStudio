using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    public sealed class CubeMergeArenaRuntimeBootstrap : MonoBehaviour
    {
        [SerializeField] private CubeMergeArenaBalance balance;
        [SerializeField] private bool buildOnStart = true;
        [SerializeField] private bool clearExistingArenaOnBuild = true;

        private static readonly Vector2[] AiSpawnPositions =
        {
            new Vector2(-37f, -37f), new Vector2(-3f, -38f), new Vector2(17f, -32f), new Vector2(39f, -23f),
            new Vector2(25f, -15f), new Vector2(-21f, -26f), new Vector2(-27f, -6f), new Vector2(-19f, 25f),
            new Vector2(-1f, 37f), new Vector2(29f, 38f), new Vector2(38f, 22f), new Vector2(-39f, 12f),
            new Vector2(14f, 23f), new Vector2(38f, 2f), new Vector2(25f, -3f), new Vector2(-4f, -22f),
            new Vector2(-32f, 40f), new Vector2(-38f, 35f), new Vector2(33f, 5f), new Vector2(-38f, -17f)
        };

        public bool IsBuilt { get; private set; }

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
            for (var i = transform.childCount - 1; i >= 0; i--)
            {
                var child = transform.GetChild(i).gameObject;
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
            if (GameObject.Find("CubeMergeArena_Ground") == null)
            {
                var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
                ground.name = "CubeMergeArena_Ground";
                ground.transform.position = new Vector3(0f, -0.05f, 0f);
                ground.transform.localScale = new Vector3(balance.mapWidth, 0.1f, balance.mapHeight);
                var renderer = ground.GetComponent<Renderer>();
                renderer.sharedMaterial = new Material(Shader.Find("Standard"))
                {
                    color = new Color32(0, 94, 210, 255)
                };
            }

            if (FindFirstObjectByType<Light>() == null)
            {
                var lightObject = new GameObject("Directional Light");
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            }

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }

            camera.transform.position = new Vector3(0f, 62f, -48f);
            camera.transform.rotation = Quaternion.Euler(58f, 0f, 0f);
            camera.fieldOfView = 45f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color32(0, 45, 140, 255);

            var follow = camera.GetComponent<CubeMergeArenaCameraFollow>();
            if (follow == null)
            {
                follow = camera.gameObject.AddComponent<CubeMergeArenaCameraFollow>();
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
            var snakeObject = new GameObject(objectName);
            snakeObject.transform.SetParent(transform, true);
            var snake = snakeObject.AddComponent<CubeSnake>();
            snake.Initialize(balance, numbers, position, angle, player, bot, dummy);
            return snake;
        }

        private void CreateSpawner()
        {
            var spawnerObject = new GameObject("PickupSpawner");
            spawnerObject.transform.SetParent(transform, false);
            var spawner = spawnerObject.AddComponent<CubeMergeArenaSpawner>();
            spawner.Initialize(balance);
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
