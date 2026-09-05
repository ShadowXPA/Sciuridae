using System;
using Godot;

public partial class Options : Control
{
	private TextureButton? _closeBtn;
	private CheckBox? _fullscreenCheckBox;
	private HSlider? _masterSlider;
	private HSlider? _musicSlider;
	private HSlider? _sfxSlider;
	private int _masterIdx;
	private int _musicIdx;
	private int _sfxIdx;

	public override void _Ready()
	{
		_closeBtn = GetNode<TextureButton>("%Close");
		_fullscreenCheckBox = GetNode<CheckBox>("%Fullscreen");
		_masterSlider = GetNode<HSlider>("%MasterSlider");
		_musicSlider = GetNode<HSlider>("%MusicSlider");
		_sfxSlider = GetNode<HSlider>("%SFXSlider");

		_masterIdx = AudioServer.GetBusIndex("Master");
		_musicIdx = AudioServer.GetBusIndex("Music");
		_sfxIdx = AudioServer.GetBusIndex("SFX");

		RefreshOptions();

		if (OS.HasFeature("mobile"))
		{
			_fullscreenCheckBox.Hide();
		}

		_closeBtn.Pressed += CloseOptions;
		_fullscreenCheckBox.Toggled += ToggleFullscreen;
		_masterSlider.ValueChanged += MasterSliderChanged;
		_musicSlider.ValueChanged += MusicSliderChanged;
		_sfxSlider.ValueChanged += SFXSliderChanged;
	}

	public override void _ExitTree()
	{
		if (_closeBtn is not null)
			_closeBtn.Pressed -= CloseOptions;
		if (_fullscreenCheckBox is not null)
			_fullscreenCheckBox.Toggled -= ToggleFullscreen;
		if (_masterSlider is not null)
			_masterSlider.ValueChanged -= MasterSliderChanged;
		if (_musicSlider is not null)
			_musicSlider.ValueChanged -= MusicSliderChanged;
		if (_sfxSlider is not null)
			_sfxSlider.ValueChanged -= SFXSliderChanged;
	}

	public void RefreshOptions()
	{
		if (_masterSlider is not null)
			_masterSlider.Value = AudioServer.GetBusVolumeLinear(_masterIdx);
		if (_musicSlider is not null)
			_musicSlider.Value = AudioServer.GetBusVolumeLinear(_musicIdx);
		if (_sfxSlider is not null)
			_sfxSlider.Value = AudioServer.GetBusVolumeLinear(_sfxIdx);
		if (_fullscreenCheckBox is not null)
			_fullscreenCheckBox.SetPressedNoSignal(DisplayServer.WindowGetMode() == DisplayServer.WindowMode.Fullscreen);
	}

	private void CloseOptions()
	{
		SignalBus.BroadcastOptionsMenu(false);
	}

	private void ToggleFullscreen(bool toggledOn)
	{
		DisplayServer.WindowSetMode(toggledOn ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
	}

	private void MasterSliderChanged(double value)
	{
		AudioServer.SetBusVolumeLinear(_masterIdx, (float)value);
	}

	private void MusicSliderChanged(double value)
	{
		AudioServer.SetBusVolumeLinear(_musicIdx, (float)value);
	}

	private void SFXSliderChanged(double value)
	{
		AudioServer.SetBusVolumeLinear(_sfxIdx, (float)value);
	}
}
