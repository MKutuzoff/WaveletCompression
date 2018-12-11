using System.Drawing;

namespace WaveletCompression {
	public class CodeBlock {

		private readonly Location _location;
		public CodeBlock(Point start, Point end) {
			_location = new Location(start, end);
		}

		public override string ToString() {
			return _location.ToString();
		}
	}
}