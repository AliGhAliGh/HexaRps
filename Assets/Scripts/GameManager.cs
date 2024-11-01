using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using Network;
using UnityEngine;
using Utilities;
using Utilities.Ui;

public partial class GameManager : RefresherSingleton<GameManager>
{
	[SerializeField] private CustomButton nextButton;
	[SerializeField] private int stackLength = 3, maxStackSize = 5;

	public static Action OnChangeTurn;
	public static int MaxStackSize => Instance.maxStackSize;

	private Vector3Int _selected;
	private ColorMode _myColor;
	private bool _isDone;

	private static readonly List<Vector3Int> OutBlocks = new()
	{
		new(-3, 3, 0),
		new(-2, 3, 0),
		new(-1, 3, 0),
		new(0, 3, 0),
		new(1, 3, 0)
	};

	private readonly List<Vector3Int> _usedBlocks = new();

	public static int CurrentTurn { get; private set; }

	public static bool IsMyTurn => NetworkManager.GameId == CurrentTurn;

	private static bool IsOuterBlocksEmpty => OutBlocks.All(c => GroundManager.GetStack(c).Count == 0);

	private void Start()
	{
		CurrentTurn = 0;
		nextButton.OnClick = NextTurn;
		_selected = Vector3Int.zero;
		_isDone = false;
		OnChangeTurn += RefreshButtons;
		OnChangeTurn += () => _isDone = false;
		OnChangeTurn?.Invoke();
		InitBlockObjects();
		_myColor = NetworkManager.GameId == 0 ? ColorMode.Red : ColorMode.Blue;
		InitBroadcasters();
	}

	private static void InitBlockObjects()
	{
		for (var i = 0; i < 5; i++)
		{
			var stack = CreateRandomStack(Instance.stackLength);
			CoroutineRunner.Run(GroundManager.AddStack(stack.ToList(), OutBlocks[i]));
		}
	}

	private static Stack<BlockMode> CreateRandomStack(int count)
	{
		var stack = new Stack<BlockMode>();
		for (var i = 0; i < count; i++) stack.Push((BlockMode)NetworkManager.SyncRandom(0, 3));
		return stack;
	}

	public static void GroundClick(Vector3Int pos)
	{
		if (!IsMyTurn || Instance._isDone) return;
		if (OutBlocks.Contains(pos))
		{
			DisableOuters();
			if (Instance._selected != pos)
			{
				Instance._selected = pos;
				GroundManager.SetColor(Instance._myColor, pos);
			}
			else Instance._selected = Vector3Int.zero;

			return;
		}

		if (Instance._selected != Vector3Int.zero && !Instance._usedBlocks.Contains(Instance._selected) &&
		    GroundManager.GetColor(pos) == Instance._myColor && GroundManager.GetStack(pos).Count is 0)
		{
			NetworkManager.Broadcast(OpCode.AddBlock, Instance._selected, pos);
			Instance._isDone = true;
			DisableOuters();
			Instance._usedBlocks.Add(Instance._selected);
			Instance._selected = Vector3Int.zero;
		}

		return;

		void DisableOuters() => GroundManager.SetColor(ColorMode.Default, OutBlocks.ToArray());
	}

	private void NextTurn()
	{
		NetworkManager.Broadcast(OpCode.TurnCmd);
		nextButton.Interactable = false;
	}

	private void RefreshButtons() => nextButton.Interactable = IsMyTurn;

	protected override void Refresh() => OnChangeTurn = null;
}
