using System.Linq;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Content
{
    public class Circle
    {
        private static double EPSILON = 1e-12;
        
        public readonly Point Center;
        public readonly double Radius;
        
        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public bool Contains(Point p)
        {
            return Center.Distance(p) <= Radius + EPSILON;
        }

        public bool Contains(ICollection<Point> ps)
        {
            return ps.All(Contains);
        }
    }
}
