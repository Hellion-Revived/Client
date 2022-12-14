using System.Collections.Generic;
using UnityEngine;
using ZeroGravity.LevelDesign;
using ZeroGravity.Math;
using ZeroGravity.Network;
using ZeroGravity.ShipComponents;

namespace ZeroGravity.Objects
{
	public class ArtificialBody : SpaceObject
	{
		public ManeuverData Maneuver;

		public bool ManeuverExited;

		private CelestialBody _ParentCelesitalBody;

		public VesselDestructionEffects DestructionEffects;

		private RadarVisibilityType _RadarVisibilityType;

		public List<VesselRequestButton> VesselRequestButtons = new List<VesselRequestButton>();

		public ArtificialBody StabilizeToTargetObj;

		public Vector3D StabilizationOffset;

		public HashSet<ArtificialBody> StabilizedChildren = new HashSet<ArtificialBody>();

		public virtual OrbitParameters Orbit { get; set; }

		public virtual CelestialBody ParentCelesitalBody
		{
			get
			{
				return Orbit.Parent.CelestialBody;
			}
		}

		public double Radius { get; protected set; }

		public override Vector3D Velocity
		{
			get
			{
				return Orbit.Velocity;
			}
		}

		public override Vector3D Position
		{
			get
			{
				return Orbit.Position;
			}
		}

		public RadarVisibilityType RadarVisibilityType
		{
			get
			{
				if (IsAlwaysVisible)
				{
					return RadarVisibilityType.AlwaysVisible;
				}
				return (!IsDistressSignalActive) ? _RadarVisibilityType : RadarVisibilityType.Distress;
			}
			set
			{
				_RadarVisibilityType = value;
				MapObject value2;
				if (this is IMapMainObject && Client.Instance.Map.AllMapObjects.TryGetValue(this as IMapMainObject, out value2))
				{
					value2.UpdateVisibility();
				}
			}
		}

		public virtual bool IsDistressSignalActive
		{
			get
			{
				return false;
			}
		}

		public virtual bool IsAlwaysVisible
		{
			get
			{
				return false;
			}
		}

		public virtual double RadarSignature
		{
			get
			{
				return 0.0;
			}
		}

		public bool IsStabilized
		{
			get
			{
				return StabilizeToTargetObj != null;
			}
		}

		public static ArtificialBody Create(SpaceObjectType type, long guid, ObjectTransform trans, bool isMainObject)
		{
			GameObject gameObject = new GameObject(type.ToString() + "_" + guid);
			ArtificialBody artificialBody = null;
			switch (type)
			{
			case SpaceObjectType.Ship:
				artificialBody = gameObject.AddComponent<Ship>();
				break;
			case SpaceObjectType.Asteroid:
				artificialBody = gameObject.AddComponent<Asteroid>();
				break;
			case SpaceObjectType.PlayerPivot:
			case SpaceObjectType.DynamicObjectPivot:
			case SpaceObjectType.CorpsePivot:
				artificialBody = gameObject.AddComponent<Pivot>();
				break;
			case SpaceObjectType.Station:
				artificialBody = gameObject.AddComponent<Station>();
				break;
			}
			if (type == SpaceObjectType.Ship || type == SpaceObjectType.Asteroid)
			{
				artificialBody.gameObject.SetActive(false);
			}
			artificialBody.GUID = guid;
			artificialBody.Orbit = new OrbitParameters();
			artificialBody.Orbit.SetArtificialBody(artificialBody);
			artificialBody.Radius = 30.0;
			if (trans.Orbit != null)
			{
				artificialBody.Orbit.ParseNetworkData(trans.Orbit);
			}
			else if (trans.Realtime != null)
			{
				artificialBody.Orbit.ParseNetworkData(trans.Realtime);
			}
			else if (trans.StabilizeToTargetGUID.HasValue && trans.StabilizeToTargetGUID.Value > 0)
			{
				ArtificialBody artificialBody2 = Client.Instance.SolarSystem.GetArtificialBody(trans.StabilizeToTargetGUID.Value);
				if (artificialBody2 != null)
				{
					artificialBody.Orbit.CopyDataFrom(artificialBody2.Orbit, Client.Instance.SolarSystem.CurrentTime, true);
				}
			}
			else
			{
				Dbg.Error("How this happend !!! Artificial bodies should always have orbit or realtime data.");
			}
			artificialBody.Forward = ((trans.Forward == null) ? Vector3.forward : trans.Forward.ToVector3());
			artificialBody.Up = ((trans.Up == null) ? Vector3.up : trans.Up.ToVector3());
			artificialBody.TransferableObjectsRoot = new GameObject("TransferableObjectsRoot");
			artificialBody.TransferableObjectsRoot.transform.parent = artificialBody.transform;
			artificialBody.TransferableObjectsRoot.transform.Reset();
			artificialBody.ConnectedObjectsRoot = new GameObject("ConnectedObjectsRoot");
			artificialBody.ConnectedObjectsRoot.transform.parent = artificialBody.transform;
			artificialBody.ConnectedObjectsRoot.transform.Reset();
			if (type == SpaceObjectType.Asteroid || type == SpaceObjectType.Ship || type == SpaceObjectType.Station)
			{
				artificialBody.GeometryPlaceholder = new GameObject("GeometryPlaceholder");
				artificialBody.GeometryPlaceholder.transform.parent = artificialBody.transform;
				artificialBody.GeometryPlaceholder.transform.Reset();
				artificialBody.GeometryRoot = new GameObject("GeometryRoot");
				GeometryRoot geometryRoot = artificialBody.GeometryRoot.AddComponent<GeometryRoot>();
				artificialBody.GeometryRoot.transform.parent = artificialBody.GeometryPlaceholder.transform;
				artificialBody.GeometryRoot.transform.Reset();
				geometryRoot.MainObject = artificialBody;
				artificialBody.TransferableObjectsRoot.transform.parent = artificialBody.GeometryPlaceholder.transform;
				artificialBody.TransferableObjectsRoot.transform.Reset();
			}
			if (isMainObject)
			{
				artificialBody.transform.parent = null;
				artificialBody.SetTargetPositionAndRotation(Vector3.zero, artificialBody.Forward, artificialBody.Up, true);
				artificialBody.transform.Reset();
			}
			else
			{
				artificialBody.transform.parent = Client.Instance.ShipExteriorRoot.transform;
				artificialBody.SetTargetPositionAndRotation((artificialBody.Position - MyPlayer.Instance.Parent.Position).ToVector3(), artificialBody.Forward, artificialBody.Up, true);
				if ((artificialBody.Position - MyPlayer.Instance.Parent.Position).SqrMagnitude < 100000000.0)
				{
					artificialBody.LoadGeometry();
				}
				else if (type == SpaceObjectType.Asteroid || type == SpaceObjectType.Ship || type == SpaceObjectType.Station)
				{
					artificialBody.RequestSpawn();
				}
			}
			Client.Instance.SolarSystem.AddArtificialBody(artificialBody);
			return artificialBody;
		}

		public void CreateArtificalRigidbody()
		{
		}

		public override void DestroyGeometry()
		{
			base.DestroyGeometry();
			if (GeometryRoot != null)
			{
				if (this is SpaceObjectVessel)
				{
					ZeroOcclusion.DestroyOcclusionObjectsFor(this as SpaceObjectVessel);
				}
				foreach (Transform child in GeometryRoot.transform.GetChildren())
				{
					if (child != null)
					{
						Object.Destroy(child.gameObject);
					}
				}
				GeometryRoot.transform.parent = GeometryPlaceholder.transform;
				GeometryRoot.transform.Reset();
			}
			if (ArtificalRigidbody != null)
			{
				Object.Destroy(ArtificalRigidbody);
			}
			ArtificalRigidbody = null;
			base.IsDummyObject = true;
		}

		public void UpdateOrbitPosition(double time, bool resetTime = false)
		{
			if (Maneuver != null || StabilizeToTargetObj != null)
			{
				return;
			}
			if (resetTime)
			{
				Orbit.ResetOrbit(time);
			}
			else
			{
				Orbit.UpdateOrbit(time);
			}
			if (StabilizedChildren == null || StabilizedChildren.Count <= 0)
			{
				return;
			}
			foreach (ArtificialBody stabilizedChild in StabilizedChildren)
			{
				stabilizedChild.UpdateStabilizedPosition();
			}
		}

		public override void OnSubscribe()
		{
		}

		public override void OnUnsubscribe()
		{
			TransferableObjectsRoot.DestroyAll(true);
		}

		public static ArtificialBody CreateArtificialBody(ObjectTransform trans)
		{
			if (trans.Type == SpaceObjectType.Ship)
			{
				return Ship.Create(trans.GUID, null, trans, false);
			}
			if (trans.Type == SpaceObjectType.PlayerPivot || trans.Type == SpaceObjectType.DynamicObjectPivot || trans.Type == SpaceObjectType.CorpsePivot)
			{
				return Pivot.Create(trans.Type, trans, false);
			}
			if (trans.Type == SpaceObjectType.Asteroid)
			{
				return Asteroid.Create(trans, null, false);
			}
			Dbg.Error("Unknown artificial body type", trans.Type, trans.GUID);
			return null;
		}

		public void UpdateStabilizedPosition()
		{
			if (!(StabilizeToTargetObj == null))
			{
				Orbit.CopyDataFrom(StabilizeToTargetObj.Orbit, Client.Instance.SolarSystem.CurrentTime, true);
				Orbit.RelativePosition += StabilizationOffset;
				Orbit.InitFromCurrentStateVectors(Client.Instance.SolarSystem.CurrentTime);
			}
		}

		public virtual void OnStabilizationChanged(bool isStabilized)
		{
		}

		public void StabilizeToTarget(long guid, Vector3D stabilizationOffset)
		{
			if (StabilizeToTargetObj != null && StabilizeToTargetObj.GUID != guid)
			{
				StabilizeToTargetObj.StabilizedChildren.Remove(this);
			}
			StabilizeToTargetObj = null;
			if (guid > 0)
			{
				StabilizeToTargetObj = Client.Instance.SolarSystem.GetArtificialBody(guid);
			}
			if (StabilizeToTargetObj != null)
			{
				StabilizationOffset = stabilizationOffset;
				StabilizeToTargetObj.StabilizedChildren.Add(this);
				OnStabilizationChanged(true);
			}
		}

		public void DisableStabilization()
		{
			if (!(StabilizeToTargetObj == null))
			{
				StabilizeToTargetObj.StabilizedChildren.Remove(this);
				StabilizeToTargetObj = null;
				OnStabilizationChanged(false);
			}
		}
	}
}
