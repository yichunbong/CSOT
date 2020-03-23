using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSOT.UserInterface.Utils
{
    public class ColorGenHelper        
    {
        public const string CATETORY_PRODUCT = "PRODUCT";
        public const string CATETORY_MASK = "MASK";

        private static Random _random = new Random(10);
        private static Dictionary<int, Color> _hashColors = new Dictionary<int, Color>();
        private static Dictionary<string, ColorGen> _colorGens = new Dictionary<string, ColorGen>();

        public static Color GetColorByKey(string key, string catetory = null)
        {
            if (catetory == null)
                catetory = string.Empty;

            ColorGen colorGen;
            if (_colorGens.TryGetValue(catetory, out colorGen) == false)
                _colorGens.Add(catetory, colorGen = new ColorGen());

            return colorGen.GetColorByKey(key);
        }

        public static Color GetColorByHashCode(string key)
        {
            if (key == null)
                return Color.White;

            int hash = key.GetHashCode() % 25;

            Color color;
            if (_hashColors.TryGetValue(hash, out color) == false)
                _hashColors.Add(hash, color = ColorGenHelper.Gen());

            return color;
        }

        public static Color GetComplementaryColor(Color c)
        {
            return Color.FromArgb(255 - c.A, 255 - c.R, 255 - c.G, 255 - c.B);
        }

        private static Color Gen()
        {
            var color = Color.FromArgb(255, _random.Next(128, 255), _random.Next(128, 255), _random.Next(128, 255));

            return color;
        }

        public class ColorGen
        {           
            private Dictionary<string, Color> _colors = new Dictionary<string, Color>();
            private HashSet<Color> _usedList = new HashSet<Color>();

            /// <summary>
            /// Returns a generated ARGB(alpha,red,green,blue) color for the specified key.
            /// </summary>
            /// <param name="productID">The key to get the generated color.</param>
            /// <returns>The generated ARGB(alpha,red,green,blue) color of the specified key.</returns>
            public Color GetColorByKey(string productID)
            {
                if (productID == null)
                    return Color.White;

                Color color;
                if (_colors.TryGetValue(productID, out color))
                    return color;

                color = this.Gen(_usedList);

                _colors.Add(productID, color);
                _usedList.Add(color);

                return color;
            }
            
            private Color Gen(HashSet<Color> list)
            {
                var color = ColorGenHelper.Gen();

                if (list == null || list.Count == 0)
                    return color;

                if (list.Contains(color) == false)
                    return color;

                for (int i = 0; i < 10; i++)
                {
                    color = ColorGenHelper.Gen();

                    if (list.Contains(color) == false)
                        return color;
                }

                return color;
            }
        }        
    }
}
