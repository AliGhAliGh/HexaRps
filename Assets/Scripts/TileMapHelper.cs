using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

public static class TileMapHelper
{
	public static IEnumerable<Vector3Int> GetNeighbors(this Vector3Int pos, int range)
	{
		var res = new Dictionary<Vector3Int, int> { [pos] = range };
		return range == 0 ? res.Keys : GetNeighbors(pos, range, res);
	}

	public static IEnumerable<Vector3Int> GetArea(this Vector3Int pos, int range)
	{
		if (range <= 0) throw new ArgumentOutOfRangeException(nameof(range));
		var res = pos.GetPeriphery(range).ToList();
		for (var i = range - 1; i >= 0; i--) res.AddRange(pos.GetPeriphery(i));
		return res;
	}

	public static IEnumerable<Vector3Int> GetPeriphery(this Vector3Int pos, int range)
	{
		if (range == 0)
			return new[] { pos };
		var down = pos.y % 2 != 0 ? 1 : 0;
		var up = 1 - down;
		var res = new List<Vector3Int>
		{
			new(pos.x + range, pos.y),
			new(pos.x - range, pos.y)
		};
		for (var i = 1; i <= range; i++)
		{
			var deltaUp = i / 2 + (i % 2 == 0 ? 0 : up);
			var deltaDown = i / 2 + (i % 2 == 0 ? 0 : down);
			res.Add(new(pos.x + range - deltaUp, pos.y + i));
			res.Add(new(pos.x - range + deltaDown, pos.y + i));
			res.Add(new(pos.x + range - deltaUp, pos.y - i));
			res.Add(new(pos.x - range + deltaDown, pos.y - i));
		}

		for (var i = 1; i < range; i++)
		{
			var deltaUp = range / 2 + (range % 2 == 0 ? 0 : up);
			res.Add(new(pos.x + range - deltaUp - i, pos.y + range));
			res.Add(new(pos.x + range - deltaUp - i, pos.y - range));
		}

		return res;
	}

	public static List<Vector3Int> Sort(this IEnumerable<Vector3Int> me)
	{
		var list = me.ToList();
		var res = new List<Vector3Int> { list.First() };
		list.RemoveAt(0);
		while (list.Count > 0)
		{
			var last = res.Last();
			var temp = list.First(c => c.IsNeighbor(last));
			list.Remove(temp);
			res.Add(temp);
		}

		return res;
	}

	private static IEnumerable<Vector3Int> GetNeighbors(this Vector3Int pos)
	{
		var xOffset = 0;
		if (pos.y % 2 != 0)
			xOffset = 1;

		return new List<Vector3Int>
		{
			new(pos.x + 1, pos.y),
			new(pos.x - 1, pos.y),
			new(pos.x + xOffset, pos.y + 1),
			new(pos.x + xOffset - 1, pos.y + 1),
			new(pos.x + xOffset, pos.y - 1),
			new(pos.x + xOffset - 1, pos.y - 1)
		};
	}

	private static bool IsNeighbor(this Vector3Int pos, Vector3Int other)
	{
		var xOffset = 0;
		if (pos.y % 2 != 0)
			xOffset = 1;

		if (other.y == pos.y)
			return pos.x == other.x + 1 || pos.x == other.x - 1;

		if (other.y == pos.y - 1)
			return pos.x + xOffset == other.x || pos.x + xOffset - 1 == other.x;

		if (other.y == pos.y + 1)
			return pos.x + xOffset == other.x || pos.x + xOffset - 1 == other.x;

		return false;
	}

	private static IEnumerable<Vector3Int> GetNeighbors(
		this Vector3Int pos, int range, Dictionary<Vector3Int, int> res)
	{
		var addedStack = new Stack<Vector3Int>();
		addedStack.Push(pos);

		for (var i = range; i > 0; i--)
		{
			var list = addedStack.ToList();
			while (list.TryPop(out var current))
				foreach (var c in current.GetNeighbors())
					Add(c, i);
		}

		return res.Keys;

		void Add(Vector3Int newPos, int r)
		{
			if (res.TryAdd(newPos, r) || res[newPos] < r)
				addedStack.Push(newPos);
		}
	}

	public static HexNode GetPathTo(this Vector3Int start, Vector3Int end)
	{
		var edges = new List<HexNode> { new(start) };
		var traversed = new HashSet<Vector3Int>();
		while (edges.Count > 0)
		{
			var current = edges.AMin((c1, c2) => c1.F < c2.F);
			if (current.Pos == end)
				return current;

			edges.Remove(current);
			traversed.Add(current.Pos);

			foreach (var n in current.Pos.GetNeighbors())
			{
				if (traversed.Contains(n)) continue;
				var node = edges.FirstOrDefault(c => c.Pos == n);
				if (node != null)
					node.CheckParent(current);
				else edges.Add(new HexNode(current, n, GetH(n)));
			}
		}

		return null;

		int GetH(Vector3Int pos) => Mathf.Abs(end.x - pos.x) + Mathf.Abs(end.y - pos.y);
	}

	public static int Count(this IEnumerator<Vector3Int> path)
	{
		var count = 0;
		while (path.MoveNext()) count++;
		return count;
	}

	public static IEnumerator<Vector3Int> GetEnumerator(this HexNode node)
	{
		while (node != null)
		{
			yield return node.Pos;
			node = node.Parent;
		}
	}
}

public class HexNode
{
	public readonly Vector3Int Pos;

	private readonly int _h;
	private int _g;

	public HexNode Parent { get; private set; }

	public int F => _g + _h;

	public HexNode(Vector3Int pos)
	{
		Pos = pos;
		_g = _h = 0;
		Parent = null;
	}

	public HexNode(HexNode parent, Vector3Int pos, int h)
	{
		SetParent(parent);
		Pos = pos;
		_h = h;
	}

	private void SetParent(HexNode parent)
	{
		Parent = parent;
		_g = parent._g + 1;
	}

	public void CheckParent(HexNode parent)
	{
		if (Parent._g > parent._g)
			SetParent(parent);
	}
}
