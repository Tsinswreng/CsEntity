namespace Tsinswreng.CsEntity.CsSql;

using Tsinswreng.CsSql;
using Tsinswreng.CsEntity;
using Tsinswreng.CsTempus;

[Doc("建表相關的共用配置：主鍵列名、三共同列的映射、軟刪列的 SQL 片段。")]
public static class TblCfg {
	public static IUpperTypeMapFnT<i64,UnixMs> MapUnixMs(){
		return UpperTypeMapFnT<i64, UnixMs>.Mk(
			raw=>new UnixMs(raw)
			,tempus=>tempus.Value
		);
	}
	public static IUpperTypeMapFnT<i64?,UnixMs?> MapUnixMsN(){
		return UpperTypeMapFnT<i64?, UnixMs?>.Mk(
			val=>val==null?null:new UnixMs(val.Value)
			,tempus=>tempus?.Value
		);
	}
	public static IUpperTypeMapFnT<str, Version> MapVersion(){
		return UpperTypeMapFnT<str, Version>.Mk(
			raw => Version.Parse(raw)
			,ver => ver.ToString()
		);
	}
	
	[Doc($$"""
	配置 {nameof(IPoBase)} 的三個共同列，並把軟刪語義接到 CsSql 上。

	*為何軟刪要在這裡接*：CsSql 的 `WhereNonDel()`／`SqlIsNonDel()` 全靠表上的
	`SoftDelCol`；不配它的話查詢會把已刪的行一起返回，這是最容易漏、也最難察覺的錯。
	#Prm[{{nameof(Setter)}}][目標表的配置器。]
	#Rtn[同一個配置器，便於串接。]
	""")]
	public static ITblSetter<T> CfgPoBase<T>(ITblSetter<T> Setter)
		where T : IBasePoBase, new()
	{
		var Tbl = Setter.Tbl;
		// 1: 主鍵在代碼裡一律叫 Id；數據庫列名也用它。
		Tbl.CodeIdName = nameof(I_Id<nil>.Id);
		Tbl.Col(nameof(I_Id<nil>.Id)).AdditionalSqls(["PRIMARY KEY"]);

		// 2: 三共同列都是 i64，不需要額外型別映射。
		Tbl.Col(nameof(IBasePoBase.DbCreatedAt));
		Tbl.Col(nameof(IBasePoBase.DbUpdatedAt));
		Tbl.Col(nameof(IBasePoBase.DelAt));

		// 3: 接上軟刪：刪除＝把 DelAt 寫成當前時刻，恢復＝寫回 0。
		Tbl.SoftDelCol = new SoftDelol {
			CodeColName = nameof(IBasePoBase.DelAt)
			, FnDelete = (_) => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
			, FnRestore = (_) => 0L
			, FnSqlIsDel = () => Tbl.QtCol<IBasePoBase>(x => x.DelAt) + " <> 0"
			, FnSqlIsNonDel = () => Tbl.QtCol<IBasePoBase>(x => x.DelAt) + " = 0"
		};
		return Setter;
	}
	
	public static ITblSetter<T> CfgBizCreateUpdateTime<T>(ITblSetter<T> Tbl)
		where T:IBizCreateUpdateTime
	{
		var o = Tbl;
		o.Col(nameof(IBizCreateUpdateTime.BizCreatedAt)).MapType(MapUnixMs());
		o.Col(nameof(IBizCreateUpdateTime.BizUpdatedAt)).MapType(MapUnixMs());
		return o;
	}
}
