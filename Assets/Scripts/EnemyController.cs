using UnityEngine;

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

        CorrectBehaviourFromArchetype();
        DungeonGridController.RegisterEnemyController(this);
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        CorrectBehaviourFromArchetype();
    }

    private void CorrectBehaviourFromArchetype()
    {
        switch (Archetype)
        {
            case EnemyStats.Archetype.Guard:
                if (CurrentBehaviour == Behaviour.MeanderUntilAggroRange || CurrentBehaviour == Behaviour.PatrolUntilAggroRange)
                {
                    CurrentBehaviour = Behaviour.StandUntilAggroRange;
                }
                break;
            case EnemyStats.Archetype.Wanderer:
                if (CurrentBehaviour == Behaviour.StandUntilAggroRange || CurrentBehaviour == Behaviour.PatrolUntilAggroRange)
                {
                    CurrentBehaviour = Behaviour.MeanderUntilAggroRange;
                }
                break;
            case EnemyStats.Archetype.Patroller:
                if (CurrentBehaviour == Behaviour.StandUntilAggroRange || CurrentBehaviour == Behaviour.MeanderUntilAggroRange)
                {
                    CurrentBehaviour = Behaviour.PatrolUntilAggroRange;
                }
                break;
        }
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


    private void DoMoveStep()
    {
        var hasSightAggro = DistanceFromCellPosition(DungeonGridController.Player.CellPosition) <= Stats.AggroRange &&
                            HasLineOfSightToCell(DungeonGridController.Player.CellPosition);

        switch (CurrentBehaviour)
        {
            case Behaviour.PatrolUntilAggroRange: // TODO: implement different
            case Behaviour.StandUntilAggroRange:
                if (hasSightAggro && Attitude == EnemyStats.Attitude.Hostile)
                {
                    _aggroStartCellPosition = CellPosition;
                    CurrentBehaviour = Behaviour.ChaseUntilOutOfRange;
                    MoveToTarget(DungeonGridController.Player.CellPosition);
                }
                break;
            case Behaviour.MeanderUntilAggroRange:
                if (hasSightAggro && Attitude == EnemyStats.Attitude.Hostile)
                {
                    CurrentBehaviour = Behaviour.ChaseUntilOutOfRange;
                    MoveToTarget(DungeonGridController.Player.CellPosition);
                }
                else
                {
                    MoveRandomDirection();
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
