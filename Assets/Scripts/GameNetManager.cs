using System.Collections;
using System.Linq;
using Network;
using Utilities;
using Utilities.Data;

public partial class GameManager
{
	private static ColorMode CurrentColor => (ColorMode)CurrentTurn + 1;

	private static void InitBroadcasters()
	{
		NetworkManager.AddBroadcastReceiver(OpCode.AddBlock, NetAddBlock);
		NetworkManager.AddBroadcastReceiver(OpCode.Reverse, NetReverseStack);
		NetworkManager.AddBroadcastReceiver(OpCode.Move, NetMoveBlock);
		NetworkManager.ConsumeSyncMessages();
	}

	private void NetNextTurn()
	{
		CurrentTurn = (CurrentTurn + 1) % NetworkManager.LOBBY_SIZE;
		attackSwapper.Code = "Deselected";
		OnChangeTurn?.Invoke();
		_isEnabled = false;
		CoroutineRunner.Run(RefreshIe());
		return;

		IEnumerator RefreshIe()
		{
			yield return GroundManager.RefreshGround(CurrentColor);
			if (GroundManager.IsGroundChanged)
			{
				yield return RefreshIe();
				yield break;
			}

			CheckEndOfGame();
			_isEnabled = true;
		}
	}

	private static void NetReverseStack(string userId, ByteArray data, bool isFast)
	{
		ReduceReverse(NetworkManager.GetGameId(userId));
		Instance._isEnabled = false;
		CoroutineRunner.Run(GroundManager.Reverse(data.GetV3Int(), Next()));
		return;

		IEnumerator Next()
		{
			Instance._isEnabled = true;
			yield break;
		}
	}

	private static void NetAddBlock(string userId, ByteArray data, bool isFast)
	{
		Instance._isWait = false;
		Instance._doneCount++;
		var selected = data.GetV3Int();
		var stack = GroundManager.GetStack(selected).Select(c => c.mode).ToList();
		stack.Reverse();
		var pos = data.GetV3Int();

		if (GroundManager.GetColor(pos) != CurrentColor)
		{
			GroundManager.SetColor(CurrentColor, pos);
			ReduceAttack(NetworkManager.GetGameId(userId));
		}

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
			Instance.RefreshUi();
			if (Instance._doneCount >= DONE_PER_TURN)
				Instance.NetNextTurn();
			yield break;
		}
	}

	private static void NetMoveBlock(string userId, ByteArray data, bool isFast)
	{
		Instance._isWait = false;
		var from = data.GetV3Int();
		var to = data.GetV3Int();
		ReduceMove(NetworkManager.GetGameId(userId));
		CoroutineRunner.Run(GroundManager.Move(from, to, AdvanceColor()));
		return;

		IEnumerator AdvanceColor()
		{
			GroundManager.AdvanceColor(to);
			yield break;
		}
	}
}
