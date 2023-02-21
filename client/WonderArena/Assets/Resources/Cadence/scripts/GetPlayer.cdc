// import WonderArenaBattleField_BasicBeasts1 from "../../contracts/WonderArenaBattleField_BasicBeasts1.cdc"
import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

pub fun main(address: Address): &WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}? {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        return playerCap.borrow()
    }
    return nil
}