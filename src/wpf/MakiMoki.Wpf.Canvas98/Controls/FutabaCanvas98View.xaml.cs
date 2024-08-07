using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using Prism.Events;
using System.Reactive.Linq;

namespace Yarukizero.Net.MakiMoki.Wpf.Canvas98.Controls {
	/// <summary>
	/// FutabaCanvas98View.xaml の相互作用ロジック
	/// </summary>
	public partial class FutabaCanvas98View : UserControl {
		enum UrlType {
			Other,
			ThreadHtml,
			FutbaPhp,
		}

		public class RoutedSucessEventArgs : RoutedEventArgs {
			public Data.UrlContext Url { get; }
			public Canvas98Data.StoredForm FormData { get; }

			public RoutedSucessEventArgs(Data.UrlContext url, Canvas98Data.StoredForm formData) : base() {
				this.Url = url;
				this.FormData = formData;
			}

			public RoutedSucessEventArgs(Data.UrlContext url, Canvas98Data.StoredForm formData, RoutedEvent routedEvent) : base(routedEvent) {
				this.Url = url;
				this.FormData = formData;
			}

			public RoutedSucessEventArgs(Data.UrlContext url, Canvas98Data.StoredForm formData, RoutedEvent routedEvent, object source) : base(routedEvent, source) {
				this.Url = url;
				this.FormData = formData;
			}
		}
		public delegate void RoutedSucessEventHandler(object sender, RoutedSucessEventArgs e);

		public static readonly DependencyProperty NavigationVisibilityProperty
			= DependencyProperty.Register(
				nameof(NavigationVisibility),
				typeof(Visibility),
				typeof(FutabaCanvas98View));
		public static RoutedEvent SuccessedEvent
			= EventManager.RegisterRoutedEvent(
				nameof(Successed),
				RoutingStrategy.Tunnel,
				typeof(RoutedSucessEventHandler),
				typeof(FutabaCanvas98View));

		public Visibility NavigationVisibility {
			get => (Visibility)this.GetValue(NavigationVisibilityProperty);
			set {
				this.SetValue(NavigationVisibilityProperty, value);
			}
		}

		public event RoutedSucessEventHandler Successed {
			add { AddHandler(SuccessedEvent, value); }
			remove { RemoveHandler(SuccessedEvent, value); }
		}

		private static string WebMessageReady { get; } = "x-makimoki-canvas98-message://ready";
		private static string WebMessageExecuteAlbam { get; } = "x-makimoki-canvas98-message://albam";
		private static string WebMessageExecuteRichPalette { get; } = "x-makimoki-canvas98-message://rich-palette";
		private static string WebMessageExecuteTimelapse { get; } = "x-makimoki-canvas98-message://timelapse";
		private static string WebMessage404 { get; } = "x-makimoki-canvas98-message://404";
		private static string WebMessagePostSucessed { get; } = "x-makimoki-canvas98-message://post-ok";
		private static string WebMessagePostError { get; } = "x-makimoki-canvas98-message://post-error";
		private static string WebMessagePostStore { get; } = "x-makimoki-canvas98-message://post-store";
		private static string WebMessageDebug { get; } = "x-makimoki-canvas98-message://debug";
		
		private static string MakiMokiProtocolViewClose { get; } = "x-makimoki-canvas98://close";
		private static string MakiMokiProtocol98Open { get; } = "x-makimoki-canvas98://open98";


		private static void OnThreadUrlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
			if(obj is UIElement el) {
				/*
				el.RaiseEvent(new RoutedPropertyChangedEventArgs<string>(
					e.OldValue as Model.IFutabaViewerContents,
					e.NewValue as Model.IFutabaViewerContents,
					ContentsChangedEvent));
				*/
				// if((obj is FutabaCanvas98View fv) && (e.NewValue is Data.UrlContext c)) {}
				//el.Visibility = Visibility.Hidden;
			}
		}

		private readonly Task webViewInitializeTask;
		private readonly Dictionary<ulong, (UrlType Type, Data.UrlContext Url)> urlCache
			= new Dictionary<ulong, (UrlType Type, Data.UrlContext Url)>();
		private Canvas98Data.StoredForm formCache;
		private IDisposable NavigateToSubscriber { get; }
		private IDisposable CloseToSubscriber { get; }

		public FutabaCanvas98View() {
			InitializeComponent();
			this.NavigateToSubscriber = ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCanvas98ViewViewModel.NavigateTo>>()
				.Subscribe(async x => {
					static string toJsBool(bool b) => b.ToString().ToLower();

					//var f = this.ThreadUrl != null;
					var threadUrl = x.Url;
					{
						if(this.DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
							if(threadUrl != vm.Url) {
								return;
							}
						}
					}
					await Task.WhenAll(webViewInitializeTask);
					this.webView.CoreWebView2.AddWebResourceRequestedFilter(null, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceContext.All);
					var isChildItem = (!string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptAlbam)
						|| !string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptRichPalette)
						|| !string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptTimelapse));
					await this.webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(
						new StringBuilder()
							.AppendLine("'use strict';")
							.AppendLine("window.addEventListener('load', ()=>{")
							// XMLHttpRequestを無効化する
							.AppendLine("  XMLHttpRequest = class {")
							.AppendLine("    constructor() {}")
							.AppendLine("    onload")
							.AppendLine("    open(method, f) {}")
							.AppendLine("    send(data) {}")
							.AppendLine("  };")
							.AppendLine($"  if(document.location.href.startsWith('{ x.Url.BaseUrl }') && document.location.href.endsWith('.htm')) {{")
							.AppendLine("    if(document.body.bgColor !== '') {")
							// フォーム以外のノード全削除
							.AppendLine("      {")
							.AppendLine("        const rn = [];")
							.AppendLine("        for(const n of document.body.childNodes) {")
							.AppendLine("          if(n.id !== 'fm') {")
							.AppendLine("            rn.push(n);")
							.AppendLine("          }")
							.AppendLine("        }")
							.AppendLine("        for(const n of rn) {")
							.AppendLine("          document.body.removeChild(n);")
							.AppendLine("        }")
							.AppendLine("      }")
							// スレ説明文を削除、フォーム位置切替リンクを削除
							.AppendLine("      {")
							.AppendLine("        const fm = document.forms.fm;")
							.AppendLine("        const ftb2 = fm.getElementsByClassName('ftb2');")
							.AppendLine("        fm.removeChild(ftb2[0]);")
							.AppendLine("        const reszb = document.getElementById('reszb');")
							.AppendLine("        reszb.parentElement.removeChild(reszb);")
							.AppendLine("      }")
							// はっちゃんキャンバス起動リンク追加
							.AppendLine("      {")
							.AppendLine("        const ftbl = document.getElementById('ftbl');")
							.AppendLine("        const tr = document.createElement('tr');")
							.AppendLine("        const td = document.createElement('td');")
							.AppendLine("        td.setAttribute('colspan', '2');")
							.AppendLine("        td.style = 'text-align: right';")
							.AppendLine("        const a = document.createElement('a');")
							.AppendLine("        a.innerText = 'はっちゃんキャンバス';")
							.AppendLine($"        a.href = '{ MakiMokiProtocol98Open }';")
							.AppendLine("        td.appendChild(a);")
							.AppendLine("        tr.appendChild(td);")
							.AppendLine("        ftbl.appendChild(tr);")
							.AppendLine("      }")
							// Viewを閉じるリンク追加
							.AppendLine("      {")
							.AppendLine("        const a = document.createElement('a');")
							.AppendLine("        a.style = 'position: fixed; top: 4px; right: 4px;';")
							.AppendLine("        a.innerText = '閉じる';")
							.AppendLine($"        a.href = '{ MakiMokiProtocolViewClose }';")
							.AppendLine("        document.body.appendChild(a);")
							.AppendLine("      }")
							// submitボタンがajaxモードになっているので差し替える
							.AppendLine("      {")
							.AppendLine("        let target = null;")
							.AppendLine("        const tr = ((document.forms.fm.sub) ? document.forms.fm.sub : document.forms.fm.email).parentNode;")
							.AppendLine("        for(const el of tr.children) {")
							.AppendLine("          if(el.type === 'submit') {")
							.AppendLine("            target = el;")
							.AppendLine("            break;")
							.AppendLine("          }")
							.AppendLine("        }")
							.AppendLine("        if(target) {")
							.AppendLine("          tr.removeChild(target);")
							.AppendLine("          const input = document.createElement('input');")
							.AppendLine("          input.type = 'submit';")
							.AppendLine("          input.value = '返信する';")
							// ptfk()を呼ばないと手書きがinputにマッピングされない
							// そのまま送信するのでXMLHttpRequestを無効化しておく必要がある
							.AppendLine($"          input.addEventListener('click', ()=>{{")
							.AppendLine("             const json = JSON.stringify({")
							.AppendLine("               name: (document.forms.fm.name) ? document.forms.fm.name.value : '',")
							.AppendLine("               sub: (document.forms.fm.sub) ? document.forms.fm.sub.value : '',")
							.AppendLine("               email: document.forms.fm.email.value,")
							.AppendLine("               pwd: document.forms.fm.pwd.value,")
							.AppendLine("             });")
							.AppendLine($"             window.chrome.webview.postMessage('{ WebMessagePostStore }?' + encodeURI(json));")
							.AppendLine($"             ptfk({ threadUrl.ThreadNo });")
							.AppendLine($"             return true;")
							.AppendLine($"           }});")
							.AppendLine("          tr.appendChild(input);")
							.AppendLine("        }")
							// フォーム位置をずらす、パスワードを表示
							.AppendLine("        document.forms.fm.style = 'margin-top: 6em;';")
							.AppendLine(
								Canvas98Config.Canvas98ConfigLoader.InitializedSetting.MaskPassword() switch {
									true => "",
									false => "        document.forms.fm.pwd.type = 'text';"
								})
							.AppendLine("      }")
							// 独自ボタンを仕込む
							.AppendLine($"      if({ toJsBool(isChildItem) }) {{")
							.AppendLine("        const callback = function(mutationsList, observer) {")
							.AppendLine("          for(const mutation of mutationsList) {")
							.AppendLine("            if(mutation.type === 'childList') {")
							.AppendLine("              const n = document.getElementById('canvas98UndoButton');")
							.AppendLine("              if(n && !document.getElementById('makimokiCanvas98Extension')) {")
							.AppendLine($"               if({ toJsBool(!string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptAlbam)) }) {{")
							.AppendLine("                  const el = document.createElement('li');")
							.AppendLine("                  el.id = 'makimokiCanvas98Extension';")
							.AppendLine("                  el.className = 'canvas98MenuItem material-icons';")
							.AppendLine("                  el.title = 'アルバム';")
							.AppendLine("                  el.innerText = 'save';")
							.AppendLine("                  el.addEventListener('click', _ => {")
							.AppendLine($"                    window.chrome.webview.postMessage('{ WebMessageExecuteAlbam }');")
							.AppendLine("                  });")
							.AppendLine("                  n.parentNode.insertBefore(el, n.parentNode.firstNode);")
							.AppendLine("                }")
							.AppendLine($"               if({toJsBool(!string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptTimelapse))}) {{")
							.AppendLine("                  const el = document.createElement('li');")
							.AppendLine("                  el.id = 'makimokiCanvas98Extension';")
							.AppendLine("                  el.className = 'canvas98MenuItem material-icons';")
							.AppendLine("                  el.title = 'タイムラプス';")
							.AppendLine("                  el.innerText = 'timelapse';")
							.AppendLine("                  el.addEventListener('click', _ => {")
							.AppendLine($"                    window.chrome.webview.postMessage('{ WebMessageExecuteTimelapse }');")
							.AppendLine("                  });")
							.AppendLine("                  n.parentNode.insertBefore(el, n.parentNode.firstNode);")
							.AppendLine("                }")
							.AppendLine($"               if({toJsBool(!string.IsNullOrEmpty(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptRichPalette))}) {{")
							.AppendLine("                  const menuNode = document.getElementById('canvas98VerticalMainMenu');")
							.AppendLine("                  const topMenuNode = menuNode.getElementsByClassName('canvas98MenuItemParent')[0];")
							.AppendLine("                  const el = document.createElement('li');")
							.AppendLine("                  el.id = 'makimokiCanvas98Extension';")
							.AppendLine("                  el.className = 'canvas98MenuItem material-icons';")
							.AppendLine("                  el.title = 'リッチパレット';")
							.AppendLine("                  el.innerText = 'palette';")
							.AppendLine("                  el.addEventListener('click', _ => {")
							.AppendLine($"                    window.chrome.webview.postMessage('{WebMessageExecuteRichPalette}');")
							.AppendLine("                  });")
							.AppendLine("                  topMenuNode.insertBefore(el, topMenuNode.firstNode);")
							.AppendLine("                }")
							.AppendLine("                break;")
							.AppendLine("              }")
							.AppendLine("            }")
							.AppendLine("          }")
							.AppendLine("        };")
							.AppendLine("        const observer = new MutationObserver(callback);")
							.AppendLine("        observer.observe(document.body, { childList: true, subtree: true });")
							.AppendLine("      }")
							.AppendLine($"      window.chrome.webview.postMessage('{ WebMessageReady }');")
							.AppendLine("    } else {")
							//.AppendLine($"      window.chrome.webview.postMessage('{ WebMessageDebug }?' + encodeURI(document.location.href));")
							.AppendLine($"      window.chrome.webview.postMessage('{ WebMessage404 }');")
							.AppendLine("    }")
							.AppendLine("  }")
							.AppendLine("});")
							.ToString());
					if(Uri.TryCreate(x.Url.BaseUrl, UriKind.Absolute, out var baseUri)
						&& Uri.TryCreate(Util.Futaba.GetFutabaThreadUrl(x.Url), UriKind.Absolute, out var targetUri)) {

						if(this.webView.Source == targetUri) {
							await this.ExecCanvas98();
						} else {
							this.webView.Source = targetUri;
						}
					}
				});
			this.CloseToSubscriber = ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
				.GetEvent<PubSubEvent<ViewModels.FutabaCanvas98ViewViewModel.CloseTo>>()
				.Subscribe(x => {
					var threadUrl = x.Url;
					if(threadUrl != null) {
						if(this.DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
							if(threadUrl != vm.Url) {
								return;
							}
						}
					}

					this.NavigateToSubscriber.Dispose();
					this.CloseToSubscriber.Dispose();
					{
						if(this.DataContext is IDisposable d) {
							d.Dispose();
						}
					}
					this.grid.Children.Clear();
					Observable.Return(0)
						.ObserveOn(Reactive.Bindings.UIDispatcherScheduler.Default)
						.Subscribe(_ => {
							this.webView.Dispose();
						});
				});

			webViewInitializeTask = webView.EnsureCoreWebView2Async();
			this.webView.NavigationStarting += async (s, e) => {
				System.Diagnostics.Debug.Assert(!this.urlCache.ContainsKey(e.NavigationId));
				var threadUrl = default(Data.UrlContext);
				{
					if(this.DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
						threadUrl = vm.Url;
					}
				}
				System.Diagnostics.Debug.Assert(threadUrl != null);
				System.Diagnostics.Debug.Assert(threadUrl.IsThreadUrl);

				/*
				if(e.Uri.StartsWith("data:text/html")) {
					urlCache.Add(e.NavigationId, this.ThreadUrl);
				}
				*/
				if(e.Uri.EndsWith(".htm")) {
					this.webView.CoreWebView2.WebResourceRequested += this.OnRequestFilter;
					this.NavigationVisibility = Visibility.Hidden;
					urlCache.Add(e.NavigationId, (UrlType.ThreadHtml, threadUrl));
				} else if(e.Uri.Contains("/futaba.php")) {
					//e.Cancel = true;
					urlCache.Add(e.NavigationId, (UrlType.FutbaPhp, threadUrl));
				} else if(e.Uri.StartsWith(MakiMokiProtocolViewClose)) {
					e.Cancel = true;
					if(DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
						vm.Close();
					}
				} else if(e.Uri.StartsWith(MakiMokiProtocol98Open)) {
					e.Cancel = true;
					await this.ExecCanvas98();
				}
			};
			this.webView.NavigationCompleted += async (s, e) => {
				this.webView.CoreWebView2.WebResourceRequested -= this.OnRequestFilter;
				if(this.urlCache.TryGetValue(e.NavigationId, out var url)) {
					this.urlCache.Remove(e.NavigationId);
					switch(url.Type) {
					case UrlType.ThreadHtml:
						this.NavigationVisibility = Visibility.Visible;
						break;
					case UrlType.FutbaPhp:
						await this.webView.ExecuteScriptAsync(new StringBuilder()
							.AppendLine("'use strict';")
							.AppendLine("let target = null;")
							// リダイレクトが入っている場合成功とみなす
							.AppendLine("head: for(const el of document.head.children) {")
							.AppendLine("  if(el.localName.toLowerCase() == 'meta') {")
							.AppendLine("    for(const atr of el.attributes) {")
							.AppendLine("      if((atr.name.toLowerCase() === 'http-equiv') && (atr.value.toLowerCase() === 'refresh')) {")
							.AppendLine($"        window.chrome.webview.postMessage('{ WebMessagePostSucessed }');")
							.AppendLine("        target = el;")
							.AppendLine("        break head;")
							.AppendLine("      }")
							.AppendLine("    }")
							.AppendLine("  }")
							.AppendLine("}")
							// エラー文字列取得処理
							.AppendLine("if(!target) {")
							.AppendLine("  let div = null;")
							.AppendLine("  let firstDiv = false;")
							.AppendLine("  for(const el of document.body.children) {")
							.AppendLine("    if(el.localName.toLowerCase() === 'div') {")
							.AppendLine("      if(!firstDiv) {")
							.AppendLine("        firstDiv = true;")
							.AppendLine("      } else {")
							.AppendLine("        div = el;")
							.AppendLine("        break;")
							.AppendLine("      }")
							.AppendLine("    }")
							.AppendLine("  }")
							.AppendLine("  if(div) {")
							.AppendLine("    if(div.innerText.startsWith('スレッドがありません')) {")
							.AppendLine($"      window.chrome.webview.postMessage('{ WebMessage404 }');")
							.AppendLine("    } else {")
							.AppendLine(@"      const s = div.innerText.replace(/\r?\n\r?\nリロード$/i, '');")
							.AppendLine($"      window.chrome.webview.postMessage('{ WebMessagePostError }?' + encodeURI(s));")
							.AppendLine("      window.history.back(-1);")
							.AppendLine("    }")
							.AppendLine("  }")
							.AppendLine("}")
							.ToString());
						break;
					}
				}
			};
			this.webView.WebMessageReceived += async (s, e) => { 
				var message = e.TryGetWebMessageAsString();
				if(message == WebMessageReady) {
					this.formCache = default;
					static string conv(string p)
						=> new string(Newtonsoft.Json.JsonConvert.ToString(p)
							.Replace("'", @"\'")
							.ToCharArray()
							.Skip(1)
							.SkipLast(1)
							.ToArray());
					await this.webView.ExecuteScriptAsync(new StringBuilder()
						.AppendLine("'use strict';")
						.AppendLine("const form = {")
						.AppendLine($"  name: '{conv(Config.ConfigLoader.FutabaApi.SavedName)}',")
						.AppendLine($"  sub: '{conv(Config.ConfigLoader.FutabaApi.SavedSubject)}',")
						.AppendLine($"  email: '{conv(Config.ConfigLoader.FutabaApi.SavedMail)}',")
						.AppendLine($"  pwd: '{conv(Config.ConfigLoader.FutabaApi.SavedPassword)}',")
						.AppendLine("};")
						.AppendLine("if(document.forms.fm.name) { document.forms.fm.name.value = form.name; }")
						.AppendLine("if(document.forms.fm.sub) { document.forms.fm.sub.value = form.sub; }")
						.AppendLine("document.forms.fm.email.value = form.email;")
						.AppendLine("document.forms.fm.pwd.value = form.pwd;")
						.ToString());
					await this.ExecCanvas98();
				} else if(message == WebMessageExecuteAlbam) {
					await this.webView.ExecuteScriptAsync(new StringBuilder()
						.AppendLine("if(document.getElementById('canvas98Element') !== null) {")
						.AppendLine(this.ConvertJs(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptAlbam))
						.AppendLine("}")
						.ToString());
				} else if(message == WebMessageExecuteRichPalette) {
					await this.webView.ExecuteScriptAsync(new StringBuilder()
						.AppendLine("if(document.getElementById('canvas98Element') !== null) {")
						.AppendLine(this.ConvertJs(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptRichPalette))
						.AppendLine("}")
						.ToString());
				} else if(message == WebMessageExecuteTimelapse) {
					await this.webView.ExecuteScriptAsync(new StringBuilder()
						.AppendLine("if(document.getElementById('canvas98Element') !== null) {")
						.AppendLine(this.ConvertJs(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ScriptTimelapse))
						.AppendLine("}")
						.ToString());
				} else if(message == WebMessage404) {
					Util.Futaba.PutInformation(new Data.Information("スレッドは落ちています"));
					if(DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
						vm.Close();
					}
					this.webView.NavigateToString("");
				} else if(message == WebMessagePostSucessed) {
					if(DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
						// TODO: 成功メッセージをリレー
						ViewModels.FutabaCanvas98ViewViewModel.Messenger.Instance
							.GetEvent<PubSubEvent<ViewModels.FutabaCanvas98ViewViewModel.PostFrom>>()
							.Publish(new ViewModels.FutabaCanvas98ViewViewModel.PostFrom(vm.Url, this.formCache));
						vm.Close();
					}
					/*
					if(this.Visibility == Visibility.Visible) {
						this.Visibility = Visibility.Hidden;
						this.RaiseEvent(new RoutedSucessEventArgs(
							this.ThreadUrl,
							this.formCache,
							SuccessedEvent));
					}
					*/
					this.webView.NavigateToString("");
				} else if(message.StartsWith(WebMessagePostError)) {
					Util.Futaba.PutInformation(new Data.Information(
						System.Web.HttpUtility.UrlDecode(message.Substring(WebMessagePostError.Length + 1))));
				} else if(message.StartsWith(WebMessagePostStore)) {
					this.formCache = JsonConvert.DeserializeObject<Canvas98Data.StoredForm>(
						System.Web.HttpUtility.UrlDecode(message.Substring(WebMessagePostStore.Length + 1)));
				} else if(message.StartsWith(WebMessageDebug)) {
#if DEBUG
					System.Diagnostics.Debug.WriteLine(
						System.Web.HttpUtility.UrlDecode(message.Substring(WebMessageDebug.Length + 1)));
#endif
				}
			};

			this.IsVisibleChanged += (s, e) => {
				if(this.Visibility == Visibility.Visible) {
					if(!Canvas98Util.Util.IsEnabledCanvas98()) {
						Util.Futaba.PutInformation(new Data.Information("はっちゃんキャンバスの設定が不正です"));
						if(DataContext is ViewModels.FutabaCanvas98ViewViewModel vm) {
							vm.Close();
						}
						return;
					}
				}
			};

		}

		private void OnRequestFilter(object _, Microsoft.Web.WebView2.Core.CoreWebView2WebResourceRequestedEventArgs e) {
			if(this.webView?.CoreWebView2 == null) {
				return;
			}

			if(!System.Text.RegularExpressions.Regex.IsMatch(e.Request.Uri, @"^https?://[^/]*2chan.net/.*$")) {
				e.Response = this.webView.CoreWebView2.Environment.CreateWebResourceResponse(null, 404, "Not found", null);
			}
		}

		private Task ExecCanvas98() {
			return this.webView.ExecuteScriptAsync(new StringBuilder()
				.AppendLine("if(document.getElementById('canvas98Element') === null) {")
				.AppendLine(this.ConvertJs(Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.Script))
				// 拡張が失敗することがあるので少し待つ
				.AppendLine("  setTimeout(() => {")
				.AppendJoin<string>(
					Environment.NewLine,
					Canvas98Config.Canvas98ConfigLoader.Bookmarklet.Value.ExtendScripts.Select(x => this.ConvertJs(x)))
				.AppendLine("  }, 100);")
				.AppendLine("}")
				.ToString());
		}

		private string ConvertJs(string s) {
			return s switch {
				string x when x.Contains("let%20") || x.Contains("return%20") => Uri.UnescapeDataString(x), // ソースコードにありそうな感じなのがあったらデコード
				string x when x.Contains('\"') || x.Contains('\'') => x, // これはエンコードされていないので通す
				string x => Uri.UnescapeDataString(x), // デフォルトデコード
				_ => throw new ArgumentNullException(),
			};
		}
	}
}
