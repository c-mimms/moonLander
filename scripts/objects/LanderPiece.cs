using Godot;
using System;

public class LanderPiece : RigidBody2D
{
    private float width = 1f;
    private Vector2 start;
    private Vector2 end;
    private Color color;

    public LanderPiece()
    {

    }

    public override void _Ready()
    {
        base._Ready();
        //this.AddChild(GeneratePhysicsPoint(start));
        //this.AddChild(GeneratePhysicsPoint(end));
        this.AddChild(GeneratePhysicsSegment(start, end));
        Update();
    }


    public LanderPiece(Vector2 start, Vector2 end, Color color)
    {
        this.color = color;
        this.start = start;
        this.end = end;


        this.ContinuousCd = CCDMode.CastRay;
        CollisionLayer = 2;
    }

    private CollisionShape2D GeneratePhysicsSegment(Vector2 start, Vector2 end)
    {
        Vector2 dirVec = end - start;
        float recLen = dirVec.Length();
        float recRot = dirVec.Angle();
        CollisionShape2D collisionShape = new CollisionShape2D();

        RectangleShape2D circleShape = new RectangleShape2D();
        circleShape.Extents = new Vector2(recLen / 2, 0.5f);
        collisionShape = new CollisionShape2D
        {
            Shape = circleShape
        };
        collisionShape.Rotation = recRot;
        collisionShape.Position = start + dirVec / 2;

        return collisionShape;
    }

    private CollisionShape2D GeneratePhysicsPoint(Vector2 center)
    {
        CollisionShape2D collisionShape = new CollisionShape2D();
        CircleShape2D circleShape = new CircleShape2D();
        circleShape.Radius = 1;
        collisionShape = new CollisionShape2D
        {
            Shape = circleShape
        };

        collisionShape.Position = center;
        return collisionShape;
    }

    public override void _Draw()
    {
        DrawLine(start, end, color, width, true);
    }
}
