using BattleBuck.Core;
using UnityEngine;

namespace BattleBuck.Player
{
    public class PlayerData
    {
        public int Id             { get; private set; }
        public string Name        { get; private set; }
        public int Score          { get; private set; }
        public PlayerState State  { get; private set; }
        public CombatState Combat { get; private set; }
        public int Health         { get; private set; }
        public int MaxHealth      { get; private set; }
        public int ShootPower     { get; private set; }
        public Vector3 Position   { get; private set; }
        public int EngagedWithId  { get; set; }
        public float ShootTimer   { get; set; }
        public Vector3 MoveTarget { get; set; }
        public bool HasMoveTarget { get; set; }
        public float RespawnTimer { get; set; }

        public PlayerData(int id, string name, int maxHealth, int shootPower)
        {
            Id = id;
            Name = name;
            MaxHealth = maxHealth;
            Health = maxHealth;
            ShootPower = shootPower;
            Score = 0;
            State = PlayerState.Alive;
            Combat = CombatState.Free;
            EngagedWithId = -1;
            ShootTimer = 0f;
            RespawnTimer = 0f;
            HasMoveTarget = false;
        }

        public void SetPosition(Vector3 pos)
        {
            Position = pos;
            GameEvents.RaisePlayerPositionChanged(Id, pos);
        }

        public void AddKill()
        {
            Score++;
            GameEvents.RaiseScoreChanged(Id, Score);
        }

        public bool TakeDamage(int damage)
        {
            if (State != PlayerState.Alive) return false;

            Health -= damage;
            if (Health < 0) Health = 0;
            GameEvents.RaisePlayerHealthChanged(Id, Health, MaxHealth);

            if (Health <= 0)
            {
                Die();
                return true;
            }
            return false;
        }

        public void Die()
        {
            State = PlayerState.Dead;
            Combat = CombatState.Free;
            EngagedWithId = -1;
            HasMoveTarget = false;
            GameEvents.RaisePlayerDied(Id);
        }

        public void Respawn(Vector3 spawnPos)
        {
            State = PlayerState.Alive;
            Health = MaxHealth;
            Combat = CombatState.Free;
            EngagedWithId = -1;
            ShootTimer = 0f;
            RespawnTimer = 0f;
            HasMoveTarget = false;
            Position = spawnPos;
            GameEvents.RaisePlayerHealthChanged(Id, Health, MaxHealth);
            GameEvents.RaisePlayerRespawned(Id);
            GameEvents.RaisePlayerPositionChanged(Id, spawnPos);
        }

        public void Engage(int targetId)
        {
            Combat = CombatState.Engaged;
            EngagedWithId = targetId;
            ShootTimer = 0f;
            HasMoveTarget = false;
        }

        public void Disengage()
        {
            Combat = CombatState.Free;
            EngagedWithId = -1;
            ShootTimer = 0f;
        }
    }
}
