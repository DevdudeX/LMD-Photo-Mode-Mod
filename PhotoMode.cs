// Mod
using MelonLoader;
using PhotoModeMod;
// Unity
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
// Megagon
using Il2CppMegagon.Downhill.Cameras;

[assembly: MelonInfo(typeof(PhotoMode), "Photo Mode", "1.0.4", "DevdudeX")]
[assembly: MelonGame()]
namespace PhotoModeMod
{
	public class PhotoMode : MelonMod
	{
		// Keep this updated!
		private const string MOD_VERSION = "1.0.4";
		public static PhotoMode instance;
		private KeyCode photoModeToggleKey;
		private KeyCode uiToggleKey;
		private KeyCode settingsResetKey;
		private KeyCode focusModeModifierKey;

		private MelonPreferences_Category mainSettingsCat;
		private MelonPreferences_Category mouseSettingsCat;
		private MelonPreferences_Category gamepadSettingsCat;

		private MelonPreferences_Entry<float> cfg_movementSpeed;
		private MelonPreferences_Entry<float> cfg_movementSprintMult;

		private MelonPreferences_Entry<float> cfg_mSensitivityHorizontal;
		private MelonPreferences_Entry<float> cfg_mSensitivityVertical;
		private MelonPreferences_Entry<float> cfg_mSensitivityMultiplier;
		private MelonPreferences_Entry<bool> cfg_mInvertHorizontal;
		private MelonPreferences_Entry<bool> cfg_mInvertVertical;

		private MelonPreferences_Entry<float> cfg_gamepadSensHorizontal;
		private MelonPreferences_Entry<float> cfg_gamepadSensVertical;
		private MelonPreferences_Entry<float> cfg_gamepadSensMultiplier;
		private MelonPreferences_Entry<float> cfg_gamepadStickDeadzoneL;
		private MelonPreferences_Entry<float> cfg_gamepadStickDeadzoneR;
		private MelonPreferences_Entry<float> cfg_gamepadTriggerDeadzoneL;
		private MelonPreferences_Entry<float> cfg_gamepadTriggerDeadzoneR;
		private MelonPreferences_Entry<bool> cfg_gamepadInvertHorizontal;
		private MelonPreferences_Entry<bool> cfg_gamepadInvertVertical;

		// Camera angle limits
		private int xMinLimit = -88;
		private int xMaxLimit = 88;

		/// <summary>The main camera itself. Used to set the field of view.</summary>
		private Camera mainCameraComponent;
		/// <summary>The ui camera. Used to toggle hud rendering.</summary>
		private Camera uiRendererCameraComponent;
		private PlayCamera defaultCameraScript;
		private Transform camTransform;
		private GameObject postProcessingObject;
		private DepthOfField m_dofSettings;

		private Quaternion rotation;
		/// <summary>Camera rotation around vertical y-axis (left-right)</summary>
		private float rotHorizontal;
		/// <summary>Camera rotation around x-axis (ear-to-ear or up-down)</summary>
		private float rotVertical;
		/// <summary>Camera rotation around z-axis (forward-facing)</summary>
		private float rotRoll;

		private bool inPhotoMode;
		private bool inFocusMode;
		private bool hasDOFSettings;
		private bool dpadDownBlocked;
		private float baseTimeScale;
		private float baseFoV;
		private float baseFocusDistanceDoF;

		// Inputs
		private float anyGamepadDpadHorizontal;

		private float anyGamepadDpadVertical;
		private bool anyGamepadDpadDownOnPress;

		private float anyGamepadTriggerInputL;
		private float anyGamepadTriggerInputR;
		private float anyGamepadStickHorizontalR;
		private float anyGamepadStickVerticalR;
		/// <summary>Gamepad [A] held state</summary>
		private bool anyGamepadBtn0;
		/// <summary>Gamepad [B] pressed state</summary>
		private bool anyGamepadBtnDown1;
		/// <summary>Gamepad [X] pressed state</summary>
		private bool anyGamepadBtnDown2;
		/// <summary>Gamepad [Y] pressed state</summary>
		private bool anyGamepadBtnDown3;
		/// <summary>Left Bumper pressed state</summary>
		private bool anyGamepadBtnDown4;
		/// <summary>Right Bumper pressed state</summary>
		private bool anyGamepadBtnDown5;
		/// <summary>Start Button pressed state</summary>
		private bool anyGamepadBtnDown7;

		// Used to treat dpad as a button
		private bool dpadVerticalBlocked = false;

		public override void OnEarlyInitializeMelon()
		{
			instance = this;
			photoModeToggleKey = KeyCode.P;
			uiToggleKey = KeyCode.H;
			settingsResetKey = KeyCode.K;
			focusModeModifierKey = KeyCode.F;
		}

		public override void OnInitializeMelon()
		{
			mainSettingsCat = MelonPreferences.CreateCategory("Main Settings");
			mainSettingsCat.SetFilePath("UserData/PhotoModeSettings.cfg");

			mouseSettingsCat = MelonPreferences.CreateCategory("Mouse Settings");
			mouseSettingsCat.SetFilePath("UserData/PhotoModeSettings.cfg");

			gamepadSettingsCat = MelonPreferences.CreateCategory("Gamepad Settings");
			gamepadSettingsCat.SetFilePath("UserData/PhotoModeSettings.cfg");

			// Main Settings
			cfg_movementSpeed = mainSettingsCat.CreateEntry<float>("CameraMovementSpeed", 1.8f);
			cfg_movementSprintMult = mainSettingsCat.CreateEntry<float>("CameraSprintSpeedMultiplier", 8f);

			// Mouse Settings
			cfg_mSensitivityHorizontal = mouseSettingsCat.CreateEntry<float>("HorizontalSensitivity", 1.0f);
			cfg_mSensitivityVertical = mouseSettingsCat.CreateEntry<float>("VerticalSensitivity", 1.0f);
			cfg_mSensitivityMultiplier = mouseSettingsCat.CreateEntry<float>("SensitivityMultiplier", 1f);
			cfg_mInvertHorizontal = mouseSettingsCat.CreateEntry<bool>("InvertHorizontal", false);
			cfg_mInvertVertical = mouseSettingsCat.CreateEntry<bool>("InvertVertical", false);

			// Gamepad Settings
			cfg_gamepadSensHorizontal = gamepadSettingsCat.CreateEntry<float>("GamepadHorizontalSensitivity", 0.6f);
			cfg_gamepadSensVertical = gamepadSettingsCat.CreateEntry<float>("GamepadVerticalSensitivity", 0.6f);
			cfg_gamepadSensMultiplier = gamepadSettingsCat.CreateEntry<float>("GamepadSensitivityMultiplier", 1f);

			cfg_gamepadStickDeadzoneL = gamepadSettingsCat.CreateEntry<float>("GamepadStickDeadzoneL", 0.1f);
			cfg_gamepadStickDeadzoneR = gamepadSettingsCat.CreateEntry<float>("GamepadStickDeadzoneR", 0.1f);
			cfg_gamepadTriggerDeadzoneL = gamepadSettingsCat.CreateEntry<float>("GamepadTriggerDeadzoneL", 0.1f);
			cfg_gamepadTriggerDeadzoneR = gamepadSettingsCat.CreateEntry<float>("GamepadTriggerDeadzoneR", 0.1f);

			cfg_gamepadInvertHorizontal = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertHorizontal", false);
			cfg_gamepadInvertVertical = gamepadSettingsCat.CreateEntry<bool>("GamepadInvertVertical", false);

			mainSettingsCat.SaveToFile();
			mouseSettingsCat.SaveToFile();
			gamepadSettingsCat.SaveToFile();
		}

		public override void OnUpdate()
		{
			ProcessInputs();

		}

		public override void OnLateUpdate()
		{
			if (!inPhotoMode) {
				return;
			}
			HandlePhotoModeControls();

			CameraLogic();

			// Reset key down states at the end of the frame
			anyGamepadDpadDownOnPress = false;
		}

		private void CameraLogic()
		{
			//inFocusMode = hasDOFSettings && (Input.GetKey(focusModeModifierKey) || anyGamepadDpadVertical < 0);
			bool holdingSprint = Input.GetKey(KeyCode.LeftShift) || anyGamepadBtn0;

			if (Input.GetAxis("Mouse ScrollWheel") > 0f || anyGamepadBtnDown5)
			{
				// Scrolling forward / right bumper
				if (inFocusMode) {
					// Move DoF focus further away
					m_dofSettings.focusDistance.value = m_dofSettings.focusDistance.GetValue<float>() + (holdingSprint ? 1f : 0.05f);
				}
				else {
					// FoV zoom in
					mainCameraComponent.fieldOfView -= 1;
				}
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0f || anyGamepadBtnDown4)
			{
				// Scrolling backwards / left bumper
				if (inFocusMode) {
					// Move DoF focus further away
					m_dofSettings.focusDistance.value = m_dofSettings.focusDistance.GetValue<float>() - (holdingSprint ? 1f : 0.05f);
				}
				else {
					// FoV zoom out
					mainCameraComponent.fieldOfView += 1;
				}
			}

			// Moving the camera
			Vector3 moveVector = InputToMoveVector() * cfg_movementSpeed.Value * Time.fixedDeltaTime;

			// Speed up movement when shift key held
			if (holdingSprint) {
				moveVector *= cfg_movementSprintMult.Value;
			}

			// Horizontal movement will make camera rotate around vertical y-axis
			// Vertical movement will make camera rotate along x-axis (your ear-to-ear axis)

			// Mouse input
			if (cfg_mInvertHorizontal.Value) {
				rotHorizontal -= Input.GetAxisRaw("Mouse X") * cfg_mSensitivityHorizontal.Value * cfg_mSensitivityMultiplier.Value;
			}
			else {
				rotHorizontal += Input.GetAxisRaw("Mouse X") * cfg_mSensitivityHorizontal.Value * cfg_mSensitivityMultiplier.Value;
			}
			if (cfg_mInvertVertical.Value) {
				rotVertical -= Input.GetAxisRaw("Mouse Y") * cfg_mSensitivityVertical.Value * cfg_mSensitivityMultiplier.Value;
			}
			else {
				rotVertical += Input.GetAxisRaw("Mouse Y") * cfg_mSensitivityVertical.Value * cfg_mSensitivityMultiplier.Value;
			}

			// Controller input
			if (cfg_gamepadInvertHorizontal.Value) {
				rotHorizontal -= ApplyInnerDeadzone(anyGamepadStickHorizontalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensHorizontal.Value * cfg_gamepadSensMultiplier.Value;
			}
			else {
				rotHorizontal += ApplyInnerDeadzone(anyGamepadStickHorizontalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensHorizontal.Value * cfg_gamepadSensMultiplier.Value;
			}
			if (cfg_gamepadInvertVertical.Value) {
				rotVertical += ApplyInnerDeadzone(anyGamepadStickVerticalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensVertical.Value * cfg_gamepadSensMultiplier.Value;
			}
			else {
				rotVertical -= ApplyInnerDeadzone(anyGamepadStickVerticalR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensVertical.Value * cfg_gamepadSensMultiplier.Value;
			}

			// Clamp the up-down rotation
			rotVertical = ClampAngle(rotVertical, (float)xMinLimit, (float)xMaxLimit);

			if (Input.GetKeyDown(KeyCode.Q)) {
				rotRoll += 1;
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				rotRoll -= 1;
			}
			rotRoll += -anyGamepadDpadHorizontal * 4f * Time.fixedDeltaTime;

			rotation = Quaternion.Euler(-rotVertical, rotHorizontal, rotRoll);
			Vector3 newPosition = camTransform.position + moveVector;

			// Apply values
			camTransform.position = newPosition;
			camTransform.rotation = rotation;
		}

		private void ProcessInputs()
		{
			anyGamepadDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			anyGamepadDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

			anyGamepadTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") + Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
			anyGamepadTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") + Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");
			anyGamepadStickHorizontalR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			anyGamepadStickVerticalR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");

			anyGamepadBtn0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);
			anyGamepadBtnDown1 = Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.Joystick2Button1) || Input.GetKeyDown(KeyCode.Joystick3Button1) || Input.GetKeyDown(KeyCode.Joystick4Button1);
			anyGamepadBtnDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);
			anyGamepadBtnDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);
			anyGamepadBtnDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);
			anyGamepadBtnDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);
			anyGamepadBtnDown7 = Input.GetKeyDown(KeyCode.Joystick1Button7) || Input.GetKeyDown(KeyCode.Joystick2Button7) || Input.GetKeyDown(KeyCode.Joystick3Button7) || Input.GetKeyDown(KeyCode.Joystick4Button7);

			// Hit full dpad and its allowed, set to true
			if ((anyGamepadDpadVertical < 0) && (dpadVerticalBlocked==false))
			{
				anyGamepadDpadDownOnPress = true;
				dpadVerticalBlocked = true; //Disable it
			}
			else if (IsBetweenInclusive(anyGamepadDpadVertical, -0.1f, 0.1f))
			{
				//if they release (small deadzone), allow them to hit it again.
				anyGamepadDpadDownOnPress = false;
				dpadVerticalBlocked = false;
			}


			if (Input.GetKeyDown(photoModeToggleKey) || anyGamepadBtnDown3)
			{
				TogglePhotoMode();
			}

			if (inPhotoMode && (Input.GetKey(KeyCode.Escape) || Input.GetKey(KeyCode.Return) || Input.GetKey(KeyCode.Backspace) || anyGamepadBtnDown1)) {
				TogglePhotoMode(false);
			}

			// Here to allow disabling the game HUD.
			if (Input.GetKeyDown(uiToggleKey) || anyGamepadBtnDown2)
			{
				ToggleGameHUD();
			}

			// Force enable hud when opening menu
			if (Input.GetKey(KeyCode.Escape) || anyGamepadBtnDown7) {
				ToggleGameHUD(true, false);
			}
		}

		private void HandlePhotoModeControls()
		{
			// Camera FoV controls
			if (mainCameraComponent == null) {
				return;
			}

			if (Input.GetKeyDown(settingsResetKey) || anyGamepadDpadVertical > 0)
			{
				mainCameraComponent.fieldOfView = baseFoV;
				rotRoll = 0;
			}

			// Treat the dpad more like a button
			if (anyGamepadDpadVertical == 0 && dpadDownBlocked == true) {
				dpadDownBlocked = false;
			}
			if (hasDOFSettings)
			{
				if (Input.GetKeyDown(focusModeModifierKey)) {
					inFocusMode = !inFocusMode;
				}
				else if (anyGamepadDpadVertical < 0 && dpadDownBlocked == false) {
					inFocusMode = !inFocusMode;
					dpadDownBlocked = true;
				}
			}
		}

		/// <summary>
		/// Grabs required objects, applies standard camera settings and enables the hasStartedOnce bool.
		/// </summary>
		private void GrabObjects()
		{
			camTransform = GameObject.Find("PlayCamera(Clone)").GetComponent<Transform>();
			mainCameraComponent = camTransform.gameObject.GetComponent<Camera>();
			uiRendererCameraComponent = GameObject.Find("UICam").GetComponent<Camera>();
			defaultCameraScript = camTransform.gameObject.GetComponent<PlayCamera>();
			postProcessingObject = camTransform.Find("DefaultPostProcessing").gameObject;
			hasDOFSettings = postProcessingObject.GetComponent<PostProcessVolume>().sharedProfile.TryGetSettings<DepthOfField>(out m_dofSettings);
		}

		/// <summary>
		/// Toggles photo mode.
		/// </summary>
		private void TogglePhotoMode()
		{
			TogglePhotoMode(!inPhotoMode);
		}
		/// <summary>
		/// Toggles photo mode to the provided state.
		/// </summary>
		private void TogglePhotoMode(bool active)
		{
			inPhotoMode = active;
			if (inPhotoMode)
			{
				//LoggerInstance.Msg("Toggle photo mode --> ["+ inPhotoMode +"]");
				// Enter photo mode
				MelonEvents.OnGUI.Subscribe(DrawInfoText, 100); // Register the info label
				baseTimeScale = Time.timeScale; // Save the original time scale before freezing
				Time.timeScale = 0;

				GrabObjects();

				// Save the original FoV and DoF focus distance
				baseFoV = mainCameraComponent.fieldOfView;
				baseFocusDistanceDoF = m_dofSettings.focusDistance.GetValue<float>();

				// Save camera rotation for later calculations
				rotHorizontal = camTransform.eulerAngles.y;
				rotVertical = -camTransform.eulerAngles.x;
				rotRoll = 0;

				// Disable normal camera controller
				defaultCameraScript.enabled = false;

				// Lock and hide the cursor
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				// Exit photo mode
				MelonEvents.OnGUI.Unsubscribe(DrawInfoText); // Unregister the info label
				Time.timeScale = baseTimeScale; // Reset the time scale to what it was before we froze the time
				mainCameraComponent.fieldOfView = baseFoV;	// Restore the original FoV
				m_dofSettings.focusDistance.value = baseFocusDistanceDoF;	// Restore the original focus distance for DoF
				rotRoll = 0;

				// Enable normal camera controller
				defaultCameraScript.enabled = true;

				// Free the cursor
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		/// <summary>
		/// Translates user input into a movement direction.
		/// </summary>
		private Vector3 InputToMoveVector()
		{
			Vector3 direction = new Vector3();

			if (Input.GetKey(KeyCode.Space)) {
				direction += Vector3.up;
			}
			if (Input.GetKey(KeyCode.LeftControl)) {
				direction += Vector3.down;
			}

			direction += ApplyInnerDeadzone(anyGamepadTriggerInputR, cfg_gamepadTriggerDeadzoneR.Value) * Vector3.up;
			direction += ApplyInnerDeadzone(anyGamepadTriggerInputL, cfg_gamepadTriggerDeadzoneL.Value) * Vector3.down;

			direction += ApplyInnerDeadzone(Input.GetAxisRaw("Vertical"), cfg_gamepadStickDeadzoneL.Value) * camTransform.forward;
			direction += ApplyInnerDeadzone(Input.GetAxisRaw("Horizontal"), cfg_gamepadStickDeadzoneL.Value) * camTransform.right;

			return direction;
		}

		/// <summary>
		/// Toggles the rendering of the game HUD.
		/// </summary>
		private void ToggleGameHUD()
		{
			if (uiRendererCameraComponent == null) {
				return;
			}
			ToggleGameHUD(!uiRendererCameraComponent.enabled, true);
		}
		/// <summary>
		/// Enables or disables rendering of the game HUD.
		/// </summary>
		/// <param name="visible">Should the HUD be rendered.</param>
		private void ToggleGameHUD(bool visible, bool drawInfo)
		{
			if (uiRendererCameraComponent == null) {
				return;
			}
			// Toggle UI camera rendering
			uiRendererCameraComponent.enabled = visible;

			// Handle info drawing
			if (visible && drawInfo) {
				MelonEvents.OnGUI.Subscribe(DrawInfoText, 100);
			}
			else {
				MelonEvents.OnGUI.Unsubscribe(DrawInfoText);
			}
		}

		/// <summary>
		/// Checks if the value is within the min and max.
		/// </summary>
		private static bool IsBetweenInclusive(float num, float min, float max)
		{
			return num >= min && num <= max;
		}

		/// <summary>
		/// Tries to clamp the angle to values between 360 and -360.
		/// </summary>
		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) {
				angle += 360f;
			}
			if (angle > 360f) {
				angle -= 360f;
			}
			return Mathf.Clamp(angle, min, max);
		}

		/// <summary>
		/// Snaps the given value to 0 if it falls within the deadzone radius in either direction.
		/// </summary>
		/// <returns>The axis if outside the deadzone, otherwise returns 0.</returns>
		private static float ApplyInnerDeadzone(float axis, float deadzone)
		{
			if (axis > deadzone) {
				return axis;
			}
			else if (axis < -deadzone) {
				return axis;
			}
			return 0;
		}

		public void DrawInfoText()
		{
			float xOffset = 10;
			float xOffset2 = 200;
			float xOffset3 = 450;

			string keyboardBinds = @"<b><color=cyan><size=20>
KEYBOARD
------------------
Space / LControl
Keyboard P
Keyboard H
LShift
Keyboard K
Q / E
Mouse Scroll
F
</size></color></b>";

			string gamepadBinds = @"<b><color=cyan><size=20>
| GAMEPAD
-----------------------
| L-Trigger / R-Trigger
| Gamepad Y
| Gamepad X
| Gamepad A
| Dpad ▲
| Dpad ◄ / ►
| L-Bumper / R-Bumper
| DPAD ▼
</size></color></b>";

			string bindDescriptions = @"<b><color=cyan><size=20>
| Action
--------------------------------
| Move up and down
| Toggle Photo Mode
| Toggle the game HUD and UI
| Speed up actions
| Reset camera rotation and FoV
| Tilt camera left and right
| Change Field of View
| Toggle DoF mode (use fov controls)
</size></color></b>";

			GUI.Label(new Rect(xOffset, 5, 1000, 200), $"<b><color=white><size=14>DevdudeX's Photo Mode v{MOD_VERSION}</size></color></b>");
			GUI.Label(new Rect(xOffset, 200, 1000, 200), "<b><color=lime><size=30>Photo Mode Active</size></color></b>");

			GUI.Label(new Rect(xOffset, 230, 2000, 2000), keyboardBinds);
			GUI.Label(new Rect(xOffset2, 230, 2000, 2000), gamepadBinds);
			GUI.Label(new Rect(xOffset3, 230, 2000, 2000), bindDescriptions);
			GUI.Label(new Rect(xOffset, 490, 1000, 200), $"<b><color={(inFocusMode ? "lime" : "white")}><size=20>Focus mode: {inFocusMode}</size></color></b>");
		}
		public override void OnDeinitializeMelon()
		{
			if (inPhotoMode)
			{
				TogglePhotoMode(); // Unfreeze the game in case the melon gets unregistered
			}
		}
	}
}
