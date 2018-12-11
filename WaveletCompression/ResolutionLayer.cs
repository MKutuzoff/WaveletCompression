using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace WaveletCompression {
	public class ResolutionLevel {

		private readonly int _level;
		private readonly Location _location;
		private Band[] _bands;

		public ResolutionLevel(int level, Point start, Size size, Size cdBlckSize) {
			_location = new Location(start, size);
			_level = level;

			if (level == 0) {
				// Zero level has only LL band
				_bands = new Band[1];
				_bands[0] = new Band(Band.Orientir.LL, start, size, cdBlckSize);
			} else {
				// Other level has only LH, HL, HH bands
				_bands = new Band[3];
				// Bands size div of size resolution
				size = new Size(MathUtils.CeilDivPow2(size.Width, 1), MathUtils.CeilDivPow2(size.Height, 1));
				_bands[0] = new Band(Band.Orientir.HL, start, size, cdBlckSize);
				_bands[1] = new Band(Band.Orientir.LH, start, size, cdBlckSize);
				_bands[2] = new Band(Band.Orientir.HH, start, size, cdBlckSize);
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"RESOLUTION LEVEL {_level}");
			sb.AppendLine(_location.ToString());
			for(int b = 0; b < _bands.Length; ++b) {
				sb.AppendLine(_bands[b].ToString());
			}
			return sb.ToString();
		}
	}
}
