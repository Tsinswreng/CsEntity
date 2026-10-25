namespace Tsinswreng.CsEntity;

using Tsinswreng.CsCore;
using Tsinswreng.CsTempus;

public interface IBasePoBase
	:I_DelAt
{
	[Doc("存入數據庫之時")]
	public UnixMs DbCreatedAt { get; set; }
	[Doc("在數據庫中被更新之時")]
	public UnixMs DbUpdatedAt { get; set; }
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



public static class IBasePoBaseExtn{
	extension<T>(T z)
		where T:IBasePoBase
	{
		
	}
	
	public static bool IsDeleted(this IBasePoBase z){
		return !z.DelAt.IsNullOrDefault();
	}
	
	[Doc(@$"從業務層之 修改時間 與 被軟先除之時間 取 最晚近者。
	因 軟刪除實體時、實體的 {nameof(IBizCreateUpdateTime.BizUpdatedAt)}可能不變。
	")]
	public static UnixMs GetNewestBizUpdOrDelTime<TSelf>(
		this TSelf z
	)where TSelf:IBizCreateUpdateTime, I_DelAt{
		i64 u = z.BizUpdatedAt;
		i64 d = z.DelAt;
		return Math.Max(u,d);
	}
	
}
