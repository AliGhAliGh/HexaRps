using System.Collections;
using Blocks;
using Network;
using UnityEngine;
using Utilities;

namespace Anim
{
	public class BlockAnimator : Singleton<BlockAnimator>
	{
		[SerializeField] private float moveSpeed = 5, scaleSpeed = 10;

		public static IEnumerator Create(Block block)
		{
			var startScale = block.transform.localScale;
			var timer = 0f;
			var speed = Instance.scaleSpeed / 10;
			if (!NetworkManager.IsLoading)
				while (timer < 1)
				{
					timer += Time.deltaTime * speed;
					SetScale();
					yield return CoroutineRunner.WaitForEndOfFrame;
				}

			timer = 1;
			SetScale();
			yield break;

			void SetScale()
			{
				if (block)
					block.transform.localScale = Vector3.Lerp(Vector3.zero, startScale, timer);
			}
		}

		public static IEnumerator Destroy(Block block)
		{
			var startScale = block.transform.localScale;
			var timer = 0f;
			var speed = Instance.scaleSpeed / 10;
			if (!NetworkManager.IsLoading)
				while (timer < 1)
				{
					timer += Time.deltaTime * speed;
					SetScale();
					yield return CoroutineRunner.WaitForEndOfFrame;
				}

			Object.Destroy(block.gameObject);
			yield break;

			void SetScale() => block.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, timer);
		}

		public static IEnumerator Move(Block block, Vector3 pos)
		{
			var currentPos = block.transform.position;
			if (Mathf.Approximately(currentPos.y, pos.y))
				yield return Mover(block.gameObject, pos);
			else if (currentPos.y > pos.y)
			{
				yield return Mover(block.gameObject, new Vector3(pos.x, currentPos.y, pos.z));
				yield return Mover(block.gameObject, pos);
			}
			else
			{
				yield return Mover(block.gameObject, new Vector3(currentPos.x, pos.y, currentPos.z));
				yield return Mover(block.gameObject, pos);
			}
		}

		public static IEnumerator Mover(GameObject target, Vector3 pos)
		{
			var startPos = target.transform.position;
			var timer = 0f;
			var speed = Instance.moveSpeed / 10;
			if (!NetworkManager.IsLoading)
				while (timer < 1)
				{
					timer += Time.deltaTime * speed;
					SetPos();
					yield return CoroutineRunner.WaitForEndOfFrame;
				}

			timer = 1;
			SetPos();
			yield break;

			void SetPos()
			{
				if (target)
					target.transform.position = Vector3.Lerp(startPos, pos, timer);
			}
		}
	}
}
