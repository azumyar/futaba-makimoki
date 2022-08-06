using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;

namespace Yarukizero.Net.MakiMoki.Bindable {
	public class CommonBindableFutaba : INotifyPropertyChanged, IDisposable {
#pragma warning disable CS0067
		protected PropertyChangedEventHandler _propertyChanged;
		public event PropertyChangedEventHandler PropertyChanged {
			add { this._propertyChanged += value; }
			remove { this._propertyChanged -= value; }
		}
#pragma warning restore CS0067

		public virtual void Dispose() {
			global::Yarukizero.Net.MakiMoki.Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}
	}

	public class CommonBindableFutabaResItem : INotifyPropertyChanged, IDisposable {
#pragma warning disable CS0067
		protected PropertyChangedEventHandler _propertyChanged;
		public event PropertyChangedEventHandler PropertyChanged {
			add { this._propertyChanged += value; }
			remove { this._propertyChanged -= value; }
		}
#pragma warning restore CS0067

		public virtual void Dispose() {
			global::Yarukizero.Net.MakiMoki.Helpers.AutoDisposable.GetCompositeDisposable(this).Dispose();
		}
	}
}