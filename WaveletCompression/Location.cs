using System.Drawing;

namespace WaveletCompression {
	public class Location {

		private readonly Point _start;
		private readonly Point _end;
		private readonly Size _size;

		public int Xstart => _start.X;
		public int Ystart => _start.Y;
		public int Xend => _end.X;
		public int Yend => _end.Y;
		public int Width => _size.Width;
		public int Height => _size.Height;

		public Location(Point start, Size size) {
			_start = start;
			_size = size;
			_end = new Point(start.X + size.Width, start.Y + size.Height);
		}

		public Location(Point start, Point end) {
			_start = start;
			_end = end;
			_size = new Size(end.X - start.X, end.Y - start.Y);
		}

		public override string ToString() {
			return $"xs = {Xstart} ys = {Ystart} xe = {Xend} ye = {Yend} w = {Width} h = {Height}";
		}
	}
}