# King Smash — Privacy Policy

**Effective date:** 2026-10-05  
**Last updated:** 2026-10-05

---

This Privacy Policy explains how King Castle Studio ("we", "us", or "our") collects, uses, and protects information when you play King Smash (the "Game") on Android.

By downloading or playing King Smash, you agree to the practices described in this policy. If you do not agree, please do not use the Game.

**Contact:** support@[studio].com

---

## 1. Introduction

King Castle Studio operates King Smash, a mobile game available on the Google Play Store. We are committed to being transparent about the information we collect, how we use it, and what choices you have.

This policy applies to:
- The King Smash Android application
- Any associated backend services operated by King Castle Studio for game functionality

This policy does **not** apply to third-party services embedded in the Game (Firebase, AdMob, Google Play). Those services operate under their own privacy policies, which are linked in Section 4.

**Contact information:**  
King Castle Studio  
Email: support@[studio].com  
Website: https://[studio-website]/  

---

## 2. Information We Collect

### 2.1 Automatically Collected Information

When you play King Smash, the following information is collected automatically to make the game function and to identify and fix issues:

**Device Information (for crash reporting)**
- Device model and manufacturer
- Operating system version
- Screen resolution and density
- Available memory (at time of crash)
- App version number

This information is collected only when a crash or error occurs and is used exclusively for diagnosing and fixing technical issues. It is processed by Firebase Crashlytics.

**Gameplay Data (for game functionality and analytics)**
- Levels played and completed
- Star ratings earned per level
- Coins and gems earned and spent
- Power-up usage
- Upgrade purchases made within the game economy
- Session duration and frequency

All gameplay data is collected at the event level only. It is not linked to your real identity unless you choose to use Google Sign-In (see Section 2.2). This data is processed by Firebase Analytics.

**Firebase Installation ID**  
A randomly generated, non-persistent identifier assigned to your app installation. Used to correlate analytics events and crash reports from the same device session. This ID is not linked to your Google account or real identity.

**Advertising ID (for AdMob)**  
Your device's advertising identifier (Google Advertising ID / GAID) is accessed by the Google AdMob SDK to show ads, measure ad performance, and support optional ad personalization. You can reset or opt out of ad personalization via your device settings (Settings > Privacy > Ads).

---

### 2.2 Account Data (Optional)

King Smash supports two sign-in methods:

**Anonymous Authentication (default)**  
When you first launch the game, a Firebase anonymous user account is created automatically. This account is identified by a randomly generated Firebase User ID (UID). This UID is not linked to your name, email, Google account, or any personal information. It is used solely to save your game progress in the cloud.

**Google Sign-In (optional, user-initiated)**  
You may choose to link your game account to your Google account. If you do:
- Your Google profile **display name** is collected
- Your Google **email address** is collected

This information is used only for:
- Displaying your name within the game (account screen)
- Linking your game progress to your Google account for cross-device continuity

Your email is not used for marketing, shared with third parties, or stored beyond what is necessary for account linking. You may unlink your Google account at any time within the game settings, which reverts your account to anonymous mode.

---

### 2.3 What We Do NOT Collect

We do not collect:

- Real name (unless you voluntarily use Google Sign-In)
- Phone number
- Precise or approximate location
- Contacts or address book data
- Photos, videos, or media files
- Audio or microphone data
- Financial information or payment card data (purchases are processed by Google Play; we never receive payment card information)
- Health or fitness data
- Private messages or communications
- Browsing history outside the game
- Information about other installed apps

---

## 3. How We Use Information

| Information | How It Is Used |
|---|---|
| Firebase UID | Saving and restoring game progress; identifying a player session for crash correlation |
| Device information | Diagnosing crashes; improving stability on specific devices and OS versions |
| Gameplay events | Understanding which levels are too hard or too easy; improving game balance; aggregate analytics |
| Google display name | Shown in-game on the account screen |
| Google email | Account linking only; never used for marketing |
| Advertising ID | Serving ads via AdMob; measuring ad campaign performance |

We do not sell personal information. We do not use personal information for automated decision-making that produces legal or similarly significant effects.

---

## 4. Third-Party Services

King Smash integrates the following third-party services. Each operates under its own privacy policy. We encourage you to review them.

### Firebase (Google LLC)

We use multiple Firebase products:

| Firebase Product | Purpose | Data Processed |
|---|---|---|
| Firebase Authentication | Anonymous and Google Sign-In account management | Firebase UID, Google profile (if Sign-In used) |
| Cloud Firestore | Cloud save for game progression | Firebase UID, gameplay progress data |
| Firebase Analytics | Gameplay event tracking | Anonymized event data, Firebase UID |
| Firebase Crashlytics | Crash reporting | Device info, app version, crash stack traces |
| Firebase Remote Config | Server-side configuration updates | Firebase installation ID |

Firebase Privacy Policy: https://firebase.google.com/support/privacy  
Google Privacy Policy: https://policies.google.com/privacy

Firebase services are operated by Google LLC and process data in accordance with Google's data processing terms.

### Google AdMob

King Smash displays ads provided by Google AdMob, including:
- **Rewarded ads** — optional, player-initiated (watch an ad to earn in-game rewards)
- **Interstitial ads** — appear between levels

AdMob may use your device's advertising ID to serve personalized ads. You can opt out of personalized ads via your device settings.

AdMob Privacy Policy: https://policies.google.com/privacy  
AdMob Data Processing Terms: https://www.google.com/ads/aboutads/

### Google Play (Google LLC)

In-app purchases in King Smash are processed entirely by Google Play. King Castle Studio does not receive, store, or process payment card information. Purchase receipts are validated against the Google Play server-side API using order IDs only.

Google Payments Privacy Policy: https://payments.google.com/payments/apis-secure/get_legal_document?ldo=0&ldt=privacynotice  
Google Play Terms of Service: https://play.google.com/intl/en_us/about/play-terms/index.html

---

## 5. In-App Purchases

King Smash offers optional in-app purchases:
- **Coin packs** — in-game currency for upgrades and power-ups
- **Gem packs** — premium currency for additional purchases
- **Remove Ads** — a one-time purchase to disable interstitial ads

All purchases are processed by Google Play. We do not store, process, or have access to payment card numbers, bank account details, or other financial information. Purchase history (order IDs and entitlements) is stored in our backend solely to restore purchases if you reinstall the game.

All in-app purchases are final per Google Play's refund policy. For refund requests, contact Google Play support.

---

## 6. Advertising

King Smash displays advertisements. This section explains how ads work in the game.

**Types of ads:**
- **Rewarded ads:** Optional. You choose to watch an ad in exchange for in-game rewards (coins, gems, extra attempts). These are entirely player-initiated.
- **Interstitial ads:** Full-screen ads that appear between levels at natural break points.

**Ad personalization:**  
By default, AdMob may use your device's advertising ID to show ads relevant to your interests. You can opt out of personalized ads at any time:
- Android: Settings > Privacy > Ads > Opt out of Ads Personalization

Opting out of personalized ads does not remove ads from the game; it changes the type of ads shown (contextual rather than interest-based).

**Children and ads:**  
The game is not directed at children under 13. If we receive information that a user is under 13, ads will be served without personalization for that user.

**Ad SDK:**  
AdMob SDK version and behavior are subject to Google's updates. The SDK may collect device information as described in Google's privacy policies.

---

## 7. Children's Privacy

King Smash is not directed at children under the age of 13 (or the applicable age of digital consent in your jurisdiction).

We do not knowingly collect personal data from children under 13. The game does not include features designed to appeal specifically to young children, and we do not market the game to children under 13.

If a parent or guardian believes that their child under 13 has provided personal information to us, please contact us at support@[studio].com. Upon verification, we will delete that information as promptly as possible.

If you are located in the European Economic Area (EEA), the applicable age of consent may be higher (up to 16 depending on member state). Players under the applicable age of consent in their country should not use the optional Google Sign-In feature without parental consent.

---

## 8. Data Retention

| Data Type | Retention Period | Notes |
|---|---|---|
| Game progress (Firestore) | Until deletion requested OR 2 years of account inactivity | Deleted upon data deletion request |
| Firebase UID (anonymous) | Linked to Firestore data above | Deleted with game progress |
| Google Sign-In data | Until account unlinked or deletion requested | Email and name deleted upon request |
| Crash reports (Crashlytics) | 90 days | Automatic Crashlytics retention policy; we cannot extend or reduce this |
| Analytics events (Firebase Analytics) | 14 months | Firebase Analytics default retention; configurable in Firebase Console |
| Ad data (AdMob) | Per Google's AdMob data retention policies | We do not control AdMob retention |

After an account has been inactive for 2 years with no login, game progress data will be scheduled for deletion. Players will not receive a warning before this automatic deletion (as we do not have contact information for anonymous accounts). Using Google Sign-In allows us to notify you if we ever implement such notifications.

---

## 9. Your Rights

### GDPR (European Economic Area and UK)

If you are in the EEA or UK, you have the following rights under the General Data Protection Regulation:

- **Right of access:** Request a copy of the personal data we hold about you
- **Right to rectification:** Request correction of inaccurate personal data
- **Right to erasure ("right to be forgotten"):** Request deletion of your personal data
- **Right to restriction:** Request that we restrict processing of your personal data
- **Right to data portability:** Request your data in a machine-readable format
- **Right to object:** Object to processing based on legitimate interests
- **Right to withdraw consent:** Where processing is based on consent, withdraw it at any time

To exercise any of these rights, email support@[studio].com with the subject line "GDPR Data Request" and include your Firebase UID (visible in game Settings) or the Google account email linked to your account.

We will respond within 30 days. We may need to verify your identity before fulfilling a request.

You also have the right to lodge a complaint with your local data protection authority.

**Legal basis for processing (GDPR Article 6):**
- Game functionality data: Legitimate interests (necessary to provide the service you requested)
- Analytics: Legitimate interests (improving game quality; no PII involved)
- Google Sign-In data: Consent (you initiated Sign-In) and contract (necessary for cross-device service)
- Advertising: Consent (where required by applicable law)

### CCPA (California)

If you are a California resident, you have the following rights under the California Consumer Privacy Act:

- **Right to know:** Request disclosure of personal information collected, used, disclosed, or sold in the past 12 months
- **Right to delete:** Request deletion of personal information we have collected
- **Right to opt out of sale:** We do not sell personal information
- **Right to non-discrimination:** We will not discriminate against you for exercising your rights

To submit a CCPA request, email support@[studio].com with subject "CCPA Data Request".

### Data Deletion Request (All Users)

To request deletion of all data associated with your account:

1. Email support@[studio].com
2. Subject line: "Data Deletion Request"
3. Include: your Firebase UID (found in game Settings > Account) or the Google account email linked to your game
4. We will confirm deletion within 30 days

Note: Deleting your data is permanent and cannot be undone. Your game progress will be lost and cannot be restored.

---

## 10. Data Security

We take reasonable measures to protect your information:

- All data transmitted between the game and our servers uses TLS encryption
- Firebase services are hosted on Google Cloud infrastructure, which implements enterprise-grade physical and logical security controls
- Firebase Security Rules restrict Firestore read/write access so players can only access their own game data
- We do not store payment card information
- Anonymous authentication means most players never share personal information with us at all

Despite these measures, no internet transmission or electronic storage is completely secure. We cannot guarantee absolute security.

---

## 11. International Data Transfers

King Smash is operated from [Country]. If you are located in a different country, your data may be transferred to and processed in countries where our service providers (Google/Firebase) operate data centers, including the United States.

For users in the EEA/UK: data transfers to the US are conducted under Google's Standard Contractual Clauses and Google's compliance with applicable data transfer frameworks.

---

## 12. Changes to This Policy

We may update this Privacy Policy from time to time. When we make material changes:

- The "Last updated" date at the top of this page will be revised
- Players will be notified via an in-app notification on next game launch
- Continued use of the game after notification constitutes acceptance of the updated policy

For minor, non-material changes (typo fixes, clarifications that don't change data practices), we will update the policy without in-app notification.

We encourage you to review this policy periodically.

---

## 13. Contact

For questions, data requests, or concerns about this Privacy Policy:

**King Castle Studio**  
Email: support@[studio].com  
Subject line for data requests: "Privacy Request"  
Website: https://[studio-website]/  

We aim to respond to all privacy inquiries within 30 days.

---

_This privacy policy template was prepared for King Smash M16 soft launch. Before publishing, replace all `[studio]` and `[studio-website]` placeholders with your legal entity name and live domain. Have the final version reviewed by a qualified legal professional familiar with GDPR, CCPA, and Google Play policies before submission._
