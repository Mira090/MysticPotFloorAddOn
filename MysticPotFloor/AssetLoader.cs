using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace MysticPotFloor
{
    public class AssetLoader
    {
        public static string GetAssetsPath(string name)
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryInfo directoryInfo = Directory.GetParent(dllPath);
            string dllDirectory = directoryInfo.FullName;
            var path = dllDirectory + @"\Assets\" + name + ".png";
            return path;
        }
        public static string GetAssetsFolder(string name)
        {
            string dllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            DirectoryInfo directoryInfo = Directory.GetParent(dllPath);
            string dllDirectory = directoryInfo.FullName;
            var path = dllDirectory + @"\Assets\" + name;
            return path;
        }
        public static Sprite LoadSprite(string name)
        {
            var path = GetAssetsPath(name);
            if (!File.Exists(path))
            {
                Core.LoggerWarning(path + " is not exist!");
                return null;
            }

            byte[] fileData = File.ReadAllBytes(path);
            Texture2D tex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            tex.LoadImage(fileData);

            var sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 16
            );
            sprite.name = name;
            sprite.texture.filterMode = FilterMode.Point;
            //sprite.bounds.extents = new Vector3(sprite.bounds.extents.x * 6, sprite.bounds.extents.y * 6, sprite.bounds.extents.z);
            return sprite;
        }
    }
}
