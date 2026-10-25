namespace Tsinswreng.CsEntity;

using Tsinswreng.CsTempus;

public interface IBasePoBase{
	[Doc("存入數據庫之時")]
	public UnixMs DbCreatedAt { get; set; }
	[Doc("在數據庫中被更新之時")]
	public UnixMs DbUpdatedAt { get; set; }

	[Doc($$"""
	軟刪時間。`0` 表示未刪，非 `0` 表示已刪。
	*為何用時間而不是布爾*：刪除時刻本身是排錯與審計所需的信息；
	而且「未刪」用 0 表示，索引條件 `DelAt = 0` 可以直接走索引。
	""")]
	public UnixMs DelAt { get; set; }
	public object ShallowCloneSelf();
}

public interface IBizCreateUpdateTime{
	#region IBizCreateUpdateTime
	/// 理則ₐ實體ˇ增ʹ時、如于單詞、則始記于文本單詞表中之時 即其CreatedAt、非 存入數據庫之時
	/// 潙null旹示與InsertedBy同。亦可早於InsertedAt。
	[Doc(@$"業務層的 創建時間,
	異于{nameof(IBasePoBase.DbCreatedAt)}
	")]
	public UnixMs BizCreatedAt{get;set;}
	#if Impl
		= new();
	#endif
	/// 理則ₐ實體ˇ改ʹ時
	/// 如ʃ有ʹ子實體ˋ變˪、則亦宜改主實體或聚合根ʹUpdatedAt
	public UnixMs BizUpdatedAt{get;set;}
	#if Impl
		= UnixMs.Zero;
	#endif

	#endregion IBizCreateUpdateTime

}

public static class IBizCreateUpdateTimeExtn{
	extension(IBizCreateUpdateTime z){
		public UnixMs LatestBizTime{
			get{
				if(z.BizUpdatedAt.IsNullOrDefault()){
					return z.BizCreatedAt;
				}
				return z.BizUpdatedAt;
			}
		}
	}
	extension<TPo>(TPo z)
		where TPo : IBizCreateUpdateTime
	{
		public TPo Touch(UnixMs? NeoUpdTime = null){
			NeoUpdTime??=UnixMs.Now();
			z.BizUpdatedAt = NeoUpdTime.Value;
			return z;
		}
	}
	extension<TPo>(IEnumerable<TPo> z)
		where TPo : IBizCreateUpdateTime
	{
		public IEnumerable<TPo> Touch(UnixMs? NeoUpdTime = null){
			return z.Select(x=>{
				x.Touch(NeoUpdTime);
				return x;
			});
		}
	}
	
	extension<TPo>(IAsyncEnumerable<TPo> z)
		where TPo : IBizCreateUpdateTime
	{
		public IAsyncEnumerable<TPo> Touch(UnixMs? NeoUpdTime = null){
			return z.Select(x=>{
				x.Touch(NeoUpdTime);
				return x;
			});
		}
	}
}


[Doc($$"""
{nameof(IPoBase)} 的默認實現。實體繼承它即可獲得三個共同列。
#Eg[
	```cs
	public partial class PoUser : PoBase, I_Id<IdUser> {
		public IdUser Id { get; set; } = new();
	}
	```
]
""")]
public class BasePoBase : IBasePoBase {
	public UnixMs DbCreatedAt { get; set; } = new();

	public UnixMs DbUpdatedAt { get; set; } = new();

	public UnixMs DelAt { get; set; }

	public object ShallowCloneSelf(){
		return MemberwiseClone();
	}

}

