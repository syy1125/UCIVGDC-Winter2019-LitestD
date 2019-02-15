using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SelectBuildingButton : MonoBehaviour
{
	[FormerlySerializedAs("BuildTool")]
	public ConstructionManager ConstructionManager;
	public TileBase Tile;

	private Button _button;

	public ColorBlock SelectedColors;
	private ColorBlock _originalColors;

	private void Reset()
	{
		ConstructionManager = GetComponentInParent<ConstructionManager>();
		SelectedColors = ColorBlock.defaultColorBlock;
	}

	private void Start()
	{
		_button = GetComponent<Button>();
		_originalColors = _button.colors;
	}

	public void OnClick()
	{
		ConstructionManager.SelectBuildTile(Tile);
	}

	public void OnBuildingSelectionChange()
	{
		_button.colors = ConstructionManager.SelectedTile == Tile ? SelectedColors : _originalColors;
	}
}