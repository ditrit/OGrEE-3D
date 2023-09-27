using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// From <see href="https://github.com/mapbox/polylabel">Polylabel</see>, 
/// adapted from <see href="https://github.com/mapbox/polylabel/issues/26">this C# implementation</see>.
/// </summary>
public class PolyLabel
{
    /// <summary>
    /// Returns the most distant point inside a polygon from its exterior and its distance from the exterior
    /// </summary>
    /// <param name="_vertices">The vertices defining the exterior of a polygon</param>
    /// <param name="_precision">The precision needed of the pole of isolation</param>
    /// <returns>The radius and the center of the biggest center entirely contained in the polygon</returns>
    public static (float radius, Vector2 pole) FindPoleOfIsolation(List<Vector2> _vertices, float _precision = 1)
    {
        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (Vector2 edgePoint in _vertices)
        {
            minX = minX > edgePoint.x ? edgePoint.x : minX;
            minY = minY > edgePoint.y ? edgePoint.y : minY;
            maxX = maxX < edgePoint.x ? edgePoint.x : maxX;
            maxY = maxY < edgePoint.y ? edgePoint.y : maxY;
        }

        float width = maxX - minX;
        float height = maxY - minY;
        float cellSize = Mathf.Min(width, height);
        float h = cellSize / 2;

        Cell bestCell = GetCentroidCell(_vertices);

        Cell bboxCell = new(minX + width / 2, minY + height / 2, 0, _vertices);
        bestCell = bboxCell.distance > bestCell.distance ? bboxCell : bestCell;

        PriorityQueue cellQueue = new();

        for (float x = minX; x < maxX; x += cellSize)
        {
            for (float y = minY; y < maxY; y += cellSize)
            {
                Cell cell = new(x + h, y + h, h, _vertices);
                cellQueue.Enqueue(cell, cell.maxDistance);
            }
        }

        while (cellQueue.Count > 0)
        {
            Cell cell = cellQueue.Dequeue();

            if (cell.distance > bestCell.distance)
                bestCell = cell;

            if (cell.maxDistance - bestCell.distance <= _precision)
                continue;

            h = cell.halfSize / 2;

            Cell cell1 = new(cell.center.x - h, cell.center.y - h, h, _vertices); ;
            cellQueue.Enqueue(cell1, cell1.maxDistance);
            Cell cell2 = new(cell.center.x + h, cell.center.y - h, h, _vertices);
            cellQueue.Enqueue(cell2, cell2.maxDistance);
            Cell cell3 = new(cell.center.x - h, cell.center.y + h, h, _vertices);
            cellQueue.Enqueue(cell3, cell3.maxDistance);
            Cell cell4 = new(cell.center.x + h, cell.center.y + h, h, _vertices);
            cellQueue.Enqueue(cell4, cell4.maxDistance);
        }

        return (bestCell.distance, bestCell.center);
    }

    /// <summary>
    /// Create a cell centered on the centroid of a polygon
    /// </summary>
    /// <param name="_vertices">The vertices of the polygon</param>
    /// <returns>A Cell centered on the centroid of a polygon</returns>
    private static Cell GetCentroidCell(List<Vector2> _vertices)
    {
        Vector2 centroid = Centroid(_vertices);
        return new(centroid.x, centroid.y, 0, _vertices);
    }

    /// <summary>
    /// Compute the centroid of a polygon 
    /// <br/><see href="https://en.wikipedia.org/wiki/Centroid#Of_a_polygon"/>
    /// </summary>
    /// <param name="_vertices">The ordonned list of the polygon's points</param>
    /// <returns>Its geometric center, aka centroid</returns>
    private static Vector2 Centroid(List<Vector2> _vertices)
    {
        int pointCount = _vertices.Count;
        float totalArea = 0;
        float centroidX = 0;
        float centroidY = 0;

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 currentPoint = _vertices[i];
            Vector2 nextPoint = _vertices[(i + 1) % pointCount]; // Wraps around to the first point for the last iteration

            float area = (currentPoint.x * nextPoint.y) - (nextPoint.x * currentPoint.y);
            totalArea += area;

            centroidX += (currentPoint.x + nextPoint.x) * area;
            centroidY += (currentPoint.y + nextPoint.y) * area;
        }

        totalArea *= 0.5f;
        centroidX /= 6 * totalArea;
        centroidY /= 6 * totalArea;

        return new(centroidX, centroidY);
    }

    /// <summary>
    /// Used for the pole of isolation algorithm
    /// </summary>
    private class Cell
    {
        public Vector2 center; //center of the Cell
        public float halfSize; //half of a cell's size
        public float distance; //real distance
        public float maxDistance; //maximum potential distance

        public Cell(float _x, float _y, float _h, List<Vector2> _vertices)
        {
            center = new(_x, _y);
            halfSize = _h;
            distance = PointToPolygonDist(center, _vertices);
            maxDistance = distance + halfSize * (float)Math.Sqrt(2);
        }

        /// <summary>
        /// Compute the distance between a point and a the exterior of a polygon
        /// </summary>
        /// <param name="_point">The _point</param>
        /// <param name="_vertices">The vertices of the polygon</param>
        /// <returns>The distance between the point and the exterior of the polygon, negative if the point is outside the polygon</returns>
        private float PointToPolygonDist(Vector2 _point, List<Vector2> _vertices)
        {
            float minDist = float.PositiveInfinity;

            for (int i = 0; i < _vertices.Count; i++)
                minDist = Math.Min(minDist, DistancePointSegment(_point, _vertices[i], _vertices[(i + 1) % _vertices.Count]));
            if (!IsPointInside(_point, _vertices))
                minDist *= -1;
            return minDist;
        }

        /// <summary>
        /// Compute the distance between a point and a segment
        /// </summary>
        /// <param name="_point">The point</param>
        /// <param name="_segStart">The start of the segment</param>
        /// <param name="_segEnd">The end of a segment</param>
        /// <returns></returns>
        private float DistancePointSegment(Vector2 _point, Vector2 _segStart, Vector2 _segEnd)
        {
            float squaredLength = Mathf.Pow(Vector2.Distance(_segEnd, _segStart), 2);
            if (squaredLength == 0)
                return Vector2.Distance(_point, _segStart);
            float t = Mathf.Clamp01(Vector2.Dot(_point - _segStart, _segEnd - _segStart) / squaredLength);
            Vector2 projection = _segStart + t * (_segEnd - _segStart);
            return Vector2.Distance(_point, projection);
        }

        /// <summary>
        /// Check if a point is inside a polygon
        /// </summary>
        /// <param name="_point">The point</param>
        /// <param name="_vertices">The vertices of the polygon</param>
        /// <returns>True if the point is inside the polygon</returns>
        public static bool IsPointInside(Vector2 _point, List<Vector2> _vertices)
        {
            int count = _vertices.Count;
            bool isInside = false;

            for (int i = 0, j = count - 1; i < count; j = i++)
                if ((_vertices[i].y > _point.y) != (_vertices[j].y > _point.y) &&
                    (_point.x < _vertices[j].x + (_point.y - _vertices[j].y) * (_vertices[i].x - _vertices[j].x) / (_vertices[i].y - _vertices[j].y)))
                    isInside = !isInside;

            return isInside;
        }
    }

    /// <summary>
    /// List of Cell, ordered by a float priority, used for the pole of isolation algorithm
    /// </summary>
    private class PriorityQueue
    {
        private readonly List<(Cell c, float p)> list;

        public int Count { get => list.Count; }

        public PriorityQueue()
        {
            list = new();
        }

        /// <summary>
        /// Add a Cell to the list
        /// </summary>
        /// <param name="_cell">The Cell</param>
        /// <param name="_priority">The priority of the Cell</param>
        public void Enqueue(Cell _cell, float _priority)
        {
            list.Add((_cell, _priority));
        }

        /// <summary>
        /// Remove the Cell with the higher priority from the list
        /// </summary>
        /// <returns>The Cell with the higher priority</returns>
        public Cell Dequeue()
        {
            float maxPriority = float.MinValue;
            int indexMax = 0;
            for (int i = 0; i < list.Count; i++)
            {
                (Cell _, float p) = list[i];
                if (maxPriority < p)
                {
                    maxPriority = p;
                    indexMax = i;
                }
            }
            Cell elem = list[indexMax].c;
            list.RemoveAt(indexMax);
            return elem;
        }
    }
}
