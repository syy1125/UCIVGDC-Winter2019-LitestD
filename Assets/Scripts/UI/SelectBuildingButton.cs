using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class SelectBuildingButton : MonoBehaviour
{
	[FormerlySerializedAs("ConstructionManager")]
	[FormerlySerializedAs("BuildTool")]
	public PlanConstructionManager PlanConstructionManager;
	public TileBase Tile;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Reset()
	{
		PlanConstructionManager = GetComponentInParent<PlanConstructionManager>();
	}

	public void OnClick()
	{
		PlanConstructionManager.SelectBuildTile(Tile);
	}

	public void OnBuildingSelectionChange()
	{
        anim.SetBool("Selected", PlanConstructionManager.SelectedTile == Tile);
	}
}