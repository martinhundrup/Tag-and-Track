# Tag & Track — iPad Deployment Guide

This guide covers setting up a **brand-new iPad** (not previously configured for development) and deploying the Tag & Track .NET MAUI app to it from a Mac.

---

## Prerequisites (Mac)

- **macOS** with **Xcode** installed (latest stable recommended)
- **.NET 10 SDK** with the `ios` workload:
  ```
  dotnet workload install ios
  ```
- An **Apple ID** signed into Xcode (free or paid Developer account)
- A **USB-C / Lightning cable** to connect the iPad

---

## 1. Enable Developer Mode on the iPad

iPadOS 16+ requires Developer Mode to be explicitly enabled before you can run locally-built apps.

1. Connect the iPad to your Mac via USB.
2. Open **Xcode** on your Mac.
3. Go to **Window → Devices and Simulators**.
4. Select the iPad in the left sidebar. Xcode will begin preparing the device. Wait for any symbol downloads to finish.
5. On the **iPad**, go to **Settings → Privacy & Security → Developer Mode**.
   - If you don't see the Developer Mode toggle, make sure the iPad is still connected to your Mac and that Xcode recognized it in step 4. You may need to restart the iPad after the initial Xcode connection for the toggle to appear.
6. Enable the **Developer Mode** toggle.
7. The iPad will prompt you to restart — tap **Restart**.
8. After reboot, confirm the prompt on the iPad to finish enabling Developer Mode.

---

## 2. Trust Your Apple Developer Certificate on the iPad

When using a free Apple Developer account, the iPad must manually trust your signing certificate.

1. Make sure your Apple ID is added in **Xcode → Settings → Accounts**.
2. Back in **Xcode → Window → Devices and Simulators**, confirm the iPad shows as **Connected** with no errors.
3. Build the app once (see Step 4 below). The first install will fail on the iPad with an **"Untrusted Developer"** alert.
4. On the **iPad**, go to **Settings → General → VPN & Device Management** (or **Profiles & Device Management** on older iPadOS).
5. Under **Developer App**, tap your Apple ID / team name.
6. Tap **Trust "[your Apple ID]"** and confirm.

> After trusting, subsequent builds will install and launch without this prompt.

---

## 3. Register the Device & Set Up Signing in Xcode

Even though you're building with `dotnet build`, the code-signing identity and provisioning profile are managed by Apple tooling under the hood. A one-time Xcode project setup ensures the device is registered and a provisioning profile is generated.

1. Open **Xcode** and create a **new blank iOS App** project (or open any existing one).
2. In the project settings, set:
   - **Bundle Identifier**: `com.martinhundrup.tagandtrack`
   - **Team**: Select your Apple ID / team.
   - **Signing**: Check **Automatically manage signing**.
3. Set the **deployment target** to the connected iPad.
4. Click **Build** (⌘B). Xcode will automatically:
   - Register the iPad's UDID with your developer account.
   - Generate a **provisioning profile** that covers `com.martinhundrup.tagandtrack` on this device.
5. Once the Xcode build succeeds (or at least signing resolves), you can close this Xcode project — its only purpose was to register the device and provision the bundle ID.

> **Why this step?** The .csproj uses `CodesignProvision=Automatic` and `CodesignKey=Apple Development`. The .NET iOS build toolchain delegates to the same signing infrastructure as Xcode, so the profile Xcode generates will be picked up automatically.

---

## 4. Find the iPad's Device Name

The `_DeviceName` build property must match the iPad's name exactly.

1. On the **iPad**: **Settings → General → About → Name**. Note the exact name (e.g., `Martin's iPad`).
2. Alternatively, in **Xcode → Window → Devices and Simulators**, the name is shown at the top of the device detail pane.

---

## 5. Build & Deploy

Open a terminal, `cd` into the project directory:

```bash
cd /Users/martinhundrup/Documents/repos/Tag-and-Track/code/TagAndTrack/TagAndTrack
```

### Build only (compile + sign, no deploy):

```bash
dotnet build TagAndTrack.csproj -f net10.0-ios -c Debug -p:RuntimeIdentifier=ios-arm64
```

### Build and run on the iPad:

```bash
dotnet build TagAndTrack.csproj -t:Run -f net10.0-ios -c Debug \
  -p:RuntimeIdentifier=ios-arm64 \
  -p:_DeviceName="<YOUR IPAD NAME>"
```

Replace `<YOUR IPAD NAME>` with the exact device name from Step 4 (e.g., `"Martin's iPad"`).

> **Tip:** If the device name contains an apostrophe, wrap it in double quotes as shown above — the shell will handle it correctly.

---

## Troubleshooting

| Problem | Fix |
|---|---|
| **Developer Mode toggle not visible** | Reconnect USB, open Xcode → Devices and Simulators, wait for device prep, then restart the iPad. |
| **"Untrusted Developer" on launch** | Settings → General → VPN & Device Management → Trust your certificate (Step 2). |
| **"Verify App" does nothing** | **1)** Ensure the iPad has internet access (try Safari). **2)** Check Settings → General → Date & Time → "Set Automatically" is **on** (clock skew breaks certificate validation). **3)** Force-restart the iPad (power + volume up until Apple logo), then retry. **4)** If still stuck: delete the app from the iPad, go to Xcode → Settings → Accounts → your Apple ID → Manage Certificates → revoke the existing Apple Development cert → create a new one, then rebuild and redeploy. Go back to Settings → VPN & Device Management and verify again. |
| **"No signing identity found"** | Ensure your Apple ID is in Xcode → Settings → Accounts and you completed Step 3. |
| **"Unable to install — device not registered"** | Open the Xcode dummy project from Step 3, select the iPad as the target, and build once to register the UDID. |
| **"Could not find device"** | Verify the `_DeviceName` value matches exactly (case-sensitive). Check in Settings → General → About → Name. |
| **Build succeeds but app won't open** | Free accounts expire after 7 days. Re-run the build to re-sign and re-deploy. |
| **Provisioning profile doesn't match** | Make sure the Xcode dummy project's bundle ID is `com.martinhundrup.tagandtrack` (must match the .csproj `ApplicationId` for the ios target). |

---

## Notes

- **Free Apple accounts** allow deploying to a physical device, but the app signature expires after **7 days**. You'll need to rebuild and redeploy periodically.
- The project targets **iOS 14.2+** (`SupportedOSPlatformVersion` in the .csproj), so the iPad must be running at least iPadOS 14.2.
- The app requests **camera access** for QR/barcode scanning — grant the permission when prompted on first launch.
