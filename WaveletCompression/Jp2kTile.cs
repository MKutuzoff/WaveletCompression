using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class Jp2kTile {

		private class TileBlock {
			readonly int _length;
			readonly BitReader _data;

			public int Size {  get { return _length; } }

			public TileBlock(int length, byte[] data) {
				_length = length;
				_data = new BitReader(data);
			}


		}

		private SotMarker _sot;
		private List<PltMarker> _pltMarkers = new List<PltMarker>();

		private List<TileBlock> _tileBlocks = new List<TileBlock>();

		public Jp2kTile(Stream stream) {
			var position = stream.Position;
			while (Jp2kMarker.Peek(stream) != MarkerType.SOD) {
				var marker = Jp2kMarker.Read(stream);
				switch (marker.Type) {
					case MarkerType.SOT:
						_sot = marker as SotMarker;
						break;
					case MarkerType.PLT:
						_pltMarkers.Add(marker as PltMarker);
						break;
					default:
						break;
				}
			}
			stream.Seek(2, SeekOrigin.Current);
			var data = new byte[position + _sot.PartLength - stream.Position];
			stream.Read(data, 0, data.Length);
			var bitReader = new BitReader(data);
			while(!bitReader.EOF) {
				var tileBlock = ReadTileBlock(bitReader);
				if (tileBlock != null) {
					Console.WriteLine($"[INFO] Tile block size = {tileBlock.Size} bytes");
					_tileBlocks.Add(tileBlock);
				}
			}
		}

		private static TileBlock ReadTileBlock(BitReader bitReader) {
			if (bitReader.Read(1) > 0) {
				if (bitReader.Read(1) == 0) { }

				do {
					if (bitReader.EOF) return null;
				} while (bitReader.Read(1) != 1);


				int numBits = 3;
				var codePasses = ReadCodePasses(bitReader);
				int lBlock = 0;
				while (bitReader.Read(1) > 0)
					++lBlock;
				numBits += lBlock;
				var l = numBits + Log2(codePasses);
				var length = bitReader.Read(l);
				return new TileBlock(length, bitReader.CopyByte(length));
			}
			return null;
		}

		private static int ReadCodePasses(BitReader bitReader) {
			int n = 0;
			if (bitReader.Read(1) == 0)
				return 1;
			else if (bitReader.Read(1) == 0)
				return 2;
			else if ((n = bitReader.Read(2)) != 3)
				return 3 + n;
			else if ((n = bitReader.Read(5)) != 31)
				return 6 + n;
			return 37 + bitReader.Read(7);
		}

		private static int Log2(int value) {
			int i = 0;
			for (i = -1; value != 0; ++i)
				value >>= 1;
			return i == -1 ? 0 : i;
		}
	}
}
