# King Smash M16 — Google Play Data Safety Form

Step-by-step guide for completing the **Data Safety** section in Google Play Console.

Navigate to: **Play Console > [App] > Store Presence > Data Safety**

---

## Overview

The Data Safety section is separate from the content rating questionnaire. It is a self-declaration that Google displays to users on your store listing. Inaccurate declarations can result in policy violations and removal from the Play Store. Complete it carefully based on what the app actually does.

---

## Step 1 — Does your app collect or share any of the required user data types?

**Answer: YES**

King Smash collects:
- User IDs (Firebase UID)
- App interactions (gameplay analytics)
- Crash logs (Firebase Crashlytics)
- Device or other identifiers (Firebase installation ID, advertising ID)

Optionally collects (if player uses Google Sign-In):
- Email address
- Name

---

## Step 2 — Is all of the user data collected by your app encrypted in transit?

**Answer: YES**

All data transmitted between King Smash and Firebase/Google services uses TLS encryption. This is enforced by the Firebase SDK and Google infrastructure.

---

## Step 3 — Do you provide a way for users to request that their data is deleted?

**Answer: YES**

Users can request data deletion by emailing support@[studio].com with subject "Data Deletion Request". The privacy policy (linked in the store listing) documents this process.

Note: As of Google Play policy (November 2023 requirement), if your app offers account creation, you must also provide an in-app account deletion option. If the game supports account deletion in-app (via Settings > Account > Delete Account), mark this as YES. If not, this must be implemented before the requirement deadline.

---

## Step 4 — Data Types: Complete Answers

Work through each data type category below. For each type, indicate whether it is collected, shared, required, and the purpose.

---

### Personal Info

#### Name
- **Collected?** OPTIONAL — only if player chooses Google Sign-In
- **Shared with third parties?** NO
- **Required or can player opt out?** Optional — player initiates Google Sign-In; can unlink at any time
- **Processing:** Ephemeral? NO — stored in Firestore while account is linked
- **Purpose:** App functionality (display name in account screen)
- **Encrypted in transit:** YES

#### Email address
- **Collected?** OPTIONAL — only if player chooses Google Sign-In
- **Shared with third parties?** NO
- **Required or can player opt out?** Optional — player initiates Google Sign-In; can unlink at any time
- **Processing:** Ephemeral? NO — stored in Firestore while account is linked
- **Purpose:** App functionality (account linking for cross-device save)
- **Encrypted in transit:** YES

#### User IDs
- **Collected?** YES
- **Shared with third parties?** NO
- **Required or can player opt out?** Required — Firebase anonymous UID is generated automatically and is required for cloud save. Cloud save can be disabled in theory but is core game functionality.
- **Processing:** Ephemeral? NO — stored in Firestore
- **Purpose:** App functionality (cloud save), Analytics (anonymous correlation), Crash reporting (session correlation)
- **Encrypted in transit:** YES

---

### Financial Info

#### Purchase history
- **Collected?** NO — Google Play processes all payments
- **Notes:** The game validates purchase receipts via Google Play server-to-server API using order IDs only. We do not receive or store payment card information.

#### Credit/debit card or bank account number
- **Collected?** NO

---

### Location

#### Approximate location
- **Collected?** NO

#### Precise location
- **Collected?** NO

---

### Web Browsing

#### Web browsing history
- **Collected?** NO

---

### App Activity

#### App interactions
- **Collected?** YES
- **Shared with third parties?** NO (note: Firebase Analytics is a first-party Google service; processed under your Firebase project)
- **Required or can player opt out?** Optional with analytics opt-out mechanism if implemented; otherwise required as part of service operation
- **Processing:** Ephemeral? NO — retained per Firebase Analytics retention settings (14 months default)
- **Purpose:** Analytics (understanding gameplay patterns, difficulty tuning, funnel analysis)
- **Encrypted in transit:** YES

#### In-app search history
- **Collected?** NO

#### Installed apps
- **Collected?** NO

#### Other user-generated content
- **Collected?** NO — no UGC features in King Smash

---

### App Info and Performance

#### Crash logs
- **Collected?** YES
- **Shared with third parties?** NO (Firebase Crashlytics is a first-party Google service processed under your Firebase project)
- **Required or can player opt out?** Crashlytics collection is enabled by default. If you implement a Crashlytics opt-out mechanism, mark as Optional. Otherwise: Required.
- **Processing:** Ephemeral? NO — retained for 90 days by Crashlytics
- **Purpose:** Crash reporting (diagnosing and fixing app crashes)
- **Encrypted in transit:** YES

#### Diagnostics
- **Collected?** YES
- **Notes:** Device model, OS version, screen resolution are collected with crash reports
- **Shared with third parties?** NO
- **Purpose:** Crash reporting
- **Encrypted in transit:** YES

#### Other app performance data
- **Collected?** NO — Firebase Performance Monitoring not integrated

---

### Device or Other Identifiers

#### Device or other identifiers
- **Collected?** YES
- **Which identifiers:**
  - Firebase Installation ID (FID): generated per app install, non-personal
  - Google Advertising ID (GAID): accessed by AdMob for ad serving and measurement
- **Shared with third parties?** GAID is shared with AdMob (Google) for ad serving. This is a Google-to-Google transfer under your AdMob account.
- **Purpose:** Advertising (AdMob ad targeting and measurement), Analytics (Firebase installation correlation)
- **Encrypted in transit:** YES
- **Required or can player opt out?** Players can reset or opt out of GAID personalization via Android device settings. Firebase Installation ID is required for service function.

---

## Step 5 — Data Sharing Details

### Is any data shared with third parties?

**YES** — the Google Advertising ID (GAID) is shared with Google AdMob for ad serving purposes. All other data is processed within your Firebase project (Google LLC) as a data processor, not a third party in the Play Console context.

**AdMob sharing declaration:**
- Data type: Device or other identifiers (advertising ID)
- Shared with: Google AdMob (Google LLC)
- Purpose: Advertising
- Required: Yes (for ad-supported monetization); players can opt out via device settings

---

## Step 6 — Certifications

### Privacy Policy URL
`https://[studio-website]/privacy-policy`

Replace placeholder with live URL before submitting. URL must:
- Be publicly accessible (no login required)
- Load over HTTPS
- Contain a functional privacy policy
- Match the app's data practices

### Does your app meet the requirements of the Families Policy?
**NO** — King Smash is not directed at children under 13 and does not comply with the additional restrictions of the Designed for Families program.

### Is your organization a government entity?
**NO**

---

## Step 7 — Review and Submit

Before submitting the Data Safety section for review:

- [ ] All collected data types are declared
- [ ] All shared data types (AdMob GAID) are declared
- [ ] Privacy Policy URL is live and accessible
- [ ] Certifications are accurate
- [ ] Data practices in the form match the actual app behavior
- [ ] If Google Sign-In is disabled in a build, email/name declarations are still present (as they apply to production)

After saving, the Data Safety section enters Google's review queue. Review typically takes 1–7 days. The section is visible to users on your store listing once approved.

---

## Important Notes and Common Mistakes

### Firebase is NOT a third party for Data Safety purposes
When Firebase processes data under your Firebase project, it acts as a data processor for your app — not a third party that you share data with. Do not declare Firebase Analytics, Crashlytics, or Firestore as "third-party sharing" unless you have cross-app data sharing or Firebase-to-Firebase sharing across different projects configured.

The exception: AdMob. Even though AdMob is a Google product, it operates under a separate advertising data framework and the advertising ID sharing should be declared as shared for advertising purposes.

### Crashlytics opt-out
If Firebase Crashlytics is initialized automatically without a user opt-in mechanism, declare crash logs as "Required" (cannot be opted out of). If you add a settings toggle that calls `FirebaseCrashlytics.setCrashlyticsCollectionEnabled(false)`, you can declare it as Optional.

Firebase Crashlytics opt-out implementation (if desired):
```csharp
// In PlayerPrefs or settings system:
FirebaseCrashlytics.SetCrashlyticsCollectionEnabled(playerHasOptedIn);
```

### Analytics opt-out
Similarly, if you want to offer Analytics opt-out:
```csharp
FirebaseAnalytics.SetAnalyticsCollectionEnabled(playerHasOptedIn);
```

If these opt-outs are not implemented, declare both as Required.

### Account deletion requirement
As of Google Play policy, apps that allow account creation must provide an in-app pathway for account deletion. King Smash uses Firebase anonymous accounts (auto-created) and optional Google Sign-In. Ensure that:
1. Players can delete their account from within the game (Settings > Account > Delete Account)
2. Account deletion triggers deletion of Firestore game data and Firebase Auth account
3. The delete button calls both `FirebaseAuth.CurrentUser.DeleteAsync()` and the Firestore document deletion

### Data Safety vs. Permissions
The Data Safety form is separate from the `<uses-permission>` declarations in your AndroidManifest.xml. Permissions are technical; Data Safety is user-facing. Make sure both are consistent. King Smash should NOT declare location permissions in the manifest if location is not collected.

---

## Quick Reference Summary

| Data Type | Collected | Shared | Required | Ephemeral | Purpose |
|---|---|---|---|---|---|
| User IDs (Firebase UID) | YES | NO | YES | NO | App functionality, Analytics, Crash |
| Email | OPTIONAL | NO | NO | NO | App functionality (account linking) |
| Name | OPTIONAL | NO | NO | NO | App functionality (display) |
| App interactions | YES | NO | YES* | NO | Analytics |
| Crash logs | YES | NO | YES* | NO | Crash reporting |
| Diagnostics | YES | NO | YES* | NO | Crash reporting |
| Device identifiers | YES | YES (AdMob) | YES* | NO | Advertising, Analytics |
| Financial info | NO | — | — | — | — |
| Location | NO | — | — | — | — |

_* Declare as Optional if you implement opt-out toggles; Required otherwise._
