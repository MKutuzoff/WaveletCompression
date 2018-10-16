using System.Diagnostics;
using System.IO;

namespace WaveletCompression {
	internal class Jp2kImageHeaderBox : Jp2kBox {

		private readonly uint _height;
		private readonly uint _width;
		private readonly byte _numC;
		private readonly byte _bPCSign;
		private readonly byte _bPCDepth;
		private readonly byte _c;
		private readonly byte _unkC;
		private readonly byte _iPR;

		public uint Height => _height;
		public uint Width => _width;

		public Jp2kImageHeaderBox(Stream stream, Jp2kBoxNavigation navigation) : base(navigation) {
			long end = navigation.Position + navigation.Length;
			stream.Seek(navigation.PositionData, SeekOrigin.Begin);
			_height = stream.ReadUInt32();
			_width = stream.ReadUInt32();
			_numC = stream.ReadUInt8();
			_bPCSign = stream.ReadUInt8();
			_bPCDepth = stream.ReadUInt8();
			_c = stream.ReadUInt8();
			_unkC = stream.ReadUInt8();
			_iPR = stream.ReadUInt8();
			Debug.Assert(end == stream.Position);
		}
	}
}