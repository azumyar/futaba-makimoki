using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reactive.Linq;
using Uno.Extensions.Navigation;
using Reactive.Bindings;
using Yarukizero.Net.MakiMoki.Reactive;

namespace Yarukizero.Net.MakiMoki.Uno.ViewModels {
	class MobilePageViewModel {
		public record class BoardItem(string Name, Data.BoardData Board) {}

		public ReactiveProperty<IEnumerable<BoardItem>> Source { get; }
		public MakiMokiCommand<Data.BoardData> ClickCommand { get; } = new MakiMokiCommand<Data.BoardData>();

		public MobilePageViewModel(INavigator navigator) {
			this.Source = new ReactiveProperty<IEnumerable<BoardItem>>(
				Config.ConfigLoader.Board.Boards.Select(x => new BoardItem(
					x.Name,
					x)).ToArray());
			this.ClickCommand.Subscribe(x => {
				navigator.NavigateViewAsync<Views.CatalogPage>(this, data: x);
			});
		}
	}
}
