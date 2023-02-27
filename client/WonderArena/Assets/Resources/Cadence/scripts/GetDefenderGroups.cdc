import WonderArenaBattleField_BasicBeasts1 from 0x469d7a2394a488bb

pub fun main(address: Address): [WonderArenaBattleField_BasicBeasts1.BeastGroup] {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        if let player = playerCap.borrow() {
            return player.getDefenderGroups()
        }
    }
    return []
}