using System.Collections.Generic;
using OpenHellion.Networking;
using UnityEngine;
using UnityEngine.Events;
using ZeroGravity.Data;
using ZeroGravity.Network;
using ZeroGravity.Objects;
using ZeroGravity.UI;

namespace ZeroGravity.LevelDesign
{
	public abstract class BaseSceneTrigger : MonoBehaviour
	{
		public enum AuthorizationType
		{
			none = 0,
			Authorized = 1,
			AuthorizedOrNoSecurity = 2,
			AuthorizedOrFreeSecurity = 3
		}

		public Localization.StandardInteractionTip StandardTip;

		public List<SceneQuestTrigger> SceneQuestTriggers;

		public UnityEvent AuthorizationFailEvent;

		public UnityEvent AuthorizationPassEvent;

		public AbstractGlossaryElement Glossary;

		public AuthorizationType CheckAuthorizationType;

		public abstract SceneTriggerType TriggerType { get; }

		public abstract bool IsNearTrigger { get; }

		public abstract bool IsInteractable { get; }

		public abstract bool CameraMovementAllowed { get; }

		public abstract bool ExclusivePlayerLocking { get; }

		public abstract PlayerHandsCheckType PlayerHandsCheck { get; }

		public abstract List<ItemType> PlayerHandsItemType { get; }

		public Ship ParentShip
		{
			get
			{
				return GetComponentInParent<GeometryRoot>().MainObject as Ship;
			}
		}

		public bool IsAuthorizedOrNoSecurity
		{
			get
			{
				return ParentShip.IsPlayerAuthorizedOrNoSecurity(MyPlayer.Instance);
			}
		}

		public bool IsAuthorizedOrFreeSecurity
		{
			get
			{
				return ParentShip.IsPlayerAuthorizedOrFreeSecurity(MyPlayer.Instance);
			}
		}

		public bool IsAuthorized
		{
			get
			{
				return ParentShip.IsPlayerAuthorized(MyPlayer.Instance);
			}
		}

		public virtual bool CheckAuthorization()
		{
			if (CheckAuthorizationType == AuthorizationType.Authorized)
			{
				return IsAuthorized;
			}
			if (CheckAuthorizationType == AuthorizationType.AuthorizedOrNoSecurity)
			{
				return IsAuthorizedOrNoSecurity;
			}
			if (CheckAuthorizationType == AuthorizationType.AuthorizedOrFreeSecurity)
			{
				return IsAuthorizedOrFreeSecurity;
			}
			return true;
		}

		public virtual bool Interact(MyPlayer player, bool interactWithOverlappingTriggers = true)
		{
			if (!CheckAuthorization())
			{
				Client.Instance.CanvasManager.CanvasUI.Alert(Localization.UnauthorizedAccess.ToUpper());
				NetworkController.Instance.SendToGameServer(new LockToTriggerMessage
				{
					TriggerID = null
				});
				if (AuthorizationFailEvent != null)
				{
					AuthorizationFailEvent.Invoke();
				}
				return false;
			}
			SceneQuestTrigger.Check(base.gameObject, SceneQuestTriggerEvent.Interact);
			if (AuthorizationPassEvent != null)
			{
				AuthorizationPassEvent.Invoke();
			}
			return true;
		}

		public virtual void CancelInteract(MyPlayer player)
		{
		}

		protected virtual void Start()
		{
			SceneQuestTrigger[] componentsInChildren = GetComponentsInChildren<SceneQuestTrigger>();
			foreach (SceneQuestTrigger item in componentsInChildren)
			{
				if (!SceneQuestTriggers.Contains(item))
				{
					SceneQuestTriggers.Add(item);
				}
			}
		}

		protected virtual void OnTriggerEnter(Collider coli)
		{
			if (coli == MyPlayer.Instance.FpsController.GetCollider())
			{
				SceneQuestTrigger.Check(base.gameObject, SceneQuestTriggerEvent.EnterTrigger);
			}
		}

		protected virtual void OnTriggerExit(Collider coli)
		{
			if (coli == MyPlayer.Instance.FpsController.GetCollider())
			{
				SceneQuestTrigger.Check(base.gameObject, SceneQuestTriggerEvent.ExitTrigger);
			}
		}
	}
}
