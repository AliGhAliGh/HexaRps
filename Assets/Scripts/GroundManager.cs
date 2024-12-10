using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Anim;
using Blocks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

public class GroundManager : Singleton<GroundManager>
{
	[SerializeField] private Tilemap map;
	[SerializeField] private TileBase defaultColor, redColor, blueColor;
	[SerializeField] private Rock rock;
	[SerializeField] private Paper paper;
	[SerializeField] private Scissors scissors;
	[SerializeField] private float blockHeight;

	private bool _isGroundChanged;

	private readonly Dictionary<Vector3Int, Stack<Block>> _map = new();

	public static bool IsGroundChanged
	{
		get
		{
			var temp = Instance._isGroundChanged;
			Instance._isGroundChanged = false;
			return temp;
		}
	}

	public static IEnumerator RefreshGround(ColorMode mode, IEnumerator next = null)
	{
		var list = Instance._map.Where(c => GetColor(c.Key) == mode)
			.OrderByDescending(c => c.Value.Count).Select(c => c.Key);
		foreach (var pos in list) yield return Attack(pos);
		yield return next;
	}

	public static IEnumerator Developer(Vector3Int pos)
	{
		yield return RefreshGround(GetColor(pos));
	}

	public static IEnumerator Reverse(Vector3Int pos, IEnumerator next = null)
	{
		var stack = GetStack(pos).ToList();
		stack.Reverse();
		for (var i = 0; i < stack.Count / 2; i++)
		{
			var first = stack[i].gameObject;
			var last = stack[^(i + 1)].gameObject;
			var firstPos = first.transform.position;
			yield return BlockAnimator.Mover(first,last.transform.position);
			yield return BlockAnimator.Mover(last, firstPos);
		}

		yield return next;
	}

	public static List<Vector3Int> GetPoses()
	{
		var res = new List<Vector3Int>();
		for (var i = -4; i <= 3; i++)
		for (var j = -3; j <= 1; j++)
		{
			var pos = new Vector3Int(i, j);
			if (IsInGround(pos))
				res.Add(pos);
		}

		return res;
	}

	private static Block GetBlock(BlockMode mode) =>
		mode switch
		{
			BlockMode.Paper => Instance.paper,
			BlockMode.Rock => Instance.rock,
			BlockMode.Scissors => Instance.scissors,
			_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
		};

	private static IEnumerator AddBlock(BlockMode blockMode, Vector3Int pos)
	{
		var block = GetBlock(blockMode);
		var instance = Instantiate(block);
		instance.SetTransparency(false);
		var stack = GetStack(pos);
		var targetPos = GetTopPos(pos);
		stack.Push(instance);
		instance.transform.position = targetPos;
		yield return BlockAnimator.Create(instance);
	}

	public static IEnumerator AddStack(List<BlockMode> stack, Vector3Int pos, IEnumerator next = null)
	{
		foreach (var blockMode in stack)
			yield return AddBlock(blockMode, pos);
		yield return next;
	}

	[NotNull] public static Stack<Block> GetStack(Vector3Int pos)
	{
		if (!Instance._map.TryGetValue(pos, out var stack)) stack = Instance._map[pos] = new();
		return stack;
	}

	public static IEnumerator Execute(Vector3Int pos, IEnumerator next = null)
	{
		yield return Absorb(pos);
		AdvanceColor(pos);
		yield return Attack(pos);
		yield return RefreshGround((ColorMode)(GameManager.CurrentTurn + 1));
		yield return RefreshGround((ColorMode)(GameManager.CurrentTurn + 1));
		yield return RefreshGround((ColorMode)(GameManager.CurrentTurn + 1), next);
	}

	private static void AdvanceColor(Vector3Int pos)
	{
		var color = GetColor(pos);
		var whiteGrounds = pos.GetPeriphery(1).Where(c => IsColoredInGround(c, ColorMode.Default)).ToList();
		SetColor(color, whiteGrounds.ToArray());
		var opposite = Opposite(color);
		foreach (var whiteGround in whiteGrounds) SetWhite(whiteGround);
		SetWhite(pos);
		return;

		void SetWhite(Vector3Int point)
		{
			foreach (var canBeWhite in point.GetPeriphery(1).Where(c => IsColoredInGround(c, opposite)))
			{
				if (canBeWhite.GetNeighbors(1).Any(c => IsColoredInGround(c, opposite) && HasAny(c)))
					continue;

				SetColor(ColorMode.Default, canBeWhite);
			}
		}
	}

	private static bool IsColoredInGround(Vector3Int pos, ColorMode color) => IsInGround(pos) && GetColor(pos) == color;

	private static IEnumerator Attack(Vector3Int pos)
	{
		if (!HasAny(pos)) yield break;

		var color = GetColor(pos);
		var opposite = Opposite(color);
		var loser = Lower(GetStack(pos).Peek().mode);
		var canBeAttacked = pos.GetPeriphery(1).Where(c => IsColoredInGround(c, opposite));
		foreach (var point in canBeAttacked)
		{
			if (GetStack(point).TryPeek()?.mode == loser)
			{
				yield return RemoveBlock(point);
				yield return Requeue(pos);
				if (HasAny(point))
				{
					yield return Attack(pos);
					break;
				}

				yield return MoveStack(pos, point);
				SetColor(color, point);
				AdvanceColor(point);
				yield return Attack(point);
				break;
			}
		}
	}

	private static bool HasAny(Vector3Int pos) => GetStack(pos).Count > 0;

	private static IEnumerator Absorb(Vector3Int pos)
	{
		var currentStack = GetStack(pos);
		if (currentStack.Count == 0)
			yield break;

		var color = GetColor(pos);
		var mode = currentStack.Peek().mode;
		var neighbors = pos.GetPeriphery(1).Where(IsInGround).Where(c => GetColor(c) == color);
		foreach (var neighbor in neighbors)
		{
			var stack = GetStack(neighbor);
			while (stack.TryPeek()?.mode == mode && currentStack.Count < GameManager.MaxStackSize)
				yield return MoveBlock(neighbor, pos);
			yield return Attack(neighbor);
		}
	}

	private static IEnumerator MoveStack(Vector3Int from, Vector3Int to)
	{
		if (HasAny(to)) yield break;

		var stack = GetStack(from);
		var temp = new Stack<Block>();
		var target = GetPosition(to);
		foreach (var block in stack)
		{
			temp.Push(block);
			var blockTarget = block.transform.position;
			blockTarget.x = target.x;
			blockTarget.z = target.z;
			yield return BlockAnimator.Mover(block.gameObject, blockTarget);
		}

		stack.Clear();
		var targetStack = GetStack(to);
		foreach (var block in temp) targetStack.Push(block);
	}

	private static bool IsInGround(Vector3Int pos)
	{
		var upperX = pos.y % 2 == 0 ? 3 : 2;
		return pos.x >= -4 && pos.x <= upperX && pos.y is >= -3 and <= 1;
	}

	private static IEnumerator MoveBlock(Vector3Int from, Vector3Int to)
	{
		var fromStack = GetStack(from);
		if (fromStack.Count == 0) yield break;
		var block = fromStack.Pop();
		yield return BlockAnimator.Move(block, GetTopPos(to));
		GetStack(to).Push(block);
	}

	private static Vector3 GetTopPos(Vector3Int pos)
	{
		var res = GetPosition(pos);
		res.y = Instance.blockHeight * (GetStack(pos).Count + .5f);
		return res;
	}

	public static IEnumerator Clear(Vector3Int pos, object next = null)
	{
		var stack = GetStack(pos);
		while (stack.Count > 0)
			yield return RemoveBlock(pos);
		yield return next;
	}

	private static IEnumerator RemoveBlock(Vector3Int pos)
	{
		var stack = GetStack(pos);
		if (stack.Count == 0) yield break;
		Instance._isGroundChanged = true;
		yield return BlockAnimator.Destroy(stack.Pop());
	}

	private static IEnumerator Requeue(Vector3Int pos)
	{
		var stack = GetStack(pos);
		if (stack.Count == 0) yield break;
		var block = stack.Pop();
		yield return BlockAnimator.Mover(block.gameObject,
			new(block.transform.position.x, Instance.blockHeight / 2, block.transform.position.z));
		var temp = new Stack<Block>();
		while (stack.Count > 0)
		{
			var tempBlock = stack.Pop();
			var target = tempBlock.transform.position;
			target.y += Instance.blockHeight;
			yield return BlockAnimator.Mover(tempBlock.gameObject, target);
			temp.Push(tempBlock);
		}

		stack.Push(block);
		while (temp.Count > 0) stack.Push(temp.Pop());
	}

	public static ColorMode GetColor(Vector3Int pos) => GetColor(Instance.map.GetTile(pos));

	public static Vector3Int GetPosition(Vector3 pos) => Instance.map.WorldToCell(pos);

	private static Vector3 GetPosition(Vector3Int pos) => Instance.map.CellToWorld(pos);

	public static void SetColor(ColorMode mode, params Vector3Int[] tiles)
	{
		var color = GetColor(mode);
		foreach (var tile in tiles) Instance.map.SetTile(tile, color);
	}

	private static TileBase GetColor(ColorMode mode) =>
		mode switch
		{
			ColorMode.Default => Instance.defaultColor,
			ColorMode.Red => Instance.redColor,
			ColorMode.Blue => Instance.blueColor,
			_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
		};

	private static ColorMode GetColor(TileBase tile)
	{
		if (tile == Instance.redColor)
			return ColorMode.Red;
		return tile == Instance.blueColor ? ColorMode.Blue : ColorMode.Default;
	}

	private static ColorMode Opposite(ColorMode mode)
	{
		if (mode is ColorMode.Default) throw new Exception("white has no opposite!");
		return mode is ColorMode.Blue ? ColorMode.Red : ColorMode.Blue;
	}

	private static BlockMode Lower(BlockMode blockMode) =>
		blockMode switch
		{
			BlockMode.Paper => BlockMode.Rock,
			BlockMode.Rock => BlockMode.Scissors,
			BlockMode.Scissors => BlockMode.Paper,
			_ => throw new ArgumentOutOfRangeException(nameof(blockMode), blockMode, null)
		};
}

public enum ColorMode
{
	Default,
	Red,
	Blue
}
