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
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Uno.Extensions.Hosting;
using Uno.Extensions.Navigation;

namespace Yarukizero.Net.MakiMoki.Uno {
	public sealed partial class App : Application {
		private readonly IHost _host;

		public App() {
			_host = UnoHost
				.CreateDefaultBuilder()
				.ConfigureServices(s => {
					//s.AddTransient<ViewModels.ShellViewModel>();
				})
				.UseNavigation(
					viewRouteBuilder: (views, routes) => {
						views.Register(
							new ViewMap<Views.ShellView, ViewModels.ShellViewModel>(),
							new ViewMap<Views.MainPage, ViewModels.MainPageViewModel>()
							);

						routes.Register(
							new RouteMap("", View: views.FindByViewModel<ViewModels.ShellViewModel>(),
								Nested: new[] {
									new RouteMap("Main", View: views.FindByViewModel<ViewModels.MainPageViewModel>())
								}));
					},
					configure: cfg => cfg with { AddressBarUpdateEnabled = true })
				//.UseToolkitNavigation()
				.Build();
			this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
			this.Suspending += OnSuspending;
#endif
		}

		protected override async void OnLaunched(LaunchActivatedEventArgs args) {
			this.DoInitilize();

#if NET5_0_OR_GREATER && WINDOWS
			var window = new Window();
			window.Activate();
#else
			var window = Window.Current;
#endif
			window.AttachNavigation(this._host.Services);
			window.Activate();
			await System.Threading.Tasks.Task.Run(() => this._host.StartAsync());

			this.DoLunch();
		}
		partial void DoInitilize();

		partial void DoLunch();

		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			// TODO: Save application state and stop any background activity
			deferral.Complete();
		}
	}
}