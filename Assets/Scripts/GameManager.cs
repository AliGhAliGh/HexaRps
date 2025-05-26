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
	public const int MAX_STACK_SIZE = 6;

	private const int CREATION_SIZE = 3,
		ATTACK_COUNT = 2,
		MOVE_COUNT = 5,
		REVERSE_COUNT = 4,
		DONE_PER_TURN = 3,
		MAX_ROUND = 20;

	[SerializeField] private LuiText turn;
	[SerializeField] private CustomButton attackButton, reverseButton, moveButton;
	[SerializeField] private SpriteSwapper attackSwapper, reverseSwapper, moveSwapper;
	[SerializeField] private TextMeshProUGUI attackCountText, reverseCountText, moveCountText, score1Text, score2Text;
	[SerializeField] private LuiText resultText;

	public static Action OnChangeTurn;

	private Vector3Int _selected, _moveSelected;
	private ColorMode _myColor;

	private int _doneCount,
		_score1,
		_constScore1,
		_score2,
		_constScore2,
		_attackCount1,
		_reverseCount1,
		_moveCount1,
		_attackCount2,
		_reverseCount2,
		_moveCount2,
		_totalTurn;

	private bool _isEnabled, _isWait, _isAbilityUsed;

	private static readonly Vector3Int DefaultMoveSelection = new(1000, 1000, 0);

	private static readonly List<Vector3Int> OutBlocks = new()
	{
		new(-3, 3, 0),
		new(-2, 3, 0),
		new(-1, 3, 0),
		new(0, 3, 0),
		new(1, 3, 0)
	};

	private static readonly Dictionary<ScoreMode, (int, int)> ConstScores = new()
	{
		[ScoreMode.BlockDestroy] = (10, 1),
		[ScoreMode.StackDestroy] = (30, 5),
		[ScoreMode.Ground] = (40, 0),
		[ScoreMode.FullStack] = (100, 0),
		[ScoreMode.AttackAbility] = (50, 0),
		[ScoreMode.MoveAbility] = (25, 0),
		[ScoreMode.ReverseAbility] = (15, 0)
	};

	private static readonly Dictionary<ScoreMode, int> Scores = new();

	private readonly List<Vector3Int> _usedBlocks = new();

	public static int CurrentTurn { get; private set; }

	private static bool IsMyTurn => NetworkManager.GameId == CurrentTurn;
	private static bool IsAttackSelected => !Instance._isAbilityUsed && Instance.attackSwapper.Code == "Selected";
	private static bool IsReverseSelected => !Instance._isAbilityUsed && Instance.reverseSwapper.Code == "Selected";
	private static bool IsMoveSelected => !Instance._isAbilityUsed && Instance.moveSwapper.Code == "Selected";
	private static bool IsOuterBlocksEmpty => OutBlocks.All(c => GroundManager.GetStack(c).Count == 0);

	private void Start()
	{
		_attackCount1 = _attackCount2 = ATTACK_COUNT;
		_reverseCount1 = _reverseCount2 = REVERSE_COUNT;
		_moveCount1 = _moveCount2 = MOVE_COUNT;
		CurrentTurn = 0;
		_selected = Vector3Int.zero;
		_moveSelected = DefaultMoveSelection;
		_doneCount = 0;
		_totalTurn = 0;
		_isEnabled = true;
		attackButton.OnClick = ToggleAttackButton;
		reverseButton.OnClick = ToggleReverseButton;
		moveButton.OnClick = ToggleMoveButton;
		OnChangeTurn += () =>
		{
			Enum.GetValues(typeof(ScoreMode)).Cast<ScoreMode>().ForEach(c => Scores[c] = ConstScores[c].Item1);
			turn.SetContext(Translator.GetLString(IsMyTurn ? "YourTurn" : "OtherTurn").Value +
			                ": " + (_totalTurn / 2 + 1));
			_doneCount = 0;
			_totalTurn++;
			_isAbilityUsed = false;
		};
		OnChangeTurn?.Invoke();
		InitBlockObjects();
		_myColor = NetworkManager.GameId == 0 ? ColorMode.Red : ColorMode.Blue;
		InitBroadcasters();
		RefreshUi();
	}

	private void RefreshUi()
	{
		RefreshConstScores();
		score1Text.text = _score1 + " + " + _constScore1;
		score2Text.text = _score2 + " + " + _constScore2;
		var attack = NetworkManager.GameId == 0 ? _attackCount1 : _attackCount2;
		var reverse = NetworkManager.GameId == 0 ? _reverseCount1 : _reverseCount2;
		var move = NetworkManager.GameId == 0 ? _moveCount1 : _moveCount2;
		attackCountText.text = attack.ToString();
		reverseCountText.text = reverse.ToString();
		moveCountText.text = move.ToString();
		attackButton.Interactable = attack > 0;
		reverseButton.Interactable = reverse > 0;
		moveButton.Interactable = move > 0;
	}

	private void RefreshConstScores()
	{
		_constScore1 = _constScore2 = 0;
		GroundManager.GetPoses().ForEach(c =>
		{
			if (GroundManager.GetColor(c) == ColorMode.Red)
			{
				_constScore1 += Scores[ScoreMode.Ground];
				if (GroundManager.GetStack(c).Count == MAX_STACK_SIZE)
					_constScore1 += Scores[ScoreMode.FullStack];
			}

			if (GroundManager.GetColor(c) == ColorMode.Blue)
			{
				_constScore2 += Scores[ScoreMode.Ground];
				if (GroundManager.GetStack(c).Count == MAX_STACK_SIZE)
					_constScore2 += Scores[ScoreMode.FullStack];
			}
		});
	}

	public static void AddScore(ScoreMode mode)
	{
		if (CurrentTurn == 0) Instance._score1 += Scores[mode];
		else Instance._score2 += Scores[mode];
		Scores[mode] += ConstScores[mode].Item2;
		Instance.RefreshUi();
	}

	private void ToggleAttackButton()
	{
		if (_isAbilityUsed || IsReverseSelected || IsMoveSelected) attackSwapper.Code = "Deselected";
		else if (IsMyTurn) attackSwapper.Code = IsAttackSelected ? "Deselected" : "Selected";
	}

	private void ToggleReverseButton()
	{
		if (_isAbilityUsed || IsAttackSelected || IsMoveSelected) reverseSwapper.Code = "Deselected";
		else if (IsMyTurn) reverseSwapper.Code = IsReverseSelected ? "Deselected" : "Selected";
	}

	private void ToggleMoveButton()
	{
		if (_isAbilityUsed || IsReverseSelected || IsAttackSelected) moveSwapper.Code = "Deselected";
		else if (IsMyTurn) moveSwapper.Code = IsMoveSelected ? "Deselected" : "Selected";
	}

	private static void InitBlockObjects()
	{
		Instance._usedBlocks.Clear();
		var sum = CREATION_SIZE * OutBlocks.Count;
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
		var res = new Stack<BlockMode>(NetworkManager.SyncPick(CREATION_SIZE, entities.ToArray()));
		foreach (var blockMode in res) entities.Remove(blockMode);
		return res;
	}

	private static void ReduceReverse(int id)
	{
		Instance.reverseSwapper.Code = "Deselected";
		switch (id)
		{
			case 0:
				Instance._reverseCount1--;
				break;
			case 1:
				Instance._reverseCount2--;
				break;
		}

		Instance._isAbilityUsed = true;
		Instance.RefreshUi();
	}

	private static void ReduceAttack(int id)
	{
		Instance.attackSwapper.Code = "Deselected";
		switch (id)
		{
			case 0:
				Instance._attackCount1--;
				break;
			case 1:
				Instance._attackCount2--;
				break;
		}

		Instance._isAbilityUsed = true;
		Instance.RefreshUi();
	}

	private static void ReduceMove(int id)
	{
		Instance.moveSwapper.Code = "Deselected";
		switch (id)
		{
			case 0:
				Instance._moveCount1--;
				break;
			case 1:
				Instance._moveCount2--;
				break;
		}

		Instance._isAbilityUsed = true;
		Instance.RefreshUi();
	}

	public static void GroundClick(Vector3Int pos)
	{
		if (!IsMyTurn || Instance._doneCount >= DONE_PER_TURN || !Instance._isEnabled)
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
		var isEmpty = GroundManager.GetStack(pos).Count is 0;
		if (IsMoveSelected)
		{
			if (!Instance._isWait && isSameColor)
			{
				if (!isEmpty) Instance._moveSelected = pos;
				else if (GroundManager.GetStack(Instance._moveSelected).Count > 0)
				{
					NetworkManager.Broadcast(OpCode.Move, Instance._moveSelected, pos);
					Instance._isWait = true;
					Instance._moveSelected = DefaultMoveSelection;
				}
			}

			return;
		}

		if (!Instance._isWait && Instance._selected != Vector3Int.zero &&
		    !Instance._usedBlocks.Contains(Instance._selected) &&
		    (isSameColor || IsAttackSelected) && isEmpty)
		{
			NetworkManager.Broadcast(OpCode.AddBlock, Instance._selected, pos);
			Instance._isWait = true;
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
		else if (_totalTurn >= 2 * MAX_ROUND)
		{
			resultText.gameObject.SetActive(true);
			resultText.SetText(_score1 + _constScore1 > _score2 + _constScore2 ? "RedWon" :
				_score1 + _constScore1 < _score2 + _constScore2 ? "BlueWon" : "Draw");
		}
	}

	public void ReturnToMenu()
	{
		_ = NetworkManager.Leave();
		SceneManager.LoadScene(0);
	}

	protected override void Refresh() => OnChangeTurn = null;
}

public enum ScoreMode
{
	BlockDestroy,
	StackDestroy,
	Ground,
	FullStack,
	AttackAbility,
	MoveAbility,
	ReverseAbility
}
