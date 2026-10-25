using Tsinswreng.CsTempus;

namespace Tsinswreng.CsEntity;

public interface I_DelAt{
	[Doc($$"""
	軟刪時間。`0` 表示未刪，非 `0` 表示已刪。
	*為何用時間而不是布爾*：刪除時刻本身是排錯與審計所需的信息；
	而且「未刪」用 0 表示，索引條件 `DelAt = 0` 可以直接走索引。
	""")]
	public UnixMs DelAt{get;set;}
}
