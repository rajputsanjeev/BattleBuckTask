using System.Collections;
using UnityEngine;

namespace BattleBuck.Utils
{
    public class ProjectilePool : MonoBehaviour
    {
        public static ProjectilePool Instance { get; private set; }

        private GameObject[] _pool;
        private int _poolSize;
        private float _projectileSpeed;
        private bool _initialized;

        public void Initialize(int size, float scale, Color color, float speed)
        {
            Instance = this;
            _poolSize = size;
            _projectileSpeed = speed;
            _pool = new GameObject[size];

            for (int i = 0; i < size; i++)
            {
                GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"Projectile_{i}";
                sphere.transform.SetParent(transform);
                sphere.transform.localScale = Vector3.one * scale;

                Destroy(sphere.GetComponent<Collider>());
                sphere.GetComponent<Renderer>().material.color = color;

                sphere.SetActive(false);
                _pool[i] = sphere;
            }

            _initialized = true;
        }

        public void FireProjectile(Vector3 from, Vector3 to)
        {
            GameObject proj = Get();
            if (proj != null)
            {
                StartCoroutine(AnimateProjectile(proj, from, to));
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private GameObject Get()
        {
            if (!_initialized) return null;

            for (int i = 0; i < _poolSize; i++)
            {
                if (!_pool[i].activeInHierarchy)
                {
                    _pool[i].SetActive(true);
                    return _pool[i];
                }
            }
            return null;
        }

        private IEnumerator AnimateProjectile(GameObject proj, Vector3 from, Vector3 to)
        {
            from.y = 1f;
            to.y = 1f;

            float dist = Vector3.Distance(from, to);
            float duration = dist / _projectileSpeed;
            float elapsed = 0f;

            proj.transform.position = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                proj.transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }

            proj.SetActive(false);
        }
    }
}
