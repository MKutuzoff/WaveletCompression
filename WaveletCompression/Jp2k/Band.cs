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

		public int DataSize => _codeBlocks.Sum(cb => cb.DataSize);

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

		public void ReadDataInfo(BitReader bitReader) {
			for (int cb = 0; cb < _codeBlocks.Length; ++cb) {
				var included = _inclustionTagTree.Decode(cb, 1, bitReader) == 1;
				if (included) {
					MsbTagTreeDecode(cb, bitReader);
					var codePasses = bitReader.ReadCodePasses();
					var code = GetCommaCode(bitReader);
					var bits = code + 3; // magic value 3
					var length = bits + MathUtils.Log2(codePasses);
					_codeBlocks[cb].DataSize = bitReader.Read(length);
				}
			}
		}

		public void CopyData(BitReader bitReader) {
			for(int cb = 0; cb < _codeBlocks.Length; ++cb) {
				_codeBlocks[cb].CopyData(bitReader);
			}
		}

		public override string ToString() {
			var sb = new StringBuilder();
			sb.AppendLine($"BAND {_orientir} SIZE: {DataSize}");
			sb.AppendLine(_location.ToString());
			for (int c = 0; c < _codeBlocks.Length; ++c) {
				sb.AppendLine($"CODE BLOCK {c} SIZE: {_codeBlocks[c].DataSize}");
				sb.AppendLine(_codeBlocks[c].ToString());
			}
			return sb.ToString();
		}

		private static int GetCommaCode(BitReader bitReader) {
			int m = 0;
			while (bitReader.ReadBit())
				++m;
			return m;
		}

		private void MsbTagTreeDecode(int codeBlock, BitReader bitReader) {
			int i = 1;
			for (; ; ) {
				int ret = _msbTagTree.Decode(codeBlock, i, bitReader);
				if (ret > 0) {
					break;
				}
				++i;
			}
		}
	}
}
