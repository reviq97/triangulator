namespace ConsoleApp8
{
    public static class MonotonePartitioner
    {
        public static List<(Vertex, Vertex)> MakeMonotone(List<Vertex> vertices)
        {
            var top = vertices
                .OrderByDescending(v => v.Y)
                .ThenBy(v => v.X)
                .First();
            var bottom = vertices
                .OrderBy(v => v.Y)
                .ThenBy(v => v.X)
                .First();

            var rightChain = new HashSet<Vertex>();
            var cur = top;
            do
            {
                rightChain.Add(cur);
                cur = cur.Right!;
            }
            while (cur != bottom);
            rightChain.Add(bottom);

            foreach (var v in vertices)
                v.IsOnRightChain = rightChain.Contains(v);

            var queue = new PriorityQueue<Vertex, (int, int)>();
            foreach (var v in vertices
                .OrderByDescending(v => v.Y)
                .ThenBy(v => v.X))
            {
                queue.Enqueue(v, (-v.Y, v.X));
            }

            var status = new List<Edge>();
            var helpers = new Dictionary<Edge, Vertex>();
            var diagonals = new List<(Vertex, Vertex)>();
            qq
            while (queue.Count > 0)
            {
                var v = queue.Dequeue();
                switch (v.Type)
                {
                    case VertexType.Start:
                        {
                            var e = new Edge(v, v.Left!);
                            status.Add(e);
                            helpers[e] = v;
                        }
                        break;
                    case VertexType.End:
                        HandleEndVertex(v, status, helpers, diagonals);
                        break;
                    case VertexType.Split:
                        HandleSplitVertex(v, status, helpers, diagonals);
                        break;
                    case VertexType.Merge:
                        HandleMergeVertex(v, status, helpers, diagonals);
                        break;
                    case VertexType.Regular:
                        HandleRegularVertex(v, status, helpers, diagonals);
                        break;
                }
            }

            return diagonals;
        }


        public static List<List<Vertex>> ExtractMonotonePolygons(
    List<Vertex> allVerts,
    List<(Vertex, Vertex)> diagonals)
        {
            var adj = allVerts.ToDictionary(v => v, v => new HashSet<Vertex>());
            foreach (var v in allVerts)
            {
                adj[v].Add(v.Left!);
                adj[v].Add(v.Right!);
            }
            foreach (var (u, w) in diagonals)
            {
                adj[u].Add(w);
                adj[w].Add(u);
            }

            var visited = new HashSet<Vertex>();
            var polys = new List<List<Vertex>>();

            foreach (var start in allVerts)
            {
                if (visited.Contains(start)) continue;
                var cycle = new List<Vertex>();
                BuildCycle(start, null, adj, cycle, visited);
                if (cycle.Count >= 3)
                    polys.Add(cycle);
            }
            return polys;
        }

        private static bool BuildCycle(
            Vertex cur,
            Vertex? parent,
            Dictionary<Vertex, HashSet<Vertex>> adj,
            List<Vertex> cycle,
            HashSet<Vertex> vis)
        {
            vis.Add(cur);
            cycle.Add(cur);
            foreach (var nb in adj[cur])
            {
                if (nb == parent) continue;
                if (!vis.Contains(nb))
                {
                    if (BuildCycle(nb, cur, adj, cycle, vis))
                        return true;
                }
                else
                {
                    cycle.Add(nb);
                    return true;
                }
            }
            return false;
        }

        private static void HandleRegularVertex(Vertex v, List<Edge> status, Dictionary<Edge, Vertex> helpers, List<(Vertex, Vertex)> diagonals)
        {
            if (v.Right!.Y < v.Y)
            {
                var incoming = new Edge(v.Right, v);
                if (helpers.TryGetValue(incoming, out var hEnd)
                    && hEnd.Type == VertexType.Merge)
                {
                    diagonals.Add((v, hEnd));
                    v.Diagonals.Add(hEnd);
                    hEnd.Diagonals.Add(v);
                }
                status.Remove(incoming);
                helpers.Remove(incoming);

                var ei = new Edge(v, v.Left!);
                status.Add(ei);
                helpers[ei] = v;
            }
            else
            {
                var leftEdge = status
                    .Where(e => e.IsLeftOf(v))
                    .OrderByDescending(e =>
                    {
                        double t = (double)(v.Y - e.U.Y) / (e.V.Y - e.U.Y);
                        return e.U.X + t * (e.V.X - e.U.X);
                    })
                    .FirstOrDefault();

                if (leftEdge != null)
                {
                    if (helpers.TryGetValue(leftEdge, out var hMerge)
                        && hMerge.Type == VertexType.Merge)
                    {
                        diagonals.Add((v, hMerge));
                        v.Diagonals.Add(hMerge);
                        hMerge.Diagonals.Add(v);
                    }
                    helpers[leftEdge] = v;
                }
            }
        }

        private static void HandleMergeVertex(Vertex v, List<Edge> status, Dictionary<Edge, Vertex> helpers, List<(Vertex, Vertex)> diagonals)
        {
            var incoming = new Edge(v.Right!, v);

            if (helpers.TryGetValue(incoming, out var h1) && h1.Type == VertexType.Merge)
            {
                diagonals.Add((v, h1));
                v.Diagonals.Add(h1);
                h1.Diagonals.Add(v);
            }

            status.Remove(incoming);
            helpers.Remove(incoming);

            var leftEdge = status
                .Where(e => e.IsLeftOf(v))
                .OrderByDescending(e =>
                {
                    double t = (double)(v.Y - e.U.Y) / (e.V.Y - e.U.Y);
                    return e.U.X + t * (e.V.X - e.U.X);
                })
                .FirstOrDefault();

            if (leftEdge == null)
                return;

            if (helpers.TryGetValue(leftEdge, out var h2) && h2.Type == VertexType.Merge)
            {
                diagonals.Add((v, h2));
                v.Diagonals.Add(h2);
                h2.Diagonals.Add(v);
            }

            helpers[leftEdge] = v;
        }

        private static void HandleEndVertex(Vertex v, List<Edge> status, Dictionary<Edge, Vertex> helpers, List<(Vertex, Vertex)> diagonals)
        {
            var e = new Edge(v.Right!, v);

            if (helpers.TryGetValue(e, out var h) && h.Type == VertexType.Merge)
            {
                diagonals.Add((v, h));
                v.Diagonals.Add(h);
                h.Diagonals.Add(v);
            }

            status.Remove(e);
            helpers.Remove(e);
        }

        private static void HandleSplitVertex(Vertex v, List<Edge> status, Dictionary<Edge, Vertex> helpers, List<(Vertex, Vertex)> diagonals)
        {
            var leftEdge = status
                .Where(e => e.IsLeftOf(v))
                .OrderByDescending(e =>
                {
                    double t = (double)(v.Y - e.U.Y) / (e.V.Y - e.U.Y);
                    return e.U.X + t * (e.V.X - e.U.X);
                })
                .FirstOrDefault();

            if (leftEdge == null)
                return;

            var h = helpers[leftEdge];
            diagonals.Add((v, h));
            v.Diagonals.Add(h);
            h.Diagonals.Add(v);

            helpers[leftEdge] = v;

            var ei = new Edge(v, v.Left!);
            status.Add(ei);
            helpers[ei] = v;
        }
    }
}
