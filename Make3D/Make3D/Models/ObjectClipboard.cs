using Barnacle.Object3DLib;
using System.Collections.Generic;

namespace Barnacle.Models
{
    internal class ObjectClipboard
    {
        public static List<Object3D> Items = new List<Object3D>();

        public static void Add(Object3D obj)
        {
            Object3D cl = obj.Clone();
            Items.Add(cl);
        }

        public static void Clear()
        {
            Items.Clear();
        }

        public static bool HasItems()
        {
            return Items.Count > 0;
        }
    }
}