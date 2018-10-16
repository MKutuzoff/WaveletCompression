using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class Jp2kTile {

		SotMarker _sot;
		List<PltMarker> _pltMarkers = new List<PltMarker>();

		public Jp2kTile(Stream stream) {
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
		}
	}
}
