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
using Uno.Extensions.Navigation.Regions;

namespace Yarukizero.Net.MakiMoki.Uno {
	public sealed partial class App : Application {
		private readonly IHost _host;

		public App() {
			_host = UnoHost
				.CreateDefaultBuilder()
				.ConfigureServices(s => {
					s.AddSingleton<UnoModels.ImageResolver>(new UnoModels.ImageResolver());
					//s.AddTransient<ViewModels.ShellViewModel>();
				})
				.UseNavigation(
					viewRouteBuilder: (views, routes) => {
						views.Register(new ViewMap<Views.ShellView, ViewModels.ShellViewModel>());
						/*
						{
							var vt = typeof(Views.ShellView);
							var vmt = typeof(ViewModels.ShellViewModel);
							var types = vt.Assembly.GetTypes();
							var vTypes = types.Where(x => x.Namespace == vt.Namespace).ToArray();
							var vmTypes = types.Where(x => x.Namespace == vmt.Namespace).ToArray();

							views.Register(vmTypes.Select<Type, (Type V, Type VM)?>(x => vTypes.FirstOrDefault(y => $"{y.Name}ViewModel" == x.Name) switch {
								Type t => (t, x),
								_ => null
							}).Where(x => x is not null)
								.Select(x =>
									(ViewMap)typeof(ViewMap<,>).MakeGenericType(x.Value.V, x.Value.VM)
										.GetConstructors()
										.First()
										.Invoke(new object[] { null, null, null }))
								.ToArray());
						}
						*/
						views.Register(
							//new ViewMap<Views.ShellView, ViewModels.ShellViewModel>(),
							new ViewMap<Views.MobilePage, ViewModels.MobilePageViewModel>(),
							new ViewMap<Views.CatalogPage, ViewModels.CatalogPageViewModel>(Data: new DataMap<Data.BoardData>()),
							new ViewMap<Views.ThreadPage, ViewModels.ThreadPageViewModel>(Data: new DataMap<Data.UrlContext>())
							);
						
						routes.Register(
							new RouteMap("", View: views.FindByViewModel<ViewModels.ShellViewModel>(), Nested: new[] {
								new RouteMap("Mobile", View: views.FindByViewModel<ViewModels.MobilePageViewModel>()),
								new RouteMap("Catalog", View: views.FindByViewModel<ViewModels.CatalogPageViewModel>()),
								new RouteMap("Thread", View: views.FindByViewModel<ViewModels.ThreadPageViewModel>()),
								/*
								new RouteMap("Mobile", View: views.FindByViewModel<ViewModels.MobilePageViewModel>(), Nested: new [] {
									new RouteMap(
										"Catalog",
										View: views.FindByViewModel<ViewModels.CatalogPageViewModel>())
								})
								*/
							}));
					})
				//.UseToolkitNavigation()
				.Build();
			this.InitializeComponent();

#if HAS_UNO || NETFX_CORE
			this.Suspending += OnSuspending;
#endif
		}

		private IRegion _region = null;

		protected override async void OnLaunched(LaunchActivatedEventArgs args) {
			this.DoInitilize();
			_host.Services.GetRequiredService<IRouteNotifier>().RouteChanged += (_, e) => {
				_region = e?.Region;
			};
			Windows.UI.Core.SystemNavigationManager.GetForCurrentView().BackRequested += async (_, e) => {
				// できない
				/*
				if(_host.Services.GetService<INavigator>() is INavigator nav) {
					var r = await nav.GoBack(global::Windows.UI.Xaml.Window.Current.Content ?? new object());
					if(r?.Success ?? false) {
						e.Handled = true;
					}
				}
				*/
			};
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