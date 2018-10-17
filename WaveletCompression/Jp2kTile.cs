using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class Jp2kTile {

		private SotMarker _sot;
		private List<PltMarker> _pltMarkers = new List<PltMarker>();
		private byte[] _data;

		public byte[] Data => _data;

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
			_data = new byte[position + _sot.PartLength - stream.Position];
			stream.Read(_data, 0, _data.Length);
		}
	}
}
