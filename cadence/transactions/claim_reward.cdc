import WonderArenaReward_BasicBeasts1 from "./WonderArenaReward_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from "./WonderArenaBattleField_BasicBeasts1.cdc"

transaction(
    host: Address,
    rewardID: UInt64
) {
    let rewardCollectionRef: &{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}
    let playerRef: &WonderArenaBattleField_BasicBeasts1.Player
    prepare(acct: AuthAccount) {
        self.rewardCollectionRef = getAccount(host)
            .getCapability(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
            .borrow<&{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>()
            ?? panic("Borrow reward collection failed")

        self.playerRef = acct
            .borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("Borrow player failed")
    }

    execute {
        let reward = self.rewardCollectionRef.borrowPublicReward(id: rewardID)
            ?? panic("Borrow reward failed")

        reward.claim(player: self.playerRef)
    }
}