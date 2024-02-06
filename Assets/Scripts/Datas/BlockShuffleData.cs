using Unity.Entities;

namespace Datas
{
	public struct BlockShuffleData : IComponentData
	{
		public int TargetColumn;
		public int TargetRow;
	} 
}
