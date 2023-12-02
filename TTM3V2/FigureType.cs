using System.Drawing;
using System.Windows.Media;

namespace TTM3V2
{
    public class FigureType
    {
        private int typeID = 0;
        private ImageSource image = null;

        public int TypeID { get { return typeID; } }
        public ImageSource Image { get { return image; } }

        public FigureType(int typeID, ImageSource image)
        {
            this.typeID = typeID;
            this.image = image;
        }
    }
}