﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    public class LoopJoinWorker
        : ReusableJoinWorker
    {
        public LoopJoinWorker(IList<String> joinVars, IEnumerable<ISet> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            if (rhs == null) throw new ArgumentNullException("rhs");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            this.Rhs = rhs;
        }

        public IEnumerable<ISet> Rhs { get; private set; }

        public IList<String> JoinVariables { get; private set; } 

        public override IEnumerable<ISet> Find(ISet lhs)
        {
            return this.Rhs.Where(s => lhs.IsCompatibleWith(s, this.JoinVariables));
        }
    }
}