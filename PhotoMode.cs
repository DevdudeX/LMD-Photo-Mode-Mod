using MelonLoader;
using PhotoModeMod;
using UnityEngine;
using Il2CppMegagon.Downhill.Cameras;

[assembly: MelonInfo(typeof(PhotoMode), "Photo Mode", "1.0.0", "DevdudeX")]
[assembly: MelonGame()]
namespace PhotoModeMod
{
	public class PhotoMode : MelonMod
	{
		public static PhotoMode instance;
		private static KeyCode photoModeToggleKey;
		private static KeyCode uiToggleKey;
		private static KeyCode settingsResetKey;

		private MelonPreferences_Category mainSettingsCat;
		private MelonPreferences_Category mouseSettingsCat;
		private MelonPreferences_Category gamepadSettingsCat;

		private MelonPreferences_Entry<float> cfg_movementSpeed;
		private MelonPreferences_Entry<float> cfg_movementSprintMult;

		private MelonPreferences_Entry<float> cfg_mSensitivityHorizontal;
		private MelonPreferences_Entry<float> cfg_mSensitivityVertical;
		private MelonPreferences_Entry<float> cfg_mSensitivityMultiplier;
		private MelonPreferences_Entry<bool> cfg_mInvertHorizontal;

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
		private static int xMinLimit = -88;
		private static int xMaxLimit = 88;

		/// <summary>The main camera itself. Used to set the field of view.</summary>
		private static Camera mainCameraComponent;
		/// <summary>The ui camera. Used to toggle hud rendering.</summary>
		private static Camera uiRendererCameraComponent;
		private static PlayCamera defaultCameraScript;
		private static Transform camTransform;

		private static Quaternion rotation;
		/// <summary>Camera rotation around vertical y-axis (left-right)</summary>
		private static float rotHorizontal;
		/// <summary>Camera rotation around x-axis (ear-to-ear or up-down)</summary>
		private static float rotVertical;
		/// <summary>Camera rotation around z-axis (forward-facing)</summary>
		private static float rotRoll;

		private static bool inPhotoMode;
		private static float baseTimeScale;
		private static float baseFoV;

		// Inputs
		float gamepadAnyDpadHorizontal;
		float gamepadAnyDpadVertical;

		float gamepadAnyTriggerInputL;
		float gamepadAnyTriggerInputR;
		float gamepadHorizontalInputStickR;
		float gamepadVerticalInputStickR;
		bool gamepadAnyButton0;	// 'A' Button
		bool gamepadAnyButton2;	// 'X' Button
		bool gamepadAnyButtonDown2;	// 'X' Button Down

		bool gamepadAnyButton3;	// 'Y' Button Held
		bool gamepadAnyButtonDown3;	// 'Y' Button Down

		/// <summary>Left Bumper Hold State</summary>
		bool gamepadAnyButton4;
		/// <summary>Left Bumper Pressed State</summary>
		bool gamepadAnyButtonDown4;

		/// <summary>Right Bumper Hold State</summary>
		bool gamepadAnyButton5;
		/// <summary>Right Bumper Pressed State</summary>
		bool gamepadAnyButtonDown5;

		public override void OnEarlyInitializeMelon()
		{
			instance = this;
			photoModeToggleKey = KeyCode.P;
			uiToggleKey = KeyCode.H;
			settingsResetKey = KeyCode.K;
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

		public override void OnLateUpdate()
		{
			HandleInputs();

			if (!inPhotoMode) {
				return;
			}
			HandlePhotoModeControls();

			CameraLogic();
		}

		private void CameraLogic()
		{
			Vector3 translation = Vector3.zero;

			if (Input.GetAxis("Mouse ScrollWheel") > 0f || gamepadAnyButtonDown5)
			{
				// Scrolling forward; zoom in
				mainCameraComponent.fieldOfView -= 1;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0f || gamepadAnyButtonDown4)
			{
				// Scrolling backwards; zoom out
				mainCameraComponent.fieldOfView += 1;
			}

			// Moving the camera
			translation = GetInputTranslationDirection() * cfg_movementSpeed.Value * Time.fixedDeltaTime;

			// Speed up movement when shift key held
			if (Input.GetKey(KeyCode.LeftShift) || gamepadAnyButton0) {
				translation *= cfg_movementSprintMult.Value;
			}

			// Horizontal movement will make camera rotate around vertical y-axis
			// Vertical movement will make camera rotate along x-axis (your ear-to-ear axis)

			// Mouse input
			rotHorizontal += Input.GetAxisRaw("Mouse X") * cfg_mSensitivityHorizontal.Value * cfg_mSensitivityMultiplier.Value;
			rotVertical += Input.GetAxisRaw("Mouse Y") * cfg_mSensitivityVertical.Value * cfg_mSensitivityMultiplier.Value;

			// Controller input
			rotHorizontal += ApplyInnerDeadzone(gamepadHorizontalInputStickR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensHorizontal.Value * cfg_gamepadSensMultiplier.Value;
			rotVertical -= ApplyInnerDeadzone(gamepadVerticalInputStickR, cfg_gamepadStickDeadzoneR.Value) * cfg_gamepadSensVertical.Value * cfg_gamepadSensMultiplier.Value;

			rotVertical = ClampAngle(rotVertical, (float)xMinLimit, (float)xMaxLimit);  // Clamp the up-down rotation


			if (Input.GetKeyDown(KeyCode.Q)) {
				rotRoll += 1;
			}
			if (Input.GetKeyDown(KeyCode.E)) {
				rotRoll -= 1;
			}
			rotRoll += -gamepadAnyDpadHorizontal * 4f * Time.fixedDeltaTime;

			rotation = Quaternion.Euler(-rotVertical, rotHorizontal, rotRoll);
			Vector3 newPosition = camTransform.position + translation;

			// Apply values
			camTransform.position = newPosition;
			camTransform.rotation = rotation;
		}

		private void HandleInputs()
		{
			gamepadAnyDpadHorizontal = Input.GetAxisRaw("Joy1Axis6") + Input.GetAxisRaw("Joy2Axis6") + Input.GetAxisRaw("Joy3Axis6") + Input.GetAxisRaw("Joy4Axis6");
			gamepadAnyDpadVertical = Input.GetAxisRaw("Joy1Axis7") + Input.GetAxisRaw("Joy2Axis7") + Input.GetAxisRaw("Joy3Axis7") + Input.GetAxisRaw("Joy4Axis7");

			gamepadAnyTriggerInputL = Input.GetAxisRaw("Joy1Axis9") + Input.GetAxisRaw("Joy2Axis9") + Input.GetAxisRaw("Joy3Axis9") + Input.GetAxisRaw("Joy4Axis9");
			gamepadAnyTriggerInputR = Input.GetAxisRaw("Joy1Axis10") + Input.GetAxisRaw("Joy2Axis10") + Input.GetAxisRaw("Joy3Axis10") + Input.GetAxisRaw("Joy4Axis10");
			gamepadHorizontalInputStickR = Input.GetAxisRaw("Joy1Axis4") + Input.GetAxisRaw("Joy2Axis4") + Input.GetAxisRaw("Joy3Axis4") + Input.GetAxisRaw("Joy4Axis4");
			gamepadVerticalInputStickR = Input.GetAxisRaw("Joy1Axis5") + Input.GetAxisRaw("Joy2Axis5") + Input.GetAxisRaw("Joy3Axis5") + Input.GetAxisRaw("Joy4Axis5");
			gamepadAnyButton0 = Input.GetKey(KeyCode.Joystick1Button0) || Input.GetKey(KeyCode.Joystick2Button0) || Input.GetKey(KeyCode.Joystick3Button0) || Input.GetKey(KeyCode.Joystick4Button0);

			gamepadAnyButton2 = Input.GetKey(KeyCode.Joystick1Button2) || Input.GetKey(KeyCode.Joystick2Button2) || Input.GetKey(KeyCode.Joystick3Button2) || Input.GetKey(KeyCode.Joystick4Button2);
			gamepadAnyButtonDown2 = Input.GetKeyDown(KeyCode.Joystick1Button2) || Input.GetKeyDown(KeyCode.Joystick2Button2) || Input.GetKeyDown(KeyCode.Joystick3Button2) || Input.GetKeyDown(KeyCode.Joystick4Button2);

			gamepadAnyButton3 = Input.GetKey(KeyCode.Joystick1Button3) || Input.GetKey(KeyCode.Joystick2Button3) || Input.GetKey(KeyCode.Joystick3Button3) || Input.GetKey(KeyCode.Joystick4Button3);
			gamepadAnyButtonDown3 = Input.GetKeyDown(KeyCode.Joystick1Button3) || Input.GetKeyDown(KeyCode.Joystick2Button3) || Input.GetKeyDown(KeyCode.Joystick3Button3) || Input.GetKeyDown(KeyCode.Joystick4Button3);

			gamepadAnyButton4 = Input.GetKey(KeyCode.Joystick1Button4) || Input.GetKey(KeyCode.Joystick2Button4) || Input.GetKey(KeyCode.Joystick3Button4) || Input.GetKey(KeyCode.Joystick4Button4);
			gamepadAnyButtonDown4 = Input.GetKeyDown(KeyCode.Joystick1Button4) || Input.GetKeyDown(KeyCode.Joystick2Button4) || Input.GetKeyDown(KeyCode.Joystick3Button4) || Input.GetKeyDown(KeyCode.Joystick4Button4);

			gamepadAnyButton5 = Input.GetKey(KeyCode.Joystick1Button5) || Input.GetKey(KeyCode.Joystick2Button5) || Input.GetKey(KeyCode.Joystick3Button5) || Input.GetKey(KeyCode.Joystick4Button5);
			gamepadAnyButtonDown5 = Input.GetKeyDown(KeyCode.Joystick1Button5) || Input.GetKeyDown(KeyCode.Joystick2Button5) || Input.GetKeyDown(KeyCode.Joystick3Button5) || Input.GetKeyDown(KeyCode.Joystick4Button5);

			if (Input.GetKeyDown(photoModeToggleKey) || gamepadAnyButtonDown3)
			{
				TogglePhotoMode();
			}

			// Here to allow disabling the game HUD.
			if (Input.GetKeyDown(uiToggleKey) || gamepadAnyButtonDown2)
			{
				ToggleGameHUD();
			}
		}

		private void HandlePhotoModeControls()
		{
			// Camera FoV controls
			if (mainCameraComponent == null) {
				return;
			}

			if (Input.GetKeyDown(settingsResetKey) || gamepadAnyDpadVertical > 0)
			{
				mainCameraComponent.fieldOfView = baseFoV;
				rotRoll = 0;
			}
		}

		/// <summary>
		/// Grabs required objects, applies standard camera settings and enables the hasStartedOnce bool.
		/// </summary>
		private void GrabObjects()
		{
			LoggerInstance.Msg("Grabbing required GameObjects.");

			// Assigning GO's
			camTransform = GameObject.Find("PlayCamera(Clone)").GetComponent<Transform>();
			mainCameraComponent = camTransform.gameObject.GetComponent<Camera>();
			uiRendererCameraComponent = GameObject.Find("UICam").GetComponent<Camera>();
			defaultCameraScript = camTransform.gameObject.GetComponent<PlayCamera>();
		}

		private void TogglePhotoMode()
		{
			inPhotoMode = !inPhotoMode;

			if (inPhotoMode)
			{
				// Enter photo mode
				LoggerInstance.Msg("Toggle photo mode ==> ["+ inPhotoMode +"]");

				MelonEvents.OnGUI.Subscribe(DrawInfoText, 100); // Register the info label
				baseTimeScale = Time.timeScale; // Save the original time scale before freezing
				Time.timeScale = 0;

				GrabObjects();

				rotHorizontal = camTransform.eulerAngles.y;
				rotVertical = -camTransform.eulerAngles.x;
				rotRoll = 0;

				// Disable normal camera controller
				defaultCameraScript.enabled = false;

				baseFoV = mainCameraComponent.fieldOfView;	// Save the original FoV

				// Lock and hide the cursor
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
			}
			else
			{
				// Exit photo mode
				LoggerInstance.Msg("Toggle photo mode ==> ["+ inPhotoMode +"]");

				MelonEvents.OnGUI.Unsubscribe(DrawInfoText); // Unregister the info label
				Time.timeScale = baseTimeScale; // Reset the time scale to what it was before we froze the time
				mainCameraComponent.fieldOfView = baseFoV;	// Restore the original FoV
				rotRoll = 0;

				// Enable normal camera controller
				defaultCameraScript.enabled = true;

				// Free the cursor
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
			}
		}

		private Vector3 GetInputTranslationDirection()
		{
			Vector3 direction = new Vector3();

			if (Input.GetKey(KeyCode.Space)) {
				direction += Vector3.up;
			}
			if (Input.GetKey(KeyCode.LeftControl)) {
				direction += Vector3.down;
			}

			direction += ApplyInnerDeadzone(gamepadAnyTriggerInputR, cfg_gamepadTriggerDeadzoneR.Value) * Vector3.up;
			direction += ApplyInnerDeadzone(gamepadAnyTriggerInputL, cfg_gamepadTriggerDeadzoneL.Value) * Vector3.down;

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
			ToggleGameHUD(!uiRendererCameraComponent.enabled);
		}
		/// <summary>
		/// Enables or disables rendering of the game HUD.
		/// </summary>
		/// <param name="visible">Should the HUD be rendered.</param>
		private void ToggleGameHUD(bool visible)
		{
			if (uiRendererCameraComponent == null) {
				return;
			}
			// Toggle UI camera rendering
			uiRendererCameraComponent.enabled = visible;
			if (visible) {
				MelonEvents.OnGUI.Subscribe(DrawInfoText, 100); // Register the info label
			}
			else {
				MelonEvents.OnGUI.Unsubscribe(DrawInfoText); // Unregister the info label
			}
			LoggerInstance.Msg("Toggled hud rendering ==> [" + uiRendererCameraComponent.enabled + "]");
		}

		/// <summary>
		/// Tries to clamp the angle to values between 360 and -360.
		/// </summary>
		private static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f)
			{
				angle += 360f;
			}
			if (angle > 360f)
			{
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
			if (axis < -deadzone) {
				return axis;
			}
			return 0;
		}

		public static void DrawInfoText()
		{
			float xOffset = 10;
			float xOffset2 = 310;

			GUI.Label(new Rect(xOffset, 200, 1000, 200), "<b><color=lime><size=30>Photo Mode Active</size></color></b>");

			GUI.Label(new Rect(xOffset, 230, 1000, 200), "<b><color=cyan><size=30>Toggle Photo Mode</size></color></b>");
			GUI.Label(new Rect(xOffset2, 230, 1000, 200), "<b><color=cyan><size=30>|    [Keyboard P] or [Gamepad Y]</size></color></b>");

			GUI.Label(new Rect(xOffset, 260, 1000, 200), "<b><color=cyan><size=30>Toggle HUD</size></color></b>");
			GUI.Label(new Rect(xOffset2, 260, 1000, 200), "<b><color=cyan><size=30>|    [Keyboard H] or [Gamepad X]</size></color></b>");

			GUI.Label(new Rect(xOffset, 290, 1000, 200), "<b><color=cyan><size=30>Change FoV</size></color></b>");
			GUI.Label(new Rect(xOffset2, 290, 1000, 200), "<b><color=cyan><size=30>|    [Mouse Scroll] or [LB, RB]</size></color></b>");

			GUI.Label(new Rect(xOffset, 320, 1000, 200), "<b><color=cyan><size=30>Tilt Camera</size></color></b>");
			GUI.Label(new Rect(xOffset2, 320, 1000, 200), "<b><color=cyan><size=30>|    [Keyboard Q, E] or [Horizontal DPAD]</size></color></b>");

			GUI.Label(new Rect(xOffset, 350, 1000, 200), "<b><color=cyan><size=30>Speed Modifier</size></color></b>");
			GUI.Label(new Rect(xOffset2, 350, 1000, 200), "<b><color=cyan><size=30>|    [LShift] or [Gamepad A]</size></color></b>");

			GUI.Label(new Rect(xOffset, 380, 1000, 200), "<b><color=cyan><size=30>Speed Modifier</size></color></b>");
			GUI.Label(new Rect(xOffset2, 380, 1000, 200), "<b><color=cyan><size=30>|    [LShift] or [Gamepad A]</size></color></b>");

			GUI.Label(new Rect(xOffset, 410, 1000, 200), "<b><color=red><size=30>Reset Angles</size></color></b>");
			GUI.Label(new Rect(xOffset2, 410, 1000, 200), "<b><color=cyan><size=30>|    [Keyboard K]</size></color></b>");
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