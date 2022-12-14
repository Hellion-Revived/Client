using System;
using ProtoBuf;

namespace ZeroGravity.Data
{
	[Serializable]
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ResourcesSpawnSettings : ISceneData
	{
		public SpawnSettingsCase Case;

		public string Tag;

		public float MinQuantity;

		public float MaxQuantity;
	}
}
