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

		private SotMarker _sot;
		private List<PltMarker> _pltMarkers = new List<PltMarker>();

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
			var bitReader = new BitReader(stream);
			for (int l = 0; l <= level; ++l) {
				for (int c = 0; c < components; ++c) {
					resolutionLevels[c, l].Read(bitReader);
					Console.WriteLine(resolutionLevels[c, l]);
				}
			}
		}
	}
}
