namespace TTM3V2
{
    public class Vector2
    {
        private int x;
        private int y;

        public int X 
        {
            get { return x; } 
            private set {  x = value; } 
        }

        public int Y
        {
            get { return y; }
            private set { y = value; }
        }

        public Vector2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
