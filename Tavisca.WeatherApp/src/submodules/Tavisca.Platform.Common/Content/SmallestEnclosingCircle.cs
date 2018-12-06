using System;
using System.Collections.Generic;

namespace Tavisca.Platform.Common.Content
{
    public class SmallestEnclosingCircle
    {
        public static Circle GetCircle(List<Point> points)
        {
            // Clone list to preserve the caller's data, randomize order
            var shuffledPoints = new List<Point>(points);
            var rand = new Random();
            for (var i = shuffledPoints.Count - 1; i > 0; i--)
            {
                var j = rand.Next(i + 1);
                var tempPoint = shuffledPoints[i];
                shuffledPoints[i] = shuffledPoints[j];
                shuffledPoints[j] = tempPoint;
            }

            // Progressively add points to circle or recompute circle
            var circle = new Circle(new Point(0, 0), -1);
            for (var i = 0; i < shuffledPoints.Count; i++)
            {
                var p = shuffledPoints[i];
                if (circle.Radius == -1 || !circle.Contains(p))
                    circle = GetCircleOnePoint(shuffledPoints.GetRange(0, i + 1), p);
            }

            return circle;
        }
        
        private static Circle GetCircleOnePoint(List<Point> points, Point p)
        {
            var circle = new Circle(p, 0);
            for (var i = 0; i < points.Count; i++)
            {
                var q = points[i];
                if (!circle.Contains(q))
                {
                    circle = circle.Radius == 0 ? GetDiameter(p, q) : GetCircleTwoPoints(points.GetRange(0, i + 1), p, q);
                }
            }
            return circle;
        }
        
        private static Circle GetCircleTwoPoints(List<Point> points, Point p, Point q)
        {
            var temp = GetDiameter(p, q);
            if (temp.Contains(points))
                return temp;

            var left = new Circle(new Point(0, 0), -1);
            var right = new Circle(new Point(0, 0), -1);
            foreach (Point r in points)
            {  // Form a circumcircle with each point
                var pq = q.Subtract(p);
                var cross = pq.Cross(r.Subtract(p));
                var c = GetCircumcircle(p, q, r);
                if (c.Radius == -1)
                    continue;
                if (cross > 0 && (left.Radius == -1 || pq.Cross(c.Center.Subtract(p)) > pq.Cross(left.Center.Subtract(p))))
                    left = c;
                else if (cross < 0 && (right.Radius == -1 || pq.Cross(c.Center.Subtract(p)) < pq.Cross(right.Center.Subtract(p))))
                    right = c;
            }

            return right.Radius == -1 || left.Radius != -1 && left.Radius <= right.Radius ? left : right;
        }
        
        private static Circle GetDiameter(Point a, Point b)
        {
            return new Circle(new Point((a.X + b.X) / 2, (a.Y + b.Y) / 2), a.Distance(b) / 2);
        }

        private static Circle GetCircumcircle(Point a, Point b, Point c)
        {
            var d = (a.X * (b.Y - c.Y) + b.X * (c.Y - a.Y) + c.X * (a.Y - b.Y)) * 2;
            if (d == 0)
                return new Circle(new Point(0, 0), -1);
            var x = (a.Norm() * (b.Y - c.Y) + b.Norm() * (c.Y - a.Y) + c.Norm() * (a.Y - b.Y)) / d;
            var y = (a.Norm() * (c.X - b.X) + b.Norm() * (a.X - c.X) + c.Norm() * (b.X - a.X)) / d;
            var p = new Point(x, y);
            return new Circle(p, p.Distance(a));
        }
    }
}
