using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace WaveletCompression {
	internal class CodestreamBox : Jp2kBox {

		public enum MarkerType : ushort {
			// Delimiting

			/// <summary> Start Of Codestream </summary>
			SOC = 0xFF4F,

			/// <summary> Start Of Tile-part </summary>
			SOT = 0xFF90,

			/// <summary> Start Of Data </summary>
			SOD = 0xFF93,

			/// <summary> End Of Codestream </summary>
			EOC = 0xFFD9,

			// Fixed information

			/// <summary> Image and tile size </summary>
			SIZ = 0xFF51,

			// Functional

			/// <summary> Coding style, Default </summary>
			COD = 0xFF52,

			/// <summary> Coding style, Component </summary>
			COC = 0xFF53,

			/// <summary> Region of interest </summary>
			RGN = 0xFF53,

			/// <summary> Quantization, Default </summary>
			QCD = 0xFF5C,

			/// <summary> Quantization, Component </summary>
			QCC = 0xFF5D,

			/// <summary> Progression Order Change </summary>
			POC = 0xFF5F,

			// Pointer

			/// <summary> Tile-part Length, Main header </summary>
			TLM = 0xFF55,

			/// <summary> Packet Length, Main header </summary>
			PLM = 0xFF57,

			/// <summary> Packet Length, Tile-part header </summary>
			PLT = 0xFF58,

			/// <summary> Packed Packet headers, Main header </summary>
			PPM = 0xFF60,

			/// <summary> Packed Packet headers, Tile-part header </summary>
			PPT = 0xFF61,

			// In bit-stream

			/// <summary> Start of packet </summary>
			SOP = 0xFF91,

			/// <summary> End of packet header </summary>
			EPH = 0xFF92,

			// Informational

			/// <summary> Component registration </summary>
			CRG = 0xFF63,

			/// <summary> Comment </summary>
			COM = 0xFF64,
		}

		private class Marker {
			protected MarkerType _type;
			protected ushort _length;
			protected byte[] _body;

			public MarkerType Type => _type;

			protected Marker(MarkerType type, ushort length, byte[] body) {
				_type = type;
				_length = length;
				_body = body;
			}

			public static MarkerType Peek(Stream stream) {
				var type = ReadMarkerType(stream);
				stream.Seek(-2, SeekOrigin.Current);
				return type;
			}

			public static Marker Read(Stream stream) {
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
					default:
						return new Marker(type, length, body);
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

		private class SizMarker: Marker {

			private ushort _profile;
			private Size _size;
			private Point _offset;
			private Size _tile;
			private ushort _components;
			private byte[] _precisions;
			private byte[] _sbSamplingX;
			private byte[] _sbSamplingY;

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
				for(int c = 0; c < _components; ++c) {
					_precisions[c] = memory.ReadUInt8();
					_sbSamplingX[c] = memory.ReadUInt8();
					_sbSamplingY[c] = memory.ReadUInt8();
				}
			}
		}

		private class SotMarker: Marker {

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

		public CodestreamBox(Stream stream, Jp2kBoxNavigation navigation): base(navigation) {
			stream.Seek(navigation.PositionData, SeekOrigin.Begin);
			Debug.Assert(MarkerType.SOC == Marker.Peek(stream));
			var markerDic = new Dictionary<MarkerType, Marker>();
			var markerTlm = new List<Marker>();
			while (Marker.Peek(stream) != MarkerType.SOT) {
				var marker = Marker.Read(stream);
				if (marker.Type == MarkerType.TLM)
					markerTlm.Add(marker);
				else if (!markerDic.ContainsKey(marker.Type))
					markerDic.Add(marker.Type, marker);
			}
			var siz = markerDic[MarkerType.SIZ] as SizMarker;
			while(Marker.Peek(stream) != MarkerType.EOC) {
				long position = stream.Position;
				var sot = Marker.Read(stream) as SotMarker;

				

				var skip = sot.PartLength - (stream.Position - position);
				stream.Seek(skip, SeekOrigin.Current);
			}

		}
	}
}