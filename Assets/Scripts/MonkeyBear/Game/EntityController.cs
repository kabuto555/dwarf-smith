using System;
using System.Collections.Generic;
using MonkeyBear.Model;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MonkeyBear.Game
{
    public class EntityController : MonoBehaviour
    {
        public const string AnimParamIsAttacking = "IsAttacking";
        public const string AnimParamIsFrameAttackValid = "IsFrameAttackValid";
        protected const string AnimParamAim = "Aim";
        protected const string AnimParamRSwing = "R_Swing";

        [SerializeField] protected Animator Animator;

        public Faction Faction;
        public virtual EntityStats CommonStats { get; set; }
        public Vector3Int CellPosition;
        public bool DebugSnapToGrid;

        protected DungeonGridController DungeonGridController => DungeonGridController.Instance != null ? DungeonGridController.Instance : FindObjectOfType<DungeonGridController>();
        protected GridLayout GridLayout => DungeonGridController.GridLayout;
        protected WeaponBehaviour AttackingWeapon;

        protected virtual void Start()
        {
            SetCellPosition(GridLayout.WorldToCell(transform.position));
        }

        public void SetCellPosition(Vector3Int newCellPosition)
        {
            CellPosition = new Vector3Int(newCellPosition.x, newCellPosition.y, CellPosition.z);
            SnapToCurrentCellPosition();
        }

        private void SnapToCurrentCellPosition()
        {
            transform.position = DungeonGridController.CellToWorldCentered(CellPosition);
        }

        public float DistanceFromCellPosition(Vector3Int otherCell)
        {
            var diffVector = otherCell - CellPosition;
            return new Vector2Int(diffVector.x, diffVector.y).magnitude;
        }

        public bool HasLineOfSightToCell(Vector3Int otherCell)
        {
            var raycastHit2D = Physics2D.Linecast(transform.position, DungeonGridController.CellToWorldCentered(otherCell), DungeonGridController.FieldOfViewLayerMask);
            return raycastHit2D.collider == null;
        }

        public void MoveToTarget(Vector3Int targetCell, bool collidesWithPlayer = true, bool collidesWithEnemies = true)
        {
            var delta = targetCell - CellPosition;
            var isXBiggerThanY = Mathf.Abs(delta.x) != Mathf.Abs(delta.y) ? Mathf.Abs(delta.x) >= Mathf.Abs(delta.y) : Random.value > 0.5f;
            Vector3Int determinedDelta;

            if (isXBiggerThanY)
            {
                determinedDelta = new Vector3Int((int)Mathf.Sign(delta.x), 0);
            }
            else
            {
                determinedDelta = new Vector3Int(0, (int)Mathf.Sign(delta.y));
            }

            var newCellPosition = CellPosition + determinedDelta;

            if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: collidesWithPlayer, includeEnemies: collidesWithEnemies))
            {
                SetCellPosition(newCellPosition);
            }
            else if (isXBiggerThanY && Mathf.Abs(delta.y) > 0) // Check if there is a Y direction component worth testing for movement
            {
                determinedDelta = new Vector3Int(0, (int)Mathf.Sign(delta.y));
                newCellPosition = CellPosition + determinedDelta;

                if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: collidesWithPlayer, includeEnemies: collidesWithEnemies))
                {
                    SetCellPosition(newCellPosition);
                }
            }
            else if (!isXBiggerThanY && Mathf.Abs(delta.x) > 0) // Check if there is a X direction component worth testing for movement
            {
                determinedDelta = new Vector3Int((int)Mathf.Sign(delta.x), 0);
                newCellPosition = CellPosition + determinedDelta;

                if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: collidesWithPlayer, includeEnemies: collidesWithEnemies))
                {
                    SetCellPosition(newCellPosition);
                }
            }
        }

        public void MoveRandomDirection(bool collidesWithPlayer = true, bool collidesWithEnemies = true)
        {
            var adjacentCells = new List<Vector3Int>
            {
                CellPosition + Vector3Int.up,
                CellPosition + Vector3Int.down,
                CellPosition + Vector3Int.left,
                CellPosition + Vector3Int.right
            };

            var freeCells = new List<Vector3Int>(4);
            foreach (var cell in adjacentCells)
            {
                if (!DungeonGridController.IsCellPositionCollider(cell, collidesWithPlayer, collidesWithEnemies))
                {
                    freeCells.Add(cell);
                }
            }

            if (freeCells.Count == 0)
            {
                return;
            }

            var randomIndex = Random.Range(0, freeCells.Count);
            SetCellPosition(freeCells[randomIndex]);
        }

        public void OnReceivedDamage(DamagePayload damagePayload)
        {
            GetComponent<SpriteRenderer>().color = Random.ColorHSV();
        }

        protected void PerformWeaponAttack(WeaponBehaviour weaponBehaviour, Action<EntityController> onHitCallback)
        {
            AttackingWeapon = weaponBehaviour;
            weaponBehaviour.StartAttack(onHitCallback);
            Animator.SetTrigger(AnimParamRSwing);
        }

        protected virtual void OnActiveAttackFramesStarted()
        {
            if (AttackingWeapon != null)
            {
                AttackingWeapon.IsAttackFrameValid = true;
            }
        }

        protected virtual void OnActiveAttackFramesEnded()
        {
            if (AttackingWeapon != null)
            {
                AttackingWeapon.IsAttackFrameValid = false;
            }
        }

        protected virtual void OnRecoveryExit()
        {
            AttackingWeapon = null;
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnValidate()
        {
            if (DebugSnapToGrid)
            {
                DebugSnapToGrid = false;
                SnapToCurrentCellPosition();
            }
        }
    }
}
