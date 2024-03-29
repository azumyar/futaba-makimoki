using Prism.Events;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.ViewModels {
	public class FutabaCanvas98ViewViewModel : BindableBase, IDisposable, INavigationAware, IJournalAware, IDestructible {
		public class Messenger : EventAggregator {
			public static Messenger Instance { get; } = new Messenger();
		}

		public class NavigateTo {
			public Data.UrlContext Url { get; }

			public NavigateTo(Data.UrlContext url) {
				this.Url = url;
			}
		}

		public class CloseTo {
			public Data.UrlContext Url { get; }

			public CloseTo() {
				this.Url = null;
			}

			public CloseTo(Data.UrlContext url) {
				this.Url = url;
			}
		}

		public class PostFrom {
			public Data.UrlContext Url { get; }
			public Canvas98Data.StoredForm Form { get; }

			public PostFrom(Data.UrlContext url, Canvas98Data.StoredForm form) {
				this.Url = url;
				this.Form = form;
			}
		}
		private IRegionNavigationService RegionNavigationService { get; set; }
		private IDisposable Subscribe { get; }
		
		public Data.UrlContext Url { get; private set; }

		public FutabaCanvas98ViewViewModel() {
			Subscribe = Messenger.Instance.GetEvent<PubSubEvent<CloseTo>>()
				.Subscribe(x => {
					if(x.Url != null && x.Url != this.Url) {
						return;
					}

					if(RegionNavigationService?.Region?.ActiveViews?.Any() ?? false) {
						RegionNavigationService?.Region.RemoveAll();
					}
				});
		}

		public void Close() {
			Messenger.Instance.GetEvent<PubSubEvent<CloseTo>>()
				.Publish(new CloseTo(this.Url));
		}

		public void Dispose() {
			Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}

		public void OnNavigatedTo(NavigationContext navigationContext) {
			RegionNavigationService = navigationContext.NavigationService;
			if(navigationContext.Parameters.TryGetValue<Data.UrlContext>(
				typeof(Data.UrlContext).FullName,
				out var url)) {

				Url = url;
				Messenger.Instance.GetEvent<PubSubEvent<NavigateTo>>()
					.Publish(new NavigateTo(url));
			}
		}

		public bool IsNavigationTarget(NavigationContext navigationContext) {
			return true;
		}

		public void OnNavigatedFrom(NavigationContext navigationContext) { }


		public bool PersistInHistory() {
			return false;
		}

		public void Destroy() {
			this.Dispose();
		}
	}
}
