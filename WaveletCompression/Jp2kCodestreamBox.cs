using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace WaveletCompression {
	internal class Jp2kCodestreamBox : Jp2kBox {

		private Dictionary<MarkerType, Jp2kMarker> _markerDic = new Dictionary<MarkerType, Jp2kMarker>();
		private ResolutionLevel[,] _resolutionLevels;
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
			if (!_markerDic.ContainsKey(MarkerType.SIZ))
				throw new MarkerIsNotException(MarkerType.SIZ);
			if (!_markerDic.ContainsKey(MarkerType.COD))
				throw new MarkerIsNotException(MarkerType.COD);
			ComputeResolution();
			/*
			while (Jp2kMarker.Peek(stream) != MarkerType.EOC) {
				_tiles.Add(new Jp2kTile(stream));
			}
			*/

		}

		private void ComputeResolution() {
			var sizMarker = _markerDic[MarkerType.SIZ] as SizMarker;
			var codMarker = _markerDic[MarkerType.COD] as CodMarker;
			var components = sizMarker.Components;
			var level = codMarker.DecompositionLevels;
			//The Code Block size has value 2 ^ CdBlkExpn
			var cdBlckSize = new Size(1 << codMarker.CdBlkExpnX, 1 << codMarker.CdBlkExpnY);
			_resolutionLevels = new ResolutionLevel[components, level + 1];
			for (int c = 0; c < components; ++c) {
				var size = sizMarker.Size;
				for (int lvl = level; lvl > 0; --lvl) {
					_resolutionLevels[c, lvl] = new ResolutionLevel(lvl, sizMarker.Offset, size, cdBlckSize);
					size = new Size(MathUtils.CeilDiv(size.Width, 2), MathUtils.CeilDiv(size.Height, 2));
				}
				_resolutionLevels[c, 0] = new ResolutionLevel(0, sizMarker.Offset, size, cdBlckSize);
			}
			for(int c = 0; c < components; ++c) {
				for (int l = 0; l <= level; ++l )
				Console.WriteLine(_resolutionLevels[c, l]);
			}
		}
	}
}