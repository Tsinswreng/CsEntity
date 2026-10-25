using Tsinswreng.CsTreeTest;
using Tsinswreng.CsEntity.Test.Domains.Calculator;
namespace Tsinswreng.CsEntity.Test;

public class Tsinswreng.CsEntityTestMgr:DiEtTestMgr{
	public static Tsinswreng.CsEntityTestMgr Inst = new();
	public override ITestNode RegisterTestsInto(ITestNode? Node){
		Node = this.TestNode;
		this.RegisterTester<TestCalculator>();
		return Node;
	}
}
