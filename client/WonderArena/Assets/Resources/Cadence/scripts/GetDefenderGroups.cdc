// import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

pub fun main(address: Address): [[UInt64]] {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        if let player = playerCap.borrow() {
            return player.getDefenderGroups()
        }
    }
    return []
}