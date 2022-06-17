using System;

public class GameManager
{
    static GameManager instance;

    Lander lander;
    private UI ui;

    public static GameManager GetInstance()
    {
        if(instance == null)
        {
            instance = new GameManager();
        }
        return instance;
    }

    public void RegisterLander(Lander lander)
    {
        this.lander = lander;
    }

    internal void RegisterUI(UI ui)
    {
        this.ui = ui;
    }

    public Lander GetLander()
    {
        return lander;
    }

    

    internal void GameOver()
    {
        //Make UI Display Gameover screen
        ui.GameOver();
    }
}