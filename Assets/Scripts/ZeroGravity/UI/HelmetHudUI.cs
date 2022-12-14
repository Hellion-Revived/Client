using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using ZeroGravity.Data;
using ZeroGravity.LevelDesign;
using ZeroGravity.Objects;

namespace ZeroGravity.UI
{
	public class HelmetHudUI : MonoBehaviour
	{
		[CompilerGenerated]
		private sealed class _003CUpdateQuickSlots_003Ec__AnonStorey0
		{
			internal List<ItemType> gre;

			internal List<ItemType> con;

			internal bool _003C_003Em__0(InventorySlot m)
			{
				return m.Item != null && gre.Contains(m.Item.Type);
			}

			internal bool _003C_003Em__1(InventorySlot m)
			{
				return m.Item != null && con.Contains(m.Item.Type);
			}
		}

		[CompilerGenerated]
		private sealed class _003CWarningsUpdate_003Ec__AnonStorey1
		{
			internal SceneTriggerRoom currentRoom;

			internal bool _003C_003Em__0(SceneTriggerRoom m)
			{
				return m.CompoundRoomID == currentRoom.CompoundRoomID;
			}
		}

		[Header("Main")]
		public GameObject HelmetRoot;

		public GameObject Active;

		public GameObject GeneralInfo;

		public GameObject JetpackInfo;

		public HelmetOverlayModel HelmetOverlayModel;

		[Header("Quick switch")]
		public GameObject QuickSlotsHolder;

		public GameObject PrimarySlot;

		public GameObject SecondarySlot;

		public GameObject GranadeSlot;

		public GameObject ConsumableSlot;

		[Header("General info")]
		public Text PressureValue;

		public Text HealthValue;

		public Image HeartBeat;

		public Animator Hb;

		public Image Armor;

		public Image HandsSlotIcon;

		public Sprite HandsSlotSprite;

		public GameObject ItemQuantity;

		public Image QuantityFiller;

		public Text ItemInfo;

		public GameObject WeaponInfo;

		public Image FireMod;

		[Header("Battery")]
		public GameObject BatteryMissing;

		public GameObject BatteryDetails;

		public Image PowerFiller;

		public GameObject LightsOn;

		[Header("Jetpack")]
		public GameObject JetpackHud;

		public GameObject NoJetpack;

		public GameObject JetpackDetails;

		public Text OxygenValue;

		public Image OxygenFiller;

		public Text RcsValue;

		public Image RCSFiller;

		public GameObject StabilityOn;

		public GameObject RCSOn;

		public GameObject RcsActive;

		public Text RCSDisabled;

		public Text ToggleTargetingInfo;

		public GameObject ZeroGravityTips;

		[Header("Warnings")]
		public GameObject WarningsHolder;

		public GameObject DebrisWarning;

		public GameObject OxygenWarning;

		public GameObject RcsWarning;

		public GameObject BatteryWarning;

		public GameObject RadiationWarning;

		public GameObject Breach;

		public GameObject Fire;

		public GameObject Gravity;

		[Header("Radar")]
		public RadarUI Radar;

		[Header("Targeting")]
		public GameObject TargetInfo;

		public Text TargetInfoName;

		public Helmet CurrentHelmet
		{
			get
			{
				return (!(MyPlayer.Instance != null)) ? null : MyPlayer.Instance.CurrentHelmet;
			}
		}

		private float currentPressure
		{
			get
			{
				if (CurrentHelmet == null)
				{
					if (MyPlayer.Instance.CurrentRoomTrigger == null)
					{
						return 0f;
					}
					return MyPlayer.Instance.CurrentRoomTrigger.AirPressure;
				}
				return CurrentHelmet.Pressure;
			}
		}

		public bool ArmorOk
		{
			get
			{
				if (MyPlayer.Instance.CurrentOutfit != null)
				{
					if (MyPlayer.Instance.CurrentOutfit.Armor > Client.Instance.GetPlayerExposureDamage(MyPlayer.Instance.GetParent<SpaceObject>().Position.Magnitude))
					{
						return true;
					}
					return false;
				}
				return false;
			}
		}

		public Item CurrentItem
		{
			get
			{
				if (MyPlayer.Instance.Inventory.HandsSlot.Item == null)
				{
					return null;
				}
				return MyPlayer.Instance.Inventory.HandsSlot.Item;
			}
		}

		public bool UiActive
		{
			get
			{
				return !MyPlayer.Instance.IsLockedToTrigger;
			}
		}

		private void Start()
		{
			ToggleTargetingInfo.text = string.Format(Localization.PressToToggleTargeting.ToUpper(), InputManager.GetAxisKeyName(InputManager.AxisNames.X));
			UpdateQuickSlots();
		}

		private void Update()
		{
			if (Radar.CanRadarWork && Radar.AllTargets.Count > 0 && InputManager.GetButtonDown(InputManager.AxisNames.X))
			{
				Radar.ToggleTargeting(!Radar.IsActive);
			}
			if (MyPlayer.Instance.CurrentRoomTrigger == null && (Breach.activeInHierarchy || Fire.activeInHierarchy || Gravity.activeInHierarchy))
			{
				WarningsUpdate();
			}
			UpdateUI();
		}

		public void UpdateUI()
		{
			if (CurrentHelmet == null)
			{
				Active.Activate(false);
				JetpackInfo.Activate(false);
				HelmetRoot.Activate(false);
				if (MyPlayer.Instance.FpsController.StarDustParticle != null && MyPlayer.Instance.FpsController.StarDustParticle.gameObject.activeInHierarchy)
				{
					TurnOffStardust();
				}
				return;
			}
			HelmetRoot.Activate(true);
			Active.Activate(CurrentHelmet.IsVisorActive && UiActive);
			if (HelmetOverlayModel.gameObject.activeSelf)
			{
				HelmetOverlayModel.Animator.SetBool("VisorRaise", !CurrentHelmet.IsVisorActive);
				HelmetOverlayModel.UpdateHelmetOverlay(CurrentHelmet.HelmetOverlay);
			}
			if (CurrentHelmet.Battery != null)
			{
				BatteryMissing.Activate(false);
				BatteryDetails.Activate(true);
				PowerFiller.fillAmount = CurrentHelmet.BatteryPower;
				if (CurrentHelmet.BatteryPower < 0.7f && CurrentHelmet.BatteryPower > 0.3f)
				{
					PowerFiller.color = Colors.YellowDark;
				}
				else if (CurrentHelmet.BatteryPower <= 0.3f)
				{
					PowerFiller.color = Colors.Red50;
				}
				else
				{
					PowerFiller.color = Colors.GreenNavColor;
				}
				BatteryWarning.Activate(CurrentHelmet.BatteryPower < 0.21f);
				if (CurrentHelmet.BatteryPower > 0f)
				{
					GeneralInfo.Activate(true);
					HandsUI();
					JetpackDetailsUI();
					WarningsHolder.Activate(BatteryWarning.activeSelf || Breach.activeSelf || Fire.activeSelf || Gravity.activeSelf || DebrisWarning.activeSelf || OxygenWarning.activeSelf || RcsWarning.activeSelf || RadiationWarning.activeSelf);
				}
				else
				{
					GeneralInfo.Activate(false);
					JetpackInfo.Activate(false);
					WarningsHolder.Activate(false);
				}
			}
			else
			{
				GeneralInfo.Activate(false);
				JetpackInfo.Activate(false);
				BatteryDetails.Activate(false);
				WarningsHolder.Activate(false);
				BatteryMissing.Activate(true);
			}
		}

		public void HandsUI()
		{
			PressureValue.text = currentPressure.ToString("0.0") + " BAR";
			if (currentPressure < 0.4f)
			{
				PressureValue.color = Colors.FormatedRed;
			}
			else
			{
				PressureValue.color = Colors.White;
			}
			Hb.speed = 3f - (float)MyPlayer.Instance.Health / 100f * 2f;
			if (MyPlayer.Instance.Health < 70 && MyPlayer.Instance.Health > 30)
			{
				HeartBeat.color = Colors.Yellow;
			}
			else if (MyPlayer.Instance.Health <= 30)
			{
				HeartBeat.color = Colors.FormatedRed;
			}
			else
			{
				HeartBeat.color = Colors.White;
			}
			if (MyPlayer.Instance.CurrentOutfit != null)
			{
				if (ArmorOk)
				{
					Armor.color = Colors.ArmorActive;
				}
				else
				{
					Armor.color = Colors.FormatedRed;
				}
			}
			else
			{
				Armor.color = Colors.FormatedRed;
			}
		}

		private void JetpackDetailsUI()
		{
			JetpackInfo.Activate(true);
			if (MyPlayer.Instance.FpsController.CurrentJetpack == null)
			{
				NoJetpack.Activate(true);
				JetpackDetails.Activate(false);
				return;
			}
			NoJetpack.Activate(false);
			JetpackDetails.Activate(true);
			JetpackHud.Activate(Radar.CanRadarWork && CurrentHelmet.IsVisorActive && UiActive);
			Radar.gameObject.Activate(Radar.CanRadarWork && CurrentHelmet.IsVisorActive);
			ToggleTargetingInfo.gameObject.Activate(Radar.CanRadarWork && CurrentHelmet.IsVisorActive);
			if ((bool)MyPlayer.Instance.LookingAtPlayer)
			{
				ShowTargetPlayer(MyPlayer.Instance.LookingAtPlayer);
			}
			else if (TargetInfo.activeInHierarchy)
			{
				TargetInfo.SetActive(false);
			}
			if (MyPlayer.Instance.FpsController.IsJetpackOn)
			{
				StabilityOn.Activate(InputManager.GetButton(InputManager.AxisNames.LeftShift));
			}
			else if (StabilityOn.activeInHierarchy)
			{
				StabilityOn.Activate(false);
			}
			if (MyPlayer.Instance.CurrentRoomTrigger != null && MyPlayer.Instance.CurrentRoomTrigger.UseGravity && MyPlayer.Instance.CurrentRoomTrigger.GravityForce != Vector3.zero)
			{
				RCSDisabled.text = Localization.Disabled.ToUpper();
			}
			else
			{
				RCSDisabled.text = string.Empty;
			}
			RCSOn.Activate(MyPlayer.Instance.FpsController.CurrentJetpack.IsActive);
			RcsValue.text = FormatHelper.Percentage(CurrentHelmet.Fuel);
			RCSFiller.fillAmount = CurrentHelmet.Fuel;
			RcsWarning.Activate(CurrentHelmet.Fuel < 0.21f);
			OxygenValue.text = FormatHelper.Percentage(CurrentHelmet.Oxygen);
			OxygenFiller.fillAmount = CurrentHelmet.Oxygen;
			OxygenWarning.Activate(CurrentHelmet.Oxygen < 0.21f);
			LightsOn.Activate(CurrentHelmet.LightOn);
			if (Client.Instance.CanvasManager.ShowTips)
			{
				ZeroGravityTips.Activate(MyPlayer.Instance.FpsController.IsZeroG);
			}
			else
			{
				ZeroGravityTips.Activate(false);
			}
		}

		public void ShowTargetPlayer(Player target)
		{
			if (target != null && target != MyPlayer.Instance)
			{
				Transform bone = (target as OtherPlayer).tpsController.animHelper.GetBone(AnimatorHelper.HumanBones.Head_END);
				Vector3 vector = MyPlayer.Instance.FpsController.MainCamera.WorldToScreenPoint(bone.position + target.transform.up * 0.14f);
				Vector2 localPoint = Vector2.zero;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(TargetInfo.transform.parent.GetComponent<RectTransform>(), vector, Client.Instance.CanvasManager.Canvas.worldCamera, out localPoint);
				TargetInfo.transform.localPosition = localPoint;
				TargetInfo.transform.rotation = Quaternion.identity;
				TargetInfo.SetActive(true);
				TargetInfoName.text = target.PlayerName;
			}
		}

		private void TurnOffStardust()
		{
			MyPlayer.Instance.FpsController.StarDustParticle.gameObject.SetActive(false);
			MyPlayer.Instance.FpsController.StarDustParticle.Stop();
		}

		public void CheckFireMod()
		{
			if (CurrentItem != null && CurrentItem is Weapon && (CurrentItem as Weapon).Mods.Count > 0)
			{
				FireMod.sprite = Client.Instance.SpriteManager.GetSprite((CurrentItem as Weapon).CurrentWeaponMod.ModsFireMode);
				WeaponInfo.SetActive(true);
			}
			else
			{
				WeaponInfo.SetActive(false);
			}
		}

		public void HandsSlotUpdate()
		{
			if (CurrentItem != null)
			{
				HandsSlotIcon.color = Colors.White;
				HandsSlotIcon.sprite = MyPlayer.Instance.Inventory.ItemInHands.Icon;
				if (MyPlayer.Instance.Inventory.ItemInHands.MaxQuantity > 0f)
				{
					float num = MyPlayer.Instance.Inventory.ItemInHands.Quantity / MyPlayer.Instance.Inventory.ItemInHands.MaxQuantity;
					QuantityFiller.fillAmount = num;
					if (num < 0.2f)
					{
						ItemInfo.color = Colors.FormatedRed;
					}
					else
					{
						ItemInfo.color = Colors.White;
					}
					ItemInfo.text = MyPlayer.Instance.Inventory.ItemInHands.Quantity.ToString("0");
					ItemQuantity.Activate(true);
				}
				else if (MyPlayer.Instance.Inventory.ItemInHands is DisposableHackingTool)
				{
					float fillAmount = MyPlayer.Instance.Inventory.ItemInHands.Health / MyPlayer.Instance.Inventory.ItemInHands.MaxHealth;
					QuantityFiller.fillAmount = fillAmount;
					ItemInfo.color = Colors.White;
					ItemInfo.text = MyPlayer.Instance.Inventory.ItemInHands.Health.ToString("0");
					ItemQuantity.Activate(true);
				}
				else
				{
					ItemQuantity.Activate(false);
				}
				CheckFireMod();
			}
			else
			{
				HandsSlotIcon.color = Colors.WhiteHalfTransparent;
				HandsSlotIcon.sprite = HandsSlotSprite;
				ItemQuantity.Activate(false);
				WeaponInfo.Activate(false);
			}
		}

		public void UpdateQuickSlots()
		{
			if (MyPlayer.Instance.CurrentOutfit != null)
			{
				_003CUpdateQuickSlots_003Ec__AnonStorey0 _003CUpdateQuickSlots_003Ec__AnonStorey = new _003CUpdateQuickSlots_003Ec__AnonStorey0();
				QuickSlotsHolder.Activate(true);
				_003CUpdateQuickSlots_003Ec__AnonStorey.gre = new List<ItemType>
				{
					ItemType.APGrenade,
					ItemType.EMPGrenade
				};
				_003CUpdateQuickSlots_003Ec__AnonStorey.con = new List<ItemType>
				{
					ItemType.AltairMedpackBig,
					ItemType.AltairMedpackSmall,
					ItemType.AltairResourceContainer
				};
				GameObject primarySlot = PrimarySlot;
				InventorySlot inventorySlot = MyPlayer.Instance.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Primary).Values.FirstOrDefault();
				primarySlot.SetActive(((inventorySlot != null) ? inventorySlot.Item : null) != null);
				GameObject secondarySlot = SecondarySlot;
				InventorySlot inventorySlot2 = MyPlayer.Instance.CurrentOutfit.GetSlotsByGroup(InventorySlot.Group.Secondary).Values.FirstOrDefault();
				secondarySlot.SetActive(((inventorySlot2 != null) ? inventorySlot2.Item : null) != null);
				GranadeSlot.SetActive(MyPlayer.Instance.CurrentOutfit.GetAllSlots().Values.FirstOrDefault(_003CUpdateQuickSlots_003Ec__AnonStorey._003C_003Em__0) != null);
				ConsumableSlot.SetActive(MyPlayer.Instance.CurrentOutfit.GetAllSlots().Values.FirstOrDefault(_003CUpdateQuickSlots_003Ec__AnonStorey._003C_003Em__1) != null);
			}
			else
			{
				QuickSlotsHolder.Activate(false);
			}
		}

		public void WarningsUpdate()
		{
			_003CWarningsUpdate_003Ec__AnonStorey1 _003CWarningsUpdate_003Ec__AnonStorey = new _003CWarningsUpdate_003Ec__AnonStorey1();
			if (Breach.activeInHierarchy)
			{
				Breach.SetActive(false);
			}
			if (Fire.activeInHierarchy)
			{
				Fire.SetActive(false);
			}
			if (Gravity.activeInHierarchy)
			{
				Gravity.SetActive(false);
			}
			_003CWarningsUpdate_003Ec__AnonStorey.currentRoom = MyPlayer.Instance.CurrentRoomTrigger;
			if (_003CWarningsUpdate_003Ec__AnonStorey.currentRoom != null)
			{
				foreach (SceneTriggerRoom item in _003CWarningsUpdate_003Ec__AnonStorey.currentRoom.ParentVessel.MainVessel.GetComponentsInChildren<SceneTriggerRoom>().Where(_003CWarningsUpdate_003Ec__AnonStorey._003C_003Em__0))
				{
					if (item.Breach)
					{
						Breach.SetActive(true);
					}
					if (item.Fire)
					{
						Fire.SetActive(true);
					}
					if (item.GravityMalfunction)
					{
						Gravity.SetActive(true);
					}
				}
			}
			DebrisWarning.SetActive(MyPlayer.Instance.InDebrisField != null);
			RadiationWarning.Activate(!ArmorOk);
		}
	}
}
