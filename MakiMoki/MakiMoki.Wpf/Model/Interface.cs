using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {

	public interface IFutabaViewerContents {
		ReactiveProperty<BindableFutaba> Futaba { get; }

		ReactiveProperty<PlatformData.FutabaMedia> MediaContents { get; }

		ReactiveProperty<object> LastVisibleItem { get; }
		ReactiveProperty<double> ScrollVerticalOffset { get; }
		ReactiveProperty<double> ScrollHorizontalOffset { get; }

		ReactiveProperty<Visibility> SearchBoxVisibility { get; }
		ReactiveProperty<Visibility> SearchButtonVisibility { get; }
		ReactiveProperty<GridLength> SearchColumnWidth { get; }

		void ShowSearchBox();
		void HideSearchBox();
	}
}
