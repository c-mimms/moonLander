﻿using Godot;
using System;

class LandingZone : Area2D
{
    private Vector2 leftExtent;
    private int scale;
    private Color color = new Color(0, 1, 0);
    //Drawing variables
    private DynamicFont font = new DynamicFont();

    private int multiplier;
    private readonly int[] multipliers = { 1, 1, 2, 2, 3, 4, 5, 6, 8, 10 };

    public LandingZone()
    {
    }

    public LandingZone(Vector2 pos, int scale)
    {
        this.leftExtent = pos;
        Position = pos;
        this.scale = scale;

        //Max multiplier 10?
        Random random = new Random();
        
        //from 0 - 5000 should be mostly 5x and under
        if(pos.x < 5000)
        {
            multiplier = multipliers[random.Next(5)];
        } else
        {
            multiplier = multipliers[random.Next(10)];
        }
    }

    public override void _Ready()
    {
        base._Ready();
        font.FontData = (DynamicFontData)GD.Load("res://fonts/vector.ttf");
        font.Size = 20;
        CollisionShape2D zone = new CollisionShape2D();
        CircleShape2D zoneArea = new CircleShape2D();
        zoneArea.Radius = 25;
        zone.Position = new Vector2(45f, 0);
        zone.Shape = zoneArea;
        this.Connect("body_entered", this, "NotifyLander");
        this.CallDeferred("add_child", zone);
        Monitoring = true;
    }

    public void NotifyLander(Node node)
    {
        if (node is Lander lander)
        {
            GD.Print(lander.LinearVelocity.Length());
            if ( lander.LinearVelocity.Length() < 5)
            {
                GD.Print("Lander has landed!");
                lander.Landed = true;
            }
        }
    }
    public override void _Draw()
    {
        DrawLine(new Vector2(0,0), new Vector2(scale * 3 * 10, 0), color, 1f);
        DrawString(font, new Vector2(15, 20), "x " + multiplier, color);
    }
}