using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace WaveletCompression {
	internal class Jp2kCodestreamBox : Jp2kBox {

		private Dictionary<MarkerType, Jp2kMarker> _markerDic = new Dictionary<MarkerType, Jp2kMarker>();
		private List<Jp2kMarker> _markerTlm = new List<Jp2kMarker>();
		private List<Jp2kTile> _tiles = new List<Jp2kTile>();

		public Jp2kCodestreamBox(Stream stream, Jp2kBoxNavigation navigation): base(navigation) {
			stream.Seek(navigation.PositionData, SeekOrigin.Begin);
			Debug.Assert(MarkerType.SOC == Jp2kMarker.Peek(stream));
			while (Jp2kMarker.Peek(stream) != MarkerType.SOT) {
				var marker = Jp2kMarker.Read(stream);
				if (marker.Type == MarkerType.TLM)
					_markerTlm.Add(marker);
				else if (!_markerDic.ContainsKey(marker.Type))
					_markerDic.Add(marker.Type, marker);
			}
			while(Jp2kMarker.Peek(stream) != MarkerType.EOC) {
				_tiles.Add(new Jp2kTile(stream));
			}

		}

		public Size Size {
			get {
				return (_markerDic[MarkerType.SIZ] as SizMarker).Size;
			}
		}

	}
}