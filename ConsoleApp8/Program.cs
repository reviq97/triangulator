using ConsoleApp8;

namespace PolygonTriangulation
{
    class Program
    {
        static void Main(string[] args)
        {
            //define a bunch of vertexes
            List<Vertex> vertices = new List<Vertex>()
            {
                new("A", 3, 0),  //A
                new("B", 6, 2),  //B
                new("C", 8, -2), //C
                new("D", 7, 5),  //D
                new("E", 11, 4), //E
                new("F", 14, 10),//F
                new("G", 13, 9), //G
                new("H", 11, 15),//H
                new("I", 9, 13), //I
                new("J", 7, 15), //J
                new("K", 5, 12), //K
                new("L", 6, 10), //L
                new("M", 5, 7),  //M
                new("N", 4, 9),  //N
                new("O", 1, 4),  //O
            };

            vertices = ConnectVertices(vertices);
            vertices = DetermineVerteciesType(vertices);
            var partitionDiagonals = MonotonePartitioner.MakeMonotone(vertices);

            Console.WriteLine("Podział na monotonne:");
            foreach (var (u, w) in partitionDiagonals)
                Console.WriteLine($"{u.Label} – {w.Label}");

            var monotonePolygons = MonotonePartitioner.ExtractMonotonePolygons(
                vertices, partitionDiagonals);

            Console.WriteLine("\nTriangulacja monotonnego:");
            int idx = 1;
            foreach (var poly in monotonePolygons)
            {
                var tris = Triangulator.TriangulateMonotonePolygon(poly);
                Console.WriteLine($"Polygon M{idx++}: " +
                    string.Join("→", poly.Select(v => v.Label)) + "→" + poly[0].Label);
                foreach (var (u, w) in tris)
                    Console.WriteLine($"  {u.Label} – {w.Label}");
            }
        }


        public static VertexType ClassifyVertex(Vertex prev, Vertex curr, Vertex next, bool isClockwise)
        {
            bool isAbovePrev = curr.Y > prev.Y;
            bool isAboveNext = curr.Y > next.Y;
            bool isBelowPrev = curr.Y < prev.Y;
            bool isBelowNext = curr.Y < next.Y;

            bool isConvex = !IsConcave(prev, curr, next, isClockwise);

            if (isAbovePrev && isAboveNext)
                return isConvex ? VertexType.Start : VertexType.Split;
            else if (isBelowPrev && isBelowNext)
                return isConvex ? VertexType.End : VertexType.Merge;

            return VertexType.Regular;
        }

        public static bool IsConcave(Vertex prev, Vertex curr, Vertex next, bool isClockwise)
        {
            float ax = prev.X - curr.X;
            float ay = prev.Y - curr.Y;

            float bx = next.X - curr.X;
            float by = next.Y - curr.Y;

            float cross = ax * by - ay * bx;

            return isClockwise ? cross > 0 : cross < 0;
        }

        public static List<Vertex> DetermineVerteciesType(List<Vertex> vertices)
        {
            foreach (Vertex v in vertices)
                v.Type = ClassifyVertex(v.Right, v, v.Left, true);

            return vertices;
        }

        private static List<Vertex> ConnectVertices(List<Vertex> vertices)
        {
            var start = vertices
                .OrderByDescending(v => v.X)
                .ThenBy(v => v.Label) 
                .First();

            var alphabetical = vertices
                .OrderBy(v => v.Label)
                .ToList();

            int startIndex = alphabetical.FindIndex(v => v.Label == start.Label);

            var sorted = alphabetical.Skip(startIndex)
                .Concat(alphabetical.Take(startIndex)) 
                .ToList();

            List<Vertex> result = new();

            int vIndex = 1;

            Vertex previous = sorted.First();
            previous.VIndex = vIndex;
            sorted.Remove(previous);
            result.Add(previous);

            for (int i = 0; i < sorted.Count; i++)
            {
                Vertex current = sorted[i];
                current.VIndex = ++vIndex;

                previous.Left = current;
                current.Right = previous;

                previous = current;
                result.Add(previous);
            }

            Vertex first = result.First();
            Vertex last = result.Last();

            first.Right = last;
            last.Left = first;

            return result;
        }
    }
}