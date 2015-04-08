#region License
/*
* A simplistic Behavior Tree implementation in C#
* 
* Copyright (C) 2011-2015 Alexander Shoulson - ashoulson@gmail.com
* (TreeSharp Copyright (C) 2010-2011 ApocDev apocdev@gmail.com)
* 
* This file is part of TreeSharpPlus.
* 
* TreeSharpPlus is free software: you can redistribute it and/or modify
* it under the terms of the GNU Lesser General Public License as published
* by the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* TreeSharpPlus is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU Lesser General Public License for more details.
* 
* You should have received a copy of the GNU Lesser General Public License
* along with TreeSharpPlus.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TreeSharpPlus.ExtensionMethods;

namespace TreeSharpPlus
{
    public abstract class NodeGroupWeighted : NodeGroup
    {
        public List<float> Weights { get; set; }

        /// <summary>
        /// Shuffles the children using the given weights
        /// </summary>
        protected void Shuffle()
        {
            this.Children.Shuffle(this.Weights);
        }

        /// <summary>
        /// Initializes, fully normalized with no given weights
        /// </summary>
        public NodeGroupWeighted(params Node[] children)
            : base(children)
        {
            this.Weights = new List<float>();
            for (int i = 0; i < this.Children.Count; i++)
                this.Weights.Add(1.0f);
        }

        public NodeGroupWeighted(params NodeWeight[] weightedchildren)
        {
            // Initialize the base Children list and our new Weights list
            this.Children = new List<Node>();
            this.Weights = new List<float>();

            // Unpack the pairs and store their individual values
            foreach (NodeWeight weightedchild in weightedchildren)
            {
                this.Children.Add(weightedchild.Composite);
                this.Weights.Add(weightedchild.Weight);
            }
        }
    }

    /// <summary>
    /// A simple pair class for composites and weights, used for stochastic control nodes
    /// </summary>
    public class NodeWeight
    {
        public Node Composite { get; set; }
        public float Weight { get; set; }

        public NodeWeight(float weight, Node composite)
        {
            this.Composite = composite;
            this.Weight = weight;
        }
    }
}
