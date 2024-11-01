using System.Collections;
using System.Linq;
using Network;
using Utilities;
using Utilities.Data;

public partial class GameManager
{
	private static void InitBroadcasters()
	{
		NetworkManager.AddBroadcastReceiver(OpCode.TurnCmd, NetNextTurn);
		NetworkManager.AddBroadcastReceiver(OpCode.AddBlock, NetAddBlock);
		NetworkManager.ConsumeSyncMessages();
	}

	private static void NetNextTurn(string userId, ByteArray data, bool isFast)
	{
		CurrentTurn = (CurrentTurn + 1) % NetworkManager.LOBBY_SIZE;
		OnChangeTurn?.Invoke();
		CoroutineRunner.Run(GroundManager.RefreshGround((ColorMode)CurrentTurn + 1));
	}

	private static void NetAddBlock(string userId, ByteArray data, bool isFast)
	{
		Instance._isDone = true;
		var selected = data.GetV3Int();
		var stack = GroundManager.GetStack(selected).Select(c => c.mode).ToList();
		stack.Reverse();
		var pos = data.GetV3Int();

		CoroutineRunner.Run(GroundManager.Clear(selected, Refresh()));
		CoroutineRunner.Run(GroundManager.AddStack(stack, pos, GroundManager.Execute(pos)));
		return;

		IEnumerator Refresh()
		{
			if (IsOuterBlocksEmpty) InitBlockObjects();
			yield break;
		}
	}
}
