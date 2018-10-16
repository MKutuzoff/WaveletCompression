using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Linq;


namespace WaveletCompression {


	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct Jp2kHead {
		[FieldOffset(0)] public readonly uint magic;
		[FieldOffset(4)] public readonly uint majver;
		[FieldOffset(8)] public readonly uint minver;
		[FieldOffset(12)] public readonly uint numcompatcodes;
	}

	class Program {

		protected const int MIN_BOX_LENGTH = 8;

		protected const int USE_EXTENDED_LENGTH = 1;
		static void Main(string[] args) {
			Console.WriteLine(BitConverter.IsLittleEndian);
			
			using (var stream = File.OpenRead("img.jp2")) {
				var boxes = ReadJp2kBoxes(stream, stream.Length);
				var hBox = boxes.FirstOrDefault(b => b.Type == BoxTypes.JP2HeaderBox);
				var hhBox = hBox.Child.FirstOrDefault(b => b.Type == BoxTypes.ImageHeaderBox);
				var head = Jp2kBox.CreateBox(stream, hhBox) as ImageHeaderBox;
				Console.WriteLine($"SIZE \t{head.Width} X {head.Height} px");
				var ccBox = boxes.FirstOrDefault(b => b.Type == BoxTypes.CodestreamBox);
				var codestream = Jp2kBox.CreateBox(stream, ccBox) as CodestreamBox;
			}
			Console.ReadKey(false);
		}

		private static IEnumerable<Jp2kBoxNavigation> ReadJp2kBoxes(Stream stream, long length) {
			List<Jp2kBoxNavigation> boxes = new List<Jp2kBoxNavigation>();
			while (length > 0) {
				var box = ReadJp2kBox(stream, length);
				length -= box.Length;
				boxes.Add(box);
			}
			return boxes;
		}

		private static Jp2kBoxNavigation ReadJp2kBox(Stream stream, long limit) {
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
			if (Jp2kBoxNavigation.IsSuperbox(type)) {
				var child = ReadJp2kBoxes(stream, length - (stream.Position - position));
				return new Jp2kBoxNavigation(position, length, type, eSize, child);
			} else {
				stream.Seek(position + length, SeekOrigin.Begin);
				return new Jp2kBoxNavigation(position, length, type, eSize);
			}
		}
	}
}