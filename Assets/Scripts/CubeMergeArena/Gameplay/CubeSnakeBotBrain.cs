using UnityEngine;

namespace CubeMergeArena.Gameplay
{
    [RequireComponent(typeof(CubeSnake))]
    public sealed class CubeSnakeBotBrain : MonoBehaviour
    {
        private enum BotState
        {
            Random,
            Follow,
            Avoid
        }

        private CubeSnake snake;
        private CubeMergeArenaBalance balance;
        private BotState state;
        private CubeSnake targetSnake;
        private float stateTimer;
        private float decisionTimer;
        private float randomRetargetTimer;
        private float currentRandomAngle;
        private float randomDriftSign = 1f;

        private void Awake()
        {
            snake = GetComponent<CubeSnake>();
        }

        private void Start()
        {
            balance = snake.Balance;
            EnterState(BotState.Random);
        }

        private void Update()
        {
            if (balance == null || !snake.IsAlive) return;

            stateTimer -= Time.deltaTime;
            decisionTimer -= Time.deltaTime;
            randomRetargetTimer -= Time.deltaTime;

            if (decisionTimer <= 0f)
            {
                decisionTimer = balance.aiDecisionInterval;
                DecideState();
                TryBoost();
                TryTrimDummy();
            }

            Steer();
        }

        private void DecideState()
        {
            var follow = FindSnakeInRange(strongerThanBot: false, balance.aiFollowRange);
            var avoid = FindSnakeInRange(strongerThanBot: true, balance.aiAvoidRange);
            var roll = Random.value;

            if (snake.IsDummy && stateTimer <= 0f)
            {
                if (roll < 0.7f) EnterState(BotState.Random);
                else ResetStateTimer();
                return;
            }

            if (state == BotState.Random && stateTimer <= 0f)
            {
                if (follow != null) EnterState(BotState.Follow, follow);
                else if (avoid != null) EnterState(BotState.Avoid, avoid);
                else EnterState(BotState.Random);
                return;
            }

            if (state == BotState.Follow && stateTimer <= 0f)
            {
                if (roll < 0.7f) EnterState(BotState.Random);
                else ResetStateTimer();
                return;
            }

            if (state != BotState.Avoid) return;

            if (stateTimer <= 0f)
            {
                if (follow != null) EnterState(BotState.Follow, follow);
                else if (avoid != null)
                {
                    if (roll < 0.6f) EnterState(BotState.Random);
                    else ResetStateTimer();
                }
                else
                {
                    EnterState(BotState.Random);
                }
            }
            else if (avoid == null)
            {
                if (follow != null) EnterState(BotState.Follow, follow);
                else EnterState(BotState.Random);
            }
        }

        private void Steer()
        {
            if (IsWallAhead(snake.DesiredAngle, out var wallAngle))
            {
                snake.SetDesiredAngle(wallAngle);
                currentRandomAngle = wallAngle;
                return;
            }

            switch (state)
            {
                case BotState.Follow:
                    if (targetSnake != null && targetSnake.IsAlive)
                    {
                        snake.SetDesiredAngle(CubeSnake.AngleToPosition(snake.HeadPosition, targetSnake.HeadPosition));
                    }
                    else
                    {
                        EnterState(BotState.Random);
                    }
                    break;

                case BotState.Avoid:
                    if (targetSnake != null && targetSnake.IsAlive)
                    {
                        snake.SetDesiredAngle(CubeSnake.AngleToPosition(targetSnake.HeadPosition, snake.HeadPosition));
                    }
                    else
                    {
                        EnterState(BotState.Random);
                    }
                    break;

                default:
                    SteerRandom();
                    break;
            }
        }

        private void SteerRandom()
        {
            if (randomRetargetTimer <= 0f)
            {
                randomRetargetTimer = balance.randomRetargetInterval;
                currentRandomAngle = Random.Range(0f, Mathf.PI * 2f);
                randomDriftSign *= -1f;
            }

            currentRandomAngle += randomDriftSign * Random.value * 0.01f;
            snake.SetDesiredAngle(currentRandomAngle);
        }

        private void TryBoost()
        {
            if (snake.IsDummy) return;
            if (Random.value < balance.botTinySpeedChancePerSecond)
            {
                snake.TinySpeedUp();
            }
        }

        private void TryTrimDummy()
        {
            if (!snake.IsDummy || snake.HeadNumber < balance.dummyAILimit) return;

            var player = FindPlayerSnake();
            if (player == null) return;

            if (Vector3.Distance(player.HeadPosition, snake.HeadPosition) > balance.playerSafeSpawnRadius)
            {
                snake.ReduceDummyHeadIfNeeded();
            }
        }

        private CubeSnake FindSnakeInRange(bool strongerThanBot, float range)
        {
            CubeSnake result = null;
            var bestDistance = float.MaxValue;

            for (var i = 0; i < CubeSnake.Snakes.Count; i++)
            {
                var other = CubeSnake.Snakes[i];
                if (other == null || other == snake || !other.IsAlive) continue;

                var distance = Vector3.Distance(snake.HeadPosition, other.HeadPosition);
                if (distance > range || distance > bestDistance) continue;

                if (strongerThanBot)
                {
                    if (other.HeadNumber <= snake.HeadNumber) continue;
                    if ((float)other.HeadNumber / snake.HeadNumber >= balance.aiAvoidPowerRatioLimit) continue;
                }
                else if (other.HeadNumber >= snake.HeadNumber)
                {
                    continue;
                }

                result = other;
                bestDistance = distance;
            }

            return result;
        }

        private CubeSnake FindPlayerSnake()
        {
            for (var i = 0; i < CubeSnake.Snakes.Count; i++)
            {
                var other = CubeSnake.Snakes[i];
                if (other != null && other.IsPlayer) return other;
            }

            return null;
        }

        private bool IsWallAhead(float angle, out float correctiveAngle)
        {
            var probe = snake.HeadPosition + CubeSnake.DirectionFromAngle(angle) * balance.wallProbeDistance;
            var halfWidth = balance.mapWidth * 0.5f - 1f;
            var halfHeight = balance.mapHeight * 0.5f - 1f;

            if (Mathf.Abs(probe.x) <= halfWidth && Mathf.Abs(probe.z) <= halfHeight)
            {
                correctiveAngle = angle;
                return false;
            }

            correctiveAngle = CubeSnake.AngleToPosition(snake.HeadPosition, Vector3.zero);
            return true;
        }

        private void EnterState(BotState nextState, CubeSnake target = null)
        {
            state = nextState;
            targetSnake = target;
            ResetStateTimer();

            if (state == BotState.Random)
            {
                currentRandomAngle = Random.Range(0f, Mathf.PI * 2f);
                randomRetargetTimer = balance.randomRetargetInterval;
            }
        }

        private void ResetStateTimer()
        {
            stateTimer = state switch
            {
                BotState.Follow => Random.Range(balance.followStateDuration.x, balance.followStateDuration.y),
                BotState.Avoid => Random.Range(balance.avoidStateDuration.x, balance.avoidStateDuration.y),
                _ => Random.Range(balance.randomStateDuration.x, balance.randomStateDuration.y)
            };
        }
    }
}
