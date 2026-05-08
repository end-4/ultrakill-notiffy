using System.IO;
using UnityEngine;

namespace Notiffy.Utils {
    // https://discussions.unity.com/t/generating-sprites-dynamically-from-png-or-jpeg-files-in-c/591103/5
    public static class Img2Sprite {
        public static Sprite LoadNewSprite(string filePath, float pixelsPerUnit = 100.0f,
            SpriteMeshType spriteType = SpriteMeshType.Tight) {
            // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

            Texture2D spriteTex = LoadTexture(filePath);
            Sprite newSprite = Sprite.Create(spriteTex, new Rect(0, 0, spriteTex.width, spriteTex.height),
                new Vector2(0, 0), pixelsPerUnit, 0, spriteType);

            return newSprite;
        }

        public static Sprite ConvertTextureToSprite(Texture2D texture, float pixelsPerUnit = 100.0f,
            SpriteMeshType spriteType = SpriteMeshType.Tight) {
            // Converts a Texture2D to a sprite, assign this texture to a new sprite and return its reference

            Sprite NewSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0),
                pixelsPerUnit, 0, spriteType);

            return NewSprite;
        }

        public static Texture2D LoadTexture(string FilePath) {
            // Load a PNG or JPG file from disk to a Texture2D
            // Returns null if load fails

            if (File.Exists(FilePath)) {
                byte[] fileData;
                Texture2D tex;
                fileData = File.ReadAllBytes(FilePath);
                tex = new Texture2D(2, 2); // Create new "empty" texture
                if (tex.LoadImage(fileData)) // Load the imagedata into the texture (size is set automatically)
                    return tex; // If data = readable -> return texture
            }

            return null; // Return null if load failed
        }
    }
}
