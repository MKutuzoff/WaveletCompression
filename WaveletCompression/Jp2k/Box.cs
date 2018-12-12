using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jp2k {

	public enum BoxTypes : uint {

		Jp2SignatureBox = 0x6A502020,
		FileTypeBox = 0x66747970,
		JP2HeaderBox = 0x6A703268,
		ImageHeaderBox = 0x69686472,
		BitsPerCompBox = 0x62706363,
		ColorSpecBox = 0x636F6C72,
		PalleteBox = 0x70636C72,
		CompMapBox = 0x636D6170,
		ChannelDefBox = 0x63646566,
		ResolutionBox = 0x72657320,
		CaptureResBox = 0x72657363,
		DisplayResBox = 0x72657364,
		CodestreamBox = 0x6A703263,
		IntellectPropRightsBox = 0x6A703269,
		XmlBox = 0x786D6C20,
		UuidBox = 0x75756964,
		UuidInfoBox = 0x75696E66,
		UuidListBox = 0x75637374,
		UrlBox = 0x75726C20,
		UnDefined = 0x0
	}


	public class BoxNavigation {

		public static bool IsSuperbox(uint boxType) {
			switch (boxType) {
				case (uint)BoxTypes.JP2HeaderBox:
				case (uint)BoxTypes.ResolutionBox:
				case (uint)BoxTypes.UuidInfoBox:
					return true;
				default:
					return false;
			}
		}

		private readonly long _position;
		private readonly long _length;
		private readonly BoxTypes _type;
		private readonly ulong _eSize;
		private readonly IEnumerable<BoxNavigation> _child;

		public long Position => _position;

		public long PositionData {
			get {
				return _position + 8 + (_eSize > 0 ? 8 : 0);
			}
		}
		public long Length => _length;

		public BoxTypes Type => _type;

		public IEnumerable<BoxNavigation> Child => _child;

		public BoxNavigation(long position, long length, uint type, ulong eSize, IEnumerable<BoxNavigation> child = null) {
			_position = position;
			_length = length;
			_type = (BoxTypes)type;
			_eSize = eSize;
			_child = child;
		}
		
	}

	public class Box {

		private readonly BoxNavigation _navigation;

		protected Box(BoxNavigation navigation) {
			_navigation = navigation;
		}


		public static Box CreateBox(Stream stream, BoxNavigation navigation) {
			switch(navigation.Type) {
				case BoxTypes.ImageHeaderBox:
					return new ImageHeaderBox(stream, navigation);
				case BoxTypes.CodestreamBox:
					return new CodestreamBox(stream, navigation);
				default:
					return null;
			}
		}
	}
}
