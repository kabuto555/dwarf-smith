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
    private int _patrolPathIndexTarget;
    private bool _isPatrolPathTraversalForward = true;

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

    protected override void Update()
    {
        base.Update();

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

    private void TargetNextPatrolPathPoint()
    {
        var pathPointCount = Stats.PatrolPath.Count;
        _patrolPathIndexTarget = Mathf.Clamp(_patrolPathIndexTarget, 0, pathPointCount - 1);

        if (Stats.PatrolPingPong)
        {
            if (_isPatrolPathTraversalForward && _patrolPathIndexTarget == pathPointCount - 1)
            {
                _isPatrolPathTraversalForward = false;
            }
            else if (!_isPatrolPathTraversalForward && _patrolPathIndexTarget == 0)
            {
                _isPatrolPathTraversalForward = true;
            }
        }

        _patrolPathIndexTarget = (_patrolPathIndexTarget + (_isPatrolPathTraversalForward ? 1 : -1) + pathPointCount) % pathPointCount;
    }

    private void DoMoveStep()
    {
        var hasSightAggro = DistanceFromCellPosition(DungeonGridController.Player.CellPosition) <= Stats.AggroRange &&
                            HasLineOfSightToCell(DungeonGridController.Player.CellPosition);

        switch (CurrentBehaviour)
        {
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
            case Behaviour.PatrolUntilAggroRange:
                if (hasSightAggro && Attitude == EnemyStats.Attitude.Hostile)
                {
                    CurrentBehaviour = Behaviour.ChaseUntilOutOfRange;
                    MoveToTarget(DungeonGridController.Player.CellPosition);
                }
                else if (Stats.PatrolPath.Count > 0)
                {
                    var targetCellPosition = Stats.PatrolPath[_patrolPathIndexTarget];
                    if (CellPosition == targetCellPosition)
                    {
                        TargetNextPatrolPathPoint();
                        targetCellPosition = Stats.PatrolPath[_patrolPathIndexTarget];
                    }
                    MoveToTarget(targetCellPosition);
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

    private void OnDrawGizmosSelected()
    {
        if (Stats.PatrolPath.Count == 0)
        {
            return;
        }

        var startColor = new Color(1.0f, 1.0f, 0.0f, 0.5f);
        var mainColor = Stats.PatrolPingPong ? new Color(1.0f, 0.0f, 0.5f, 0.5f) : new Color(0.0f, 1.0f, 1.0f, 0.5f);
        Gizmos.color = startColor;

        var cellSize = DungeonGridController.GridLayout.cellSize;
        var radius = (cellSize.x + cellSize.y) * 0.25f; // 0.25 because divide by 2 for average and divide by 2 again for changing diameter to radius
        Vector3 previousPoint = DungeonGridController.CellToWorldCentered(Stats.PatrolPath[^1]);

        for (int i = 0; i < Stats.PatrolPath.Count; i++)
        {
            var pathCell = Stats.PatrolPath[i];
            var point = DungeonGridController.CellToWorldCentered(pathCell);

            Gizmos.color = i == 0 ? startColor : mainColor;
            Gizmos.DrawSphere(point, radius);

            if (Stats.PatrolPingPong && i == 1)
            {
                Gizmos.color = startColor;
            }
            else if (!Stats.PatrolPingPong && i == 1)
            {
                Gizmos.color = startColor;
            }
            else
            {
                Gizmos.color = mainColor;
            }

            if (!Stats.PatrolPingPong || i > 0)
            {
                Gizmos.DrawLine(previousPoint, point);
            }

            previousPoint = point;
        }
    }
}
