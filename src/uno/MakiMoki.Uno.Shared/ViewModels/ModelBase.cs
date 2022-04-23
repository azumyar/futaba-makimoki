using System;
using System.Collections.Generic;
using System.Text;

namespace MakiMoki.Uno.ViewModels {
	internal class ModelBase : Prism.Mvvm.BindableBase, IDisposable {
		public virtual void Dispose() {
		}
	}
}