using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

public class GroundManager : Singleton<GroundManager>
{
	[SerializeField] private Tilemap map;
	[SerializeField] private TileBase defaultColor, redColor, blueColor;

	private readonly HashSet<Vector3Int> _obstacles = new();

	public static void SetObstacle(Vector3Int pos) => Instance._obstacles.Add(pos);

	public static bool CanEnter(Vector3Int pos) => !Instance._obstacles.Contains(pos);

	public static Vector3Int GetPosition(Vector3 pos) => Instance.map.WorldToCell(pos);

	public static Vector2 GetPosition(Vector3Int pos) => Instance.map.CellToWorld(pos);

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
