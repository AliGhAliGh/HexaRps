using UnityEngine;

namespace Blocks
{
	public class Block: MonoBehaviour
	{
		[SerializeField] public BlockMode mode;
	}

	public enum BlockMode
	{
		Paper,
		Rock,
		Scissors
	}
}
