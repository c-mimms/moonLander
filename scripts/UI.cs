using Godot;
using System;

public class UI : Control
{
	Lander lander;

	//UI Variables
	String velocity;
	String fuel;
	String score;
	String time;
	int height;
	bool gameStart = true;
	bool gameOver = false;

	//Drawing variables
	private DynamicFont font = new DynamicFont();
	private Vector2 textOffset = new Vector2(50, 50);

	private Vector2 centerTextOffset = new Vector2(500, 500);
	private Vector2 textHeight = new Vector2(0, 70);

	public UI()
    {
		GameManager.GetInstance().RegisterUI(this);
    }

    public override void _Ready()
    {
        lander = GameManager.GetInstance().GetLander();
		font.FontData = (DynamicFontData)GD.Load("res://fonts/vector.ttf");
		font.Size = 30;
	}
	public override void _Draw()
	{
		String scoreText = "Score : " + score;

		if (gameOver)
		{
			String gameOverText = "Game Over!";

			DrawString(font, centerTextOffset, gameOverText);
			DrawString(font, centerTextOffset + textHeight, scoreText);
		} else
		{
			String velocityText = "Current Velocity : " + velocity;
			String fuelText = "Fuel Left : " + fuel;
			String heightText = "Height : " + height;

			DrawString(font, textOffset, velocityText);
			DrawString(font, textOffset + textHeight, fuelText);
			DrawString(font, textOffset + textHeight * 2, heightText);
			DrawString(font, textOffset + textHeight * 3, scoreText);
		}
	}

    internal void GameOver()
    {
        gameOver = true;
    }

    public override void _Process(float delta)
    {
        base._Process(delta);

		velocity = lander.LinearVelocity.Length().ToString("0");
		fuel = lander.Fuel.ToString();
		height = lander.Height;
		score = lander.Score.ToString();

		Update();
    }
}
