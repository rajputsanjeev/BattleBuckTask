using BattleBuck.Core;
using BattleBuck.UI;
using BattleBuck.Utils;
using UnityEngine;

namespace BattleBuck.Match
{
    /// <summary>
    /// Runtime-only bootstrapper.
    /// Spawns player visuals and projectile pool at play time.
    /// Camera, lighting, ground, and UI are set up once via BattleBuck > Setup Match Scene.
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private MatchConfig _config;
        [SerializeField] private Color[] _playerColors = new Color[]
        {
            new Color(0.92f, 0.30f, 0.34f),
            new Color(0.20f, 0.67f, 0.86f),
            new Color(0.55f, 0.82f, 0.38f),
            new Color(0.95f, 0.77f, 0.25f),
            new Color(0.70f, 0.45f, 0.85f),
            new Color(0.98f, 0.55f, 0.30f),
            new Color(0.40f, 0.85f, 0.75f),
            new Color(0.85f, 0.45f, 0.65f),
            new Color(0.50f, 0.60f, 0.90f),
            new Color(0.75f, 0.75f, 0.40f),
        };

        private void Awake()
        {
            TimeFormatter.Initialize(Mathf.CeilToInt(_config.matchDuration) + 1);
            SpawnPlayerVisuals();
            SetupProjectilePool();
        }

        private void SpawnPlayerVisuals()
        {
            int count = _config.playerCount;
            float halfSize = _config.groundHalfSize;

            for (int i = 0; i < count; i++)
            {
                PlayerConfig pc = _config.GetPlayerConfig(i);

                float x = Random.Range(-halfSize, halfSize);
                float z = Random.Range(-halfSize, halfSize);
                Vector3 pos = new Vector3(x, 0.75f, z);

                GameObject capsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                capsule.name = $"Player_{i}_{pc.name}";
                capsule.transform.position = pos;
                capsule.transform.localScale = new Vector3(0.6f, 0.75f, 0.6f);

                Color color = _playerColors[i % _playerColors.Length];
                capsule.GetComponent<Renderer>().material.color = color;

                Destroy(capsule.GetComponent<Collider>());

                PlayerVisual visual = capsule.AddComponent<PlayerVisual>();
                visual.Initialize(i, pc.name, color, pos, pc.health);
            }
        }

        private void SetupProjectilePool()
        {
            GameObject poolObj = new GameObject("ProjectilePool");
            ProjectilePool pool = poolObj.AddComponent<ProjectilePool>();
            pool.Initialize(20, 0.15f, new Color(1f, 0.85f, 0.2f), _config.projectileSpeed);
        }
    }
}
