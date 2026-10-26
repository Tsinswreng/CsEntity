namespace Tsinswreng.CsEntity;

using System.Text.Json;
using System.Text.Json.Serialization;
using Tsinswreng.CsCore;
using Tsinswreng.CsU128Id;


[Doc($$"""
把強類型 Id 寫成裸 Low64Base 字串的轉換器——本應用的線上式約定就這一處。

*為何不在 Id 上實作鉤子*：那會讓領域值型別知道序列化層存在。改成轉換器之後，Id 只是普通的
強類型值，誰把它寫上網線、誰負責決定格式。

*為何一個泛型類就能蓋住全部 Id*：本應用每個 Id 都經
`IAppIdU128 : {{nameof(Tsinswreng.CsEntity.IIdU128)}}`，所以 `T : struct, IIdU128` 一個轉換器
就覆蓋全部；格式則沿用 CsEntity 的既有約定——`ToString()` 給 Low64Base、
{{nameof(U128Id.Low64BaseToUInt128)}} 還原。

*誰註冊它*：`proj/TswgCsTpl.Scripts` 的 `GenAppJsonCtx` 掃
{{nameof(Tsinswreng.CsEntity.IIdU128)}} 的實作者，逐個產 `new JsonConvtrIdU128<IdXxx>()`
掛進 AppJsonCtx 的轉換器清單。故新增 Id 時不必手動登記。

#Eg[
	```cs
	AppJson.Inst.Serialize(new ResUser{ Id = new IdUser() });
	// 得到 {"Id":"01J…"} —— 裸字串，不是包一層 Value 物件
	```
]
""")]
public sealed class JsonConvtrU128Id<T> : JsonConverter<T>
	where T : struct, IIdU128
{
	[Doc($$"""
	從 JSON 讀回 Id。

	*只認字串*：線上式就是 Low64Base 字串，`null` 給 `default`；其餘形狀交給
	{nameof(U128Id.Low64BaseToUInt128)} 去拋，不在這裡擅自降級。

	#Prm[{{nameof(Reader)}}][讀取器。]
	#Rtn[還原後的 Id。]
	#Throw[{{nameof(FormatException)}}][字串不是合法 Low64Base 時由 CsU128Id 拋出。]
	""")]
	public sealed override T Read(ref Utf8JsonReader Reader, Type TypeToConvert, JsonSerializerOptions Options){
		var S = Reader.GetString();
		if(S is null){
			return default;
		}
		// 先取零值再寫 Value：struct 沒有「帶值構造」的通道，default(T) 正好是零值。
		var Ans = default(T);
		Ans.Value = U128Id.Low64BaseToUInt128(S);
		return Ans;
	}

	[Doc($$"""
	把 Id 寫成 Low64Base 字串。

	#Prm[{{nameof(Writer)}}][寫入器。]
	#Prm[{{nameof(Value)}}][要寫出的 Id。]
	""")]
	public sealed override void Write(Utf8JsonWriter Writer, T Value, JsonSerializerOptions Options){
		// 與 Id 自己的 ToString() 同一個口徑，避免顯示形態與線上式分岔。
		Writer.WriteStringValue(U128Id.ToLow64Base(Value.Value));
	}
}
