using System.Drawing;
using WaveletCompression;

namespace Jp2k {
	public class CodeBlock {

		private readonly Location _location;
		private byte[] _data;
		public int DataSize { get; set; }
		public CodeBlock(Point start, Point end) {
			_location = new Location(start, end);
		}

		public override string ToString() {
			return _location.ToString();
		}

		public void CopyData(BitReader bitReader) {
			_data = bitReader.CopyByte(DataSize);
		}
	}
}