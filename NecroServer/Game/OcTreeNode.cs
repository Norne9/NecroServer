using System;
using System.Collections.Generic;
using System.Text;
using GameMath;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.Primitives;

namespace Game
{
    public class OcTreeNode
    {
        public BoundingBox NodeBox { get; }
        public List<OcTreeObject> Objects { get; set; }
        public List<OcTreeNode> Nodes { get; set; } = new List<OcTreeNode>();

        public OcTreeNode(BoundingBox box, List<OcTreeObject> objects, float minBoxSize)
        {
            NodeBox = box;
            Objects = objects;
            if (NodeBox.Width <= minBoxSize && NodeBox.Height <= minBoxSize || Objects.Count <= 1) return;

            BoundingBox box1, box2;
            if (NodeBox.Width >= NodeBox.Height)
            {
                float halfWidth = NodeBox.Width / 2f;
                box1 = new BoundingBox(            NodeBox.X, NodeBox.Y, halfWidth, NodeBox.Height);
                box2 = new BoundingBox(NodeBox.X + halfWidth, NodeBox.Y, halfWidth, NodeBox.Height);
            }
            else
            {
                float halfHeight = NodeBox.Height / 2f;
                box1 = new BoundingBox(NodeBox.X,              NodeBox.Y, NodeBox.Width, halfHeight);
                box2 = new BoundingBox(NodeBox.X, NodeBox.Y + halfHeight, NodeBox.Width, halfHeight);
            }

            var objs1 = new List<OcTreeObject>();
            var objs2 = new List<OcTreeObject>();

            for (int i = Objects.Count - 1; i >= 0; i--)
            {
                var obj = Objects[i];
                if (box1.Contains(obj.Box))
                {
                    objs1.Add(obj);
                    Objects.RemoveAt(i);
                }
                else if (box2.Contains(obj.Box))
                {
                    objs2.Add(obj);
                    Objects.RemoveAt(i);
                }
            }

            if (objs1.Count > 0)
                Nodes.Add(new OcTreeNode(box1, objs1, minBoxSize));
            if (objs2.Count > 0)
                Nodes.Add(new OcTreeNode(box2, objs2, minBoxSize));
        }

        public bool Intersect(OcTreeObject obj)
        {
            foreach (var item in Objects)
                if (obj.LinkedObject != item.LinkedObject && obj.Intersect(item))
                    return true;

            foreach (var node in Nodes)
                if (node.NodeBox.Intersect(obj.Box))
                    if (node.Intersect(obj))
                        return true;

            return false;
        }

        public void Overlap(OcTreeObject obj, List<OcTreeObject> result)
        {
            foreach (var item in Objects)
                if (obj.Intersect(item))
                    result.Add(item);

            foreach (var node in Nodes)
                if (node.NodeBox.Intersect(obj.Box))
                    node.Overlap(obj, result);
        }


        #region Visual
        private void DrawRect(Image<Rgba32> image, BoundingBox mainRect, BoundingBox rect, Rgba32 color)
        {
            
            var top = (rect.Y - mainRect.Y) / mainRect.Height * image.Height;
            var height = rect.Height / mainRect.Height * image.Height;

            var left = (rect.X - mainRect.X) / mainRect.Width * image.Width;
            var width = rect.Width / mainRect.Width * image.Width;

            var right = left + width;
            var bottom = top + height;

            image.Mutate(x => x.DrawPolygon(color, 2f, new PointF[] {
                        new PointF(left, top),
                        new PointF(right, top),
                        new PointF(right, bottom),
                        new PointF(left, bottom),
            }));
        }
        public void DrawNode(Image<Rgba32> image, BoundingBox rect)
        {
            DrawRect(image, rect, NodeBox, Rgba32.Green);

            foreach (var item in Objects)
                DrawRect(image, rect, item.Box, Rgba32.Red);

            foreach (var node in Nodes)
                node.DrawNode(image, rect);
        }
        #endregion
    }
}
