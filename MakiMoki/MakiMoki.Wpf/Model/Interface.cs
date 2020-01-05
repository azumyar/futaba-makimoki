using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {

	public interface IFutabaViewerContents {
		ReactiveProperty<BindableFutaba> Futaba { get; }

		ReactiveProperty<object> LastVisibleItem { get; }
		ReactiveProperty<double> ScrollVerticalOffset { get; }
		ReactiveProperty<double> ScrollHorizontalOffset { get; }
	}
}
