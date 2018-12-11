using System.Drawing;

namespace WaveletCompression {
	public class CodeBlock: Location {
		
		public CodeBlock(Point start, Point end) : 
			base(start, end)  {	}
	}
}