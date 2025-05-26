namespace ConsoleApp8
{
    public enum VertexType
    {
        Start,
        End,
        Split,
        Merge,
        Regular
    }

    public class Vertex
    {
        public string Label { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public int VIndex { get; set; }
        public VertexType Type { get; set; }
        public Vertex? Left { get; set; }
        public Vertex? Right { get; set; }
        public List<Vertex> Diagonals { get; set; } = new List<Vertex>();
        public bool IsOnRightChain { get; set; }

        public Vertex(string label, int x, int y)
        {
            Label = label;
            this.X = x;
            this.Y = y;
        }
    }

    public class Edge : IEquatable<Edge>
    {
        public Vertex U { get; }
        public Vertex V { get; }

        public Edge(Vertex a, Vertex b)
        {
            if (a.Y > b.Y || (a.Y == b.Y && a.X < b.X))
            {
                U = a;
                V = b;
            }
            else
            {
                U = b;
                V = a;
            }
        }

        public bool Equals(Edge? other)
        {
            if (other is null) return false;
            return U == other.U && V == other.V;
        }

        public override bool Equals(object? obj)
            => obj is Edge e && Equals(e);

        public override int GetHashCode()
            => HashCode.Combine(U.VIndex, V.VIndex);


        public bool IsLeftOf(Vertex v)
        {
            if (U.Y == V.Y) return false;

            double t = (double)(v.Y - U.Y) / (V.Y - U.Y);
            if (t < 0 || t > 1) return false;

            double xAtY = U.X + t * (V.X - U.X);
            return xAtY < v.X;
        }
    }
}
