using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SelectBuilding : MonoBehaviour
{
	public BuildOnTile BuildTool;
	public TileBase Tile;

	private Button _button;

	public ColorBlock SelectedColors;
	private ColorBlock _originalColors;

	private void Reset()
	{
		BuildTool = GetComponentInParent<BuildOnTile>();
		SelectedColors = ColorBlock.defaultColorBlock;
	}

	private void Start()
	{
		_button = GetComponent<Button>();
		_originalColors = _button.colors;
	}

	public void OnClick()
	{
		BuildTool.SelectBuildTile(Tile);
	}

	public void OnBuildingSelectionChange()
	{
		_button.colors = BuildTool.SelectedTile == Tile ? SelectedColors : _originalColors;
	}
}