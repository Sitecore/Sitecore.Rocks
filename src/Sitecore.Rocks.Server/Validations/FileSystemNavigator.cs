// © 2015-2016 Sitecore Corporation A/S. All rights reserved.

using System;
using System.IO;
using System.Xml;
using System.Xml.XPath;

namespace Sitecore.Rocks.Server.Validations
{
    public class FileSystemNavigator : XPathNavigator
    {
        public enum NodeTypes
        {
            Root,

            Element,

            Attribute,

            Text
        };

        private static readonly string[][] attributeIds = new[]
        {
            new[]
            {
                "Name",
                "CreationTime"
            },
            new[]
            {
                "Name",
                "CreationTime",
                "Length"
            }
        };

        private static readonly int[] numberOfAttributes =
        {
            2,
            3
        };

        private readonly NameTable nametable;

        internal NavigatorState State;

        public FileSystemNavigator(string rootNode)
        {
            FileSystemInfo document = Directory.CreateDirectory(rootNode);

            nametable = new NameTable();
            nametable.Add(string.Empty);

            if (!document.Exists)
            {
                var tempStream = File.Open(rootNode, FileMode.OpenOrCreate);
                tempStream.Close();
                document = new FileInfo(rootNode);
            }

            if (document.Exists)
            {
                State = new NavigatorState(document);
            }
            else
            {
                throw new Exception("Root node must be a directory or a file");
            }
        }

        public FileSystemNavigator(FileSystemInfo document)
        {
            nametable = new NameTable();
            nametable.Add(string.Empty);

            if (document.Exists)
            {
                State = new NavigatorState(document);
            }
            else
            {
                throw new Exception("Root node must be a directory or a file");
            }
        }

        public FileSystemNavigator(FileSystemNavigator navigator)
        {
            State = new NavigatorState(navigator.State);
            nametable = (NameTable)navigator.NameTable;
        }

        public int AttributeCount
        {
            get
            {
                if (State.Node != NodeTypes.Root)
                {
                    return numberOfAttributes[State.ElementType];
                }

                return 0;
            }
        }

        public override string BaseURI
        {
            get { return string.Empty; }
        }

        public int ChildCount
        {
            get
            {
                switch (State.Node)
                {
                    case NodeTypes.Root:
                        return 1;
                    case NodeTypes.Element:
                        if (State.ElementType == 0)
                        {
                            return ((DirectoryInfo)State.Doc).GetFileSystemInfos().Length;
                        }
                        return 0;
                    default:
                        return 0;
                }
            }
        }

        public override bool HasAttributes
        {
            get { return (State.Node != NodeTypes.Root) && (State.Node != NodeTypes.Attribute) && (State.Node != NodeTypes.Text); }
        }

        public override bool HasChildren
        {
            get { return ChildCount > 0; }
        }

        public int IndexInParent
        {
            get { return State.ElementIndex; }
        }

        public override bool IsEmptyElement
        {
            get { return State.ElementType == 1; }
        }

        public override string LocalName
        {
            get
            {
                nametable.Add(Name);
                return nametable.Get(Name);
            }
        }

        public override string Name
        {
            get
            {
                switch (State.Node)
                {
                    case NodeTypes.Text:
                        return State.TextValue;
                    case NodeTypes.Attribute:
                        return State.AttributeText;
                    case NodeTypes.Element:
                        return State.ElementText;
                    default:
                        return string.Empty;
                }
            }
        }

        public override string NamespaceURI
        {
            get { return nametable.Get(string.Empty); }
        }

        public override XmlNameTable NameTable
        {
            get { return nametable; }
        }

        public override XPathNodeType NodeType
        {
            get
            {
                switch (State.Node)
                {
                    case NodeTypes.Root:
                        return XPathNodeType.Root;
                    case NodeTypes.Element:
                        return XPathNodeType.Element;
                    case NodeTypes.Attribute:
                        return XPathNodeType.Attribute;
                    case NodeTypes.Text:
                        return XPathNodeType.Text;
                }
                return XPathNodeType.All;
            }
        }

        public override string Prefix
        {
            get { return nametable.Get(string.Empty); }
        }

        public override string Value
        {
            get { return State.TextValue; }
        }

        public override string XmlLang
        {
            get { return "en-us"; }
        }

        public override XPathNavigator Clone()
        {
            return new FileSystemNavigator(this);
        }

        public override string GetAttribute(string localName, string namespaceUri)
        {
            if (HasAttributes)
            {
                int i;
                for (i = 0; i < numberOfAttributes[State.ElementType]; i++)
                {
                    if (attributeIds[State.ElementType][i] == localName)
                    {
                        break;
                    }
                }

                if (i < numberOfAttributes[State.ElementType])
                {
                    var tempAttribute = State.Attribute;
                    var tempNodeType = State.Node;
                    State.Attribute = i;
                    State.Node = NodeTypes.Attribute;
                    var attributeValue = State.TextValue;
                    State.Node = tempNodeType;
                    State.Attribute = tempAttribute;

                    return attributeValue;
                }
            }

            return string.Empty;
        }

        public override string GetNamespace(string localname)
        {
            return string.Empty;
        }

        public override bool IsSamePosition(XPathNavigator other)
        {
            if (other is FileSystemNavigator)
            {
                if (State.Node == NodeTypes.Root)
                {
                    return ((FileSystemNavigator)other).State.Node == NodeTypes.Root;
                }

                return State.Doc.FullName == ((FileSystemNavigator)other).State.Doc.FullName;
            }

            return false;
        }

        public override bool MoveTo(XPathNavigator other)
        {
            if (other is FileSystemNavigator)
            {
                State = new NavigatorState(((FileSystemNavigator)other).State);
                return true;
            }

            return false;
        }

        public override bool MoveToAttribute(string localName, string namespaceURI)
        {
            if (State.Node == NodeTypes.Attribute)
            {
                MoveToElement();
            }

            if (State.Node == NodeTypes.Element)
            {
                int i;
                for (i = 0; i < AttributeCount; i++)
                {
                    if (attributeIds[State.ElementType][i] == localName)
                    {
                        State.Attribute = i;
                        State.Node = NodeTypes.Attribute;
                        return true;
                    }
                }
            }

            return false;
        }

        public bool MoveToChild(int i)
        {
            if (i >= 0)
            {
                if (State.Node == NodeTypes.Root && i == 0)
                {
                    State.Doc = Directory.CreateDirectory(State.Root);
                    State.ElementType = 0;
                    if (!State.Doc.Exists)
                    {
                        var tempStream = File.Open(State.Root, FileMode.OpenOrCreate);
                        tempStream.Close();
                        State.Doc = new FileInfo(State.Root);
                        State.ElementType = 1;
                    }

                    State.Node = NodeTypes.Element;
                    State.Attribute = -1;
                    State.ElementIndex = 0;

                    return true;
                }

                if (State.Node == NodeTypes.Element && State.ElementType == 0)
                {
                    var directoryEnumerator = ((DirectoryInfo)State.Doc).GetFileSystemInfos();

                    if (i < directoryEnumerator.Length)
                    {
                        State.Node = NodeTypes.Element;
                        State.Attribute = -1;
                        State.ElementIndex = i;

                        if (directoryEnumerator[i] is DirectoryInfo)
                        {
                            State.Doc = directoryEnumerator[i];
                            State.ElementType = 0;
                        }
                        else if (directoryEnumerator[i] is FileInfo)
                        {
                            State.Doc = directoryEnumerator[i];
                            State.ElementType = 1;
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public bool MoveToElement()
        {
            State.Attribute = -1;
            State.Node = NodeTypes.Element;

            if (State.Doc is DirectoryInfo)
            {
                State.ElementType = 0;
            }
            else
            {
                State.ElementType = 1;
            }

            return true;
        }

        public override bool MoveToFirst()
        {
            var tempState = (FileSystemNavigator)Clone();
            if (MoveToParent())
            {
                if (MoveToChild(0))
                {
                    return true;
                }
            }

            State = new NavigatorState(tempState.State);
            return false;
        }

        public override bool MoveToFirstAttribute()
        {
            if (State.Node == NodeTypes.Attribute)
            {
                MoveToElement();
            }

            if (AttributeCount > 0)
            {
                State.Attribute = 0;
                State.Node = NodeTypes.Attribute;
                return true;
            }

            return false;
        }

        public override bool MoveToFirstChild()
        {
            var tempState = (FileSystemNavigator)Clone();
            if (MoveToChild(0))
            {
                return true;
            }

            State = new NavigatorState(tempState.State);

            return false;
        }

        public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToId(string id)
        {
            return false;
        }

        public override bool MoveToNamespace(string Namespace)
        {
            return false;
        }

        //Tree Navigation

        public override bool MoveToNext()
        {
            var nextElement = IndexInParent + 1;
            var tempState = (FileSystemNavigator)Clone();

            if (MoveToParent())
            {
                if (MoveToChild(nextElement))
                {
                    return true;
                }
            }

            State = new NavigatorState(tempState.State);
            return false;
        }

        public override bool MoveToNextAttribute()
        {
            var tempAttribute = -1;
            if (State.Node == NodeTypes.Attribute)
            {
                tempAttribute = State.Attribute;
                MoveToElement();
            }

            if (tempAttribute + 1 < AttributeCount)
            {
                State.Attribute = tempAttribute + 1;
                State.Node = NodeTypes.Attribute;
                return true;
            }

            State.Node = NodeTypes.Attribute;
            State.Attribute = tempAttribute;
            return false;
        }

        public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
        {
            return false;
        }

        public override bool MoveToParent()
        {
            switch (State.Node)
            {
                case NodeTypes.Root:
                    return false;
                default:
                    if (State.Root != State.Doc.FullName)
                    {
                        if (State.Doc is DirectoryInfo)
                        {
                            State.Doc = ((DirectoryInfo)State.Doc).Parent;
                        }
                        else if (State.Doc is FileInfo)
                        {
                            State.Doc = ((FileInfo)State.Doc).Directory;
                        }
                        State.Node = NodeTypes.Element;
                        State.Attribute = -1;
                        State.ElementType = 0;
                        if (State.Root != State.Doc.FullName)
                        {
                            var fileSystemEnumerator = ((DirectoryInfo)State.Doc).Parent.GetFileSystemInfos();
                            for (var i = 0; i < fileSystemEnumerator.Length; i++)
                            {
                                if (fileSystemEnumerator[i].Name == State.Doc.Name)
                                {
                                    State.ElementIndex = i;
                                }
                            }
                        }
                        else
                        {
                            State.ElementIndex = 0;
                        }

                        return true;
                    }

                    MoveToRoot();
                    return true;
            }
        }

        public override bool MoveToPrevious()
        {
            var nextElement = IndexInParent - 1;
            var tempState = (FileSystemNavigator)Clone();
            if (MoveToParent())
            {
                if (MoveToChild(nextElement))
                {
                    return true;
                }
            }
            State = new NavigatorState(tempState.State);
            return false;
        }

        public override void MoveToRoot()
        {
            State.Node = NodeTypes.Root;
            State.Doc = new FileInfo(State.Root);
            State.Attribute = -1;
            State.ElementType = -1;
            State.ElementIndex = -1;
        }

        //This class keeps track of the state the navigator is in.

        internal class NavigatorState
        {
            //Represents the element that the navigator is currently at.
            //The type of attribute that the current node is. -1 if the
            // navigator is not currently positioned on an attribute.

            public int attribute;

            public FileSystemInfo doc;

            //elementType of 0 is a directory and elementType of 1 is a file

            public int elementIndex;

            public int elementType;

            //The type of the current node

            public NodeTypes node;

            private string root;

            public NavigatorState(FileSystemInfo document)
            {
                Doc = document;
                Root = doc.FullName;
                Node = NodeTypes.Root;
                Attribute = -1;
                ElementType = -1;
                ElementIndex = -1;
            }

            public NavigatorState(NavigatorState navState)
            {
                Doc = navState.Doc;
                Root = navState.Root;
                Node = navState.Node;
                Attribute = navState.Attribute;
                ElementType = navState.ElementType;
                ElementIndex = navState.ElementIndex;
            }

            public int Attribute
            {
                get { return attribute; }
                set { attribute = value; }
            }

            public string AttributeText
            {
                get
                {
                    if (Node == NodeTypes.Attribute)
                    {
                        return attributeIds[ElementType][Attribute];
                    }
                    return null;
                }
            }

            public FileSystemInfo Doc
            {
                get { return doc; }
                set { doc = value; }
            }

            public int ElementIndex
            {
                get { return elementIndex; }
                set { elementIndex = value; }
            }

            public string ElementText
            {
                get { return doc.Name; }
            }

            public int ElementType
            {
                get { return elementType; }
                set { elementType = value; }
            }

            public NodeTypes Node
            {
                get { return node; }
                set { node = value; }
            }

            public string Root
            {
                get { return root; }
                set { root = value; }
            }

            //Returns the TextValue of the current node

            public string TextValue
            {
                get
                {
                    switch (Node)
                    {
                        case NodeTypes.Root:
                            return null;
                        case NodeTypes.Element:
                            return null;
                        case NodeTypes.Attribute:
                            if (ElementType == 0)
                            {
                                var dInfo = (DirectoryInfo)Doc;
                                switch (Attribute)
                                {
                                    case 0:
                                        return dInfo.Name;
                                    case 1:
                                        return dInfo.CreationTime.ToString();
                                }
                            }
                            else if (ElementType == 1)
                            {
                                var fInfo = (FileInfo)Doc;
                                switch (Attribute)
                                {
                                    case 0:
                                        return fInfo.Name;
                                    case 1:
                                        return fInfo.CreationTime.ToString();
                                    case 2:
                                        return fInfo.Length.ToString();
                                }
                            }
                            break;
                        case NodeTypes.Text:
                            return null;
                    }
                    return null;
                }
            }
        }
    }
}
