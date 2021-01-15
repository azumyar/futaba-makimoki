/*
 * kanaxs C# 拡張版 1.0.2
 * Copyright (c) 2011, DOBON! <http://dobon.net>
 * All rights reserved.
 * 
 * New BSD License（修正BSDライセンス）
 * http://wiki.dobon.net/index.php?free%2FkanaxsCSharp%2Flicense
 * 
 * このクラスは、以下のソースを参考にして作成しました。
 * kanaxs Kana.JS
 * shogo4405 <shogo4405 at gmail.com>
 * http://code.google.com/p/kanaxs/
*/

using System;

namespace CSharp.Japanese.Kanaxs {
    /// <summary>
    /// ひらがなとカタカナ、半角と全角の文字変換を行うメソッドを提供します。
    /// Kanaクラスの拡張版です。
    /// </summary>
    public sealed class KanaEx {
        private KanaEx() {
        }

        /// <summary>
        /// 全角カタカナを全角ひらがなに変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToHiraganaメソッドと違い、「ヽヾヷヸヹヺ」も変換します。
        /// </remarks>
        public static string ToHiragana(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = new char[str.Length * 2];
            int len = 0;
            int f = str.Length;

            for(int i = 0; i < f; i++) {
                char c = str[i];
                // ァ(0x30A1) ～ ン(0x30F3)
                // ヴ(0xu30F4)
                // ヵ(0x30F5) ～ ヶ(0x30F6)
                // ヽ(0x30FD) ヾ(0x30FE)
                if(('ァ' <= c && c <= 'ヶ') ||
                    ('ヽ' <= c && c <= 'ヾ')) {
                    cs[len++] = (char)(c - 0x0060);
                }
                // ヷ(0x30F7) ～ ヺ(0x30FA)
                else if('ヷ' <= c && c <= 'ヺ') {
                    cs[len++] = (char)(c - 0x0068);
                    cs[len++] = '゛';
                } else {
                    cs[len++] = c;
                }
            }

            return new string(cs, 0, len);
        }

        /// <summary>
        /// 全角ひらがなを全角カタカナに変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToKatakanaメソッドと違い、「ゝゞ」も変換します。
        /// </remarks>
        public static string ToKatakana(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = str.ToCharArray();
            int f = cs.Length;

            for(int i = 0; i < f; i++) {
                char c = cs[i];
                // ぁ(0x3041) ～ ゖ(0x3096)
                // ゝ(0x309D) ゞ(0x309E)
                if(('ぁ' <= c && c <= 'ゖ') ||
                    ('ゝ' <= c && c <= 'ゞ')) {
                    cs[i] = (char)(c + 0x0060);
                }
            }

            return new string(cs);
        }

        /// <summary>
        /// 全角英数字および記号を半角英数字および記号に変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToHankakuメソッドと違い、「¥”’」も変換します。
        /// </remarks>
        public static string ToHankaku(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = str.ToCharArray();
            int f = cs.Length;

            for(int i = 0; i < f; i++) {
                char c = cs[i];
                // ！(0xFF01) ～ ～(0xFF5E)
                if('！' <= c && c <= '～') {
                    cs[i] = (char)(c - 0xFEE0);
                }
                // 全角スペース(0x3000) -> 半角スペース(0x0020)
                else if(c == '　') {
                    cs[i] = ' ';
                } else if(c == '¥') {
                    cs[i] = '\\';
                } else if(c == '”' || c == '“') {
                    cs[i] = '"';
                } else if(c == '’' || c == '‘') {
                    cs[i] = '\'';
                }
            }

            return new string(cs);
        }

        /// <summary>
        /// 半角英数字および記号を全角に変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToZenkakuメソッドと違い、「\"'」を「¥”’」に変換します。
        /// </remarks>
        public static string ToZenkaku(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = str.ToCharArray();
            int f = cs.Length;

            for(int i = 0; i < f; i++) {
                char c = cs[i];
                if(c == '\\') {
                    cs[i] = '¥';
                } else if(c == '"') {
                    cs[i] = '”';
                } else if(c == '\'') {
                    cs[i] = '’';
                }
                  // !(0x0021) ～ ~(0x007E)
                  else if('!' <= c && c <= '~') {
                    cs[i] = (char)(c + 0xFEE0);
                }
                  // 半角スペース(0x0020) -> 全角スペース(0x3000)
                  else if(c == ' ') {
                    cs[i] = '　';
                }
            }

            return new string(cs);
        }

        /// <summary>
        /// 全角カタカナを半角カタカナに変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToHankakuKanaメソッドと違い、
        /// 『、。「」・゛゜U+3099（濁点）U+309A（半濁点）ヴヷヺ』も変換します。
        /// </remarks>
        public static string ToHankakuKana(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = new char[str.Length * 2];
            int len = 0;

            int f = str.Length;

            for(int i = 0; i < f; i++) {
                char c = str[i];
                // 、(0x3001) ～ ー(0x30FC)
                if('、' <= c && c <= 'ー') {
                    char m = ConvertToHankakuKanaChar(c);
                    if(m != '\0') {
                        cs[len++] = m;
                    }
                    // カ(0x30AB) ～ ド(0x30C9)
                    else if('カ' <= c && c <= 'ド') {
                        cs[len++] = ConvertToHankakuKanaChar((char)(c - 1));
                        cs[len++] = '゛';
                    }
                    // ハ(0x30CF) ～ ポ(0x30DD)
                    else if('ハ' <= c && c <= 'ポ') {
                        int mod3 = c % 3;
                        cs[len++] = ConvertToHankakuKanaChar((char)(c - mod3));
                        cs[len++] = (mod3 == 1 ? '゛' : '゜');
                    }
                    // ヴ(0x30F4)
                    else if(c == 'ヴ') {
                        cs[len++] = 'ウ';
                        cs[len++] = '゛';
                    }
                    // ヷ(0x30F7)
                    else if(c == 'ヷ') {
                        cs[len++] = 'ワ';
                        cs[len++] = '゛';
                    }
                    // ヺ(0x30FA)
                    else if(c == 'ヺ') {
                        cs[len++] = 'ヲ';
                        cs[len++] = '゛';
                    } else {
                        cs[len++] = c;
                    }
                } else {
                    cs[len++] = c;
                }
            }

            return new string(cs, 0, len);
        }

        /// <summary>
        /// 半角カタカナを全角カタカナに変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToZenkakuKanaと違い、「。「」、・」も変換します。
        /// また、濁点、半濁点がその前の文字と合体できる時は合体させて1文字にします。
        /// </remarks>
        public static string ToZenkakuKana(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = new char[str.Length];
            int pos = str.Length - 1;

            for(int i = str.Length - 1; 0 <= i; i--) {
                char c = str[i];

                // 濁点(0xFF9E)
                if(c == '゛' && 0 < i) {
                    char c2 = str[i - 1];
                    // カ(0xFF76) ～ チ(0xFF81)
                    if('カ' <= c2 && c2 <= 'チ') {
                        cs[pos--] = (char)((c2 - 0xFF76) * 2 + 0x30AC);
                        i--;
                    }
                    // ツ(0xFF82) ～ ト(0xFF84)
                    else if('ツ' <= c2 && c2 <= 'ト') {
                        cs[pos--] = (char)((c2 - 0xFF82) * 2 + 0x30C5);
                        i--;
                    }
                    // ハ(0xFF8A) ～ ホ(0xFF8E)
                    else if('ハ' <= c2 && c2 <= 'ホ') {
                        cs[pos--] = (char)((c2 - 0xFF8A) * 3 + 0x30D0);
                        i--;
                    }
                    // ウ(0xFF73)
                    else if(c2 == 'ウ') {
                        cs[pos--] = 'ヴ';
                        i--;
                    }
                    // ワ(0xFF9C)
                    else if(c2 == 'ワ') {
                        cs[pos--] = 'ヷ';
                        i--;
                    }
                    // ヲ(0xFF66)
                    else if(c2 == 'ヲ') {
                        cs[pos--] = 'ヺ';
                        i--;
                    }
                    // 全角濁点
                    else {
                        cs[pos--] = '゛';
                    }
                }
                // 半濁点(0xFF9F)
                else if(c == '゜' && 0 < i) {
                    char c2 = str[i - 1];
                    // ハ(0xFF8A) ～ ホ(0xFF8E)
                    if('ハ' <= c2 && c2 <= 'ホ') {
                        cs[pos--] = (char)((c2 - 0xFF8A) * 3 + 0x30D1);
                        i--;
                    }
                    // 全角半濁点
                    else {
                        cs[pos--] = '゜';
                    }
                }
                // 。(0xFF61) ～ ゜(0xFF9F)
                else if('。' <= c && c <= '゜') {
                    char m = ConvertToZenkakuKanaChar(c);
                    if(m != '\0') {
                        cs[pos--] = m;
                    } else {
                        cs[pos--] = c;
                    }
                } else {
                    cs[pos--] = c;
                }
            }

            return new string(cs, pos + 1, cs.Length - pos - 1);
        }

        /// <summary>
        /// 「は゛」を「ば」のように、濁点や半濁点を前の文字と合わせて1つの文字に変換します。
        /// </summary>
        /// <param name="str">変換する String。</param>
        /// <returns>変換された String。</returns>
        /// <remarks>
        /// Kana.ToPaddingと違い、「ゔゞヴヷヸヹヺヾ」への変換も行います。
        /// また、U+3099（濁点）とU+309A（半濁点）も前の文字と合体させて1文字にします。
        /// </remarks>
        public static string ToPadding(string str) {
            if(str == null || str.Length == 0) {
                return str;
            }

            char[] cs = new char[str.Length];
            int pos = str.Length - 1;

            int f = str.Length - 1;

            for(int i = f; 0 <= i; i--) {
                char c = str[i];

                // 濁点
                if((c == '゛' || c == '\u3099') && 0 < i) {
                    char c2 = str[i - 1];
                    int mod2 = c2 % 2;
                    int mod3 = c2 % 3;

                    // か(0x304B) ～ ち(0x3061)
                    // カ(0x30AB) ～ チ(0x30C1)
                    // つ(0x3064) ～ と(0x3068)
                    // ツ(0x30C4) ～ ト(0x30C8)
                    // は(0x306F) ～ ほ(0x307B)
                    // ハ(0x30CF) ～ ホ(0x30DB)
                    // ゝ(0x309D) ヽ(0x30FD)
                    if(('か' <= c2 && c2 <= 'ち' && mod2 == 1) ||
                        ('カ' <= c2 && c2 <= 'チ' && mod2 == 1) ||
                        ('つ' <= c2 && c2 <= 'と' && mod2 == 0) ||
                        ('ツ' <= c2 && c2 <= 'ト' && mod2 == 0) ||
                        ('は' <= c2 && c2 <= 'ほ' && mod3 == 0) ||
                        ('ハ' <= c2 && c2 <= 'ホ' && mod3 == 0) ||
                        c2 == 'ゝ' || c2 == 'ヽ') {
                        cs[pos--] = (char)(c2 + 1);
                        i--;
                    }
                    // う(0x3046) ウ(0x30A6) -> ゔヴ
                    else if(c2 == 'う' || c2 == 'ウ') {
                        cs[pos--] = (char)(c2 + 0x004E);
                        i--;
                    }
                    // ワ(0x30EF)ヰヱヲ(0x30F2) -> ヷヸヹヺ
                    else if('ワ' <= c2 && c2 <= 'ヲ') {
                        cs[pos--] = (char)(c2 + 8);
                        i--;
                    } else {
                        cs[pos--] = c;
                    }
                }
                // ゜(0x309C)
                else if((c == '゜' || c == '\u309A') && 0 < i) {
                    char c2 = str[i - 1];
                    int mod3 = c2 % 3;

                    // は(0x306F) ～ ほ(0x307B)
                    // ハ(0x30CF) ～ ホ(0x30DB)
                    if(('は' <= c2 && c2 <= 'ほ' && mod3 == 0) ||
                        ('ハ' <= c2 && c2 <= 'ホ' && mod3 == 0)) {
                        cs[pos--] = (char)(c2 + 2);
                        i--;
                    } else {
                        cs[pos--] = c;
                    }
                } else {
                    cs[pos--] = c;
                }
            }

            return new string(cs, pos + 1, cs.Length - pos - 1);
        }

        private static char ConvertToHankakuKanaChar(char zenkakuChar) {
            switch(zenkakuChar) {
            case 'ァ':
                return 'ァ';
            case 'ィ':
                return 'ィ';
            case 'ゥ':
                return 'ゥ';
            case 'ェ':
                return 'ェ';
            case 'ォ':
                return 'ォ';
            case 'ー':
                return 'ー';
            case 'ア':
                return 'ア';
            case 'イ':
                return 'イ';
            case 'ウ':
                return 'ウ';
            case 'エ':
                return 'エ';
            case 'オ':
                return 'オ';
            case 'カ':
                return 'カ';
            case 'キ':
                return 'キ';
            case 'ク':
                return 'ク';
            case 'ケ':
                return 'ケ';
            case 'コ':
                return 'コ';
            case 'サ':
                return 'サ';
            case 'シ':
                return 'シ';
            case 'ス':
                return 'ス';
            case 'セ':
                return 'セ';
            case 'ソ':
                return 'ソ';
            case 'タ':
                return 'タ';
            case 'チ':
                return 'チ';
            case 'ツ':
                return 'ツ';
            case 'テ':
                return 'テ';
            case 'ト':
                return 'ト';
            case 'ナ':
                return 'ナ';
            case 'ニ':
                return 'ニ';
            case 'ヌ':
                return 'ヌ';
            case 'ネ':
                return 'ネ';
            case 'ノ':
                return 'ノ';
            case 'ハ':
                return 'ハ';
            case 'ヒ':
                return 'ヒ';
            case 'フ':
                return 'フ';
            case 'ヘ':
                return 'ヘ';
            case 'ホ':
                return 'ホ';
            case 'マ':
                return 'マ';
            case 'ミ':
                return 'ミ';
            case 'ム':
                return 'ム';
            case 'メ':
                return 'メ';
            case 'モ':
                return 'モ';
            case 'ヤ':
                return 'ヤ';
            case 'ユ':
                return 'ユ';
            case 'ヨ':
                return 'ヨ';
            case 'ラ':
                return 'ラ';
            case 'リ':
                return 'リ';
            case 'ル':
                return 'ル';
            case 'レ':
                return 'レ';
            case 'ロ':
                return 'ロ';
            case 'ワ':
                return 'ワ';
            case 'ヲ':
                return 'ヲ';
            case 'ン':
                return 'ン';
            case 'ッ':
                return 'ッ';

            //ャュョ を追加
            case 'ャ':
                return 'ャ';
            case 'ュ':
                return 'ュ';
            case 'ョ':
                return 'ョ';

            // 、。「」・ を追加
            case '、':
                return '、';
            case '。':
                return '。';
            case '「':
                return '「';
            case '」':
                return '」';
            case '・':
                return '・';

            //゛゜ を追加
            case '゛':
                return '゛';
            case '゜':
                return '゜';

            //U+3099とU+309Aの濁点と半濁点を追加
            case '\u3099':
                return '゛';
            case '\u309A':
                return '゜';

            default:
                return '\0';
            }
        }

        private static char ConvertToZenkakuKanaChar(char hankakuChar) {
            switch(hankakuChar) {
            case 'ヲ':
                return 'ヲ';
            case 'ァ':
                return 'ァ';
            case 'ィ':
                return 'ィ';
            case 'ゥ':
                return 'ゥ';
            case 'ェ':
                return 'ェ';
            case 'ォ':
                return 'ォ';
            case 'ー':
                return 'ー';
            case 'ア':
                return 'ア';
            case 'イ':
                return 'イ';
            case 'ウ':
                return 'ウ';
            case 'エ':
                return 'エ';
            case 'オ':
                return 'オ';
            case 'カ':
                return 'カ';
            case 'キ':
                return 'キ';
            case 'ク':
                return 'ク';
            case 'ケ':
                return 'ケ';
            case 'コ':
                return 'コ';
            case 'サ':
                return 'サ';
            case 'シ':
                return 'シ';
            case 'ス':
                return 'ス';
            case 'セ':
                return 'セ';
            case 'ソ':
                return 'ソ';
            case 'タ':
                return 'タ';
            case 'チ':
                return 'チ';
            case 'ツ':
                return 'ツ';
            case 'テ':
                return 'テ';
            case 'ト':
                return 'ト';
            case 'ナ':
                return 'ナ';
            case 'ニ':
                return 'ニ';
            case 'ヌ':
                return 'ヌ';
            case 'ネ':
                return 'ネ';
            case 'ノ':
                return 'ノ';
            case 'ハ':
                return 'ハ';
            case 'ヒ':
                return 'ヒ';
            case 'フ':
                return 'フ';
            case 'ヘ':
                return 'ヘ';
            case 'ホ':
                return 'ホ';
            case 'マ':
                return 'マ';
            case 'ミ':
                return 'ミ';
            case 'ム':
                return 'ム';
            case 'メ':
                return 'メ';
            case 'モ':
                return 'モ';
            case 'ヤ':
                return 'ヤ';
            case 'ユ':
                return 'ユ';
            case 'ヨ':
                return 'ヨ';
            case 'ラ':
                return 'ラ';
            case 'リ':
                return 'リ';
            case 'ル':
                return 'ル';
            case 'レ':
                return 'レ';
            case 'ロ':
                return 'ロ';
            case 'ワ':
                return 'ワ';
            case 'ン':
                return 'ン';
            case '゛':
                return '゛';
            case '゜':
                return '゜';

            // ャュョッ を追加
            case 'ャ':
                return 'ャ';
            case 'ュ':
                return 'ュ';
            case 'ョ':
                return 'ョ';
            case 'ッ':
                return 'ッ';

            // 。「」、・ を追加
            case '。':
                return '。';
            case '「':
                return '「';
            case '」':
                return '」';
            case '、':
                return '、';
            case '・':
                return '・';

            default:
                return '\0';
            }
        }
    }
}