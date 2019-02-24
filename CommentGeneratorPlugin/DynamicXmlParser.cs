using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Dynamic;

namespace CommentViewer.Plugin
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// case sensitive
    /// 同じ要素名を持つ要素が複数ある場合には一番最初のものが優先される。
    /// </remarks>
    public class DynamicXmlParser : DynamicObject
    {
        /// <summary>
        /// 
        /// </summary>
        private XElement _element;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        private DynamicXmlParser(TextReader reader)
        {
            _element = XElement.Load(reader);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        private DynamicXmlParser(string xml)
            : this(new StringReader(xml))
        {
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        private DynamicXmlParser(XElement element)
        {
            _element = element;
        }
        public static dynamic Parse(string xml)
        {
            return new DynamicXmlParser(xml);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="binder"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (_element == null)
            {
                result = null;
                return false;
            }

            var child = _element.Element(binder.Name);
            if (child == null)
            {
                result = null;
                return false;
            }
            else
            {
                result = new DynamicXmlParser(child);
                return true;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //<p>foo<b>bar</b></p>みたいな場合、_element.valueだと"foobar"になってしまう。でも多くの場合fooを取得したい。
            //子ノードのタイプを見て、XTextのノードだけを抽出し、その文字列を結合する。
            if (_element == null)
                return string.Empty;
            var textNodes = _element.Nodes().Where(c => c.NodeType == System.Xml.XmlNodeType.Text || c.NodeType == System.Xml.XmlNodeType.CDATA).Select(c => (XText)c);
            string str = "";
            foreach (var t in textNodes)
            {
                str += t.Value;
            }
            return str;
        }
        /// <summary>
        /// 
        /// </summary>
        public string LocalName
        {
            get { return (_element != null) ? _element.Name.LocalName : string.Empty; }
        }
        /// <summary>
        /// 属性値
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public string this[string attr]
        {
            get
            {
                return (_element == null) ? string.Empty : _element.Attribute(attr).Value;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasAttribute(string name)
        {
            return (_element != null && _element.Attribute(name) != null);
        }
        /// <summary>
        /// 同じ要素名が複数ある場合にindexで要素を指定。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        /// <remarks>同じ要素名を持つ要素の数はCount()で取得できる。</remarks>
        public DynamicXmlParser this[int index]
        {
            get
            {
                var parent = _element.Parent;
                if (parent == null)
                    return null;
                int i = 0;
                foreach (var dog in parent.Elements(_element.Name))
                {
                    if (i == index)
                    {
                        return new DynamicXmlParser(dog);
                    }
                    i++;
                }
                return null;
            }
        }
        /// <summary>
        /// 同じ要素名を持つ要素の数
        /// </summary>
        /// <returns></returns>
        public int Count()
        {
            var parent = _element.Parent;
            if (parent == null)
                return 0;
            else
            {
                return parent.Elements(_element.Name).Count();
            }
        }
        public IEnumerable<DynamicXmlParser> GetEnumerator()
        {
            var parent = _element.Parent;
            if (parent == null)
                yield break;
            else
            {
                foreach (var e in parent.Elements(_element.Name))
                    yield return new DynamicXmlParser(e);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public bool HasElements
        {
            get
            {
                return _element.HasElements;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool HasElement(string name)
        {
            var s = _element.Element(name);
            return (s != null);
        }
        public string GetElementValue(string name)
        {
            if (!HasElement(name))
                return null;
            else
                return _element.Element(name).Value;
        }
        /// <summary>
        /// 
        /// </summary>
        public bool HasAttributes
        {
            get
            {
                return _element.HasAttributes;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parser"></param>
        /// <returns></returns>
        public static implicit operator string(DynamicXmlParser parser)
        {
            return parser.ToString();
        }
        /// <summary>
        /// 子ノードの名前
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetChildrenNames()
        {
            //if (_element == null)
            //    yield break;
            //var ar = _element.Elements();
            //foreach (var el in ar)
            //{
            //    yield return el.Name.LocalName;
            //}
            foreach (var child in GetChildren())
            {
                yield return child.LocalName;
            }
        }
        public IEnumerable<DynamicXmlParser> GetChildren()
        {
            if (_element == null)
                yield break;
            var ar = _element.Elements();
            foreach (var el in ar)
            {
                yield return new DynamicXmlParser(el);
            }
        }
    }
}
