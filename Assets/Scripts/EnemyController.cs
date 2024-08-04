using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyController : EntityController
{
    public enum Behaviour
    {
        StandUntilAggroRange,
        MeanderUntilAggroRange,
        PatrolUntilAggroRange,
        ChaseUntilOutOfRange,
        ReturnToInitial
    }

    public EnemyStats Stats = new();
    public override EntityStats CommonStats => Stats;

    public EnemyStats.Archetype Archetype => Stats.EnemyArchetype;
    public EnemyStats.Attitude Attitude => Stats.EnemyAttitude;
    public Behaviour CurrentBehaviour = Behaviour.StandUntilAggroRange;

    private Vector3Int _aggroStartCellPosition;

    protected override void Start()
    {
        base.Start();

        DungeonGridController.RegisterEnemyController(this);
    }

    private void Update()
    {
        if (CommonStats.LastMovedTime == 0)
        {
            CommonStats.LastMovedTime = Time.time;
        }

        var moveRecoveryElapsed = Time.time - CommonStats.LastMovedTime;
        if (moveRecoveryElapsed >= CommonStats.MoveRecovery)
        {
            DoMoveStep();
        }
    }

    private void LateUpdate()
    {
        var colour = HasLineOfSightToCell(DungeonGridController.Player.CellPosition) ? Color.green : Color.red;
        Debug.DrawLine(transform.position, DungeonGridController.CellToWorldCentered(DungeonGridController.Player.CellPosition), colour);
    }

    // TODO: refactor to base class EntityController for reuse (ex: knockback cases)
    private void MoveToTarget(Vector3Int targetCell)
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

        if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: true))
        {
            SetCellPosition(newCellPosition);
        }
        else if (isXBiggerThanY && Mathf.Abs(delta.y) > 0) // Check if there is a Y direction component worth testing for movement
        {
            determinedDelta = new Vector3Int(0, (int)Mathf.Sign(delta.y));
            newCellPosition = CellPosition + determinedDelta;

            if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: true))
            {
                SetCellPosition(newCellPosition);
            }
        }
        else if (!isXBiggerThanY && Mathf.Abs(delta.x) > 0) // Check if there is a X direction component worth testing for movement
        {
            determinedDelta = new Vector3Int((int)Mathf.Sign(delta.x), 0);
            newCellPosition = CellPosition + determinedDelta;

            if (!DungeonGridController.IsCellPositionCollider(newCellPosition, includePlayer: true))
            {
                SetCellPosition(newCellPosition);
            }
        }
    }

    private void DoMoveStep()
    {
        var hasSightAggro = DistanceFromCellPosition(DungeonGridController.Player.CellPosition) <= Stats.AggroRange &&
                            HasLineOfSightToCell(DungeonGridController.Player.CellPosition);

        switch (CurrentBehaviour)
        {
            case Behaviour.MeanderUntilAggroRange: // TODO: implement different
            case Behaviour.PatrolUntilAggroRange: // TODO: implement different
            case Behaviour.StandUntilAggroRange:
                if (hasSightAggro && Attitude == EnemyStats.Attitude.Hostile)
                {
                    _aggroStartCellPosition = CellPosition;
                    CurrentBehaviour = Behaviour.ChaseUntilOutOfRange;
                    MoveToTarget(DungeonGridController.Player.CellPosition);
                }
                break;
            case Behaviour.ChaseUntilOutOfRange:
                var isInLeashRange = (CellPosition - _aggroStartCellPosition).magnitude <= Stats.LeashRange;
                if (DistanceFromCellPosition(DungeonGridController.Player.CellPosition) <= Stats.AggroRange || isInLeashRange)
                {
                    MoveToTarget(DungeonGridController.Player.CellPosition);
                }
                else
                {
                    CurrentBehaviour = Archetype switch
                    {
                        EnemyStats.Archetype.Guard => Behaviour.ReturnToInitial,
                        EnemyStats.Archetype.Wanderer => Behaviour.MeanderUntilAggroRange,
                        EnemyStats.Archetype.Patroller => Behaviour.PatrolUntilAggroRange,
                        _ => Behaviour.ReturnToInitial
                    };
                    DoMoveStep();
                }
                break;
            case Behaviour.ReturnToInitial:
                if (hasSightAggro && Attitude == EnemyStats.Attitude.Hostile)
                {
                    CurrentBehaviour = Behaviour.ChaseUntilOutOfRange;
                    DoMoveStep();
                }
                else
                {
                    MoveToTarget(_aggroStartCellPosition);
                    if (CellPosition == _aggroStartCellPosition)
                    {
                        CurrentBehaviour = Behaviour.StandUntilAggroRange;
                    }
                }
                break;
        }

        CommonStats.LastMovedTime = Time.time;
    }
}
