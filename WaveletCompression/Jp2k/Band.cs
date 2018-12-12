using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveletCompression;

namespace Jp2k {
	public enum BandOrientir {
		LL = 0,
		LH = 1,
		HL = 2,
		HH = 3
	}
	public class Band {

		private readonly Location _location;
		private readonly BandOrientir _orientir;
		private readonly CodeBlock[] _codeBlocks;
		private readonly TagTree _inclustionTagTree;
		private readonly TagTree _msbTagTree;

		public BandOrientir Orientir => _orientir;
		public CodeBlock[] CodeBlocks => _codeBlocks;
		public TagTree InclustionTT => _inclustionTagTree;
		public TagTree MsbTT => _msbTagTree;


		public Band(BandOrientir orientir, Point start, Size size, Size cdBlckSize) {
			// Compute code block counts
			int widthCount = MathUtils.CeilDiv(size.Width, cdBlckSize.Width);
			int heightCount = MathUtils.CeilDiv(size.Height, cdBlckSize.Height);
			_orientir = orientir;
			_location = new Location(start, size);
			
			_codeBlocks = new CodeBlock[widthCount * heightCount];
			int iCdBlck = 0;
			for (int h = 0; h < heightCount; ++h) {
				for (int w = 0; w < widthCount; ++w) {
					var cdBlckStart = new Point(w * cdBlckSize.Width, h * cdBlckSize.Height);
					var cdBlckEnd = (cdBlckStart + cdBlckSize).Clamp(size);
					_codeBlocks[iCdBlck] = new CodeBlock(cdBlckStart, cdBlckEnd);
					++iCdBlck;
				}
			}
			_inclustionTagTree = new TagTree(widthCount, heightCount);
			_msbTagTree = new TagTree(widthCount, heightCount);
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
