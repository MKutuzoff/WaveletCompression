using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using WaveletCompression;

namespace Jp2k {
	internal class CodestreamBox : Box {

		private Dictionary<MarkerType, Marker> _markerDic = new Dictionary<MarkerType, Marker>();
		private ResolutionLevel[,] _resolutionLevels;
		private List<Marker> _markerTlm = new List<Marker>();
		private List<Tile> _tiles = new List<Tile>();

		private SizMarker SizMarker {
			get {
				if (!_markerDic.ContainsKey(MarkerType.SIZ))
					throw new MarkerIsNotException(MarkerType.SIZ);
				return _markerDic[MarkerType.SIZ] as SizMarker;
			}
		}

		private CodMarker CodMarker {
			get {
				if (!_markerDic.ContainsKey(MarkerType.COD))
					throw new MarkerIsNotException(MarkerType.COD);
				return _markerDic[MarkerType.COD] as CodMarker;
			}
		}

		public CodestreamBox(Stream stream, BoxNavigation navigation): base(navigation) {
			stream.Seek(navigation.PositionData, SeekOrigin.Begin);
			Debug.Assert(MarkerType.SOC == Marker.Peek(stream));
			while (Marker.Peek(stream) != MarkerType.SOT) {
				var marker = Marker.Read(stream);
				if (marker.Type == MarkerType.TLM)
					_markerTlm.Add(marker);
				else if (!_markerDic.ContainsKey(marker.Type))
					_markerDic.Add(marker.Type, marker);
			}
			ComputeResolution(SizMarker, CodMarker);
			
			while (Marker.Peek(stream) != MarkerType.EOC) {
				_tiles.Add(new Tile(SizMarker.Components, CodMarker.DecompositionLevels, stream, _resolutionLevels));
			}

		}

		private void ComputeResolution(SizMarker sizMarker, CodMarker codMarker) {
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
		}
	}
}