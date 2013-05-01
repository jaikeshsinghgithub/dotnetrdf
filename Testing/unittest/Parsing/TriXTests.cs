/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    [TestFixture]
    public class TriXTests
    {
        private TriXWriter _writer = new TriXWriter();
        private TriXParser _parser = new TriXParser();

        [Test, Timeout(2500)]
        public void ParsingTriXPerformance1()
        {
            //1 Graph, 100 Triples per Graph = 100 Triples total
            this.TestTrixPerformance(1, 100);
        }

        [Test, Timeout(2500)]
        public void ParsingTriXPerformance2()
        {
            //10 Graphs, 1000 Triples per Graph = 10,000 Triples total
            this.TestTrixPerformance(10, 1000);
        }

        [Test, Timeout(2500)]
        public void ParsingTriXPerformance3()
        {
            //1000 Graphs, 10 Triples per Graph = 10,000 Triples total
            this.TestTrixPerformance(1000, 10);
        }

        [Test, Timeout(25000)]
        public void ParsingTriXPerformance4()
        {
            //1000 Graphs, 100 Triples per Graph = 100,000 Triples total
            this.TestTrixPerformance(1000, 100);
        }

        [Test, Timeout(2500)]
        public void ParsingTriXPerformance5()
        {
            //Test case from CORE-351
            TripleStore store = new TripleStore();
            Stopwatch timer = new Stopwatch();
            timer.Start();
            this._parser.Load(store, @"lib_p11_ontology.trix");
            timer.Stop();
            Console.WriteLine("Took " + timer.Elapsed + " to read from disk");
        }

        private void TestTrixPerformance(int numGraphs, int triplesPerGraph)
        {
            //Generate data
            TripleStore store = new TripleStore();
            for (int i = 1; i <= numGraphs; i++)
            {
                Graph g = new Graph();
                g.BaseUri = new Uri("http://example.org/graph/" + i);

                for (int j = 1; j <= triplesPerGraph; j++)
                {
                    g.Assert(new Triple(g.CreateUriNode(UriFactory.Create("http://example.org/subject/" + j)), g.CreateUriNode(UriFactory.Create("http://example.org/predicate/" + j)), (j).ToLiteral(g)));
                }
                store.Add(g);
            }

            Console.WriteLine("Generated dataset with " + numGraphs + " named graphs (" + triplesPerGraph + " triples/graph) with a total of " + (numGraphs * triplesPerGraph) + " triples");

            Stopwatch timer = new Stopwatch();

            //Write out to disk
            timer.Start();
            this._writer.Save(store, "temp.trix");
            timer.Stop();
            Console.WriteLine("Took " + timer.Elapsed + " to write to disk");
            timer.Reset();

            //Read back from disk
            TripleStore store2 = new TripleStore();
            timer.Start();
            this._parser.Load(store2, "temp.trix");
            timer.Stop();
            Console.WriteLine("Took " + timer.Elapsed + " to read from disk");

            Assert.AreEqual(numGraphs * triplesPerGraph, store2.Graphs.Sum(g => g.Triples.Count));

        }
    }
}
