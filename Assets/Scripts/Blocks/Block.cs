using UnityEngine;

namespace Blocks
{
	public class Block: MonoBehaviour
	{
		[SerializeField] public BlockMode mode;
		[SerializeField] private Material transparentMaterial;
		[SerializeField] private MeshRenderer myRenderer;

		private Material _initialMaterial;

		private void Start() => _initialMaterial = myRenderer.material;

		public void SetTransparency(bool isOpaque) => myRenderer.material = isOpaque ? _initialMaterial : transparentMaterial;
	}

	public enum BlockMode
	{
		Paper,
		Rock,
		Scissors
	}
}
