using System;
using UnityEngine;

namespace BattleBuck.Core
{
    [Serializable]
    public struct PlayerConfig
    {
        public string name;
        [Range(1, 10)] public int health;
        [Range(1, 5)] public int shootPower;
    }

    [CreateAssetMenu(fileName = "MatchConfig", menuName = "BattleBuck/Match Config")]
    public class MatchConfig : ScriptableObject
    {
        [Header("Players")]
        [Range(2, 20)]
        public int playerCount = 10;

        [Header("Player Configurations")]
        public PlayerConfig[] players = new PlayerConfig[]
        {
            new PlayerConfig { name = "Alpha",   health = 3, shootPower = 1 },
            new PlayerConfig { name = "Bravo",   health = 4, shootPower = 1 },
            new PlayerConfig { name = "Charlie", health = 2, shootPower = 2 },
            new PlayerConfig { name = "Delta",   health = 3, shootPower = 1 },
            new PlayerConfig { name = "Echo",    health = 5, shootPower = 1 },
            new PlayerConfig { name = "Foxtrot", health = 2, shootPower = 3 },
            new PlayerConfig { name = "Golf",    health = 4, shootPower = 2 },
            new PlayerConfig { name = "Hotel",   health = 3, shootPower = 1 },
            new PlayerConfig { name = "India",   health = 3, shootPower = 2 },
            new PlayerConfig { name = "Juliet",  health = 2, shootPower = 2 },
        };

        [Header("Movement")]
        [Min(0.5f)] public float moveSpeed = 2f;
        [Min(2f)] public float groundHalfSize = 8f;

        [Header("Combat")]
        [Min(1f)] public float killRange = 4f;
        [Min(0.2f)] public float shootInterval = 1f;
        [Min(2f)] public float projectileSpeed = 8f;

        [Header("Respawn")]
        [Min(0.5f)] public float respawnDelay = 3f;

        [Header("Match End Conditions")]
        [Min(10f)] public float matchDuration = 180f;
        [Min(1)] public int scoreLimit = 10;

        public PlayerConfig GetPlayerConfig(int index)
        {
            if (players != null && index < players.Length)
                return players[index];

            return new PlayerConfig
            {
                name = $"Player_{index}",
                health = 3,
                shootPower = 1
            };
        }
    }
}
