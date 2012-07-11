﻿/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF;

namespace VDS.Common.Trees
{
    /// <summary>
    /// Abstract base implementation of an unbalanced binary search tree
    /// </summary>
    /// <typeparam name="TNode">Tree Node Type</typeparam>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public abstract class BinaryTree<TNode, TKey, TValue>
        : ITree<TNode, TKey, TValue>
        where TNode : class, IBinaryTreeNode<TKey, TValue>
    {
        private IComparer<TKey> _comparer = Comparer<TKey>.Default;

        /// <summary>
        /// Creates a new unbalanced Binary Tree
        /// </summary>
        public BinaryTree()
            : this(null) { }

        /// <summary>
        /// Creates a new unbalanced Binary Tree
        /// </summary>
        /// <param name="comparer">Comparer for keys</param>
        public BinaryTree(IComparer<TKey> comparer)
        {
            this._comparer = (comparer != null ? comparer : this._comparer);
        }

        /// <summary>
        /// Gets/Sets the Root of the Tree
        /// </summary>
        public TNode Root
        {
            get;
            internal set;
        }

        /// <summary>
        /// Adds a Key Value pair to the tree, replaces an existing value if the key already exists in the tree
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        public virtual bool Add(TKey key, TValue value)
        {
            if (this.Root == null)
            {
                //No root yet so just insert at the root
                this.Root = this.CreateNode(null, key, value);
                return true;
            }
            else
            {
                //Move to the node
                IBinaryTreeNode<TKey, TValue> node = this.MoveToNode(key);
                node.Value = value;
                return true;
            }
        }

        /// <summary>
        /// Creates a new node for the tree
        /// </summary>
        /// <param name="parent">Parent node</param>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        /// <returns></returns>
        protected abstract TNode CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value);

        /// <summary>
        /// Finds a Node based on the key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Node associated with the given Key or null if the key is not present in the tree</returns>
        public virtual TNode Find(TKey key)
        {
            //Iteratively binary search for the key
            TNode current = this.Root;
            int c;
            do
            {
                c = this._comparer.Compare(key, current.Key);
                if (c < 0)
                {
                    current = (TNode)current.LeftChild;
                }
                else if (c > 0)
                {
                    current = (TNode)current.RightChild;
                }
                else
                {
                    //If we find a match on the key then return it
                    return current;
                }
            } while (current != null);

            return null;
        }

        /// <summary>
        /// Moves to the node with the given key inserting a new node if necessary
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns></returns>
        public virtual TNode MoveToNode(TKey key)
        {
            if (this.Root == null)
            {
                this.Root = this.CreateNode(null, key, default(TValue));
                return this.Root;
            }
            else
            {
                //Iteratively binary search for the key
                TNode current = this.Root;
                TNode parent = null;
                int c;
                do
                {
                    c = this._comparer.Compare(key, current.Key);
                    if (c < 0)
                    {
                        parent = current;
                        current = (TNode)current.LeftChild;
                    }
                    else if (c > 0)
                    {
                        parent = current;
                        current = (TNode)current.RightChild;
                    }
                    else
                    {
                        //If we find a match on the key then return it
                        return current;
                    }
                } while (current != null);

                //Key doesn't exist so need to do an insert
                current = this.CreateNode(parent, key, default(TValue));
                if (c < 0)
                {
                    parent.LeftChild = current;
                    this.AfterLeftInsert(parent, current);
                }
                else
                {
                    parent.RightChild = current;
                    this.AfterRightInsert(parent, current);
                }

                //Return the newly inserted node
                return current;
            }
        }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after an insert
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="node">Newly inserted node</param>
        protected virtual void AfterLeftInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        { }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after an insert
        /// </summary>
        /// <param name="parent">Parent</param>
        /// <param name="node">Newly inserted node</param>
        protected virtual void AfterRightInsert(IBinaryTreeNode<TKey, TValue> parent, IBinaryTreeNode<TKey, TValue> node)
        { }

        /// <summary>
        /// Removes a Node based on the Key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if a Node was removed</returns>
        public virtual bool Remove(TKey key)
        {
            //Iteratively binary search for the key
            TNode current = this.Root;
            int c;
            do
            {
                c = this._comparer.Compare(key, current.Key);
                if (c < 0)
                {
                    current = (TNode)current.LeftChild;
                }
                else if (c > 0)
                {
                    current = (TNode)current.RightChild;
                }
                else
                {
                    //If we find a match on the key then stop searching
                    //Calculate the comparison with the parent key (if there is a parent) as we need this info
                    //for the deletion implementation
                    c = (current.Parent != null ? this._comparer.Compare(current.Key, current.Parent.Key) : c);
                    break;
                }
            } while (current != null);

            //Perform the delete if we found a node
            if (current != null)
            {
                if (current.ChildCount == 2)
                {
                    //Has two children
                    //Therefore we need to select the leftmost successor and move it's key and value to this node and then
                    //delete that leftmost successor
                    TNode successor = this.FindLeftmostChild(current);
                    current.Key = successor.Key;
                    current.Value = successor.Value;
                    successor.Parent.LeftChild = null;
                    this.AfterDelete(current);
                    return true;
                }
                else if (current.HasChildren)
                {
                    //Is an internal node with a single child
                    //Thus just set the appropriate child of the parent to the appropriate child of the node we are deleting
                    if (c < 0)
                    {
                        current.Parent.LeftChild = (current.LeftChild != null ? current.LeftChild : current.RightChild);
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else if (c > 0)
                    {
                        current.Parent.RightChild = (current.LeftChild != null ? current.LeftChild : current.RightChild);
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    //Must be an external node
                    //Thus just set the appropriate child of the parent to be null
                    if (c < 0)
                    {
                        current.Parent.LeftChild = null;
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else if (c > 0)
                    {
                        current.Parent.RightChild = null;
                        this.AfterDelete((TNode)current.Parent);
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Finds the leftmost child of the given node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        protected TNode FindLeftmostChild(TNode node)
        {
            while (node.LeftChild != null)
            {
                node = (TNode)node.LeftChild;
            }
            return node;
        }

        /// <summary>
        /// Finds the rightmost child of the given node
        /// </summary>
        /// <param name="node">Node</param>
        /// <returns></returns>
        protected TNode FindRightmostChild(TNode node)
        {
            while (node.RightChild != null)
            {
                node = (TNode)node.RightChild;
            }
            return node;
        }

        /// <summary>
        /// Virtual method that can be used by derived implementations to perform tree balances after a delete
        /// </summary>
        /// <param name="node">Node at which the deletion happened</param>
        protected virtual void AfterDelete(TNode node)
        { }

        /// <summary>
        /// Determines whether a given Key exists in the Tree
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>True if the key exists in the Tree</returns>
        public virtual bool ContainsKey(TKey key)
        {
            return this.Find(key) != null;
        }

        /// <summary>
        /// Gets/Sets the value for a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Returns the value associated with the key</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the key doesn't exist</exception>
        public TValue this[TKey key]
        {
            get
            {
                TNode n = this.Find(key);
                if (n != null)
                {
                    return n.Value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
            set
            {
                TNode n = this.Find(key);
                if (n != null)
                {
                    n.Value = value;
                }
                else
                {
                    throw new KeyNotFoundException();
                }
            }
        }

        /// <summary>
        /// Tries to get a value based on a key
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value or default for the value type if the key is not present</param>
        /// <returns>True if there is a value associated with the key</returns>
        public bool TryGetValue(TKey key, out TValue value)
        {
            TNode n = this.Find(key);
            if (n != null)
            {
                value = n.Value;
                return true;
            }
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public IEnumerable<TNode> Nodes
        {
            get
            {
                if (this.Root == null)
                {
                    return Enumerable.Empty<TNode>();
                }
                else
                {
                    return (this.Root.LeftChild != null ? this.Root.LeftChild.Nodes.OfType<TNode>() : Enumerable.Empty<TNode>()).Concat(this.Root.AsEnumerable()).Concat(this.Root.RightChild != null ? this.Root.RightChild.Nodes.OfType<TNode>() : Enumerable.Empty<TNode>());
                }
            }
        }

        public IEnumerable<TKey> Keys
        {
            get 
            {
                return (from n in this.Nodes
                        select n.Key);
            }
        }

        public IEnumerable<TValue> Values
        {
            get 
            {
                return (from n in this.Nodes
                        select n.Value);
            }
        }

    }

    public class UnbalancedBinaryTree<TKey, TValue>
        : BinaryTree<IBinaryTreeNode<TKey, TValue>, TKey, TValue>
    {
        public UnbalancedBinaryTree()
            : base() { }

        public UnbalancedBinaryTree(IComparer<TKey> comparer)
            : base(comparer) { }

        protected override IBinaryTreeNode<TKey, TValue> CreateNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            return new BinaryTreeNode<TKey, TValue>(parent, key, value);
        }
    }

    /// <summary>
    /// Binary Tree node implementation
    /// </summary>
    /// <typeparam name="TKey">Key Type</typeparam>
    /// <typeparam name="TValue">Value Type</typeparam>
    public class BinaryTreeNode<TKey, TValue>
        : IBinaryTreeNode<TKey, TValue>
    {
        public BinaryTreeNode(IBinaryTreeNode<TKey, TValue> parent, TKey key, TValue value)
        {
            this.Parent = parent;
            this.Key = key;
            this.Value = value;
        }

        public IBinaryTreeNode<TKey, TValue> Parent
        {
            get;
            set;
        }

        public IBinaryTreeNode<TKey, TValue> LeftChild
        {
            get;
            set;
        }

        public IBinaryTreeNode<TKey, TValue> RightChild
        {
            get;
            set;
        }

        public TKey Key
        {
            get;
            set;
        }

        public TValue Value
        {
            get;
            set;
        }

        public bool HasChildren
        {
            get
            {
                return this.LeftChild != null && this.RightChild != null;
            }
        }

        public int ChildCount
        {
            get
            {
                return (this.LeftChild != null ? 1 : 0) + (this.RightChild != null ? 1 : 0);
            }
        }

        public IEnumerable<IBinaryTreeNode<TKey, TValue>> Nodes
        {
            get
            {
                return (this.LeftChild != null ? this.LeftChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>()).Concat(this.AsEnumerable()).Concat(this.RightChild != null ? this.RightChild.Nodes : Enumerable.Empty<IBinaryTreeNode<TKey, TValue>>());
            }
        }
    }
}