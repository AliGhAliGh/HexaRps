using System.Collections;
using System.Linq;
using Network;
using Utilities;
using Utilities.Data;

public partial class GameManager
{
	private static void InitBroadcasters()
	{
		NetworkManager.AddBroadcastReceiver(OpCode.AddBlock, NetAddBlock);
		NetworkManager.ConsumeSyncMessages();
	}

	private static void NetNextTurn()
	{
		CurrentTurn = (CurrentTurn + 1) % NetworkManager.LOBBY_SIZE;
		OnChangeTurn?.Invoke();
		CoroutineRunner.Run(Refresh(Refresh(Refresh())));
		return;

		IEnumerator Refresh(object next = null)
		{
			yield return GroundManager.RefreshGround((ColorMode)CurrentTurn + 1);
			yield return GroundManager.RefreshGround((ColorMode)CurrentTurn + 1);
			yield return GroundManager.RefreshGround((ColorMode)CurrentTurn + 1, next);
		}
	}

	private static void NetAddBlock(string userId, ByteArray data, bool isFast)
	{
		Instance._isDone = true;
		var selected = data.GetV3Int();
		var stack = GroundManager.GetStack(selected).Select(c => c.mode).ToList();
		stack.Reverse();
		var pos = data.GetV3Int();

		CoroutineRunner.Run(GroundManager.Clear(selected, Refresh()));
		CoroutineRunner.Run(GroundManager.AddStack(stack, pos, GroundManager.Execute(pos, Next())));
		return;

		IEnumerator Refresh()
		{
			if (IsOuterBlocksEmpty) InitBlockObjects();
			yield break;
		}

		IEnumerator Next()
		{
			NetNextTurn();
			yield break;
		}
	}
}
