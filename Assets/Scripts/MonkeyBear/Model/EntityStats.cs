using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EntityStats
{
    [Tooltip("Time elapsed before entity can move again")]
    public float MoveRecovery;
    [NonSerialized]
    public float LastMovedTime;
}

[Serializable]
public class EnemyStats : EntityStats
{
    public enum Archetype
    {
        Guard,
        Wanderer,
        Patroller
    }

    public enum Attitude
    {
        Friendly,
        Neutral,
        Hostile
    }

    public Archetype EnemyArchetype = Archetype.Guard;
    public Attitude EnemyAttitude = Attitude.Hostile;

    [Tooltip("Maximum cell distance the player is when the enemy begins chase")]
    public int AggroRange = 4;
    [Tooltip("This cell distance + AggroRange is how far the enemy is from original aggro location before giving up chase (unless player is still in aggro range)")]
    public int ChaseRange = 2;
    [Tooltip("The predetermined path points that patrolling entity will aim to take")]
    public List<Vector3Int> PatrolPath = new();
    [Tooltip("If true, patrol path will reverse once reaching the end, otherwise, reaching the end will loop to the starting point")]
    public bool PatrolPingPong;

    public int LeashRange => AggroRange + ChaseRange;
}
