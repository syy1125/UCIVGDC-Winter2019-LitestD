using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class SelectBuildingButton : MonoBehaviour
{
	[FormerlySerializedAs("ConstructionManager")]
	[FormerlySerializedAs("BuildTool")]
	public PlanConstructionManager PlanConstructionManager;
	public TileBase Tile;

	private Button _button;

	public ColorBlock SelectedColors;
	private ColorBlock _originalColors;

	private void Reset()
	{
		PlanConstructionManager = GetComponentInParent<PlanConstructionManager>();
		SelectedColors = ColorBlock.defaultColorBlock;
	}

	private void Start()
	{
		_button = GetComponent<Button>();
		_originalColors = _button.colors;
	}

	public void OnClick()
	{
		PlanConstructionManager.SelectBuildTile(Tile);
	}

	public void OnBuildingSelectionChange()
	{
		_button.colors = PlanConstructionManager.SelectedTile == Tile ? SelectedColors : _originalColors;
	}
}