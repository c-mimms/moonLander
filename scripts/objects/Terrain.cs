using Godot;
using System;
using System.Collections.Generic;


//[Tool]
public class Terrain : StaticBody2D
{
    private Vector2[] terrainPoints;
    private int scale = 3;


    private float width = 1f;
    private Color color = new Color(1, 1, 1);


    //Landing zone generation params
    private float landingFrequency = 0.03f;
    private int landingSize = 3; //3 segments
    private List<LandingZone> landingZones = new List<LandingZone>();

    public override void _Ready()
    {
        Random random = new Random();

        int mapSize = 500;

        terrainPoints = new Vector2[mapSize];
        float startX = -300f;
        float start = 100f;
        double delta = 10;
        float maxHeight = 600f;

        for (int i = 0; i < terrainPoints.Length; i++)  
        {
            if ((float)random.NextDouble() < landingFrequency)
            {
                LandingZone landingZone = new LandingZone(new Vector2(scale * (startX + (i-1) * 10), scale * start -1), landingSize);
                GetParent().CallDeferred("add_child", landingZone);
                landingZones.Add(landingZone);

                for (int j = 0; j < landingSize && i < terrainPoints.Length ; j++, i++)
                {
                    terrainPoints[i] = new Vector2(scale * (startX + i * 10), scale * start);
                }
                i--;
            }
            else
            {
                delta = (float)random.NextDouble() * 20 - 10;
                if (random.Next(10) < 2)
                {
                    delta *= 6;
                }
                start = (float)(start + delta);
                if(start * scale > maxHeight)
                {
                    start = maxHeight / scale;
                }
                terrainPoints[i] = new Vector2(scale * (startX + i * 10), scale * start);
            }
        }


        CollisionShape2D[] collisionShapes = new CollisionShape2D[mapSize - 1];

        for (int i = 0; i < terrainPoints.Length - 1; i++)
        {
            Vector2[] points = new Vector2[4];
            Vector2 yOffset = new Vector2(0, 20);
            points[0] = terrainPoints[i];
            points[1] = terrainPoints[i + 1];
            points[2] = terrainPoints[i + 1] + yOffset;
            points[3] = terrainPoints[i] + yOffset;

            ConvexPolygonShape2D seg = new ConvexPolygonShape2D();
            seg.Points = points;

            collisionShapes[i] = new CollisionShape2D();
            collisionShapes[i].Shape = seg;
            this.AddChild(collisionShapes[i]);
        }

    }

    public override void _Draw()
    {
        DrawPolyline(terrainPoints, color, width, true);
    }
}
