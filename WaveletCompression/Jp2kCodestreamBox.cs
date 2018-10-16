using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace WaveletCompression {
	internal class Jp2kCodestreamBox : Jp2kBox {

		private List<Jp2kTile> _tiles = new List<Jp2kTile>();

		public Jp2kCodestreamBox(Stream stream, Jp2kBoxNavigation navigation): base(navigation) {
			stream.Seek(navigation.PositionData, SeekOrigin.Begin);
			Debug.Assert(MarkerType.SOC == Jp2kMarker.Peek(stream));
			var markerDic = new Dictionary<MarkerType, Jp2kMarker>();
			var markerTlm = new List<Jp2kMarker>();
			while (Jp2kMarker.Peek(stream) != MarkerType.SOT) {
				var marker = Jp2kMarker.Read(stream);
				if (marker.Type == MarkerType.TLM)
					markerTlm.Add(marker);
				else if (!markerDic.ContainsKey(marker.Type))
					markerDic.Add(marker.Type, marker);
			}
			var siz = markerDic[MarkerType.SIZ] as SizMarker;
			while(Jp2kMarker.Peek(stream) != MarkerType.EOC) {
				_tiles.Add(new Jp2kTile(stream));
			}

		}
	}
}