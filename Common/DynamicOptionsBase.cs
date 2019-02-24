using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
namespace Common
{
    [Serializable]
    public abstract class DynamicOptionsBase : INotifyPropertyChanged
    {
        Dictionary<string, Item> _dict = new Dictionary<string, Item>();
        protected Dictionary<string, Item> Dict { get => _dict; }
        public string Serialize()
        {
            var sb = new StringBuilder();
            foreach (var kv in _dict)
            {
                var k = kv.Key;
                var v = kv.Value;
                sb.Append(k);
                sb.Append("=");
                sb.Append(v.Serializer(v.Value));
                sb.Append(Environment.NewLine);
            }
            return sb.ToString();
        }
        protected abstract void Init();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s">name=value separated by CRLF</param>
        public void Deserialize(string s)
        {
            Reset();
            if (string.IsNullOrEmpty(s))
                return;
            var arr = s.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in arr)
            {
                var kv = line.Split('=');
                if (kv.Length != 2)
                    continue;
                var k = kv[0];
                var v = kv[1];
                if (_dict.TryGetValue(k, out Item item))
                {
                    try
                    {
                        item.Value = item.Deserializer(v);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        //最初にReset()しているから、ここに来たらitem.Value==item.DefaultValue
                    }
                    if (!item.Predicate(item.Value))
                    {
                        item.Value = item.DefaultValue;
                        RaisePropertyChanged(k);
                    }
                }
            }
        }
        protected dynamic GetValue([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var item = _dict[propertyName];
            return item.Value;
        }
        protected void SetValue(dynamic d, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            var item = _dict[propertyName];
            if(item.Predicate(d))
                item.Value = d;
            RaisePropertyChanged(propertyName);
        }
        public DynamicOptionsBase()
        {
            Init();
            Reset();
        }
        /// <summary>
        /// すべての値を初期化する
        /// </summary>
        public void Reset()
        {
            foreach (var kv in _dict)
            {
                var k = kv.Key;
                var v = kv.Value;
                v.Value = v.DefaultValue;
                RaisePropertyChanged(k);
            }
        }
        private void CheckValidation()
        {
            foreach (var kv in _dict)
            {
                var k = kv.Key;
                var v = kv.Value;
                if (!v.Predicate(v.Value))
                {
                    v.Value = v.DefaultValue;
                }
            }
        }
        protected static T DeepClone<T>(T obj)
        {
            return Test.ObjectExtensions.DeepCopy(obj);
        }
        #region INotifyPropertyChanged
        [NonSerialized]
        private System.ComponentModel.PropertyChangedEventHandler _propertyChanged;
        /// <summary>
        /// 
        /// </summary>
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChanged += value; }
            remove { _propertyChanged -= value; }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            _propertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
        #endregion
        [Serializable]
        protected class Item
        {
            /// <summary>
            /// 文字列をDeserializerに通した後の値の妥当性を評価する。
            /// 文字列に形式上の問題がある場合はDeserializerで例外が投げられるだろうからcatchしてDefaultValueを入れる
            /// </summary>
            public Predicate<dynamic> Predicate { get; set; }
            public dynamic DefaultValue { get; set; }
            public dynamic Value { get; set; }
            public Func<dynamic, string> Serializer { get; set; }
            public Func<string, dynamic> Deserializer { get; set; }

        }
    }
    namespace Test
    {
        //https://github.com/Burtsev-Alexey/net-object-deep-copy
        using System.Collections.Generic;
        using System.Reflection;
        using ArrayExtensions;
        using System;

        public static class ObjectExtensions
        {
            private static readonly MethodInfo CloneMethod = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance);

            public static bool IsPrimitive(this Type type)
            {
                if (type == typeof(String)) return true;
                return (type.IsValueType & type.IsPrimitive);
            }

            public static Object Copy(this Object originalObject)
            {
                return InternalCopy(originalObject, new Dictionary<Object, Object>(new ReferenceEqualityComparer()));
            }
            private static Object InternalCopy(Object originalObject, IDictionary<Object, Object> visited)
            {
                if (originalObject == null) return null;
                var typeToReflect = originalObject.GetType();
                if (IsPrimitive(typeToReflect)) return originalObject;
                if (visited.ContainsKey(originalObject)) return visited[originalObject];
                if (typeof(Delegate).IsAssignableFrom(typeToReflect))
                {
                    //デリゲートの場合はオリジナルをそのまま帰すようにした。
                    return originalObject;
                }
                var cloneObject = CloneMethod.Invoke(originalObject, null);
                if (typeToReflect.IsArray)
                {
                    var arrayType = typeToReflect.GetElementType();
                    if (IsPrimitive(arrayType) == false)
                    {
                        Array clonedArray = (Array)cloneObject;
                        clonedArray.ForEach((array, indices) => array.SetValue(InternalCopy(clonedArray.GetValue(indices), visited), indices));
                    }

                }
                visited.Add(originalObject, cloneObject);
                CopyFields(originalObject, visited, cloneObject, typeToReflect);
                RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect);
                return cloneObject;
            }

            private static void RecursiveCopyBaseTypePrivateFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect)
            {
                if (typeToReflect.BaseType != null)
                {
                    RecursiveCopyBaseTypePrivateFields(originalObject, visited, cloneObject, typeToReflect.BaseType);
                    CopyFields(originalObject, visited, cloneObject, typeToReflect.BaseType, BindingFlags.Instance | BindingFlags.NonPublic, info => info.IsPrivate);
                }
            }

            private static void CopyFields(object originalObject, IDictionary<object, object> visited, object cloneObject, Type typeToReflect, BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy, Func<FieldInfo, bool> filter = null)
            {
                foreach (FieldInfo fieldInfo in typeToReflect.GetFields(bindingFlags))
                {
                    if (filter != null && filter(fieldInfo) == false) continue;
                    if (IsPrimitive(fieldInfo.FieldType)) continue;
                    var originalFieldValue = fieldInfo.GetValue(originalObject);
                    var clonedFieldValue = InternalCopy(originalFieldValue, visited);
                    fieldInfo.SetValue(cloneObject, clonedFieldValue);
                }
            }
            public static T DeepCopy<T>(this T original)
            {
                return (T)Copy((Object)original);
            }
        }

        public class ReferenceEqualityComparer : EqualityComparer<Object>
        {
            public override bool Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }
            public override int GetHashCode(object obj)
            {
                if (obj == null) return 0;
                return obj.GetHashCode();
            }
        }

        namespace ArrayExtensions
        {
            public static class ArrayExtensions
            {
                public static void ForEach(this Array array, Action<Array, int[]> action)
                {
                    if (array.LongLength == 0) return;
                    ArrayTraverse walker = new ArrayTraverse(array);
                    do action(array, walker.Position);
                    while (walker.Step());
                }
            }

            internal class ArrayTraverse
            {
                public int[] Position;
                private int[] maxLengths;

                public ArrayTraverse(Array array)
                {
                    maxLengths = new int[array.Rank];
                    for (int i = 0; i < array.Rank; ++i)
                    {
                        maxLengths[i] = array.GetLength(i) - 1;
                    }
                    Position = new int[array.Rank];
                }

                public bool Step()
                {
                    for (int i = 0; i < Position.Length; ++i)
                    {
                        if (Position[i] < maxLengths[i])
                        {
                            Position[i]++;
                            for (int j = 0; j < i; j++)
                            {
                                Position[j] = 0;
                            }
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

    }
}
