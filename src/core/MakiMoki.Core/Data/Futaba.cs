using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Yarukizero.Net.MakiMoki.Util;
using System.Text.RegularExpressions;
using System.IO;

namespace Yarukizero.Net.MakiMoki.Data {
	public class FutabaResonse : JsonObject {
		public class ResConverter : JsonConverter {
			public override bool CanConvert(Type objectType) {
				return typeof(NumberedResItem) == objectType;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				var dic = new Dictionary<string, ResItem>();
				var keys = new List<string>();
				if(reader.TokenType == JsonToken.StartObject) {
					while(reader.Read()) {
						switch(reader.TokenType) {
						case JsonToken.Null:
						case JsonToken.EndObject:
						case JsonToken.EndArray:
							goto end;
						case JsonToken.PropertyName:
							var prop = reader.Value?.ToString();
							reader.Read();

							keys.Add(prop);
							dic.Add(prop, serializer.Deserialize<ResItem>(reader));
							break;
						}
					}
				}
			end:
				return keys.Select(x => new NumberedResItem(x, dic[x])).OrderBy(x => x.Res.Rsc).ToArray();
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				if(value is NumberedResItem[] nri) {
					serializer.Serialize(writer, nri.ToDictionary(x => x.No, x => x.Res));
				} else {
					serializer.Serialize(writer, value);
				}
			}
		}
		public class SoudaneConverter : JsonConverter {
			public override bool CanConvert(Type objectType) {
				return typeof(Dictionary<string, string>) == objectType;
			}

			public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) {
				var dic = new Dictionary<string, string>();
				while(reader.Read()) {
					switch(reader.TokenType) {
					case JsonToken.EndObject:
					case JsonToken.EndArray:
						goto end;
					case JsonToken.PropertyName:
						var prop = reader.Value?.ToString();
						reader.Read();

						dic.Add(prop, reader.Value.ToString());
						break;
					}
				}
			end:
				return dic;
			}

			public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) {
				if((value is Dictionary<string, string> d) && (d.Count == 0)) {
					// 0件の場合ふたば定義に合わせて配列にする
					serializer.Serialize(writer, new object[0]);
				} else {
					serializer.Serialize(writer, value);
				}
			}
		}

		[JsonProperty("die")]
		public string Die { get; internal set; }
		[JsonProperty("dielong")]
		public string DieLong { get; internal set; }
		[JsonProperty("dispname")]
		public int Dispname { get; internal set; }
		[JsonProperty("dispsod")]
		public int Dispsod { get; internal set; }
		[JsonProperty("maxres")]
		public string MaxRes { get; internal set; }
		[JsonProperty("nowtime")]
		public long NowTime { get; internal set; }
		[JsonProperty("old")]
		public long Old { get; internal set; }
		[JsonProperty("res")]
		[JsonConverter(typeof(ResConverter))]
		public NumberedResItem[] Res { get; internal set; }

		// そうだね
		// 2019/12/13現在
		// そうだねが存在しない場合 => []
		// そうだねが存在する場合 => { resNO: count, ... }
		// という形式で帰ってくるため大変めどい
		[JsonProperty("sd")]
		[JsonConverter(typeof(SoudaneConverter))]
		public Dictionary<string, string> Sd { get; internal set; }

		[JsonIgnore]
		public bool IsDisplayName => (Dispname != 0);

		[JsonIgnore]
		public bool IsOld => (Old != 0);

		// DieLongは RFC 1123 形式で格納されている cf: Sun, 15 Dec 2019 18:02:07 GMT
		[JsonIgnore]
		public DateTime? DieDateTime => DateTime.TryParse(DieLong,
			System.Globalization.CultureInfo.InvariantCulture,
			System.Globalization.DateTimeStyles.None, out var d) ? (DateTime?)d : null;

		// スレが落ちると Thu, 01 Jan 1970 01:07:29 GMT といった1970年まで落ちるので1日くらいマージンとっても問題ない
		// …と思ってたらつけっぱで普通に誤爆したので1年マージンとる
		[JsonIgnore]
		public bool IsDie => ((DieDateTime?.AddDays(365) ?? DateTime.MaxValue) < DateTime.Now);

		// 最大レスの閾値を超えた場合MaxResには下記のような文言が入る
		// 上限2000レスに達しました
		[JsonIgnore]
		public bool IsMaxRes => !string.IsNullOrEmpty(MaxRes);
	}

	public class NumberedResItem : JsonObject {
		[JsonProperty("no")]
		public string No { get; private set; }

		[JsonProperty("res")]
		public ResItem Res { get; private set; }

		[JsonProperty("isolate")]
		public bool? Isolate { get; private set; }

		[JsonIgnore]
		public bool IsolateValue => Isolate ?? false;

		/// <summary>JSONシリアライザ用</summary>
		private NumberedResItem() { }


		internal NumberedResItem(string no, ResItem res, bool? isolate = null) {
			this.No = no;
			this.Res = res;
			this.Isolate = isolate;
		}
	}

	public class ResItem : JsonObject {
		[JsonProperty("com")]
		public string Com { get; private set; }
		[JsonProperty("del")]
		public string Del { get; private set; }
		[JsonProperty("email")]
		public string Email { get; private set; }
		[JsonProperty("ext")]
		public string Ext { get; private set; }
		[JsonProperty("fsize")]
		public int Fsize { get; private set; }
		[JsonProperty("h")]
		public int H { get; private set; }
		[JsonProperty("host")]
		public string Host { get; private set; }
		[JsonProperty("id")]
		public string Id { get; private set; }
		[JsonProperty("name")]
		public string Name { get; private set; }
		[JsonProperty("now")]
		public string Now { get; private set; }
		[JsonProperty("rsc")]
		public int Rsc { get; private set; }
		[JsonProperty("src")]
		public string Src { get; private set; }
		[JsonProperty("sub")]
		public string Sub { get; private set; }
		[JsonProperty("thumb")]
		public string Thumb { get; private set; }
		[JsonProperty("tim")]
		public string Tim { get; private set; }
		[JsonProperty("w")]
		public int W { get; private set; }

		[JsonIgnore]
		public bool IsDel => (Del == "del");

		[JsonIgnore]
		public bool IsDel2 => (Del == "del2");

		[JsonIgnore]
		public DateTime NowDateTime {
			get {
				if(long.TryParse(Tim, out var v)) {
					return DateTimeOffset.FromUnixTimeMilliseconds(v).LocalDateTime;
				}
				return DateTime.MinValue;
			}
		}

		internal static ResItem From(
			string sub, string name,
			string email, string com,
			string id, string host, string del,
			string src, string thumb, string ext, int fsize, int w, int h,
			string now, string tim,
			int rsc) {

			return new ResItem() {
				Sub = sub,
				Name = name,
				Email = email,
				Com = com,
				Id = id,
				Host = host,
				Del = del,
				Src = src,
				Thumb = thumb,
				Ext = ext,
				Fsize = fsize,
				H = h,
				W = w,
				Now = now,
				Tim = tim,
				Rsc = rsc,
			};
		}
	}

	public static class DelReason {
		public static (string name, DelReasonItem[] value)[] Items => new[] {
			("文字・画像", new DelReasonItem[] {
				new DelReasonItem("中傷・侮辱・名誉毀損", "101"),
				new DelReasonItem("脅迫・自殺", "102"),
				new DelReasonItem("個人情報・プライバシー", "103"),
				new DelReasonItem("つきまとい・ストーカー", "104"),
				new DelReasonItem("連投・負荷増大・無意味な羅列", "105"),
				new DelReasonItem("広告・spam", "106"),
				new DelReasonItem("売春・援交", "107"),
				new DelReasonItem("侵害・妨害", "108"),
				new DelReasonItem("荒らし・嫌がらせ・混乱の元", "110"),
				new DelReasonItem("政治・宗教・民族", "111"),
			} ),
			("２次画像", new DelReasonItem[] {
				new DelReasonItem("グロ画像(２次)", "201"),
				new DelReasonItem("猥褻画像・無修正画像(２次)", "202"),
			} ),
			( "３次画像", new DelReasonItem[] {
				new DelReasonItem("グロ画像(３次)", "301"),
				new DelReasonItem("エロ画像(３次)", "302"),
				new DelReasonItem("個児童ポルノ画像(３次)", "303"),
				new DelReasonItem("猥褻画像・無修正画像(３次)", "304"),
			} ),
		};
	}

	public class DelReasonItem {
		public string Name { get; }
		public string ApiValue { get; }

		internal DelReasonItem(string name, string apiValue) {
			this.Name = name;
			this.ApiValue = apiValue;
		}
	}

	public static class CatalogSort {
		public static CatalogSortItem Catalog => new CatalogSortItem("カタログ", "0");
		public static CatalogSortItem New => new CatalogSortItem("新順", "1");
		public static CatalogSortItem Old => new CatalogSortItem("古順", "2");
		public static CatalogSortItem Many => new CatalogSortItem("多順", "3");
		public static CatalogSortItem Momentum => new CatalogSortItem("勢順", "6");
		public static CatalogSortItem Few => new CatalogSortItem("少順", "4");
		public static CatalogSortItem Soudane => new CatalogSortItem("そ順", "8");

		public static CatalogSortItem[] Items => new[] {
				Catalog,
				New,
				Old,
				Many,
				Momentum,
				Few,
				Soudane,
		};
	}


	public class CatalogSortItem {
		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; private set; }
		[JsonProperty("value", Required = Required.Always)]
		public string ApiValue { get; private set; }

		// JSONシリアライザ用
		private CatalogSortItem() { }

		internal CatalogSortItem(string name, string apiValue) {
			this.Name = name;
			this.ApiValue = apiValue;
		}
	}

	public class PostedResItem : JsonObject {
		[JsonProperty("bord", Required = Required.Always)]
		public string BordUrl { get; private set; }

		[JsonProperty("res", Required = Required.Always)]
		public NumberedResItem Res { get; private set; }

		// JSONシリアライザ用
		private PostedResItem() { }

		public PostedResItem(string bordUrl, NumberedResItem res) {
			this.BordUrl = bordUrl;
			this.Res = res;
		}
	}

	public class ShiokaraCompleteResponse : JsonObject {
		[JsonProperty("url")]
		public string Url { get; private set; }

		[JsonProperty("id")]
		public string Id { get; private set; }

		[JsonProperty("error")]
		public string Error { get; private set; }
	}

	public class AppsweetsThumbnailCompleteResponse : JsonObject {
		[JsonProperty("content", Required = Required.Always)]
		public string Content { get; private set; } // BASE64されたサムネデータが格納される/サムネがない場合は空文字列

		[JsonProperty("width", Required = Required.Always)]
		public int Width { get; private set; }

		[JsonProperty("height", Required = Required.Always)]
		public int Height { get; private set; }

		[JsonProperty("board", Required = Required.Always)]
		public string Board { get; private set; }

		[JsonProperty("name", Required = Required.Always)]
		public string Name { get; private set; }

		[JsonProperty("base", Required = Required.Always)]
		public string Base { get; private set; }

		[JsonProperty("mime", Required = Required.Always)]
		public string Mime { get; private set; }

		[JsonProperty("size", Required = Required.Always)]
		public int Size { get; private set; }

		[JsonProperty("comment", Required = Required.Always)]
		public string Comment { get; private set; }

		[JsonProperty("dimensions", Required = Required.Always)]
		public AppsweetsThumbnailDimensionData Dimensions { get; private set; }
	}

	public class AppsweetsThumbnailDimensionData : JsonObject {
		[JsonProperty("thumbnail", Required = Required.Always)]
		public AppsweetsThumbnailDimensionDataItem Thumbnail { get; private set; }


		[JsonProperty("original", Required = Required.Always)]
		public AppsweetsThumbnailDimensionDataItem Original { get; private set; }
	}

	public class AppsweetsThumbnailDimensionDataItem : JsonObject {
		[JsonProperty("width", Required = Required.Always)]
		public int Width { get; private set; }

		[JsonProperty("height", Required = Required.Always)]
		public int Height { get; private set; }
	}

	public class AppsweetsThumbnailErrorResponse : JsonObject {
		[JsonProperty("status", Required = Required.Always)]
		public int Status { get; private set; }

		[JsonProperty("error", Required = Required.Always)]
		public string Error { get; private set; }
	}

	public class FutabaContext {
			// TODO: 名前後で変える
			public class Item {
				public UrlContext Url { get; }
				public NumberedResItem ResItem { get; }
				public int CounterCurrent { get; private set; } = 0;
				public int CounterPrev { get; private set; } = 0;

				public int Soudane { get; private set; } = 0;

				public Quot[] QuotLines { get; private set; } = new Quot[0];

				public Item(UrlContext url, NumberedResItem item) {
					this.Url = url;
					this.ResItem = item;
				}

				public static Item FromCatalog(UrlContext url, NumberedResItem item, int counterCurrent, int counterPrev) {
					return new Item(url, item) {
						CounterCurrent = counterCurrent,
						CounterPrev = counterPrev,
					};
				}

				public static Item FromThreadRes(int soudane, Item old) {
					return new Item(old.Url, old.ResItem) {
						Soudane = soudane,
						QuotLines = old.QuotLines,
					};
				}

				public static Item FromThreadRes(UrlContext url, NumberedResItem item, int soudane, List<NumberedResItem> resItems) {
					// 引用解析
					var com = TextUtil.RowComment2Text(item.Res.Com).Replace("\r", "").Split('\n');
					var q = new Quot[com.Length];
					for(var i = 0; i < com.Length; i++) {
						var c = com[i];
						if(!string.IsNullOrEmpty(c) && (c[0] == '>')) {
							c = c.Substring(1);
							if(Regex.IsMatch(c, @"^No.\d+$")) {
								var no = c.Substring(3);
								var it = resItems.Where(x => x.No == no).FirstOrDefault();
								if(it != null) {
									q[i] = new Quot(true, true, it.No);
									goto end;
								}
							}
							if(Regex.IsMatch(c, @"^\d+\.[a-zA-Z0-9]+$")) {
								var it = resItems.Where(x => x.Res.Src.EndsWith(c)).FirstOrDefault();
								if(it != null) {
									q[i] = new Quot(true, true, it.No);
									goto end;
								}
							}
							if(Regex.IsMatch(c, @"^>.*$")) {
								// 二重引用以上は完全一致
								foreach(var it in resItems.Reverse<NumberedResItem>()) {
									foreach(var c2 in TextUtil.RowComment2Text(it.Res.Com).Replace("\r", "").Split('\n')) {
										if(c2 == c) {
											q[i] = new Quot(true, true, it.No);
											goto end;
										}
									}
								}
							} else {
								foreach(var it in resItems.Reverse<NumberedResItem>()) {
									foreach(var c2 in TextUtil.RowComment2Text(it.Res.Com).Replace("\r", "").Split('\n')) {
										// 引用は捨てる
										if(!Regex.IsMatch(c2, @"^>.*$")) {
											if(c2.Contains(c)) {
												q[i] = new Quot(true, true, it.No);
												goto end;
											}
										}
									}
								}
							}

							q[i] = new Quot(true, false, "");
						end:;
						} else {
							q[i] = new Quot();
						}
					}

					return new Item(url, item) {
						Soudane = soudane,
						QuotLines = q,
					};
				}

				public string HashText => ResItem.Res.Id
					+ ResItem.Res.Del
					+ ResItem.Res.Host
					+ ResItem.Res.Com
					+ ResItem.Res.Thumb
					+ ResItem.Res.Fsize
					+ Soudane;
			}

			public class Quot {
				public bool IsQuot { get; } = false;
				public bool IsHit { get; } = false;
				public string ResNo { get; } = "";

				public Quot() { }

				public Quot(bool isQuot, bool isHit, string resNo) {
					this.IsQuot = isQuot;
					this.IsHit = isHit;
					this.ResNo = resNo;
				}
			}

			public string Name { get; set; }

			public BordData Bord { get; private set; }
			public UrlContext Url { get; private set; }
			public FutabaResonse Raw { get; private set; }

			public Item[] ResItems { get; private set; }

			public long Token { get; private set; }

			private FutabaContext() {
				this.Token = DateTime.Now.Ticks;
			}

			public static FutabaContext FromCatalogEmpty(BordData bord) {
				return new FutabaContext() {
					Name = bord.Name,
					Bord = bord,
					Url = new UrlContext(bord.Url),
					ResItems = new Item[] { },
					Raw = null,
				};
			}

			public static FutabaContext FromCatalogResponse(BordData bord, FutabaResonse response, Data.NumberedResItem[] sortRes, Dictionary<string, int> counter, Data.FutabaContext oldValue) {
				var url = new UrlContext(bord.Url);
				return new FutabaContext() {
					Name = bord.Name,
					Bord = bord,
					Url = url,
					// ResItems = response.Res.Reverse().Select(x => {
					ResItems = sortRes.Select(x => {
						var cc = counter.ContainsKey(x.No) ? counter[x.No] : 0;
						var cp = oldValue?.ResItems.Where(y => y.ResItem.No == x.No).FirstOrDefault()?.CounterCurrent ?? 0;
						return Item.FromCatalog(url, x, cc, cp);
					}).ToArray(),
					Raw = response,
				};
			}

			public static FutabaContext FromThreadEmpty(BordData bord, string threadNo) {
				return new FutabaContext() {
					Name = string.Format("No.{0}", threadNo),
					Bord = bord,
					Url = new UrlContext(bord.Url, threadNo),
					ResItems = new Item[] { },
					Raw = null,
				};
			}

			public static FutabaContext FromThreadResResponse(FutabaContext parent, FutabaResonse response) {
				var list = parent.ResItems?.ToList() ?? new List<Item>();
				// そうだねの更新
				for(var i = 0; i < list.Count; i++) {
					var k = response.Sd.Keys.Where(x => x == list[i].ResItem.No).FirstOrDefault();
					if((k != null) && int.TryParse(response.Sd[k], out var v)) {
						if(list[i].Soudane != v) {
							list[i] = Item.FromThreadRes(v, list[i]);
						}
					}
				}
				// レスの追加
				if(response.Res != null) {
					AddResponseToList(list,
						parent.Url,
						response.IsDie ? RenumberRsc(parent, response.Res) : response.Res,
						response.Sd);
				}
				return new FutabaContext() {
					Name = parent.Name,
					Bord = parent.Bord,
					Url = parent.Url,
					ResItems = list.ToArray(),
					Raw = response,
				};
			}

			public static FutabaContext FromThreadResResponse(BordData bord, string threadNo, FutabaResonse response, Data.NumberedResItem parent, int soudane) {
				var url = new UrlContext(bord.Url, threadNo);
				var list = new List<Item>() { Item.FromThreadRes(url, parent, soudane, new List<NumberedResItem>()) };
				if(response.Res != null) {
					AddResponseToList(list, url, response.Res, response.Sd);
				}
				return new FutabaContext() {
					Name = Util.TextUtil.SafeSubstring(
						Util.TextUtil.RemoveCrLf(
							Util.TextUtil.RowComment2Text(parent.Res.Com)
						), 8),
					Bord = bord,
					Url = url,
					ResItems = list.ToArray(),
					Raw = response,
				};
			}

		public static FutabaContext FromThreadResResponse404(FutabaContext catalog, FutabaContext thread, FutabaResonse response) {
			if(response == null) {
				return null;
			}

			if(thread.ResItems.Length == 0) {
				var c = catalog?.ResItems?.Where(x => x.ResItem.No == thread.Url.ThreadNo).FirstOrDefault()?.ResItem;
				var sub = "MakiMoki";
				var name = "MakiMoki";
				var com = "<font color=\"#ff0000\">取得できませんでした</font>";
				var date = DateTime.Now;
				var now = string.Format("{0}({1}){2}",
					date.ToString("yy/MM/dd", System.Globalization.CultureInfo.InvariantCulture),
					date.ToString("ddd"),
					date.ToString("HH:mm:ss"));
				var tim = new DateTimeOffset(date.Ticks, new TimeSpan(+09, 00, 00)).ToUnixTimeMilliseconds().ToString();
				if(c == null) {
					c = new NumberedResItem(thread.Url.ThreadNo, ResItem.From(
						sub, name, "", com,
						"", "", "", "", "", "", 0, 0, 0, now, tim, 0));
				}
				var ad = new List<Item>() { Item.FromThreadRes(thread.Url, c, 0, new List<NumberedResItem>()) };
				if((response.Res != null) && (0 < response.Res.Length)) {
					/* これはレスが消えたらf.Res.Rscも更新されてできなかったのでお蔵入り
					var i = 1;
					var f = response.Res.First();
					if(i < f.Res.Rsc) {
						ad.Add(new NumberedResItem(thread.Url.ThreadNo, ResItem.From(
							sub, name, "", com,
							"", "", "", "", "", "", 0, 0, 0, now, tim, i)));
						i++;
					}
					*/
					AddResponseToList(ad, thread.Url, response.Res, response.Sd);
				}
				return new FutabaContext() {
					Name = thread.Name,
					Bord = thread.Bord,
					Url = thread.Url,
					ResItems = ad.ToArray(),
					Raw = response,
				};
			} else {
				// 足りないレスを抜き出す
				var ad = new List<NumberedResItem>();
				var list = new List<Item>();
				if(response.Res != null) {
					foreach(var r in response.Res.Reverse()) {
						if(thread.ResItems.Select(x => x.ResItem.No).Contains(r.No)) {
							break;
						} else {
							ad.Insert(0, r);
						}
					}
					AddResponseToList(list, thread.Url, RenumberRsc(thread, ad.ToArray()), response.Sd);
				}

				return new FutabaContext() {
					Name = thread.Name,
					Bord = thread.Bord,
					Url = thread.Url,
					ResItems = thread.ResItems.Concat(list).ToArray(),
					Raw = response,
				};
			}
		}

		private static void AddResponseToList(List<Item> list, UrlContext url, IEnumerable<NumberedResItem> res, Dictionary<string, string> soudane) {
			// ふたばが重いときに重複する可能性があるのでチェックして除去
			foreach(var it in res) {
				foreach(var d in list.Where(x => x.ResItem.No == it.No).ToArray()) {
					list.Remove(d);
				}
			}

			var q = list.Select(x => x.ResItem).ToList();
			foreach(var it in res) {
				var sd = 0;
				if(soudane.TryGetValue(it.No, out var s) && int.TryParse(s, out var v)) {
					sd = v;
				}
				list.Add(Item.FromThreadRes(url, it, sd, q));
				q.Add(it);
			}
		}

		private static NumberedResItem[] RenumberRsc(FutabaContext thread, NumberedResItem[] res) {
			var result = new NumberedResItem[res.Length];
			// rscをふりなおす
			for(var i = 0; i < res.Length; i++) {
				var r = res[i];
				result[i] = new NumberedResItem(
						r.No,
						ResItem.From(
							sub: r.Res.Sub,
							name: r.Res.Name,
							email: r.Res.Email,
							com: r.Res.Com,
							id: r.Res.Id,
							host: r.Res.Host,
							del: r.Res.Del,
							src: r.Res.Src,
							thumb: r.Res.Thumb,
							ext: r.Res.Ext,
							fsize: r.Res.Fsize,
							w: r.Res.W,
							h: r.Res.H,
							now: r.Res.Now,
							tim: r.Res.Tim,
							rsc: thread.ResItems.Length + i));
			}
			return result;
		}

		public static FutabaContext FromCatalog_(BordData bord, FutabaResonse response, string[] sortRes, Dictionary<string, int> counter) {
			var url = new UrlContext(bord.Url);
			var res = new List<NumberedResItem>(response.Res);
			var resItems = sortRes.Select(x => {
				var r = res.Where(y => y.No == x).FirstOrDefault();
				if(r != null) {
					var cc = counter.ContainsKey(x) ? counter[x] : 0;
					res.Remove(r);
					return Item.FromCatalog(url, r, cc, 0);
				} else {
					return null;
				}
			}).Where(x => x != null).ToList();
			resItems.AddRange(res.Select(x => Item.FromCatalog(url, new NumberedResItem(x.No, x.Res, true), 0, 0)));
			return new FutabaContext() {
				Name = bord.Name,
				Bord = bord,
				Url = url,
				ResItems = resItems.ToArray(),
				Raw = response,
			};
		}

		public static FutabaContext FromThread_(BordData bord, UrlContext url, FutabaResonse response) {
			var res = response.Res.FirstOrDefault();
			if(res == null) {
				return FromThreadEmpty(bord, url.ThreadNo);
			} else {
				return new FutabaContext() {
					Name = Util.TextUtil.SafeSubstring(
						Util.TextUtil.RemoveCrLf(
							Util.TextUtil.RowComment2Text(res.Res.Com)
						), 8),
					Bord = bord,
					Url = url,
					// ResItems = response.Res.Reverse().Select(x => {
					ResItems = response.Res.Select(x => {
						var soudane = 0;
						if(response.Sd.TryGetValue(x.No, out var sd) && int.TryParse(sd, out soudane)) {
							// なにもすることがない
						}

						return Item.FromThreadRes(url, x, soudane, response.Res.ToList());
					}).Where(x => x != null).ToArray(),
					Raw = response,
				};
			}
		}

		public FutabaResonse GetFullResponse() {
			if(Raw != null) {
				try {
					//var s = JsonConvert.SerializeObject(Raw);
					var r = JsonConvert.DeserializeObject<FutabaResonse>(Raw.ToString());
					r.Res = ResItems.Select(x => x.ResItem).ToArray();
					return r;
				}
				catch(JsonSerializationException) {
					throw;
				}
			}
			return null;
		}
	}

	public class Information { 
		public string Message { get; }

		public Information(string message) {
			this.Message = message;
		}
	}
}
