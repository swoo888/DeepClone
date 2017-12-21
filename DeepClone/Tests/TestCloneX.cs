using DeepClone.CloningService;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace DeepClone.Tests
{
    public class CloningServiceTest
    {
        public class BasicTest
        {
            public int I;
            public double D;
            public long L;
            public string S { get; set; }
            [Cloneable(CloningMode.Ignore)]
            public string Ignored { get; set; }
            [Cloneable(CloningMode.Shallow)]
            public object ShallowObj { get; set; }

            public virtual string CS => S + I + ShallowObj + L;
        }

        public struct StructTest
        {
            public int I;
            public string S { get; set; }
            [Cloneable(CloningMode.Ignore)]
            public string Ignored { get; set; }

            public string Computed => S + I;

            public StructTest(int i, string s)
            {
                I = i;
                S = s;
                Ignored = null;
            }
        }

        public class BasicTest2 : BasicTest
        {
            public float F;
            public StructTest testSS;
            public override string CS => S + I + testSS.Computed;
        }

        public class Node
        {
            public Node Left;
            public Node Right;
            public Double D;
            public String S;
            public object V;
            public int TotalNodeCount =>
                1 + (Left?.TotalNodeCount ?? 0) + (Right?.TotalNodeCount ?? 0);
        }

        public ICloningService Cloner = new CloningService.AttributeBaseCloner();
        public Action[] AllTests => new Action[] {
            RunBasicTest1,
            RunStructTest1,
            RunBasicTest2,
            RunNodeTest1,
            RunArrayTest1,
            RunCollectionTest1,
            RunArrayTest2,
            RunCollectionTest2,
            RunMixedCollectionTest,
            RunRecursionTest1,
            RunRecursionTest2,
            RunPerformanceTest,
        };

        public static void Assert(bool criteria)
        {
            if (!criteria)
                throw new InvalidOperationException("Assertion failed.");
        }

        public void Measure(string title, Action test)
        {
            test(); // Warmup
            var sw = new Stopwatch();
            GC.Collect();
            sw.Start();
            test();
            sw.Stop();
            // Console.WriteLine($"{title}: {sw.Elapsed.TotalMilliseconds:0.000}ms");
        }

        public void RunBasicTest1()
        {
            var s = new BasicTest() { I = 1, S = "2", Ignored = "3", ShallowObj = new object() };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(s.CS == c.CS);
            Assert(c.Ignored == null);
            Assert(ReferenceEquals(s.ShallowObj, c.ShallowObj));
        }

        public void RunStructTest1()
        {
            var s = new StructTest(1, "2") { Ignored = "3" };
            var c = Cloner.Clone(s);
            Assert(s.Computed == c.Computed);
            Assert(c.Ignored == null);
        }

        public void RunBasicTest2()
        {
            var s = new BasicTest2()
            {
                I = 1,
                S = "2",
                D = 3,
                testSS = new StructTest(3, "4"),
            };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(s.CS == c.CS);
        }

        public void RunNodeTest1()
        {
            var s = new Node
            {
                Left = new Node
                {
                    Right = new Node()
                },
                Right = new Node()
            };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(s.TotalNodeCount == c.TotalNodeCount);
        }

        public void RunRecursionTest1()
        {
            var s = new Node();
            s.Left = s;
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(null == c.Right);

            var d = c.Left;
            Assert(c == c.Left);
        }

        public void RunArrayTest1()
        {
            var n = new Node
            {
                Left = new Node
                {
                    Right = new Node()
                },
                Right = new Node()
            };
            var s = new[] { n, n };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(s.Sum(n1 => n1.TotalNodeCount) == c.Sum(n1 => n1.TotalNodeCount));
            Assert(c[0] == c[1]);
        }

        public void RunCollectionTest1()
        {
            var n = new Node
            {
                Left = new Node
                {
                    Right = new Node()
                },
                Right = new Node()
            };
            var s = new List<Node>() { n, n };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(s.Sum(n1 => n1.TotalNodeCount) == c.Sum(n1 => n1.TotalNodeCount));
            Assert(c[0] == c[1]);
        }

        public void RunArrayTest2()
        {
            var s = new[] { new[] { 1, 2, 3 }, new[] { 4, 5 } };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(15 == c.SelectMany(a => a).Sum());
        }

        public void RunCollectionTest2()
        {
            var s = new List<List<int>> { new List<int> { 1, 2, 3 }, new List<int> { 4, 5 } };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(15 == c.SelectMany(a => a).Sum());
        }

        public void RunMixedCollectionTest()
        {
            var s = new List<IEnumerable<int[]>> {
                new List<int[]> {new [] {1}},
                new List<int[]> {new [] {2, 3}},
            };
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(6 == c.SelectMany(a => a.SelectMany(b => b)).Sum());
        }

        public void RunRecursionTest2()
        {
            var l = new List<Node>();
            var n = new Node { V = l };
            n.Left = n;
            l.Add(n);
            var s = new object[] { null, l, n };
            s[0] = s;
            var c = Cloner.Clone(s);
            Assert(s != c);
            Assert(c[0] == c);
            var cl = (List<Node>)c[1];
            Assert(l != cl);
            var cn = cl[0];
            Assert(n != cn);
            Assert(cl == cn.V);
            Assert(cn.Left == cn);
        }

        public void RunPerformanceTest()
        {
            Func<int, Node> makeTree = null;
            makeTree = depth => {
                if (depth == 0)
                    return null;
                return new Node
                {
                    V = depth,
                    Left = makeTree(depth - 1),
                    Right = makeTree(depth - 1),
                };
            };
            for (var i = 10; i <= 20; i++)
            {
                var root = makeTree(i);
                Measure($"Cloning {root.TotalNodeCount} nodes", () => {
                    var copy = Cloner.Clone(root);
                    Assert(root != copy);
                });
            }
        }

        public void RunAllTests()
        {
            foreach (var test in AllTests)
                test.Invoke();
            Console.WriteLine("Done.");
        }
    }
}
