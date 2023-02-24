import WonderArenaBattleField_BasicBeasts1 from "../contracts/WonderArenaBattleField_BasicBeasts1.cdc"

pub fun main(address: Address): [WonderArenaBattleField_BasicBeasts1.BeastGroup] {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        if let player = playerCap.borrow() {
            return player.getDefenderGroups()
        }
    }
    return []
}
