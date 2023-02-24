import WonderArenaReward_BasicBeasts1 from "./WonderArenaReward_BasicBeasts1.cdc"

pub fun hasRewardCollection(_ address: Address): Bool {
  return getAccount(address)
    .getCapability<&WonderArenaReward_BasicBeasts1.RewardCollection{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
    .check()
}

transaction {
  prepare(acct: AuthAccount) {
    if !hasRewardCollection(acct.address) {
      if acct.borrow<&WonderArenaReward_BasicBeasts1.RewardCollection>(from: WonderArenaReward_BasicBeasts1.RewardCollectionStoragePath) == nil {
        acct.save(<-WonderArenaReward_BasicBeasts1.createEmptyRewardCollection(), to: WonderArenaReward_BasicBeasts1.RewardCollectionStoragePath)
      }

      acct.unlink(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath)
      acct.link<&WonderArenaReward_BasicBeasts1.RewardCollection{WonderArenaReward_BasicBeasts1.IRewardCollectionPublic}>(WonderArenaReward_BasicBeasts1.RewardCollectionPublicPath, target: WonderArenaReward_BasicBeasts1.RewardCollectionStoragePath)
    }
  }
}