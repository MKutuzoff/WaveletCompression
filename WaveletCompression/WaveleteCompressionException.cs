using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WaveletCompression {
	public class MarkerIsNotException: Exception {

		private readonly MarkerType _marker;

		public MarkerIsNotException(MarkerType marker, string message, Exception innerException) : 
			base(message, innerException) {
			_marker = marker;
		}

		public MarkerIsNotException(MarkerType marker, string message) : 
			this(marker, message, null) {
		}

		public MarkerIsNotException(MarkerType marker): 
			this(marker, string.Format("The {0} marker is not", marker.ToString())) {
		}
		
	}
}
