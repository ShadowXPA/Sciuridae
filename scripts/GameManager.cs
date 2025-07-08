using Godot;
using System;

public partial class GameManager : Node
{
    private const string HIGHSCORE_PATH = "user://hs.dat";

    [Export]
    public Character? Character { get; set; }
    [Export]
    public Ui? Ui { get; set; }
    [Export]
    public Timer? GameTimer { get; set; }
    [Export]
    public GameOver? GameOverUi { get; set; }
    public bool GameOver { get; private set; }

    private int _score;
    private int _highscore;

    public override void _Ready()
    {
        if (Character is null || Ui is null || GameTimer is null || GameOverUi is null) return;

        SetHighscore(LoadHighscore());

        GameTimer.Timeout += GameTimerTimeout;
        Character.AcornGrabbed += CharacterAcornGrabbed;
    }

    public override void _Process(double delta)
    {
        var timeLeft = GameTimer!.TimeLeft;
        var minutes = Mathf.FloorToInt(timeLeft / 60);
        var seconds = Mathf.FloorToInt(timeLeft % 60);
        Ui!.Time!.Text = $"{minutes:D2}:{seconds:D2}";
    }

    private void SetHighscore(int score)
    {
        _highscore = score;
        Ui!.Highscore!.Text = $"Highscore: {_highscore}";
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

    private void CharacterAcornGrabbed()
    {
        _score += 1;
        GD.Print("As game manager I can attest to that...");
        Ui!.Score!.Text = $"{_score}";
    }

    private void GameTimerTimeout()
    {
        GameOver = true;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        Character!.SetInputEnabled(false);
        Character.AcornGrabbed -= CharacterAcornGrabbed;
        GD.Print("Game is over!");
        if (_score > _highscore)
        {
            SetHighscore(_score);
            GameOverUi!.Highscore!.Visible = true;
            SaveHighscore(_score);
        }

        GameOverUi!.AnimationPlayer!.Play("game_over");
        GameOverUi.Visible = true;
    }
}
