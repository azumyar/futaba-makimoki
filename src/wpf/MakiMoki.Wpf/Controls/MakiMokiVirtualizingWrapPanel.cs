using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	internal class MakiMokiVirtualizingWrapPanel : WpfToolkit.Controls.VirtualizingWrapPanel {
		// 現状何もしていないがデバッグのため残しておく
#if false
		protected override void VirtualizeItems() {
			for(int childIndex = InternalChildren.Count - 1; childIndex >= 0; childIndex--) {
				var generatorPosition = GetGeneratorPositionFromChildIndex(childIndex);

				int itemIndex = ItemContainerGenerator.IndexFromGeneratorPosition(generatorPosition);

				if(itemIndex != -1 && !ItemRange.Contains(itemIndex)) {
					if(VirtualizationMode == VirtualizationMode.Recycling) {
						ItemContainerGenerator.Recycle(generatorPosition, 1);
					} else {
						ItemContainerGenerator.Remove(generatorPosition, 1);
					}
					RemoveInternalChildRange(childIndex, 1);
				}
			}
		}
#endif
	}
}