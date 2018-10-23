using System;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System.Drawing.Text;
using System.Drawing;
//using ImGui.Common.Primitive;
using System.Numerics;

namespace PongGlobe.ImGui
{
    internal class GlyphData
    {
        public char Character;
        public string FontFamily;
        public FontStyle FontStyle { get; }
        public FontWeight FontWeight { get; }

        public List<List<Vector2>> Polygons;
        public List<(Vector2, Vector2, Vector2)> QuadraticCurveSegments;

        public GlyphData(char character, string fontFamily,
            FontStyle fontStyle, FontWeight fontWeight,
            List<List<Vector2>> polygons,
            List<(Vector2, Vector2, Vector2)> quadraticCurveSegments)
        {
            this.Character = character;
            this.FontFamily = fontFamily;
            this.FontStyle = fontStyle;
            this.FontWeight = fontWeight;
            this.Polygons = polygons;
            this.QuadraticCurveSegments = quadraticCurveSegments;
        }
    }

    internal class GlyphCache
    {
        public static GlyphCache Default { get; } = new GlyphCache();

        private MemoryCache cache = new MemoryCache(new MemoryCacheOptions());

        private int CalcKey(char character, string fontFamily, FontStyle fontStyle, FontWeight fontWeight)
        {
            int hash = 17;
            hash = hash * 23 + character.GetHashCode();
            hash = hash * 23 + fontFamily.GetHashCode();
            //TODO consider fontStyle and fontWeight when Typography is ready.
            //hash = hash * 23 + fontStyle.GetHashCode();
            //hash = hash * 23 + fontWeight.GetHashCode();
            return hash;
        }

        public GlyphData AddGlyph(char character, string fontFamily, FontStyle fontStyle, FontWeight fontWeight,
            List<List<Vector2>> polygons,
            List<(Vector2, Vector2, Vector2)> quadraticCurveSegments)
        {
            GlyphData glyph = new GlyphData(character, fontFamily, fontStyle, fontWeight,
            polygons, quadraticCurveSegments);

            int key = CalcKey(character, fontFamily, fontStyle, fontWeight);

            cache.Set<GlyphData>(key, glyph);

            return glyph;
        }

        public GlyphData GetGlyph(char character, string fontFamily, FontStyle fontStyle, FontWeight fontWeight)
        {
            int key = CalcKey(character, fontFamily, fontStyle, fontWeight);
            return cache.Get<GlyphData>(key);
        }
    }
}
