using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class SelectBuildingButton : MonoBehaviour
{
	[FormerlySerializedAs("ConstructionManager")]
	[FormerlySerializedAs("BuildTool")]
	public PlanConstructionManager PlanConstructionManager;
	public TileBase Tile;

	private Animator _anim;
	private AudioSource _audio;

	private void Awake()
	{
		_anim = GetComponent<Animator>();
		_audio = GetComponent<AudioSource>();
	}

	private void Reset()
	{
		PlanConstructionManager = GetComponentInParent<PlanConstructionManager>();
	}

	public void OnClick()
	{
		_audio.volume = OptionsMenu.GetEffectsVolume();
		_audio.Play();
		PlanConstructionManager.SelectBuildTile(Tile);
	}

	public void OnBuildingSelectionChange()
	{
		_anim.SetBool("Selected", PlanConstructionManager.SelectedTile == Tile);
	}
}