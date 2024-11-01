using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace Utilities.Data
{
	public static class DataManager
	{
		public static void SaveData<T>(T data, string additionalPath = null) where T : class
		{
			var formatter = new BinaryFormatter();
			var path = Application.persistentDataPath;

			if (additionalPath != null)
			{
				path += "/" + additionalPath;
				if (!Directory.Exists(path))
					Directory.CreateDirectory(path);
			}

			path += "/" + typeof(T).Name;
			if (File.Exists(path))
				File.Delete(path);
			using var stream = new FileStream(path, FileMode.Create);
			formatter.Serialize(stream, data);
		}

		public static T LoadData<T>(string additionalPath = null) where T : class
		{
			var formatter = new BinaryFormatter();
			var path = Application.persistentDataPath;

			if (additionalPath != null)
			{
				path += "/" + additionalPath;
				if (!Directory.Exists(path))
					return null;
			}

			path += "/" + typeof(T).Name;
			if (!File.Exists(path))
				return null;
			using var stream = new FileStream(path, FileMode.Open);
			return formatter.Deserialize(stream) as T;
		}

		public static string GetPrefs(string key, string def = "") =>
			PlayerPrefs.GetString(Application.platform + key, def);

		public static void SetPrefs(string key, string value) =>
			PlayerPrefs.SetString(Application.platform + key, value);

		public static void AddShort(this ByteArray container, short id) =>
			container.Data = container.Data.Concat(BitConverter.GetBytes(id)).ToArray();

		public static void AddBool(this ByteArray container, bool id) =>
			container.Data = container.Data.Concat(BitConverter.GetBytes(id)).ToArray();

		public static void AddV3Int(this ByteArray container, Vector3Int v3)
		{
			container.AddShort((short)v3.x);
			container.AddShort((short)v3.y);
			container.AddShort((short)v3.z);
		}

		public static void AddString(this ByteArray container, string data)
		{
			var res = Encoding.UTF8.GetBytes(data);
			container.Data = container.Data.Concat(BitConverter.GetBytes((short)res.Length)).Concat(res).ToArray();
		}

		public static short GetShort(this ByteArray refData)
		{
			var res = GetPreferredData(refData, 2);
			return BitConverter.ToInt16(res.Data);
		}

		public static bool GetBool(this ByteArray refData)
		{
			var res = GetPreferredData(refData, 1);
			return BitConverter.ToBoolean(res.Data);
		}

		public static float GetFloat(this ByteArray refData)
		{
			var res = GetPreferredData(refData, 4);
			return BitConverter.ToSingle(res.Data);
		}

		public static string GetString(this ByteArray refData)
		{
			var length = GetShort(refData);
			var res = GetPreferredData(refData, length);
			return Encoding.UTF8.GetString(res.Data);
		}

		public static Quaternion GetRotation(this ByteArray refData)
		{
			var res = GetPreferredData(refData, 8);
			var split = res.Split(2).ToList();
			return new(split[0].GetFloat(), split[1].GetFloat(), split[2].GetFloat(), split[3].GetFloat());
		}

		public static Vector3Int GetV3Int(this ByteArray refData)
		{
			var res = GetPreferredData(refData, 6);
			var split = res.Split(2).ToList();
			return new(split[0].GetShort(), split[1].GetShort(), split[2].GetShort());
		}

		private static ByteArray GetPreferredData(this ByteArray data, int length)
		{
			if (data.Data.Length < length)
				throw new("data length is to short!");
			var res = new ByteArray(data.Data[..length]);
			data.Data = data.Data[length..];
			return res;
		}

		private static bool IsEqual<T>(this IEnumerable<T> first, IEnumerable<T> second) where T : IEquatable<T>
		{
			var firstArray = first as T[] ?? first.ToArray();
			var secondArray = second as T[] ?? second.ToArray();
			if (firstArray.Length != secondArray.Length) return false;
			return !firstArray.Where((t, i) => !t.Equals(secondArray[i])).Any();
		}

		private static IEnumerable<ByteArray> Split(this ByteArray array, int size)
		{
			for (var i = 0; i < array.Data.Length / (float)size; i++)
				yield return new(array.Data.Skip(i * size).Take(size).ToArray());
		}
	}

	public class ByteArray
	{
		public byte[] Data;

		public ByteArray(byte[] data) => Data = data;

		public ByteArray(object[] data) : this(Array.Empty<byte>())
		{
			foreach (var o in data) AddData(o);
		}

		private void AddData(object data)
		{
			switch (data)
			{
				case short s:
					this.AddShort(s);
					break;
				case int i:
					this.AddShort((short)i);
					break;
				case string or Enum:
					this.AddString(data.ToString());
					break;
				case bool b:
					this.AddBool(b);
					break;
				case Vector3Int v3:
					this.AddV3Int(v3);
					break;
				default:
					throw new ArgumentException("type not handled: " + data.GetType().Name);
			}
		}

		public static implicit operator byte[](ByteArray byteArray) => byteArray.Data;
		public static implicit operator ByteArray(byte[] byteArray) => new(byteArray);
	}
}
