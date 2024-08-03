public class EnemyController : EntityController
{
    protected override void Start()
    {
        base.Start();

        DungeonGridController.RegisterEnemyController(this);
    }
}
