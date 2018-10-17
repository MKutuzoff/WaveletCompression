using System;
using System.IO;
using System.Drawing;

namespace WaveletCompression {

	public enum MarkerType : ushort {
		SOC = 0xFF4F,
		SOT = 0xFF90,
		SOD = 0xFF93,
		EOC = 0xFFD9,
		SIZ = 0xFF51,
		COD = 0xFF52,
		COC = 0xFF53,
		RGN = 0xFF53,
		QCD = 0xFF5C,
		QCC = 0xFF5D,
		POC = 0xFF5F,
		TLM = 0xFF55,
		PLM = 0xFF57,
		PLT = 0xFF58,
		PPM = 0xFF60,
		PPT = 0xFF61,
		SOP = 0xFF91,
		EPH = 0xFF92,
		CRG = 0xFF63,
		COM = 0xFF64,
	}

	public class Jp2kMarker {
		protected MarkerType _type;
		protected ushort _length;
		protected byte[] _body;

		public MarkerType Type => _type;

		protected Jp2kMarker(MarkerType type, ushort length, byte[] body) {
			_type = type;
			_length = length;
			_body = body;
		}

		public static MarkerType Peek(Stream stream) {
			var type = ReadMarkerType(stream);
			stream.Seek(-2, SeekOrigin.Current);
			return type;
		}

		public static Jp2kMarker Read(Stream stream) {
			MarkerType type = ReadMarkerType(stream);
			ushort length = 0;
			byte[] body = null;
			if (IsSegment(type)) {
				length = stream.ReadUInt16();
				body = new byte[length - sizeof(ushort)];
				stream.Read(body, 0, body.Length);
			} else {
				body = new byte[0];
			}
			switch (type) {
				case MarkerType.SIZ:
					return new SizMarker(length, body);
				case MarkerType.SOT:
					return new SotMarker(length, body);
				case MarkerType.PLT:
					return new PltMarker(length, body);
				case MarkerType.COD:
					return new CodMarker(length, body);
				default:
					return new Jp2kMarker(type, length, body);
			}

		}
		private static MarkerType ReadMarkerType(Stream stream) {
			var b = stream.ReadUInt16();
			if (!Enum.IsDefined(typeof(MarkerType), b)) {
				throw new ArgumentException();
			}
			return (MarkerType)b;
		}

		private static bool IsSegment(MarkerType type) {
			switch (type) {
				case MarkerType.SOC:
				case MarkerType.SOD:
				case MarkerType.EOC:
					return false;
				default:
					return true;
			}
		}
	}

	public class SizMarker : Jp2kMarker {

		private ushort _profile;
		private Size _size;
		private Point _offset;
		private Size _tile;
		private ushort _components;
		private byte[] _precisions;
		private byte[] _sbSamplingX;
		private byte[] _sbSamplingY;

		public Size Size => _size;

		public SizMarker(ushort length, byte[] body) :
			base(MarkerType.SIZ, length, body) {

			var memory = new MemoryStream(body);
			_profile = memory.ReadUInt16();
			_size = new Size((int)memory.ReadUInt32(), (int)memory.ReadUInt32());
			_offset = new Point((int)memory.ReadUInt32(), (int)memory.ReadUInt32());
			_tile = new Size((int)memory.ReadUInt32(), (int)memory.ReadUInt32());
			_components = memory.ReadUInt16();
			_precisions = new byte[_components];
			_sbSamplingX = new byte[_components];
			_sbSamplingY = new byte[_components];
			for (int c = 0; c < _components; ++c) {
				_precisions[c] = memory.ReadUInt8();
				_sbSamplingX[c] = memory.ReadUInt8();
				_sbSamplingY[c] = memory.ReadUInt8();
			}
		}
	}

	public class SotMarker : Jp2kMarker {

		private readonly ushort _index;
		private readonly uint _pLength;
		private readonly byte _pIndex;
		private readonly byte _pCount;

		public uint PartLength => _pLength;

		public SotMarker(ushort length, byte[] body)
			: base(MarkerType.SOT, length, body) {
			var memory = new MemoryStream(body);
			_index = memory.ReadUInt16();
			_pLength = memory.ReadUInt32();
			_pIndex = memory.ReadUInt8();
			_pCount = memory.ReadUInt8();
		}
	}

	public class PltMarker : Jp2kMarker {

		private byte _zIndex;
		private ushort _packetLength;
		private MemoryStream _packet;

		public PltMarker(ushort length, byte[] body) 
			: base(MarkerType.PLT, length, body) {

			_zIndex = body[0];
			_packetLength = (ushort)(length - 3);
			_packet = new MemoryStream(body, 1, _packetLength);

		}
	}

	public class CodMarker: Jp2kMarker {

		public enum CodingStyle : byte {
			None = 0,
			UsePrecincts = 1,
			UseSopMarker = 2,
			UseEphMarker = 4
		}

		public enum CodeblockStyle : byte {
			None = 0,
			SelectiveArithmeticCodingBypass = 1,
			ResetContextProbabilitiesOnCodingPassBoundaries = 2,
			TerminateOnEachCodingPass = 4,
			VerticalCausalContext = 8,
			PredictableTermination = 16,
			SegmentationSymbols = 32
		}

		public enum WaveletTransform : byte {
			Irreversible_9_7 = 0,
			Reversible_5_3 = 1
		}

		public enum ProgressionOrder : byte {
			LRCP = 0,
			RLCP = 1,
			RPCL = 2,
			PCRL = 3,
			CPRL = 4
		}

		private CodingStyle _scod;
		private byte _cblkExpnX;
		private byte _cblkExpnY;
		private byte[] _ppx;
		private byte[] _ppy;

		public ProgressionOrder Progression { get; private set; }
		public ushort QualityLayers { get; private set; }
		public byte DecompositionLevels { get; private set; }
		public CodeblockStyle CBlkStyle { get; private set; }
		public bool UseMultipleComponentTransform { get; private set; }
		public WaveletTransform WaveletFilter { get; private set; }
		public bool UsePrecincts {
			get {
				return (_scod & CodingStyle.UsePrecincts) != 0;
			}
		}

		public CodMarker(ushort lenght, byte[] body) 
			: base(MarkerType.COD, lenght, body) {
			var mem = new MemoryStream(body);
			_scod = (CodingStyle)mem.ReadUInt8();
			Progression = (ProgressionOrder)mem.ReadUInt8();
			QualityLayers = mem.ReadUInt16();
			UseMultipleComponentTransform = mem.ReadUInt8() == 1;
			DecompositionLevels = mem.ReadUInt8();
			_cblkExpnX = mem.ReadUInt8();
			_cblkExpnY = mem.ReadUInt8();
			CBlkStyle = (CodeblockStyle)mem.ReadUInt8();
			WaveletFilter = (WaveletTransform)mem.ReadUInt8();
			_ppx = new byte[DecompositionLevels + 1];
			_ppy = new byte[DecompositionLevels + 1];
			for (int r = 0; r <= DecompositionLevels; r++) {
				if (UsePrecincts) {
					byte val = mem.ReadUInt8();
					_ppx[r] = (byte)(val & 0xF);
					_ppy[r] = (byte)((val >> 4) & 0xF);
				} else {
					_ppx[r] = _ppy[r] = 0xF;
				}
			}

		}

		public override string ToString() {
			return $"CodingStyle: {_scod}\nProgression: {Progression}\nCodeblockStyle: {CBlkStyle}\nWaveletTransform: {WaveletFilter}";
		}

	}
}
