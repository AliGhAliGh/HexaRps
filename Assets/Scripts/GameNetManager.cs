using Network;
using Utilities.Data;

public partial class GameManager
{
	private void InitBroadcasters()
	{
		NetworkManager.AddBroadcastReceiver(OpCode.TurnCmd, NetNextTurn);
		NetworkManager.ConsumeSyncMessages();
	}

	private void NetNextTurn(string userId, ByteArray data, bool isFast)
	{
		CurrentTurn = (CurrentTurn + 1) % NetworkManager.LOBBY_SIZE;
		OnChangeTurn?.Invoke();
		RefreshButtons();
	}
}
