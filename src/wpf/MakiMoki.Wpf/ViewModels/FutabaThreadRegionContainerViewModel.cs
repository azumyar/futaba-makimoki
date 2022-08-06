using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Yarukizero.Net.MakiMoki.Reactive;
using Yarukizero.Net.MakiMoki.Wpf.Model;

namespace Yarukizero.Net.MakiMoki.Wpf.ViewModels {
	class FutabaThreadRegionContainerViewModel : BindableBase, IDisposable, IDestructible {
		public ReactiveProperty<string> RegionName { get; } = new ReactiveProperty<string>(
			$"ThreadViewerRegion-{ Guid.NewGuid() }");

		public MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>> ContentsChangedCommand { get; }
			= new MakiMokiCommand<RoutedPropertyChangedEventArgs<Model.IFutabaViewerContents>>();

		public ReadOnlyReactiveProperty<IRegionManager> RegionManager { get; }
		private IContainerProvider ContainerProvider { get; }

		public FutabaThreadRegionContainerViewModel(IContainerProvider containerProvider, IRegionManager regionManager) {
			ContainerProvider = containerProvider;
			RegionManager = new ReactiveProperty<IRegionManager>(regionManager.CreateRegionManager())
				.ToReadOnlyReactiveProperty();

			ContentsChangedCommand.Subscribe(x => OnContentsChanged(x));
			System.Diagnostics.Debug.WriteLine(RegionName.Value);
		}

		private void OnContentsChanged(RoutedPropertyChangedEventArgs<IFutabaViewerContents> e) {
			if(e.NewValue == null) {
				System.Diagnostics.Debug.WriteLine("!!!!!!!!---OnContentsChanged(null)---!!!!!!!!!!!");
			} else {
				Observable.Create<IRegion>(async o => {
					try {
						o.OnNext(this.RegionManager.Value.Regions[this.RegionName.Value]);
					}
					catch(UpdateRegionsException ex) {
						// アクセス例外が出ることがあるので再送する
						await Task.Delay(1);
						o.OnError(ex);
					}
					return System.Reactive.Disposables.Disposable.Empty;
				})
					.Retry(5)
					.Subscribe(r => {
						var tab = e.NewValue;
						var v = r.GetView(tab.Futaba.Value.Url.ToUrlString());
						if(v != null) {
							r.Activate(v);
							return;
						}

						if(tab.ThreadView.Value == null) {
							var fv = this.ContainerProvider.Resolve<Controls.FutabaThreadResViewer>();
							fv.Contents = tab;
							tab.Region.Value = r;
							tab.ThreadView.Value = fv;
						}

						r.Add(tab.ThreadView.Value, tab.Futaba.Value.Url.ToUrlString());
						r.Activate(tab.ThreadView.Value);
					});
			}
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public void Destroy() {
			foreach(var r in this.RegionManager.Value.Regions.ToArray()) {
				r.RemoveAll();
				this.RegionManager.Value.Regions.Remove(r.Name);
			}
			this.Dispose();
		}
	}
}
