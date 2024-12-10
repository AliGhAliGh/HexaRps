using System;
using System.Collections.Generic;
using System.Linq;
using Blocks;
using Network;
using TextHandlers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.Ui;

public partial class GameManager : RefresherSingleton<GameManager>
{
	[SerializeField] private LuiText turn;
	[SerializeField] private CustomButton attackButton, reverseButton;
	[SerializeField] private SpriteSwapper attackSwapper, reverseSwapper;
	[SerializeField] private TextMeshProUGUI attackCountText, reverseCountText;
	[SerializeField] private int creationSize, maxStackSize = 5, attackCount = 3, reverseCount = 3;
	[SerializeField] private LuiText resultText;

	public static Action OnChangeTurn;
	public static int MaxStackSize => Instance.maxStackSize;

	private Vector3Int _selected;
	private ColorMode _myColor;
	private bool _isDone, _isEnabled;

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

	private static bool IsMyTurn => NetworkManager.GameId == CurrentTurn;
	private static bool IsAttackSelected => Instance.attackSwapper.Code == "Selected";
	private static bool IsReverseSelected => Instance.reverseSwapper.Code == "Selected";
	private static bool IsOuterBlocksEmpty => OutBlocks.All(c => GroundManager.GetStack(c).Count == 0);

	private void Start()
	{
		attackCountText.text = attackCount.ToString();
		reverseCountText.text = reverseCount.ToString();
		CurrentTurn = 0;
		_selected = Vector3Int.zero;
		_isDone = false;
		_isEnabled = true;
		attackButton.OnClick = ToggleAttackButton;
		reverseButton.OnClick = ToggleReverseButton;
		OnChangeTurn += () =>
		{
			turn.SetText(IsMyTurn ? "YourTurn" : "OtherTurn");
			_isDone = false;
		};
		OnChangeTurn?.Invoke();
		InitBlockObjects();
		_myColor = NetworkManager.GameId == 0 ? ColorMode.Red : ColorMode.Blue;
		InitBroadcasters();
	}

	private void ToggleAttackButton()
	{
		if (IsMyTurn) attackSwapper.Code = IsAttackSelected ? "Deselected" : "Selected";
	}

	private void ToggleReverseButton()
	{
		if (IsMyTurn) reverseSwapper.Code = IsReverseSelected ? "Deselected" : "Selected";
	}

	private static void InitBlockObjects()
	{
		Instance._usedBlocks.Clear();
		var sum = Instance.creationSize * OutBlocks.Count;
		var count = sum / 3;
		var blocks = new List<BlockMode>();
		for (var i = count * 3; i < sum; i++) blocks.Add((BlockMode)NetworkManager.SyncRandom(0, 3));
		for (var i = 0; i < count; i++)
		{
			blocks.Add(BlockMode.Paper);
			blocks.Add(BlockMode.Rock);
			blocks.Add(BlockMode.Scissors);
		}

		foreach (var block in OutBlocks)
		{
			var stack = CreateRandomStack(blocks);
			CoroutineRunner.Run(GroundManager.AddStack(stack.ToList(), block));
		}
	}

	private static Stack<BlockMode> CreateRandomStack(List<BlockMode> entities)
	{
		var res = new Stack<BlockMode>(NetworkManager.SyncPick(Instance.creationSize, entities.ToArray()));
		foreach (var blockMode in res) entities.Remove(blockMode);
		return res;
	}

	private static void ReduceReverse()
	{
		Instance.reverseSwapper.Code = "Deselected";
		Instance.reverseCount--;
		Instance.reverseButton.Interactable = Instance.reverseCount > 0;
		Instance.reverseCountText.text = Instance.reverseCount.ToString();
	}

	private static void ReduceAttack()
	{
		Instance.attackSwapper.Code = "Deselected";
		Instance.attackCount--;
		Instance.attackButton.Interactable = Instance.attackCount > 0;
		Instance.attackCountText.text = Instance.attackCount.ToString();
	}

	public static void GroundClick(Vector3Int pos)
	{
		if (!IsMyTurn || Instance._isDone || !Instance._isEnabled)
			return;

		if (OutBlocks.Contains(pos))
		{
			DisableOuters();
			if (Instance._selected != pos)
			{
				Instance._selected = pos;
				GroundManager.SetColor(Instance._myColor, pos);
				if (IsReverseSelected) NetworkManager.Broadcast(OpCode.Reverse, pos);
			}
			else Instance._selected = Vector3Int.zero;

			return;
		}

		var isSameColor = GroundManager.GetColor(pos) == Instance._myColor;
		if (Instance._selected != Vector3Int.zero && !Instance._usedBlocks.Contains(Instance._selected) &&
		    (isSameColor || IsAttackSelected) && GroundManager.GetStack(pos).Count is 0)
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

	private void CheckEndOfGame()
	{
		if (GroundManager.GetPoses().Where(c => GroundManager.GetColor(c) == CurrentColor)
		    .All(c => GroundManager.GetStack(c).Count > 0))
		{
			resultText.gameObject.SetActive(true);
			resultText.SetText(CurrentColor == ColorMode.Blue ? "RedWon" : "BlueWon");
		}
	}

	public void ReturnToMenu()
	{
		_ = NetworkManager.Leave();
		SceneManager.LoadScene(0);
	}

	protected override void Refresh() => OnChangeTurn = null;
}
