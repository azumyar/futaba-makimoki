using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Reactive.Bindings;

namespace Yarukizero.Net.MakiMoki.Wpf.Model {
	class TreeItem: INotifyPropertyChanged, IDisposable {
		public event PropertyChangedEventHandler PropertyChanged;
		private CompositeDisposable Disposable { get; } = new CompositeDisposable();

		public string Name { get; private set; }
		public Data.UrlContext Url { get; }

		public ReactiveProperty<ImageSource> ThumbSource { get; }
		public ReactiveProperty<Visibility> ThumbVisibility { get; }
		public ReactiveProperty<BindableFutaba> Futaba { get; }

		public ReactiveProperty<bool> IsSelected { get; }
			= new ReactiveProperty<bool>(false);
		public ReactiveProperty<bool> IsExpanded { get; }
			= new ReactiveProperty<bool>();
	
		public ReactiveProperty<TreeItem[]> ChildItems { get; }
			= new ReactiveProperty<TreeItem[]>(new TreeItem[0]);

		public TreeItem(Data.BordConfig bord) {
			this.Name = bord.Name;
			this.Url = new Data.UrlContext(bord.Url);

			this.Futaba = new ReactiveProperty<BindableFutaba>();
			//this.Futaba.Subscribe(x => this.Name.Value = x.Name);
			this.ThumbSource = new ReactiveProperty<ImageSource>();
			this.ThumbVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
		}

		public TreeItem(Data.FutabaContext thread) {
			this.Name = thread.Name;
			this.Url = thread.Url;
			this.Futaba = new ReactiveProperty<BindableFutaba>(new BindableFutaba(thread));
			this.Futaba.Subscribe(x => this.Name = x.Name);
			this.ThumbSource = WpfUtil.ImageUtil.ToThumbProperty(this.Futaba);
			this.ThumbVisibility = this.ThumbSource
				.Select(x => (x != null) ? Visibility.Visible : Visibility.Collapsed)
				.ToReactiveProperty();
		}

		public void Dispose() {
			Disposable.Dispose();
		}
	}
}
