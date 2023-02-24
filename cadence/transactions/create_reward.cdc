import WonderArenaRewards_BasicBeasts1 from "../contracts/WonderArenaRewards_BasicBeasts1.cdc"
import BasicBeasts from "../contracts/basicbeasts/BasicBeasts.cdc"
// 
// flow transactions send ./cadence/transactions/create_reward.cdc 'For Best Trainers' 'Basic Beasts for you, the best trainers!' '[133176753, 133176718, 133176728, 133176813, 133176573, 133176478]' 60 true --signer=admin --network=testnet
transaction(
    name: String,
    description: String,
    beastIDs: [UInt64],
    scoreThreshold: Int64,
    isEnabled: Bool
) {
    let rewardCollectionRef: &WonderArenaRewards_BasicBeasts1.RewardCollection
    let beastCollectionRef: &BasicBeasts.Collection
    prepare(acct: AuthAccount) {
        self.rewardCollectionRef = acct
            .borrow<&WonderArenaRewards_BasicBeasts1.RewardCollection>(from: WonderArenaRewards_BasicBeasts1.RewardCollectionStoragePath)
            ?? panic("Borrow reward collection failed")

        self.beastCollectionRef = acct
            .borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
            ?? panic("Borrow beasts collection failed")
    }

    execute {
        let rewards <- BasicBeasts.createEmptyCollection()
        for id in beastIDs {
            let beast <- self.beastCollectionRef.withdraw(withdrawID: id)
            rewards.deposit(token: <- beast)
        }

        self.rewardCollectionRef.createReward(
            name: name,
            description: description,
            collection: <- rewards,
            scoreThreshold: scoreThreshold,
            isEnabled: isEnabled
        )
    }
}