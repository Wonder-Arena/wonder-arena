// import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

transaction(beastIDs: [UInt64]) {
    let playerRef: &WonderArenaBattleField_BasicBeasts1.Player

    prepare(acct: AuthAccount) {
        self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
            ?? panic("borrow player failed")
    }

    execute {
        self.playerRef.removeDefenderGroup(members: beastIDs)
    }
}