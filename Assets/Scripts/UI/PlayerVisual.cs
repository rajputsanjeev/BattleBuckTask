using System.Collections;
using BattleBuck.Core;
using BattleBuck.Utils;
using TMPro;
using UnityEngine;

namespace BattleBuck.UI
{
    public class PlayerVisual : MonoBehaviour
    {
        private int _playerId;
        private Renderer _renderer;
        private Color _aliveColor;
        private Vector3 _originalScale;
        private TextMeshPro _nameLabel;
        private GameObject[] _healthPips;
        private int _maxHealth;
        private Vector3 _targetPosition;
        private bool _needsMove;
        private float _moveSpeed = 6f;

        public void Initialize(int playerId, string playerName, Color color, Vector3 position, int maxHealth)
        {
            _playerId = playerId;
            _aliveColor = color;
            _maxHealth = maxHealth;
            _renderer = GetComponentInChildren<Renderer>();
            _targetPosition = position;
            _needsMove = false;

            transform.position = position;
            _originalScale = transform.localScale;

            if (_renderer != null)
            {
                _renderer.material.color = _aliveColor;
            }

            GameObject labelObj = new GameObject("NameLabel");
            labelObj.transform.SetParent(transform);
            labelObj.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            labelObj.transform.localScale = Vector3.one * 2f;
            _nameLabel = labelObj.AddComponent<TextMeshPro>();
            _nameLabel.text = playerName;
            _nameLabel.fontSize = 3f;
            _nameLabel.alignment = TextAlignmentOptions.Center;
            _nameLabel.color = Color.white;

            CreateHealthPips(maxHealth);

            GameEvents.OnPlayerKilled += HandlePlayerKilled;
            GameEvents.OnPlayerRespawned += HandlePlayerRespawned;
            GameEvents.OnPlayerPositionChanged += HandlePositionChanged;
            GameEvents.OnPlayerHealthChanged += HandleHealthChanged;
            GameEvents.OnProjectileFired += HandleProjectileFired;
        }

        private void OnDestroy()
        {
            GameEvents.OnPlayerKilled -= HandlePlayerKilled;
            GameEvents.OnPlayerRespawned -= HandlePlayerRespawned;
            GameEvents.OnPlayerPositionChanged -= HandlePositionChanged;
            GameEvents.OnPlayerHealthChanged -= HandleHealthChanged;
            GameEvents.OnProjectileFired -= HandleProjectileFired;
        }

        private void Update()
        {
            if (!_needsMove) return;

            transform.position = Vector3.MoveTowards(
                transform.position, _targetPosition, _moveSpeed * Time.deltaTime);

            if (Vector3.SqrMagnitude(transform.position - _targetPosition) < 0.001f)
            {
                transform.position = _targetPosition;
                _needsMove = false;
            }
        }

        private void CreateHealthPips(int count)
        {
            _healthPips = new GameObject[count];
            float totalWidth = (count - 1) * 0.25f;
            float startX = -totalWidth / 2f;

            for (int i = 0; i < count; i++)
            {
                GameObject pip = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pip.name = $"HP_{i}";
                pip.transform.SetParent(transform);
                pip.transform.localPosition = new Vector3(startX + i * 0.25f, 1.4f, 0f);
                pip.transform.localScale = new Vector3(0.15f, 0.15f, 0.15f);
                pip.GetComponent<Renderer>().material.color = Color.green;
                Destroy(pip.GetComponent<Collider>());
                _healthPips[i] = pip;
            }
        }

        private void HandlePositionChanged(int playerId, Vector3 newPos)
        {
            if (playerId != _playerId) return;
            _targetPosition = newPos;
            _needsMove = true;
        }

        private void HandleHealthChanged(int playerId, int currentHP, int maxHP)
        {
            if (playerId != _playerId) return;

            for (int i = 0; i < _healthPips.Length; i++)
            {
                if (i < currentHP)
                {
                    _healthPips[i].SetActive(true);
                    _healthPips[i].GetComponent<Renderer>().material.color = Color.green;
                }
                else
                {
                    _healthPips[i].SetActive(false);
                }
            }
        }

        private void HandlePlayerKilled(int killerId, int victimId)
        {
            if (victimId == _playerId)
            {
                gameObject.SetActive(false);
            }
            else if (killerId == _playerId && gameObject.activeInHierarchy)
            {
                StartCoroutine(KillPunch());
            }
        }

        private void HandlePlayerRespawned(int playerId)
        {
            if (playerId != _playerId) return;

            gameObject.SetActive(true);

            transform.localScale = _originalScale;
            if (_renderer != null)
                _renderer.material.color = _aliveColor;

            if (_nameLabel != null)
                _nameLabel.alpha = 1f;

            for (int i = 0; i < _healthPips.Length; i++)
                _healthPips[i].SetActive(true);
        }

        private void HandleProjectileFired(int shooterId, int targetId, Vector3 from, Vector3 to)
        {
            if (shooterId != _playerId) return;

            if (ProjectilePool.Instance != null)
            {
                ProjectilePool.Instance.FireProjectile(from, to);
            }
        }

        private IEnumerator KillPunch()
        {
            Vector3 punched = _originalScale * 1.3f;
            transform.localScale = punched;
            yield return new WaitForSeconds(0.15f);
            if (this != null)
                transform.localScale = _originalScale;
        }
    }
}
