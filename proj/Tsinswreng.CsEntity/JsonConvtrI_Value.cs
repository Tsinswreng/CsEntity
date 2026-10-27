namespace Tsinswreng.CsEntity;

using System.Text.Json;
using System.Text.Json.Serialization;
using Tsinswreng.CsCore;

[Doc($$"""
把「只包一個底層值」的型別（{{nameof(I_Value<>)}} 的實作者）寫成裸值——線上式就是它的 `Value`。

*為何要有它*：這類包裝型別（時間戳、狀態、計數…）若照預設序列化會變成 `{"Value": …}`；
而線上式要的就是那一個裸值。轉換規則只有一條：*直接取 `Value`*，不另造約定。

*與 {{nameof(JsonConvtrU128Id<>)}} 的分工*：那個處理「線上式 ≠ 底層值」的 Id（`UInt128` → Low64Base 字串）；
這個處理「線上式 ＝ 底層值」的包裝型別。兩者都不需要型別自己實作任何序列化接口。

*註冊*：轉換器是泛型的，兩個型別參數都要寫出來，例如
`new JsonConvtrI_Value<UnixMs, i64>()` 掛進 `JsonSerializerOptions.Converters`。

#Eg[
	```cs
	// UnixMs 是 i64 的包裝，註冊後 JSON 裏就是裸數字
	new JsonSerializerOptions{ Converters = { new JsonConvtrI_Value<UnixMs, i64>() } };
	// 得到 {"DbCreatedAt":1730000000000}，而不是再包一層 Value 物件
	```
]
""")]
public sealed class JsonConvtrI_Value<T, TPrim> : JsonConverter<T>
	where T : struct, I_Value<TPrim>
{
	[Doc($$"""
	從 JSON 讀回包裝值：把裸值讀成 {{nameof(TPrim)}}，再塞進一個零值實例的 `Value`。

	#Prm[{{nameof(Reader)}}][讀取器。]
	#Rtn[還原後的包裝值；JSON 是 `null` 時回 `default`。]
	#Throw[{{nameof(JsonException)}}][裸值的形狀與 {{nameof(TPrim)}} 不符。]
	""")]
	public sealed override T Read(ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options){
		// 先取零值再寫 Value：struct 沒有「帶值構造」的通道，default(T) 正好是零值。
		var Ans = default(T);
		if(Reader.TokenType == JsonTokenType.Null){
			return Ans;
		}
		var Raw = JsonSerializer.Deserialize<TPrim>(ref Reader, Options);
		if(Raw is not null){
			Ans.Value = Raw;
		}
		return Ans;
	}

	[Doc($$"""
	把包裝值寫成裸值：直接寫 `Value`，不加外殼。

	#Prm[{{nameof(Writer)}}][寫入器。]
	#Prm[{{nameof(Value)}}][要寫出的包裝值。]
	""")]
	public sealed override void Write(Utf8JsonWriter Writer, T Value, JsonSerializerOptions Options){
		JsonSerializer.Serialize<TPrim>(Writer, Value.Value, Options);
	}
}
