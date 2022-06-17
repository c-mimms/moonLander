using Godot;
using System;
using System.Collections.Generic;

[Tool]

public class Lander : RigidBody2D
{
    private Camera2D cam;

    //Store the lines to draw
    private List<List<Vector2>> shuttle;
    private List<List<Vector2>> fire;
    private List<List<Vector2>> leftFire;
    private List<List<Vector2>> rightFire;

    private List<LanderPiece> pieces = new List<LanderPiece>();

    //Drawing settings
    //private Vector2 DrawingOffset = new Vector2(325, 170);
    private Vector2 DrawingOffset = new Vector2(19, 9);
    private int scale = 3;

    private float width = 1f;
    private Color white = new Color(1, 1, 1);
    private Color fireColor = new Color(1, 0, 0);
    private bool thrust_visible = false;
    private bool leftThrustVisible = false;
    private bool rightThrustVisible = false;
    
    //Physics settings
    private Vector2 _thrust = new Vector2(0, -250);
    private float _torque = 2000;


    //Historical physics 

    public float lastVelocity;
    public bool lastCollisionUpright = false;

    //Model properties
    public int Fuel { get; internal set; }
    public bool Crashed = false;
    public bool Landed = false;
    private int crashSpeed = 30;
    public int Height { get; internal set; }
    public int Score = 100000;
    public Lander()
    {
        GameManager.GetInstance().RegisterLander(this);
        shuttle = ShuttleLines(scale, DrawingOffset);
        fire = FireLines(scale, DrawingOffset);
        leftFire = LeftFireLines(scale, DrawingOffset);
        rightFire = RightFireLines(scale, DrawingOffset);
        if (!Engine.EditorHint)
        {
            GeneratePieces(shuttle);
        }
    }

    private void GeneratePieces(List<List<Vector2>> shuttle)
    {
        //For each line in the shuttle generate a piece we can break apart later
        for (int i = 0; i < shuttle.Count; i++)
        {
            for (int j = 0; j < shuttle[i].Count - 1; j++)
            {
                LanderPiece piece = new LanderPiece(shuttle[i][j], shuttle[i][j + 1], white);
                pieces.Add(piece);
            }
        }
    }

    public override void _Draw()
    {
        foreach (var line in shuttle)
        {
            DrawPolyline(line.ToArray(), white, width, true);
        }
        if (thrust_visible)
        {
            foreach (var line in fire)
            {
                DrawPolyline(line.ToArray(), fireColor, width, true);
            }
        }
        if (leftThrustVisible)
        {
            foreach (var line in leftFire)
            {
                DrawPolyline(line.ToArray(), white, width, true);
            }
        }

        if (rightThrustVisible)
        {
            foreach (var line in rightFire)
            {
                DrawPolyline(line.ToArray(), white, width, true);
            }
        }
    }

    public override void _Ready()
    {
        if (!Engine.EditorHint)
        {
            cam = (Camera2D)GetNode("Camera2D");

            //Set initial conditions
            LinearVelocity = new Vector2(100, 0);
            lastVelocity = LinearVelocity.Length();
            Fuel = 1000;
            Height = 100;
            Crashed = false;

        }
    }

    public override void _PhysicsProcess(float delta)
    {
        base._PhysicsProcess(delta);


        if (!Crashed && !Landed)
        {
            Score -= 1;
        }
        if (!Engine.EditorHint)
        {

            //Camera logic
            //if we're moving fast zoom out more, if we get close to the ground zoom in more
            //float speed = LinearVelocity.Length() / 100;
            //if (speed > 4)
            //{
            //	cam.Zoom = new Vector2(speed, speed);
            //}
            if (Input.IsActionPressed("restart"))
            {
                CleanUpOrphans();
                GD.Print("New game");
                GetTree().ReloadCurrentScene();
            }
            if (Input.IsActionPressed("quit"))
            {
                CleanUpOrphans();
                GC.Collect();
                GD.Print("Quit");
                GetTree().Quit();
            }
        }
    }

    private void CheckIfLanded()
    {
        // use global coordinates, not local to node
        var spaceState = GetWorld2d().DirectSpaceState;
        var collides_zone = spaceState.IntersectRay(Position, Position + new Vector2(0, 21), collideWithAreas: true, collideWithBodies: false);
        if (collides_zone.Count > 0)
        {
            object collided = collides_zone["collider"];
            GD.Print("Collided with " + collided.GetType().Name);

            //Ensure we haven't crashed or already landed, and speed is low
            if (collided is LandingZone zone && !Crashed && !Landed && LinearVelocity.Length() < 5)
            {
                Landed = true;
                Score = Score * zone.multiplier;
                EndGame();
            }
        }
    }

    private void SetDistanceAboveGround()
    {
        var spaceState = GetWorld2d().DirectSpaceState;


        var result = spaceState.IntersectRay(Position, Position + new Vector2(0, 2000), new Godot.Collections.Array { this });
        if (result.Count > 0)
        {
            Vector2 ground = (Vector2)result["position"];
            Vector2 heightVec = ground - Position;
            Height = ((int)heightVec.y) - 20;
        }
    }

    private void CleanUpOrphans()
    {
        foreach(var piece in pieces)
        {
            piece.Free();
        }
    }

    public override void _IntegrateForces(Physics2DDirectBodyState state)
    {
        if (!Engine.EditorHint)
        {
            //Measure distance above surface
            SetDistanceAboveGround();

            //Check if we're in landing zone
            CheckIfLanded();

            if (Input.IsActionPressed("up") && Fuel > 0)
            {
                AppliedForce = _thrust.Rotated(Rotation);
                thrust_visible = true;
                Fuel -= 1;
                Update();
            }
            else
            {
                AppliedForce = new Vector2();
                thrust_visible = false;
                Update();
            }

            var rotationDir = 0;
        
            if (Input.IsActionPressed("right") && Fuel > 0) {
                leftThrustVisible = true;
                rotationDir += 1;
                Fuel -= 1;
                Update();
            } else {
                leftThrustVisible = false;
                Update();
            }

            if (Input.IsActionPressed("left") && Fuel > 0) {
                rightThrustVisible = true;
                Fuel -= 1;
                rotationDir -= 1;
                Update();
            } else
            {
                rightThrustVisible= false;
                Update();
            }

            this.AppliedTorque = rotationDir * _torque;


            //We don't need to check collisions if we just trust the physics engine and check if our speed changed by a lot
            //But we're checking them here so we can tell the relative between the ship and the ground
            //if (GetCollidingBodies().Count > 0)
            //{
            //    //GD.Print("Im touching bodies : " + state.GetContactCount() + " and " + GetCollidingBodies().Count);
            //    if (GetCollidingBodies()[0] is Terrain terrain)
            //    {
            //        if (state.GetContactCount() > 0)
            //        {
            //            Vector2 normal = state.GetContactLocalNormal(0);
            //            //GD.Print("Angle " + toDeg(normal.Angle()));

            //            //Keep track of the angle of our last collision
            //            lastCollisionUpright = ShipIsUpright(normal);
            //        }
            //    }
            //}

            if (state.GetContactCount() > 0)
            {
                Vector2 normal = state.GetContactLocalNormal(0);
                //GD.Print("Angle " + toDeg(normal.Angle()));

                //Keep track of the angle of our last collision
                lastCollisionUpright = ShipIsUpright(normal);
            }


            //Collision logic
            var newVelocity = LinearVelocity.Length();
            var speedChange = lastVelocity - newVelocity;

            //Hitting the feet straight on is stronger than angled
            if (lastCollisionUpright) {
                if (speedChange > 60)
                {
                    GD.Print("Using higher crash speed.");
                    Crash();
                }
            }
            else
            {
                if (speedChange > 30)
                {
                    GD.Print("Using lower crash speed.");
                    Crash();
                }
            }
            lastVelocity = newVelocity;

        }
    }

    //Not using signals anymore for detecting collisions with terrain

    //public void _on_body_entered(Node other)
    //{
    //    if (other is Terrain terrain)
    //    {
    //        if (LinearVelocity.Length() > crashSpeed || !ShipIsUpright() )
    //        {
    //            if (!Crashed)
    //            {
    //                //Crash();
    //            }
    //        }
    //    }
    //}

    //Check if the ship is within some degrees of a normal, default of up
    private bool ShipIsUpright(Vector2? normal = null)
    {
        var effectiveNormal = normal ?? Vector2.Up;
        var normalVectorAngleOffset = toDeg(effectiveNormal.AngleTo(Vector2.Up));

        //GD.Print("Normal is " + normalVectorAngleOffset + " collision angle is : " + Math.Abs(RotationDegrees + normalVectorAngleOffset));

        if(Math.Abs(RotationDegrees + normalVectorAngleOffset) > 70 )
        {
            return false;
        }   
        return true;
    }

    private double toDeg(float rad)
    {
        return rad * (180 / Math.PI);
    }

    private void Crash()
    {
        if (!Crashed)
        {
            Crashed = true;

            //Make myself invisible
            Visible = false;

            //Spawn an explosion object
            SpawnLanderPieces(new Vector2(0, 0));

            //Show game over screen
            GameManager.GetInstance().GameOver();
        }
    }

    private void EndGame()
    {
        GameManager.GetInstance().GameOver();
        //SetPhysicsProcess(false);
    }

    private void SpawnLanderPieces(Vector2 offset)
    {
        Vector2 offsetPosition = Position - offset;
        foreach (var piece in pieces)
        {
            piece.Position = offsetPosition;
            piece.Rotation = Rotation;
            piece.LinearVelocity = LinearVelocity;

            //piece.ApplyImpulse(LinearVelocity.Normalized()* 15, LinearVelocity.Normalized().Rotated(3.14f) * 300);
            
            //piece.AngularVelocity = AngularVelocity;
            Node parent = GetParent();
            parent.CallDeferred("add_child", piece);
        }
    }

    List<List<Vector2>> FireLines(int scale, Vector2 offset)
    {
        String fireStr = "6,9:5,11: :7,9:8,11: :6.5,9:7,13: :6.5,9:6,13";
        List<List<Vector2>> fire = ParseMultilineStringToLines(fireStr);

        for (int i = 0; i < fire.Count; i++)
        {
            for (int j = 0; j < fire[i].Count; j++)
            {
                fire[i][j] = fire[i][j] * scale - offset;
            }
        }

        return fire;
    }

    List<List<Vector2>> LeftFireLines(int scale, Vector2 offset)
    {
        String fireStr = "6,9:5,11: :7,9:8,11: :6.5,9:7,13: :6.5,9:6,13";
        List<List<Vector2>> fire = ParseMultilineStringToLines(fireStr);

        for (int i = 0; i < fire.Count; i++)
        {
            for (int j = 0; j < fire[i].Count; j++)
            {
                fire[i][j] = (fire[i][j] * scale - offset + new Vector2(-5,-5)).Rotated((float) Math.PI / 2.0f);
            }
        }

        return fire;
    }

    List<List<Vector2>> RightFireLines(int scale, Vector2 offset)
    {
        String fireStr = "6,9:5,11: :7,9:8,11: :6.5,9:7,13: :6.5,9:6,13";
        List<List<Vector2>> fire = ParseMultilineStringToLines(fireStr);

        for (int i = 0; i < fire.Count; i++)
        {
            for (int j = 0; j < fire[i].Count; j++)
            {
                fire[i][j] = (fire[i][j] * scale - offset + new Vector2(5,-5)).Rotated((float) -Math.PI / 2.0f);
            }
        }

        return fire;
    }

    List<List<Vector2>> ShuttleLines(int scale, Vector2 offset)
    {
        String hexagon = "3,2:5,0:8,0:10,2:10,5:8,7:5,7:3,5:3,2";

        String engineStr = "6,7:7,7:8,9:5,9:6,7";

        String legStr = "0,10:2,10: :1,10:2,7: :2,7:3,5: :2,7:5,7: :10,5:11,7: :11,7:12,10: :8,7:11,7: :11,10:13,10";

        List<Vector2> shuttleBody = ParseMultilineStringToVector(hexagon);
        List<Vector2> engine = ParseMultilineStringToVector(engineStr);
        List<List<Vector2>> legs = ParseMultilineStringToLines(legStr);

        legs.Add(engine);
        legs.Add(shuttleBody);

        for (int i = 0; i < legs.Count; i++)
        {
            for (int j = 0; j < legs[i].Count; j++)
            {
                legs[i][j] = legs[i][j] * scale - offset;
            }
        }

        return legs;
    }

    private List<List<Vector2>> ParseMultilineStringToLines(string multString)
    {
        List<List<Vector2>> listList = new List<List<Vector2>>();


        List<Vector2> vecList = new List<Vector2>();
        var lines = multString.Split(':');
        foreach (String line in lines)
        {
            if (line.Trim().Length > 0)
            {
                var vect = ParseStringToVector(line);
                vecList.Add(vect);
            }
            else
            {
                listList.Add(vecList);
                vecList = new List<Vector2>();
            }
        }
        listList.Add(vecList);
        return listList;
    }

    List<Vector2> ParseMultilineStringToVector(String multString)
    {
        List<Vector2> vecList = new List<Vector2>();

        var lines = multString.Split(':');

        foreach (String line in lines)
        {
            var vect = ParseStringToVector(line);
            vecList.Add(vect);
        }
        return vecList;
    }

    //String formatted like "2,2"
    Vector2 ParseStringToVector(String vecString)
    {
        string[] split = vecString.Split(',');
        return new Vector2((float)Double.Parse(split[0]), (float)Double.Parse(split[1]));
    }
}
