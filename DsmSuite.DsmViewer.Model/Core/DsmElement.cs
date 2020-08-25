﻿using System.Linq;
using System.Collections.Generic;
using DsmSuite.DsmViewer.Model.Interfaces;

namespace DsmSuite.DsmViewer.Model.Core
{
    /// <summary>
    /// Represent an element in the dsm hierarchy.
    /// </summary>
    public class DsmElement : IDsmElement
    {
        private char _typeId;
        private readonly List<IDsmElement> _children = new List<IDsmElement>();
        private DsmElement _parent;
        private static readonly TypeRegistration TypeRegistration = new TypeRegistration();
        private readonly Dictionary<int /*providerId*/, int /*weight*/> _directWeights = new Dictionary<int, int>();
        private readonly Dictionary<int /*providerId*/, int /*weight*/> _weights = new Dictionary<int, int>();
        
        public DsmElement(int id, string name, string type, int order = 0, bool isExpanded = false)
        {
            Id = id;
            Name = name;
            _typeId = TypeRegistration.AddTypeName(type);
            Order = order;
            IsExpanded = isExpanded;
            IsIncludedInTree = true;
        }

        public Dictionary<int, int> DirectWeights => _directWeights;
        public Dictionary<int, int> Weights => _weights;

        public void AddWeight(IDsmElement provider, int weight)
        {
            int currentWeight = 0;
            if (_weights.ContainsKey(provider.Id))
            {
                currentWeight = _weights[provider.Id];
            }
            _weights[provider.Id] = currentWeight + weight;
        }

        public void RemoveWeight(IDsmElement provider, int weight)
        {
            int currentWeight = _weights[provider.Id];
            if (currentWeight >= weight)
            {
                _weights[provider.Id] = currentWeight - weight;
            }
        }

        /// <summary>
        /// Number uniquely identifying element.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Number identifying sequential order of the element in element tree.
        /// </summary>
        public int Order { get; set; }

        /// <summary>
        /// Type of element.
        /// </summary>
        public string Type
        {
            get { return TypeRegistration.GetTypeName(_typeId); }
            set { _typeId = TypeRegistration.AddTypeName(value); }
        }

        /// <summary>
        /// Name of the element.
        /// </summary>
        public string Name { get; set; }

        public bool IsDeleted { get; set; }

        public bool IsRoot => Parent == null;

        /// <summary>
        /// Full name of the element based on its position in the element hierarchy
        /// </summary>
        public string Fullname
        {
            get
            {
                string fullname = Name;
                IDsmElement parent = Parent;
                while (parent != null)
                {
                    if (parent.Name.Length > 0)
                    {
                        fullname = parent.Name + "." + fullname;
                    }
                    parent = parent.Parent;
                }
                return fullname;
            }
        }

        /// <summary>
        /// Is the element expanded in the viewer.
        /// </summary>
        public bool IsExpanded { get; set; }

        /// <summary>
        /// Is the element match in search.
        /// </summary>
        public bool IsMatch { get; set; }

        /// <summary>
        /// Is the element included in the tree
        /// </summary>
        public bool IsIncludedInTree { get; set; }

        /// <summary>
        /// Parent of the element.
        /// </summary>
        public IDsmElement Parent => _parent;

        /// <summary>
        /// Children of the element.
        /// </summary>
        public IList<IDsmElement> Children => _children.Where(child => ((child.IsDeleted == false) && (child.IsIncludedInTree == true))).ToList();

        public int ChildCount => Children.Count;

        public IList<IDsmElement> AllChildren => _children;

        /// <summary>
        /// Has the element any children.
        /// </summary>
        public bool HasChildren => Children.Count > 0;

        /// <summary>
        /// Add a child to the element.
        /// </summary>
        /// <param name="child">The child to be added</param>
        public void AddChild(IDsmElement child)
        {
            _children.Add(child);
            DsmElement c = child as DsmElement;
            if (c != null)
            {
                c._parent = this;
            }
        }

        /// <summary>
        /// Remove a child from the element.
        /// </summary>
        /// <param name="child">The child to be added</param>
        public void RemoveChild(IDsmElement child)
        {
            _children.Remove(child);
            DsmElement c = child as DsmElement;
            if (c != null)
            {
                c._parent = null;
            }
        }

        public void RemoveAllChildren()
        {
            _children.Clear();
        }

        public bool Swap(IDsmElement element1, IDsmElement element2)
        {
            bool swapped = false;

            if (_children.Contains(element1) && _children.Contains(element2))
            {
                int index1 = _children.IndexOf(element1);
                int index2 = _children.IndexOf(element2);

                _children[index2] = element1;
                _children[index1] = element2;

                swapped = true;
            }

            return swapped;
        }

        public int CompareTo(object obj)
        {
            DsmElement element = obj as DsmElement;
            return Id.CompareTo(element?.Id);
        }
    }
}
