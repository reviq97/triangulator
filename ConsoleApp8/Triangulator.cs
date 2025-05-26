namespace ConsoleApp8
{
    public static class Triangulator
    {
        private static long Cross(Vertex a, Vertex b, Vertex c)
            => (a.X - b.X) * (c.Y - b.Y) - (a.Y - b.Y) * (c.X - b.X);

        private static bool DiagonalInside(Vertex uj, Vertex w)
        {
            var next = w.IsOnRightChain ? w.Right! : w.Left!;
            var cr = Cross(next, w, uj);
            return w.IsOnRightChain ? cr < 0 : cr > 0;
        }

        public static List<(Vertex, Vertex)> TriangulateMonotonePolygon(List<Vertex> poly)
        {
            int n = poly.Count;
            var u = poly
                .OrderByDescending(v => v.Y)
                .ThenBy(v => v.X)
                .ToList();

            var S = new Stack<Vertex>();
            S.Push(u[0]);
            S.Push(u[1]);

            var diagonals = new List<(Vertex, Vertex)>();

            for (int j = 2; j < n - 1; j++)
            {
                var uj = u[j];
                var top = S.Peek();

                if (uj.IsOnRightChain != top.IsOnRightChain)
                {
                    var popped = new List<Vertex>();
                    while (S.Count > 0)
                        popped.Add(S.Pop());

                    for (int k = 0; k < popped.Count - 1; k++)
                        diagonals.Add((uj, popped[k]));

                    S.Push(u[j - 1]);
                    S.Push(uj);
                }
                else
                {
                    var last = S.Pop();

                    while (S.Count > 0 && DiagonalInside(uj, S.Peek()))
                    {
                        var w = S.Pop();
                        diagonals.Add((uj, w));
                    }

                    S.Push(last);
                    S.Push(uj);
                }
            }

            var un = u[n - 1];
            var stackVerts = S.Reverse().ToList(); 
            for (int i = 1; i < stackVerts.Count - 1; i++)
                diagonals.Add((un, stackVerts[i]));

            return diagonals;
        }
    }
}
