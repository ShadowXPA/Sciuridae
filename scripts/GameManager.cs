using System;
using Godot;

public partial class GameManager : Node
{
    private const string HIGHSCORE_PATH = "user://hs.dat";

    [Export]
    public Character? Character { get; set; }
    [Export]
    public MainMenu? MainMenu { get; set; }
    [Export]
    public Ui? Ui { get; set; }
    [Export]
    public Timer? GameTimer { get; set; }
    [Export]
    public GameOver? GameOver { get; set; }

    private int _score;
    private int _highscore;

    public override void _Ready()
    {
        if (Character is null || MainMenu is null || Ui is null || GameTimer is null || GameOver is null) return;

        SignalBus.StartGame += StartGame;
        SignalBus.RestartGame += RestartGame;
        SignalBus.ReturnToMainMenu += ResetGame;
        SignalBus.QuitGame += QuitGame;
        SignalBus.AcornGrabbed += CharacterAcornGrabbed;

        GameTimer.Timeout += GameTimerTimeout;

        ResetGame();
    }

    public override void _ExitTree()
    {
        SignalBus.StartGame -= StartGame;
        SignalBus.RestartGame -= RestartGame;
        SignalBus.ReturnToMainMenu -= ResetGame;
        SignalBus.QuitGame -= QuitGame;
        SignalBus.AcornGrabbed -= CharacterAcornGrabbed;

        if (GameTimer is not null)
            GameTimer.Timeout -= GameTimerTimeout;
    }

    public override void _Process(double delta)
    {
        var timeLeft = GameTimer!.TimeLeft;
        var minutes = Mathf.FloorToInt(timeLeft / 60);
        var seconds = Mathf.FloorToInt(timeLeft % 60);
        Ui!.Time!.Text = $"{minutes:D2}:{seconds:D2}";
    }

    private void ReturnToMainMenu()
    {
        GetTree().ReloadCurrentScene();
    }

    private void ResetGame()
    {
        Character!.SetInputEnabled(false);
        Character!.Reset();
        Character!.RotateCamera();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GameTimer?.Stop();
        MainMenu?.Show();
        Ui?.Hide();
        GameOver?.Hide();

        _score = 0;
        Ui!.Score!.Text = $"{_score}";
        GameOver!.Highscore!.Visible = false;
        SetHighscore(LoadHighscore());
    }

    private void StartGame()
    {
        Character!.Reset();
        MainMenu?.Hide();
        Ui?.Show();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        Character!.SetInputEnabled(true);
        GameTimer?.Start();
    }

    private void RestartGame()
    {
        ResetGame();
        StartGame();
    }

    private void QuitGame()
    {
        GetTree().Quit();
    }

    private void SetHighscore(int score)
    {
        _highscore = score;
        Ui!.Highscore!.Text = $"Highscore: {_highscore}";
    }

    private void CharacterAcornGrabbed(Acorn acorn)
    {
        _score += acorn.Score;
        Ui!.Score!.Text = $"{_score}";
    }

    private void GameTimerTimeout()
    {
        SignalBus.BroadcastGameOver();
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Character!.Stop();

        if (_score > _highscore)
        {
            SetHighscore(_score);
            GameOver!.Highscore!.Visible = true;
            SaveHighscore(_score);
        }

        GameOver!.AnimationPlayer!.Play("game_over");
        GameOver.Visible = true;
    }

    private int LoadHighscore()
    {
        if (!FileAccess.FileExists(HIGHSCORE_PATH)) return 0;

        using var file = FileAccess.Open(HIGHSCORE_PATH, FileAccess.ModeFlags.Read);
        var encoded = file.GetAsText();

        return BitConverter.ToInt32(Marshalls.Base64ToRaw(encoded));
    }

    private void SaveHighscore(int score)
    {
        var encoded = Marshalls.RawToBase64(BitConverter.GetBytes(score));
        using var file = FileAccess.Open(HIGHSCORE_PATH, FileAccess.ModeFlags.Write);
        file.StoreString(encoded);
    }
}
