using System;

namespace Tavisca.Platform.Common.Content
{
    public class Point
    {
        public readonly decimal X;
        public readonly decimal Y;

        public Point(decimal x, decimal y)
        {
            X = x;
            Y = y;
        }

        public Point Subtract(Point p)
        {
            return new Point(X - p.X, Y - p.Y);
        }
        
        public double Distance(Point p)
        {
            double dx = Convert.ToDouble(X - p.X);
            double dy = Convert.ToDouble(Y - p.Y);
            return Math.Sqrt(dx * dx + dy * dy);
        }
        
        public decimal Cross(Point p)
        {
            return X * p.Y - Y * p.X;
        }
        
        public decimal Norm()
        {
            return X * X + Y * Y;
        }
    }
}
