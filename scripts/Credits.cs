using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class Credits : Control
{
	private TextureButton? _closeBtn;
    private IEnumerable<RichTextLabel>? _metaLabels;

	public override void _Ready()
	{
		_closeBtn = GetNode<TextureButton>("%Close");
		_closeBtn.Pressed += CloseCredits;

        _metaLabels = GetTree().GetNodesInGroup("meta_label").Cast<RichTextLabel>();
        foreach (RichTextLabel label in _metaLabels)
            label.MetaClicked += OnMetaClicked;
	}

	public override void _ExitTree()
	{
		if (_closeBtn is not null)
			_closeBtn.Pressed -= CloseCredits;
        if (_metaLabels is not null)
            foreach (var label in _metaLabels)
                label.MetaClicked -= OnMetaClicked;
	}

	private void CloseCredits()
	{
		SignalBus.BroadcastCreditsMenu(false);
	}

    private void OnMetaClicked(Variant meta)
    {
        OS.ShellOpen(meta.AsString());
    }
}
