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
    public class OcTree : PhysicalObject
    {
        private readonly OcTreeNode _mainNode;

        public OcTree(BoundingBox gameZone, IEnumerable<PhysicalObject> objects, bool advanced = false, float minBoxSize = 3f)
        {
            var treeObjects = objects.Select((o) => new OcTreeObject(o)).ToList();
            _mainNode = new OcTreeNode(gameZone, treeObjects, minBoxSize, advanced);
        }

        public bool Intersect(PhysicalObject obj) =>
            _mainNode.Intersect(new OcTreeObject(obj));

        public bool Intersect(Vector2 pos, float rad) =>
            _mainNode.Intersect(new OcTreeObject(new PhysicalObject() { Position = pos, Radius = rad }));

        public List<T> Overlap<T>(Vector2 pos, float rad) where T : PhysicalObject
        {
            var result = new List<OcTreeObject>();
            var obj = new OcTreeObject(new PhysicalObject() { Position = pos, Radius = rad });
            _mainNode.Overlap(obj, result);
            return result.Select((o) => o.LinkedObject as T).ToList();
        }


        public void DrawTree(int res = 2048, string file = "tree.png")
        {
            using (Image<Rgba32> image = new Image<Rgba32>(res, res))
            {
                image.Mutate((x) => _mainNode.DrawNode(x, res, res, _mainNode.NodeBox));
                image.Save(file);
            }
        }
    }
}
