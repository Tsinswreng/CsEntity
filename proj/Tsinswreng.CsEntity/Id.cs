namespace Tsinswreng.CsEntity;
using Tsinswreng.CsU128Id;
using Tsinswreng.CsSql;

[Doc(@$"強類型Id標記接口
TPrm[{nameof(T)}][具體的Id類型]
")]
public partial interface IStrongId<T>: I_Value<T>{

}

[Doc("UInt128 Id 標記接口")]
public partial interface IIdU128
	:IStrongId<UInt128>
{

}

public static class IIdU128Extn{
	extension<T>(T z)
		where T:struct, IIdU128
	{
		public static T Zero => default!;
		public static T FromByteArr(byte[] bytes){
			var Num = U128Id.ByteArrToUInt128(bytes);
			return new T(){Value = Num};
		}
		public static IUpperTypeMapFnT<u8[], T> MkTypeMapFn(){
			return UpperTypeMapFnT<u8[], T>.Mk(
				//(raw)=>T.FromByteArr(raw)
				(raw)=>FromByteArr<T>(raw)
				,(upper)=>upper.Value.ToByteArr()
			);
		}
		
		[Doc(@$"從64進制數字字串解析Id。
		64進制不是Base64
		")]
		public static T FromLow64Base(string Low64Base){
			var Num = U128Id.Low64BaseToUInt128(Low64Base);
			return new T(){Value = Num};
		}
		public static bool operator ==(T left, T right)
				=> left.Value == right.Value;
				
		public static bool operator !=(T left, T right)
				=> !(left == right);
				
		
		[Doc("嘗試從字串解釋")]
		public static bool TryParse(string? S, out T R){
			if(S is null){
				R = default!;
				return false;
			}
			if(U128Id.TryLow64BaseToUInt128(S, out var Num)){
				R = new T(){Value = Num};
				return true;
			}
			R = default!;
			return false;
		}
		
		[Doc(@$"
		#Throw[{nameof(ArgumentException)}][解析失敗時]
		")]
		public static T Parse(string S){
			return FromLow64Base<T>(S);
		}
	}
}
