using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using WaveletCompression;

namespace Jp2k {
	public class ResolutionLevel {

		private readonly int _level;
		private readonly Location _location;
		private Band[] _bands;

		public Band[] Bands => _bands;

		public ResolutionLevel(int level, Point start, Size srcSize, Size cdBlckSize) {
			_location = new Location(start, srcSize);
			_level = level;

			/* Band orientirs
			 * 
			 * -----------
			 * | LL | HL |
			 * -----------
			 * | LH | HH |
			 * -----------
			 * 
			*/

			if (level == 0) {
				// Zero level has only LL band
				_bands = new Band[1];
				_bands[0] = new Band(BandOrientir.LL, start, srcSize, cdBlckSize);
			} else {
				// Other level has only LH, HL, HH bands
				_bands = new Band[3];
				// Bands size div of size resolution
				var width = MathUtils.CeilDivPow2(srcSize.Width, 1);
				var height = MathUtils.CeilDivPow2(srcSize.Height, 1);                         
				_bands[0] = new Band(BandOrientir.HL, start, new Size(srcSize.Width - width, height), cdBlckSize);
				_bands[1] = new Band(BandOrientir.LH, start, new Size(width, srcSize.Height - height), cdBlckSize);
				_bands[2] = new Band(BandOrientir.HH, start, new Size(srcSize.Width - width, srcSize.Height - height), cdBlckSize);
			}
		}

		public override string ToString() {
			StringBuilder sb = new StringBuilder();
			sb.AppendLine($"RESOLUTION LEVEL {_level}");
			sb.AppendLine(_location.ToString());
			sb.AppendLine();
			for (int b = 0; b < _bands.Length; ++b) {
				sb.AppendLine(_bands[b].ToString());
			}
			return sb.ToString();
		}
	}
}
