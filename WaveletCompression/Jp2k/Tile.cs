using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveletCompression;

namespace Jp2k {
	public class Tile {

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

		public Tile(int components, int level, Stream stream, ResolutionLevel[,] resolutionLevels) {
			var position = stream.Position;
			while (Marker.Peek(stream) != MarkerType.SOD) {
				var marker = Marker.Read(stream);
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

			for (int l = 0; l <= level; ++l) {
				for (int c = 0; c < components; ++c) {
					ReadTileBlock(bitReader, resolutionLevels[c,l]);
				}
			}
			
		}

		private static void ReadTileBlock(BitReader bitReader, ResolutionLevel resolutionLevel) {
			if (!bitReader.ReadBit() || bitReader.EOF)
				return;
			int resolutionSize = 0;
			foreach (var band in resolutionLevel.Bands) {
				int bandSize = 0;
				for (int cb = 0; cb < band.CodeBlocks.Length; ++cb) {
					int included = band.InclustionTT.Decode(cb, 1, bitReader);
					if (included > 0) {
						int i = 1;
						for (; ; ) {
							int ret = band.MsbTT.Decode(cb, i, bitReader);
							if (ret > 0) {
								break;
							}
							++i;
						}

						int numBits = 3;
						var codePasses = ReadCodePasses(bitReader);
						int lBlock = 0;
						while (bitReader.ReadBit())
							++lBlock;
						numBits += lBlock;
						var l = numBits + MathUtils.Log2(codePasses);
						var length = bitReader.Read(l);
						bandSize += length;
						Console.WriteLine($"Code block {cb} size {length}");
					}
				}
				resolutionSize += bandSize;
				Console.WriteLine($"Band {band.Orientir} size {bandSize}");
			}
			new TileBlock(resolutionSize, bitReader.CopyByte(resolutionSize));
			Console.WriteLine($"Resolution size {resolutionSize}");
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

	}
}
