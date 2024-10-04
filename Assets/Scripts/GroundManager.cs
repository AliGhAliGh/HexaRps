using System;
using System.Collections.Generic;
using Blocks;
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

	private readonly Dictionary<Vector3Int, Stack<Block>> _map = new();

	private static Block GetBlock(BlockMode mode) =>
		mode switch
		{
			BlockMode.Paper => Instance.paper,
			BlockMode.Rock => Instance.rock,
			BlockMode.Scissors => Instance.scissors,
			_ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
		};

	public static void AddBlock(BlockMode blockMode, Vector3Int pos)
	{
		var block = GetBlock(blockMode);
		var instance = Instantiate(block);
		var v2 = GetPosition(pos);

		if (Instance._map.TryGetValue(pos, out var stack))
			stack.Push(instance);
		else
		{
			stack = Instance._map[pos] = new();
			stack.Push(instance);
		}

		instance.transform.position = new Vector3(v2.x, Instance.blockHeight * (stack.Count - .5f), v2.z);
	}

	public static void RemoveBlock(Vector3Int pos)
	{
		if (!Instance._map.TryGetValue(pos, out var stack)) return;
		Destroy(stack.Pop().gameObject);
	}

	public static void Pushback(Vector3Int pos)
	{
		if (!Instance._map.TryGetValue(pos, out var stack)) return;
		var block = stack.Pop();
		block.transform.position =
			new(block.transform.position.x, Instance.blockHeight / 2, block.transform.position.z);
		var temp = new Stack<Block>();
		while (stack.Count > 0)
		{
			var tempBlock = stack.Pop();
			tempBlock.transform.position += new Vector3(0, Instance.blockHeight, 0);
			temp.Push(tempBlock);
		}

		stack.Push(block);
		while (temp.Count > 0) stack.Push(temp.Pop());
	}

	public static Vector3Int GetPosition(Vector3 pos) => Instance.map.WorldToCell(pos);

	public static Vector3 GetPosition(Vector3Int pos) => Instance.map.CellToWorld(pos);

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
}

public enum ColorMode
{
	Default,
	Red,
	Blue
}
