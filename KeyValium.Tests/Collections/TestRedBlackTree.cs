using KeyValium.Cache;
using KeyValium.Collections;
using KeyValium.Memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Tests.Collections
{
    public class TestRedBlackTree : IDisposable
    {
        const int BUFFERSIZE = 64;

        public TestRedBlackTree()
        {
        }

        [Fact]
        public void Test1()
        {
            //var treex1 = new SortedSetX<int>();
            //var treex2 = new SortedSetX<int>();
            //var treex3 = new SortedSetX<int>();
            var tree1 = new RedBlackTree<int>();
            var tree2 = new RedBlackTree<int>();
            var tree3 = new RedBlackTree<int>();

            var count = 1000;
            var rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                //treex1.Insert(i);
                tree1.Insert(i);

                //treex2.Insert(count - i - 1);
                tree2.Insert(count - i - 1);

                var x = rnd.Next(count);
                //treex3.Insert(x);
                tree3.Insert(x);

                //CompareTrees(tree1, treex1);
                //CompareTrees(tree2, treex2);
                //CompareTrees(tree3, treex3);

                tree1.CheckParents();
                tree2.CheckParents();
                tree3.CheckParents();
            }

            Console.WriteLine("After Insert");
            Console.WriteLine("Count1: {0}", tree1.Count);
            Console.WriteLine("Count2: {0}", tree2.Count);
            Console.WriteLine("Count3: {0}", tree3.Count);

            for (int i = 0; i < count; i++)
            {
                //if (i==88)
                //{
                //    Console.WriteLine();
                //}

                //if (i==89)
                //{
                //    DumpTrees(tree1, treex1);
                //}

                //treex1.Remove(i);
                tree1.Remove(i);

                //treex2.Remove(count - i - 1);
                tree2.Remove(count - i - 1);

                var x = rnd.Next(count);
                //treex3.Remove(x);
                tree3.Remove(x);

                //CompareTrees(tree1, treex1);
                //CompareTrees(tree2, treex2);
                //CompareTrees(tree3, treex3);

                tree1.CheckParents();
                tree2.CheckParents();
                tree3.CheckParents();
            }

            Console.WriteLine("After Delete");
            Console.WriteLine("Count1: {0}", tree1.Count);
            Console.WriteLine("Count2: {0}", tree2.Count);
            Console.WriteLine("Count3: {0}", tree3.Count);

            Console.WriteLine("Success");
        }


        [Fact]
        public void Test2()
        {
            var temp = new SortedSet<int>();
            var tree = new RedBlackTree<int>();
            var list = new List<int>();

            var count = 1000;
            var rnd = new Random();

            for (int i = 0; i < count; i++)
            {
                var x = rnd.Next(count * 10);

                temp.Add(x);
            }

            foreach (var x in temp)
            {
                tree.Insert(x);
                list.Add(x);
            }

            list = list.OrderBy(x => x).ToList();

            // test parents
            tree.CheckParents();

            for (int i = 0; i < list.Count; i++)
            {
                // test find
                var index = tree.FindNode(list[i]);
                if (index < 0)
                {
                    throw new Exception("Item not found");
                }

                // test GetItem
                var val = tree.GetItem(index);
                if (val != list[i])
                {
                    throw new Exception("Value mismatch!");
                }

                // test GetPrev
                if (i > 0)
                {
                    if (tree.TryGetPrev(index, out var prev))
                    {
                        var prevval = tree.GetItem(prev);
                        if (prevval != list[i - 1])
                        {
                            throw new Exception("Prev Value mismatch!");
                        }
                    }
                    else
                    {
                        throw new Exception("Item not found");
                    }
                }

                // testGetNext
                if (i < list.Count - 1)
                {
                    if (tree.TryGetNext(index, out var next))
                    {
                        var nextval = tree.GetItem(next);
                        if (nextval != list[i + 1])
                        {
                            throw new Exception("Next Value mismatch!");
                        }
                    }
                    else
                    {
                        throw new Exception("Item not found");
                    }
                }
            }

            // test GetMaxLEQ
            for (int i = 0; i < count * 10; i++)
            {
                var expected = FindMaxLeq(list, i);

                var index = tree.FindMaxLeq(i);
                if (!((expected >= 0) == (index >= 0)))
                {
                    throw new Exception("FindMaxLEQ mismatch!");
                }

                if (index >= 0)
                {
                    var node = tree.GetNode(index);
                    if (node.Item != expected)
                    {
                        throw new Exception("FindMaxLEQ mismatch!");
                    }
                }
            }

            // test GetMinGEQ
            for (int i = 0; i < count * 10; i++)
            {
                var expected = FindMinGeq(list, i);

                var index = tree.FindMinGeq(i);
                if (!((expected >= 0) == (index >= 0)))
                {
                    throw new Exception("FindMinGeq mismatch!");
                }

                if (index >= 0)
                {
                    var node = tree.GetNode(index);
                    if (node.Item != expected)
                    {
                        throw new Exception("FindMinGeq mismatch!");
                    }
                }
            }

            // test min
            if (tree.TryGetMin(out var min))
            {
                Console.WriteLine("Min: {0}", tree.GetItem(min));

                if (tree.GetItem(min) != list[0])
                {
                    throw new Exception("Min mismatch!");
                }
            }
            else
            {
                throw new Exception("Min not found!");
            }

            // test max
            if (tree.TryGetMax(out var max))
            {
                Console.WriteLine("Max: {0}", tree.GetItem(max));
                if (tree.GetItem(max) != list[^1])
                {
                    throw new Exception("Max mismatch!");
                }
            }
            else
            {
                throw new Exception("Max not found!");
            }
        }

        private int FindMaxLeq(List<int> list, int val)
        {
            var result = -1;

            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > val)
                {
                    break;
                }

                result = list[i];
            }

            return result;
        }

        private int FindMinGeq(List<int> list, int val)
        {
            var result = -1;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (list[i] < val)
                {
                    break;
                }

                result = list[i];
            }

            return result;
        }

        //private void DumpTrees<T>(RedBlackTree<T> tree1, SortedSetX<T> tree2)
        //{
        //    Console.WriteLine("RedBlackTree:\n{0}\n", tree1.AsString());
        //    Console.WriteLine("SortedSet:\n{0}\n", tree2.AsString());
        //}

        //private void CompareTrees<T>(RedBlackTree<T> tree1, SortedSetX<T> tree2)
        //{

        //    var c1 = tree1.Count;
        //    var c2 = tree2.Count;

        //    var s1 = tree1.AsString();
        //    var s2 = tree2.AsString();

        //    if (s1 != s2)
        //    {
        //        DumpTrees(tree1, tree2);
        //        throw new Exception("Trees are not equal!");
        //    }
        //}

        public void Dispose()
        {

        }
    }
}






