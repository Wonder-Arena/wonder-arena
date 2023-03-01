# Wonder Arena

<div style="text-align: center;">
<img src="https://user-images.githubusercontent.com/88026162/222029643-4928b605-8189-4c47-960a-2ebff959290c.png" width="500">
</div>

Wonder Arena Contract Address

- Testnet: `0x469d7a2394a488bb`

To receive an app invite on iPhone, please email: bz.vanity@gmail.com  

Android APK: https://drive.google.com/drive/folders/1lOPrFZH4YDGPzWAX0iemCo_i-Iuta2zi?usp=share_link

## Walletless Onboarding

<img src="https://user-images.githubusercontent.com/88026162/222029341-b8c4c4a7-f981-4219-a404-c588d0805271.png">
We are committed to achieving a frictionless experience for WonderArena, making it accessible to mainstream audiences. Therefore, we have implemented the Walletless Onboarding Approach.  

In WonderArena, the basic process is as follows: first, the user logs in using their email / google accounts, and we generate a Flow account dedicated to Wonder Arena for that user. When the user needs to perform operations on the Flow blockchain, WonderArena's server will call the encrypted private key stored in the WonderArena database to sign transactions on behalf of the user. If users need to make payments, we also support payment through Stripe, so users do not need to hold any cryptocurrency.

This approach enables users to interact with WonderArena without needing to connect their wallets or sign transactions, making it a walletless and user-friendly experience for the mainstream. Here are a few things to note:

### Pre-made Flow accounts

**It takes about 10 seconds for a Flow blockchain transaction to be sealed. How can we ensure that users get their Flow accounts as soon as possible?**

To address this issue, we generate a batch of Flow accounts in advance and bind them to users after they register. This way, users can use their Flow accounts without waiting. Additionally, when the number of unallocated Flow accounts falls below a certain level, we will automatically replenish them. If a user consumes all the pre-generated Flow accounts in a short period of time, new Flow accounts will be generated immediately.

<img src="https://user-images.githubusercontent.com/88026162/222029002-875f750f-d9b4-4cdb-8408-16a5f1eee299.png" width="400">

### Key rotation

**Some on-chain operations require the administrator account of WonderArena to perform. If there are a large number of users simultaneously making multiple requests, how can we avoid users' requests from queuing up?**

In Flow, an account can have multiple keys, and each key has its own independent sequence number. Using different keys from the same account to initiate transactions simultaneously will not block each other. Thanks to this feature, we have generated many keys for the WonderArena administrator account, allowing administrators to handle multiple requests at the same time.

<img src="https://user-images.githubusercontent.com/88026162/222030181-12ca3eb0-e08e-4785-8d35-1fcda3069062.png" width="400">

### Hybrid Custody

**Since the keys are managed by WonderArena, users do not actually have control over the assets in their WonderArena accounts. How can this problem be solved?**

A complete walletless experience does not exclude Web3 Native users. By using the latest Account Linking feature from Flow, we allow users to set their WonderArena accounts as child accounts of their own wallets. This turns their WonderArena accounts into Hybrid Custody Accounts, giving users the freedom to control assets in their WonderArena accounts.

<img src="https://user-images.githubusercontent.com/88026162/222030311-61e397e5-f885-4dad-b38a-528e799ee0a1.png" width="400">

