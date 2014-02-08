using System;


namespace Growthstories.Core
{
    public struct Size
    {

        public Size(double width, double height)
        {
            _Width = default(double);
            _Height = default(double);

            Width = width;
            Height = height;
        }

        public static Size Empty
        {
            get
            {
                var s = new Size(0, 0);
                s._Height = -1;
                s._Width = -1;
                return s;
            }
        }

        private double _Height;
        public double Height
        {
            get { return _Height; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Height cannot be negative");
                _Height = value;
            }
        }

        public bool IsEmpty { get { return this == Empty; } }

        private double _Width;
        public double Width
        {
            get { return _Width; }
            set
            {
                if (value < 0)
                    throw new ArgumentException("Width cannot be negative");
                _Width = value;
            }
        }

        public static bool operator !=(Size left, Size right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(Size left, Size right)
        {
            return left.Equals(right);
        }
        public override bool Equals(object o)
        {
            if (o is Size)
            {
                return Equals((Size)o);
            }
            return false;
        }

        public bool Equals(Size other)
        {
            return Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            return Width.GetHashCode() ^ Height.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Size of {0}x{1}", Width, Height);
        }
    }
}
