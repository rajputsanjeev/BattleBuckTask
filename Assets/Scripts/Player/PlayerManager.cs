using System.Collections.Generic;
using BattleBuck.Core;
using UnityEngine;

namespace BattleBuck.Player
{
    public class PlayerManager
    {
        public PlayerData[] Players => _players;
        public int PlayerCount => _players.Length;

        private readonly PlayerData[] _players;
        private readonly List<int> _freeAliveIndices;
        private readonly List<PendingHit> _pendingHits;
        private readonly float _respawnDelay;
        private readonly float _moveSpeed;
        private readonly float _killRange;
        private readonly float _killRangeSq;
        private readonly float _shootInterval;
        private readonly float _projectileSpeed;
        private readonly float _groundHalfSize;
        private readonly int _scoreLimit;
        private bool _matchActive;

        private struct PendingHit
        {
            public int ShooterId;
            public int TargetId;
            public float TimeRemaining;
        }

        public PlayerManager(MatchConfig config)
        {
            int count = config.playerCount;
            _respawnDelay = config.respawnDelay;
            _moveSpeed = config.moveSpeed;
            _killRange = config.killRange;
            _killRangeSq = config.killRange * config.killRange;
            _shootInterval = config.shootInterval;
            _projectileSpeed = config.projectileSpeed;
            _groundHalfSize = config.groundHalfSize;
            _scoreLimit = config.scoreLimit;

            _players = new PlayerData[count];
            _freeAliveIndices = new List<int>(count);
            _pendingHits = new List<PendingHit>(count * 2);

            for (int i = 0; i < count; i++)
            {
                PlayerConfig pc = config.GetPlayerConfig(i);
                _players[i] = new PlayerData(i, pc.name, pc.health, pc.shootPower);
                _players[i].SetPosition(GetRandomGroundPosition());
            }

            _matchActive = false;
        }

        public void StartMatch()
        {
            _matchActive = true;
        }

        public void StopMatch()
        {
            _matchActive = false;
        }

        public void Tick(float deltaTime)
        {
            if (!_matchActive) return;

            UpdateRespawnTimers(deltaTime);
            UpdateMovement(deltaTime);
            ScanForEngagements();
            UpdateCombat(deltaTime);
            UpdatePendingHits(deltaTime);
        }

        public PlayerData GetPlayer(int id) => _players[id];

        private void UpdateRespawnTimers(float deltaTime)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].State != PlayerState.Dead) continue;

                _players[i].RespawnTimer -= deltaTime;
                if (_players[i].RespawnTimer <= 0f)
                {
                    Vector3 spawnPos = GetRandomGroundPosition();
                    _players[i].Respawn(spawnPos);
                }
            }
        }

        private void UpdateMovement(float deltaTime)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                PlayerData p = _players[i];
                if (p.State != PlayerState.Alive) continue;
                if (p.Combat == CombatState.Engaged) continue;

                if (!p.HasMoveTarget)
                {
                    p.MoveTarget = GetRandomGroundPosition();
                    p.HasMoveTarget = true;
                }

                Vector3 dir = p.MoveTarget - p.Position;
                float dist = dir.magnitude;

                if (dist < 0.3f)
                {
                    p.HasMoveTarget = false;
                }
                else
                {
                    Vector3 step = (dir / dist) * _moveSpeed * deltaTime;
                    if (step.magnitude > dist) step = dir;

                    Vector3 newPos = p.Position + step;
                    newPos = ClampToGround(newPos);
                    p.SetPosition(newPos);
                }
            }
        }

        private void ScanForEngagements()
        {
            _freeAliveIndices.Clear();
            for (int i = 0; i < _players.Length; i++)
            {
                if (_players[i].State == PlayerState.Alive &&
                    _players[i].Combat == CombatState.Free)
                {
                    _freeAliveIndices.Add(i);
                }
            }

            for (int a = 0; a < _freeAliveIndices.Count; a++)
            {
                int idA = _freeAliveIndices[a];
                PlayerData pA = _players[idA];

                if (pA.Combat != CombatState.Free) continue;

                for (int b = a + 1; b < _freeAliveIndices.Count; b++)
                {
                    int idB = _freeAliveIndices[b];
                    PlayerData pB = _players[idB];

                    if (pB.Combat != CombatState.Free) continue;

                    float distSq = (pA.Position - pB.Position).sqrMagnitude;
                    if (distSq <= _killRangeSq)
                    {
                        pA.Engage(idB);
                        pB.Engage(idA);
                        break;
                    }
                }
            }
        }

        private void UpdateCombat(float deltaTime)
        {
            for (int i = 0; i < _players.Length; i++)
            {
                PlayerData p = _players[i];
                if (p.State != PlayerState.Alive) continue;
                if (p.Combat != CombatState.Engaged) continue;

                int targetId = p.EngagedWithId;
                if (targetId < 0 || targetId >= _players.Length ||
                    _players[targetId].State != PlayerState.Alive)
                {
                    p.Disengage();
                    continue;
                }

                p.ShootTimer += deltaTime;
                if (p.ShootTimer >= _shootInterval)
                {
                    p.ShootTimer = 0f;
                    FireProjectile(i, targetId);
                }
            }
        }

        private void FireProjectile(int shooterId, int targetId)
        {
            Vector3 from = _players[shooterId].Position;
            Vector3 to = _players[targetId].Position;
            float dist = Vector3.Distance(from, to);
            float travelTime = dist / _projectileSpeed;

            GameEvents.RaiseProjectileFired(shooterId, targetId, from, to);

            _pendingHits.Add(new PendingHit
            {
                ShooterId = shooterId,
                TargetId = targetId,
                TimeRemaining = travelTime
            });
        }

        private void UpdatePendingHits(float deltaTime)
        {
            for (int i = _pendingHits.Count - 1; i >= 0; i--)
            {
                PendingHit hit = _pendingHits[i];
                hit.TimeRemaining -= deltaTime;

                if (hit.TimeRemaining <= 0f)
                {
                    _pendingHits.RemoveAt(i);
                    ApplyHit(hit.ShooterId, hit.TargetId);
                }
                else
                {
                    _pendingHits[i] = hit;
                }
            }
        }

        private void ApplyHit(int shooterId, int targetId)
        {
            if (targetId < 0 || targetId >= _players.Length) return;

            PlayerData target = _players[targetId];
            PlayerData shooter = _players[shooterId];

            if (target.State != PlayerState.Alive) return;

            bool died = target.TakeDamage(shooter.ShootPower);
            if (died)
            {
                target.RespawnTimer = _respawnDelay;
                shooter.AddKill();
                shooter.Disengage();

                GameEvents.RaisePlayerKilled(shooterId, targetId);

                if (shooter.Score >= _scoreLimit)
                {
                    _matchActive = false;
                    GameEvents.RaiseMatchEnded(MatchEndReason.ScoreLimitReached, shooterId);
                }
            }
        }

        private Vector3 GetRandomGroundPosition()
        {
            float x = Random.Range(-_groundHalfSize, _groundHalfSize);
            float z = Random.Range(-_groundHalfSize, _groundHalfSize);
            return new Vector3(x, 0.75f, z);
        }

        private Vector3 ClampToGround(Vector3 pos)
        {
            pos.x = Mathf.Clamp(pos.x, -_groundHalfSize, _groundHalfSize);
            pos.z = Mathf.Clamp(pos.z, -_groundHalfSize, _groundHalfSize);
            return pos;
        }
    }
}
