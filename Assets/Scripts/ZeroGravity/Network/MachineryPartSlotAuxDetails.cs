using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class MachineryPartSlotAuxDetails : IAuxDetails
	{
		public bool IsActive;
	}
}
