using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WaveletCompression {
	public class ResolutionLevel: Location {

		private int _level;
		private Band[] _bands;

		public ResolutionLevel(int level, Point start, Size size, Size cdBlckSize) 
			: base(start, size) {
			_level = level;

			if (level == 0) {
				// Zero level has only LL band
				_bands = new Band[1];
				_bands[0] = new Band(Band.Orientir.LL, start, size, cdBlckSize);
			} else {
				// Other level has only LH, HL, HH bands
				_bands = new Band[3];
				// Bands size div of size resolution
				size = new Size(MathUtils.CeilDiv(size.Width, 2), MathUtils.CeilDiv(size.Height, 2));
				for (int i = 0; i < 3; ++i) {
					_bands[i] = new Band((Band.Orientir)(i + 1), start, size, cdBlckSize);
				}
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"RESOLUTION LEVEL {_level}");
			sb.AppendLine(base.ToString());
			for(int b = 0; b < _bands.Length; ++b) {
				sb.AppendLine(_bands[b].ToString());
			}
			return sb.ToString();
		}
	}
}
