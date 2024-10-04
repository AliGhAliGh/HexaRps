using System;
using Network;
using UnityEngine;
using Utilities;
using Utilities.Ui;

public partial class GameManager : RefresherSingleton<GameManager>
{
	[SerializeField] private CustomButton nextButton;

	public static Action OnChangeTurn;

	public static int CurrentTurn { get; private set; }

	public static bool IsMyTurn => NetworkManager.GameId == CurrentTurn;

	private void Start()
	{
		CurrentTurn = 0;
		nextButton.OnClick = NextTurn;
		RefreshButtons();
		OnChangeTurn?.Invoke();
		InitBroadcasters();
	}

	private void NextTurn()
	{
		NetworkManager.Broadcast(OpCode.TurnCmd);
		nextButton.Interactable = false;
	}

	private void RefreshButtons() => nextButton.Interactable = false;

	protected override void Refresh() => OnChangeTurn = null;
}
