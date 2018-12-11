using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class Band {

		public enum Orientir {
			LL = 0,
			LH = 1,
			HL = 2,
			HH = 3
		}

		private readonly Location _location;
		private readonly Orientir _orientir;
		private readonly CodeBlock[] _codeBlocks;

		public Band(Orientir orientir, Point start, Size size, Size cdBlckSize) {
			_orientir = orientir;
			_location = new Location(start, size);
			// Compute code block counts
			int cCdBlkW = MathUtils.CeilDiv(size.Width, cdBlckSize.Width);
			int cCdBlkH = MathUtils.CeilDiv(size.Height, cdBlckSize.Height);
			_codeBlocks = new CodeBlock[cCdBlkH * cCdBlkW];
			int iCdBlck = 0;
			for (int h = 0; h < cCdBlkH; ++h) {
				for (int w = 0; w < cCdBlkW; ++w) {
					var cdBlckStart = new Point(w * cdBlckSize.Width, h * cdBlckSize.Height);
					var cdBlckEnd = (cdBlckStart + cdBlckSize).Clamp(size);
					_codeBlocks[iCdBlck] = new CodeBlock(cdBlckStart, cdBlckEnd);
					++iCdBlck;
				}
			}
		}

		public override string ToString() {
			var sb = new StringBuilder();
			sb.AppendLine($"BAND {_orientir}");
			sb.AppendLine(_location.ToString());
			for (int c = 0; c < _codeBlocks.Length; ++c) {
				sb.AppendLine($"CODE BLOCK {c}");
				sb.AppendLine(_codeBlocks[c].ToString());
			}
			return sb.ToString();
		}
	}
}
