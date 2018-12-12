using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WaveletCompression;

namespace Jp2k {

	public class TagTree {

		private const int CAPACITY = 8;

		private struct TagTreeNode {
			public int parentIndex;
			public int value;
			public int low;
			public int known;

			public bool IsRoot => parentIndex < 0;
		}

		private readonly int _horizontalRes;
		private readonly int _verticalRes;
		private TagTreeNode[] _tree;

		public TagTree(int horizaontalRes, int verticalRes) {
			_horizontalRes = horizaontalRes;
			_verticalRes = verticalRes;

			List<int> nplmh = new List<int>(CAPACITY);
			List<int> nplmv = new List<int>(CAPACITY);
			nplmh.Add(horizaontalRes);
			nplmv.Add(verticalRes);
			int level = 0;
			int size = 0;
			int n = 0;
			do {
				n = nplmh[level] * nplmv[level];
				nplmh.Add(MathUtils.CeilDivPow2(nplmh[level], 1));
				nplmv.Add(MathUtils.CeilDivPow2(nplmv[level], 1));
				size += n;
				++level;
			} while (n > 1);

			_tree = CreateTree(size, level, nplmh, nplmv);
		}

		// Create tree as line array
		// This code copy from JasPer Tag Tree Library
		private TagTreeNode[] CreateTree(int size, int level, List<int> nplmh, List<int> nplmv) {
			TagTreeNode[] tree = new TagTreeNode[size];
			int node = 0;
			int parent = nplmh[0] * nplmv[0];
			int parent0 = parent;
			for (int i = 0; i < level - 1; ++i) {
				for (int j = 0; j < nplmv[i]; ++j) {
					int k = nplmh[i];
					while (--k >= 0) {
						tree[node].parentIndex = parent;
						++node;
						if (--k >= 0) {
							tree[node].parentIndex = parent;
							++node;
						}
						++parent;
					}
					if ((j & 1) > 0 || (j == (nplmv[i] - 1))) {
						parent0 = parent;
					} else {
						parent = parent0;
						parent0 += nplmh[i];
					}
				}
			}
			tree[node].parentIndex = -1;
			ResetNodes(tree);
			return tree;
		}

		private void ResetNodes(TagTreeNode[] nodes) {
			for(int i = 0; i < nodes.Length; ++i) {
				nodes[i].value = int.MaxValue;
				nodes[i].low = 0;
				nodes[i].known = 0;
			}
		}

		// This code copy from JasPer Tag Tree Library
		public int Decode(int node, int threshold, BitReader bitReader) {
			// Parent nodes stack
			var stk = new Stack<int>(CAPACITY);

			while(!_tree[node].IsRoot) {
				stk.Push(node);
				node = _tree[node].parentIndex;
			}
			int low = 0;
			for(; ; ) {
				if (low > _tree[node].low) {
					_tree[node].low = low;
				} else {
					low = _tree[node].low;
				}
				while(low < threshold && low < _tree[node].value) {
					if (bitReader.EOF)
						return -1;
					if (bitReader.ReadBit()) {
						_tree[node].value = low;
					} else {
						++low;
					}
				}
				_tree[node].low = low;
				if (stk.Count > 0) {
					node = stk.Pop();
				} else {
					break;
				}
			}
			return _tree[node].value < threshold ? 1 : 0;
		}
	}
}
