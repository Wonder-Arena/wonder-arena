import WonderArenaBattleField_BasicBeasts1 from 0x469d7a2394a488bb

pub fun main(address: Address): &WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}? {
    if let playerCap = WonderArenaBattleField_BasicBeasts1.players[address] {
        return playerCap.borrow()
    }
    return nil
}