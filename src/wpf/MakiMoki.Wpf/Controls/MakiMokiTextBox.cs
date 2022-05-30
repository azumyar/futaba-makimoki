using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Yarukizero.Net.MakiMoki.Wpf.Controls {
	// Emoji.Wpf.RichTextBoxがIME制御を考慮していないためやっつけ対応する
	class MakiMokiTextBox : Emoji.Wpf.RichTextBox {
		// IME入力中はtrue
		private bool isImeInput = false;

		public MakiMokiTextBox() {
			TextCompositionManager.AddPreviewTextInputHandler(this, this.OnManagerPreviewTextInput);
			TextCompositionManager.AddPreviewTextInputStartHandler(this, this.OnManagerPreviewTextInputStart);
			TextCompositionManager.AddPreviewTextInputUpdateHandler(this, this.OnManagerPreviewTextInputUpdate);
		}

		private void OnManagerPreviewTextInput(object sender, TextCompositionEventArgs e) {
			System.Diagnostics.Debug.WriteLine($"OnManagerPreviewTextInput={isImeInput}");
			this.isImeInput = false;
		}

		private void OnManagerPreviewTextInputStart(object sender, TextCompositionEventArgs e) {
			System.Diagnostics.Debug.WriteLine($"OnManagerPreviewTextInputStart={isImeInput}");
			//this.isImeInput = (InputMethod.Current.ImeState == InputMethodState.On);
			if(InputMethod.Current.ImeState != InputMethodState.On) {
				this.isImeInput = false;
			}
		}

		private void OnManagerPreviewTextInputUpdate(object sender, TextCompositionEventArgs e) {
			System.Diagnostics.Debug.WriteLine($"OnManagerPreviewTextInputUpdate={isImeInput}");
			//絵文字が入力されるとEmoji.Wpf.RichTextBoxが確定処理を行うのでIME入力中フラグをたてる
			if(!this.isImeInput && Emoji.Wpf.EmojiData.MatchOne.IsMatch(e.TextComposition.CompositionText)) {
				this.isImeInput = true;
			} else if(e.TextComposition.CompositionText.Length == 0) {
				this.isImeInput = false;
			}
		}

		protected override void OnPreviewTextInput(TextCompositionEventArgs e) {
			System.Diagnostics.Debug.WriteLine($"OnPreviewTextInput={isImeInput}");
			this.isImeInput = false;
			base.OnPreviewTextInput(e);
		}

		protected override void OnTextChanged(TextChangedEventArgs e) {
			// 絵文字確定で変換処理が走るのでIME入力中は握りつぶす
			System.Diagnostics.Debug.WriteLine($"OnTextChanged={isImeInput}");
			if(!this.isImeInput) {
				base.OnTextChanged(e);
			}
		}
	}
}
