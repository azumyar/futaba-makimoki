using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
//using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

namespace Yarukizero.Net.MakiMoki.Uno {
	public sealed partial class App : Application {
		public App() {
			this.InitializeComponent();
		}

		protected override UIElement CreateShell() {
			return Container.Resolve<Views.Shell>();
		}

		protected override void RegisterTypes(IContainerRegistry containerRegistry) {
			containerRegistry.RegisterForNavigation<Views.Shell>();
		}

		protected override void OnInitialized() {
			base.OnInitialized();

			this.DoInitilize();
		}
		partial void DoInitilize();

		protected override void OnLaunched(LaunchActivatedEventArgs args) {
			base.OnLaunched(args);
			this.DoLunch();
		}

		partial void DoLunch();

		protected override void OnSuspending(SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}