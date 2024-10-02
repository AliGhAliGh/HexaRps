using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Utilities.Data
{
    public class FlexData
    {
        private readonly Dictionary<string, FlexData> _children = new();

        private (object _data, DataType _type) _data;

        private bool IsObject => _children.Count > 0;

        private bool IsDataArray => Data is List<FlexData>;

        private object Data
        {
            get => _data._data;
            set
            {
                var res = value switch
                {
                    null => DataType.None,
                    int => DataType.Int,
                    long => DataType.Long,
                    float => DataType.Float,
                    bool => DataType.Boolean,
                    string => DataType.String,
                    List<FlexData> => DataType.List,
                    _ => DataType.None
                };

                _data = (value, res);
                if (value is not null) _children.Clear();
            }
        }

        public FlexData(object data = null) => Data = data;

        public static FlexData Parse(string data) => Parse(JObject.Parse(data));

        private static FlexData Parse(JObject data)
        {
            var flexData = new FlexData
            {
                Data = null
            };

            foreach (var jProperty in data.Properties())
            {
                if (jProperty.Value.Type is JTokenType.Object)
                    flexData._children.Add(jProperty.Name, (JObject)jProperty.Value);
                else
                    flexData._children.Add(jProperty.Name, new(jProperty.Value.Type switch
                    {
                        JTokenType.Integer => jProperty.Value.ToObject<int>(),
                        JTokenType.Float => jProperty.Value.ToObject<float>(),
                        JTokenType.String => jProperty.Value.ToObject<string>(),
                        JTokenType.Boolean => jProperty.Value.ToObject<bool>(),
                        JTokenType.Array => jProperty.Value.ToObject<List<FlexData>>(),
                        JTokenType.Null => null,
                        _ => throw new ArgumentOutOfRangeException()
                    }));
            }

            return flexData;
        }

        public override string ToString() => Data?.ToString();

        public string ToJson => ToJObject().ToString();

        private JObject ToJObject()
        {
            var res = new JObject();

            foreach (var j in _children)
            {
                if (j.Value.IsObject)
                    res.Add(j.Key, j.Value.ToJObject());
                else if (j.Value.IsDataArray)
                    res.Add(new JProperty(j.Key, (j.Value.Data as List<FlexData>)!.Select(c => c.Data).ToArray()));
                else if (j.Value.Data is null)
                    res.Add(j.Key, null);
                else
                    res.Add(new JProperty(j.Key, j.Value.Data));
            }

            return res;
        }

        public T GetData<T>(T def = default)
        {
            if (Data is T data)
                return data;

            try
            {
                return (T)Convert.ChangeType(Data, typeof(T));
            }
            catch (InvalidCastException)
            {
                LogManager.ShowMessage(Color.red, "invalid casting : " + nameof(T));
                return def;
            }
        }

        public void SetData<T>(T value) => Data = value;

        public FlexData this[string name]
        {
            get
            {
                if (_children.TryGetValue(name, out var data)) return data;
                if (Data != null) return null;
                data = new();
                _children.Add(name, data);
                return data;
            }
            set
            {
                Data = null;
                if (!_children.TryAdd(name, value))
                    _children[name].Data = value;
            }
        }

        #region Implicit

        public static implicit operator FlexData(string value) => new(value);

        public static implicit operator FlexData(long value) => new(value);

        public static implicit operator FlexData(float value) => new(value);

        public static implicit operator FlexData(bool value) => new(value);

        public static implicit operator FlexData(List<FlexData> value) => new(value);

        public static implicit operator FlexData(JObject value) => Parse(value);

        #endregion

        #region Explicit

        public static explicit operator string(FlexData value) => value.GetData<string>();

        public static explicit operator int(FlexData value) =>value.GetData<int>();

        public static explicit operator long(FlexData value) =>value.GetData<long>();

        public static explicit operator float(FlexData value) => value.GetData<float>();

        public static explicit operator bool(FlexData value) => value.GetData<bool>();

        #endregion
    }

    public enum DataType
    {
        Int,
        Long,
        Float,
        String,
        Boolean,
        List,
        None
    }
}
