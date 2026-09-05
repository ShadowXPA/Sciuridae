using System;
using Godot;

public partial class GameManager : Node
{
    private const string HIGHSCORE_PATH = "user://hs.dat";
    private const string SETTINGS_PATH = "user://settings.dat";

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
    [Export]
    public Options? Options { get; set; }
    [Export]
    public Credits? Credits { get; set; }

    private int _score;
    private int _highscore;

    private int _masterIdx;
    private int _musicIdx;
    private int _sfxIdx;

    public override void _Ready()
    {
        if (Character is null || MainMenu is null || Ui is null || GameTimer is null || GameOver is null) return;

        SignalBus.StartGame += StartGame;
        SignalBus.RestartGame += RestartGame;
        SignalBus.ReturnToMainMenu += ResetGame;
        SignalBus.QuitGame += QuitGame;
        SignalBus.AcornGrabbed += CharacterAcornGrabbed;
        SignalBus.OptionsMenu += ToggleOptionsMenu;
        SignalBus.CreditsMenu += ToggleCreditsMenu;

        _masterIdx = AudioServer.GetBusIndex("Master");
        _musicIdx = AudioServer.GetBusIndex("Music");
        _sfxIdx = AudioServer.GetBusIndex("SFX");

        GameTimer.Timeout += GameTimerTimeout;

        LoadSettings();
        ResetGame();
    }

    public override void _ExitTree()
    {
        SignalBus.StartGame -= StartGame;
        SignalBus.RestartGame -= RestartGame;
        SignalBus.ReturnToMainMenu -= ResetGame;
        SignalBus.QuitGame -= QuitGame;
        SignalBus.AcornGrabbed -= CharacterAcornGrabbed;
        SignalBus.OptionsMenu -= ToggleOptionsMenu;
        SignalBus.CreditsMenu -= ToggleCreditsMenu;

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

    private void ToggleOptionsMenu(bool open)
    {
        if (open)
        {
            Options?.RefreshOptions();
            Options?.Show();
            MainMenu?.Hide();
        }
        else
        {
            SaveSettings();
            Options?.Hide();
            MainMenu?.Show();
        }
    }

    private void ToggleCreditsMenu(bool open)
    {
        if (open)
        {
            Credits?.Show();
            MainMenu?.Hide();
        }
        else
        {
            Credits?.Hide();
            MainMenu?.Show();
        }
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

    private void LoadSettings()
    {
        if (!FileAccess.FileExists(SETTINGS_PATH)) return;

        using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Read);

        var isFullscreen = file.Get8() == 1;

        DisplayServer.WindowSetMode(isFullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        AudioServer.SetBusVolumeLinear(_masterIdx, file.GetHalf());
        AudioServer.SetBusVolumeLinear(_musicIdx, file.GetHalf());
        AudioServer.SetBusVolumeLinear(_sfxIdx, file.GetHalf());
    }

    private void SaveSettings()
    {
        using var file = FileAccess.Open(SETTINGS_PATH, FileAccess.ModeFlags.Write);
        var isFullscreen = DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen;

        file.Store8(isFullscreen ? (byte)1 : (byte)0);
        file.StoreHalf(AudioServer.GetBusVolumeLinear(_masterIdx));
        file.StoreHalf(AudioServer.GetBusVolumeLinear(_musicIdx));
        file.StoreHalf(AudioServer.GetBusVolumeLinear(_sfxIdx));
    }
}
