using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	static class MathUtils {

		// Compute the ceiling of the quotient of two integers
		public static int CeilDiv(int x, int y) {
			return (x + y - 1) / y;
		}

		// Compute the floor of ( x / 2^y)
		public static int FloorDivPow2(int x, int y) {
			return x >> y;
		}

		// Compute the ceil of ( x / 2^y)
		public static int CeilDivPow2(int x, int y) {
			return (x + (1 << y) - 1) >> y;
		}

		// Compute log 2 (x)
		public static int Log2(int x) {
			int i = 0;
			for (i = -1; x != 0; ++i)
				x >>= 1;
			return i == -1 ? 0 : i;
		}

		public static Point Clamp(this Point value, Size maxSize) {
			value.X = Math.Min(value.X, maxSize.Width);
			value.Y = Math.Min(value.Y, maxSize.Height);
			return value;
		}
	}
}
