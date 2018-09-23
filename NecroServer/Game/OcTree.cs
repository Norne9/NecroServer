using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GameMath;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.PixelFormats;

namespace Game
{
    public class OcTree<T> where T : PhysicalObject
    {
        private readonly OcTreeNode MainNode;

        public OcTree(BoundingBox gameZone, IEnumerable<T> objects, float minBoxSize = 3f)
        {
            var treeObjects = objects.Select((o) => new OcTreeObject(o)).ToList();
            MainNode = new OcTreeNode(gameZone, treeObjects, minBoxSize);
        }

        public bool Intersect(T obj) =>
            MainNode.Intersect(new OcTreeObject(obj));

        public bool Intersect(Vector2 pos, float rad) =>
            MainNode.Intersect(new OcTreeObject(new PhysicalObject() { Position = pos, Radius = rad }));

        public List<T> Overlap(Vector2 pos, float rad)
        {
            var result = new List<OcTreeObject>();
            var obj = new OcTreeObject(new PhysicalObject() { Position = pos, Radius = rad });
            MainNode.Overlap(obj, result);
            return result.Select((o) => (T)o.LinkedObject).ToList();
        }


        public void DrawTree(int res = 2048, string file = "tree.png")
        {
            using (Image<Rgba32> image = new Image<Rgba32>(res, res))
            {
                MainNode.DrawNode(image, MainNode.NodeBox);
                image.Save(file);
            }
        }
    }
}
