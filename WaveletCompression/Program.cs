using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using Jp2k;

namespace WaveletCompression {


	class Program {

		protected const int MIN_BOX_LENGTH = 8;

		protected const int USE_EXTENDED_LENGTH = 1;
		static void Main(string[] args) {

			using (var stream = File.OpenRead("img.jp2")) {
				var boxes = ReadJp2kBoxes(stream, stream.Length);
				var hBox = boxes.FirstOrDefault(b => b.Type == BoxTypes.JP2HeaderBox);
				var hhBox = hBox.Child.FirstOrDefault(b => b.Type == BoxTypes.ImageHeaderBox);
				var head = Box.CreateBox(stream, hhBox) as ImageHeaderBox;
				var ccBox = boxes.FirstOrDefault(b => b.Type == BoxTypes.CodestreamBox);
				var codestream = Box.CreateBox(stream, ccBox) as CodestreamBox;
			}
		}

		private static IEnumerable<BoxNavigation> ReadJp2kBoxes(Stream stream, long length) {
			List<BoxNavigation> boxes = new List<BoxNavigation>();
			while (length > 0) {
				var box = ReadJp2kBox(stream, length);
				length -= box.Length;
				boxes.Add(box);
			}
			return boxes;
		}

		private static BoxNavigation ReadJp2kBox(Stream stream, long limit) {
			long position = stream.Position;
			long length = stream.ReadUInt32();
			if (length == 0)
				length = limit;
			if (length > USE_EXTENDED_LENGTH && length < MIN_BOX_LENGTH)
				throw new Exception();
			var type = stream.ReadUInt32();
			ulong eSize = 0;
			if (length == USE_EXTENDED_LENGTH) {
				eSize = stream.ReadUInt64();
			}
			if (BoxNavigation.IsSuperbox(type)) {
				var child = ReadJp2kBoxes(stream, length - (stream.Position - position));
				return new BoxNavigation(position, length, type, eSize, child);
			} else {
				stream.Seek(position + length, SeekOrigin.Begin);
				return new BoxNavigation(position, length, type, eSize);
			}
		}
	}
}