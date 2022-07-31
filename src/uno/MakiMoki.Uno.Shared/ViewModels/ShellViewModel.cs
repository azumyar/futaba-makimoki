using System;
using System.Collections.Generic;
using System.Text;
using Uno.Extensions.Navigation;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	internal class ShellViewModel {
		public ShellViewModel(INavigator navigator) {
			System.Diagnostics.Debug.WriteLine("init shell vm");
			navigator.NavigateViewAsync<Views.MainPage>(this);
		}
	}
}
