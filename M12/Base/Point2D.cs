namespace M12.Base
{
    public class Point2D
    {
        public Point2D()
        {
            X = double.NaN;
            Y = double.NaN;
        }

        public Point2D(double X, double Y)
        {
            this.X = X;
            this.Y = Y;
        }

        public double X { get; set; }
        public double Y { get; set; }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }
    }
}
