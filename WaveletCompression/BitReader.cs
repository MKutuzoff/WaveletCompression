using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class BitReader {

		byte[] _data = null;
		byte _curByte = 0;
		byte _offset = 0;
		int _position = 0;

		public BitReader(byte[] data) {
			_data = data;
		}

		public bool EOF {
			get {
				return _position >= _data.Length;
			}
		}

		private bool ReadBit() {
			if (_data != null && _position < _data.Length) {
				if (_offset == 0) {
					_curByte = _data[_position++];
					_offset = 8;
				}
				--_offset;
				return ((_curByte >> _offset) & 1) == 1;
			}
			return false;
		}

		public int Read(int length) {
			int result = 0;
			for (var i = length - 1; i >= 0; --i) {
				if (ReadBit()) result |= 0x01 << i;
			}
			return result;
		}

		public byte[] CopyByte(int length) {
			if (_position + length < _data.Length) {
				byte[] data = new byte[length];
				Array.Copy(_data, _position, data, 0, length);
				_offset = 0;
				_position += length;
				return data;
			} else {
				return null;
			}
		}
	}
}
