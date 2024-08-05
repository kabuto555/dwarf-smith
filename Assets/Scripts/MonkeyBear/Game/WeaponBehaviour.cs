using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonkeyBear.Game
{
    public class WeaponBehaviour : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer SpriteRenderer;
        [SerializeField] private Collider2D Collider;

        public Faction Faction;
        public bool IsAttackFrameValid { get; set; }

        private Action<EntityController> _onWeaponCollide;

        private readonly List<Collider2D> _collidersList = new();
        private ContactFilter2D _contactFilter;

        private readonly HashSet<EntityController> _hitEntities = new();

        private void Start()
        {
            _contactFilter = new ContactFilter2D
            {
                layerMask = LayerMask.NameToLayer("DamageReceiver")
            };
        }

        private void Update()
        {
            if (!IsAttackFrameValid)
            {
                return;
            }

            Collider.OverlapCollider(_contactFilter, _collidersList);
            foreach (var otherCollider in _collidersList)
            {
                var entity = otherCollider.gameObject.GetComponent<EntityController>();
                if (entity.Faction != Faction && !_hitEntities.Contains(entity))
                {
                    _hitEntities.Add(entity);
                    _onWeaponCollide?.Invoke(entity);
                }
            }
        }

        public void StartAttack(Action<EntityController> weaponCollideCallback)
        {
            _hitEntities.Clear();

            _onWeaponCollide = null;
            _onWeaponCollide += weaponCollideCallback;
        }
    }
}
