using ProtoBuf;

namespace ZeroGravity.Network
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class CheckConnectionMessage : NetworkData
	{
	}
}
