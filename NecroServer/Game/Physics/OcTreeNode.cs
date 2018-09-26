using System;
using System.Collections.Generic;
using System.Text;
using GameMath;
using System.Linq;

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

        public OcTreeNode(BoundingBox box, List<OcTreeObject> objects, float minBoxSize, bool advanced)
        {
            NodeBox = box;
            Objects = objects;
            if (NodeBox.Width <= minBoxSize && NodeBox.Height <= minBoxSize || Objects.Count <= 1) return;

            var (box1, box2) = advanced ? SplitAdvanced(NodeBox, Objects) : SplitDefault(NodeBox);
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
                Nodes.Add(new OcTreeNode(box1, objs1, minBoxSize, advanced));
            if (objs2.Count > 0)
                Nodes.Add(new OcTreeNode(box2, objs2, minBoxSize, advanced));
        }

        private (BoundingBox, BoundingBox) SplitDefault(BoundingBox nodeBox)
        {
            BoundingBox box1, box2;
            if (nodeBox.Width >= nodeBox.Height)
            {
                float halfWidth = nodeBox.Width / 2f;
                box1 = new BoundingBox(nodeBox.X, nodeBox.Y, halfWidth, nodeBox.Height);
                box2 = new BoundingBox(nodeBox.X + halfWidth, nodeBox.Y, halfWidth, nodeBox.Height);
            }
            else
            {
                float halfHeight = nodeBox.Height / 2f;
                box1 = new BoundingBox(nodeBox.X, nodeBox.Y, nodeBox.Width, halfHeight);
                box2 = new BoundingBox(nodeBox.X, nodeBox.Y + halfHeight, nodeBox.Width, halfHeight);
            }
            return (box1, box2);
        }

        private (BoundingBox, BoundingBox) SplitAdvanced(BoundingBox nodeBox, List<OcTreeObject> objects)
        {
            BoundingBox box1, box2;
            if (nodeBox.Width >= nodeBox.Height)
            {
                float mid = objects.Select((o) => o.Position.X).Average();
                box1 = new BoundingBox(nodeBox.X, nodeBox.Y, mid - nodeBox.X, nodeBox.Height);
                box2 = new BoundingBox(mid, nodeBox.Y, nodeBox.Width - (mid - nodeBox.X), nodeBox.Height);
            }
            else
            {
                float mid = objects.Select((o) => o.Position.Y).Average();
                box1 = new BoundingBox(nodeBox.X, nodeBox.Y, nodeBox.Width, mid - nodeBox.Y);
                box2 = new BoundingBox(nodeBox.X, mid, nodeBox.Width, nodeBox.Height - (mid - nodeBox.Y));
            }
            return (box1, box2);
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
        private void DrawRect(IImageProcessingContext<Rgba32> image, int iwidth, int iheight, BoundingBox mainRect, BoundingBox rect, Rgba32 color)
        {
            
            var top = (rect.Y - mainRect.Y) / mainRect.Height * iheight;
            var height = rect.Height / mainRect.Height * iheight;

            var left = (rect.X - mainRect.X) / mainRect.Width * iwidth;
            var width = rect.Width / mainRect.Width * iwidth;

            var right = left + width;
            var bottom = top + height;

            image.DrawPolygon(color, 2f, new PointF[] {
                        new PointF(left, top),
                        new PointF(right, top),
                        new PointF(right, bottom),
                        new PointF(left, bottom),
            });
        }
        public void DrawNode(IImageProcessingContext<Rgba32> image, int width, int height, BoundingBox rect)
        {
            DrawRect(image, width, height, rect, NodeBox, Rgba32.Green);

            foreach (var item in Objects)
                DrawRect(image, width, height, rect, item.Box, Rgba32.Red);

            foreach (var node in Nodes)
                node.DrawNode(image, width, height, rect);
        }
        #endregion
    }
}
